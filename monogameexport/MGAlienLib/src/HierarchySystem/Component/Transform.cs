using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MGAlienLib
{
    /// <summary>
    /// 게임 오브젝트의 변환 정보를 담는 컴포넌트
    /// </summary>
    public class Transform : ComponentBase, IEnumerable<Transform>
    {
        // 로컬 변환 요소
        [SerializeField] private Vector3 _localPosition;
        [SerializeField] private Quaternion _localRotation;
        [SerializeField] private Vector3 _localScale = Vector3.One;
        [SerializeField(hideInInspector: true)] private Transform _parent;

        private List<Transform> children = new List<Transform>();

        // 캐싱 및 Dirty Flag
        private Matrix _transformMatrix;
        private Matrix _invTransformMatrix;
        private bool isLocalDirty = true;

        // todo : parent 의 matrix 들 까지 모두 cache
        private Matrix _cachedLocalToWorldMatrix;
        private Matrix _cachedWorldToLocalMatrix;
        private bool isWorldDirty = true;
        private bool _hideInHierarchy = false;

        /// <summary>
        /// local space 에서의 위치를 가져오거나 설정합니다.
        /// </summary>
        public Vector3 localPosition
        {
            get => _localPosition;
            set
            {
                if (_localPosition != value)
                {
                    _localPosition = value;
                    SetLocalDirty();
                }
            }
        }

        /// <summary>
        /// local space 에서의 회전을 가져오거나 설정합니다.
        /// </summary>
        public Quaternion localRotation
        {
            get => _localRotation;
            set
            {
                if (_localRotation != value)
                {
                    _localRotation = value;
                    SetLocalDirty();
                }
            }
        }

        /// <summary>
        /// local space 에서의 크기를 가져오거나 설정합니다.
        /// </summary>
        public Vector3 localScale
        {
            get => _localScale;
            set
            {
                if (_localScale != value)
                {
                    _localScale = value;
                    SetLocalDirty();
                }
            }
        }

        // 계층 구조
        /// <summary>
        /// 부모 트랜스폼을 가져오거나 설정합니다.
        /// </summary>
        public Transform parent
        {
            get => _parent;
            set => SetParent(value);
        }

        public bool hideInHierarchy
        {
            get => _hideInHierarchy;
            set => _hideInHierarchy = value;
        }

        // 생성자
        /// <summary>
        /// 새로운 Transform 인스턴스를 생성합니다.
        /// </summary>
        public Transform()
        {
            _localPosition = Vector3.Zero;
            _localRotation = Quaternion.Identity;
            _localScale = Vector3.One;
            _transformMatrix = Matrix.Identity;
            _invTransformMatrix = Matrix.Identity;
        }

        /// <summary>
        /// world space 에서의 위치를 가져오거나 설정합니다.
        /// </summary>
        public Vector3 position
        {
            get
            {
                if (_parent == null)
                    return _localPosition;
                return Vector3.Transform(_localPosition, _parent.localToWorldMatrix);
            }
            set
            {
                if (_parent == null)
                    localPosition = value;
                else
                {
                    Vector3 parentScale = _parent.scale;
                    if (Math.Abs(parentScale.X) < 1e-6f || Math.Abs(parentScale.Y) < 1e-6f)
                    {
                        localPosition = value;
                    }
                    else
                    {
                        Matrix inverseParentMatrix = _parent.worldToLocalMatrix;
                        localPosition = Vector3.Transform(value, inverseParentMatrix);
                    }
                }
            }
        }

        /// <summary>
        /// world space 에서의 회전을 가져오거나 설정합니다.
        /// </summary>
        public Quaternion rotation
        {
            get
            {
                if (_parent == null)
                    return _localRotation;
                return _parent.rotation * _localRotation;
            }
            set
            {
                if (_parent == null)
                    localRotation = value;
                else
                    localRotation = Quaternion.Inverse(_parent.rotation) * value;
            }
        }

        /// <summary>
        /// world space 에서의 크기를 가져오거나 설정합니다.
        /// </summary>
        public Vector3 scale
        {
            get
            {
                if (_parent == null)
                    return _localScale;
                Vector3 parentScale = _parent.scale;
                return parentScale * _localScale;
            }
            set
            {
                if (_parent == null)
                    localScale = value;
                else
                {
                    Vector3 parentScale = _parent.scale;

                    // 0일 경우를 방지하기 위해 안전한 연산 수행
                    Vector3 safeParentScale = new Vector3(
                        parentScale.X != 0 ? parentScale.X : 1, // 0이면 기본값 1 사용
                        parentScale.Y != 0 ? parentScale.Y : 1,
                        parentScale.Z != 0 ? parentScale.Z : 1
                    );

                    localScale = value / safeParentScale;
                }
            }
        }

        /// <summary>
        /// world space 에서 transform 이 바라보는 방향에 상대적으로 오른쪽 방향을 가져옵니다.
        /// </summary>
        public Vector3 right => Vector3.Transform(Vector3.UnitX, rotation);
        /// <summary>
        /// world space 에서 transform 이 바라보는 방향에 상대적으로 왼쪽 방향을 가져옵니다.
        /// </summary>
        public Vector3 left => Vector3.Transform(-Vector3.UnitX, rotation);
        /// <summary>
        /// world space 에서 transform 이 바라보는 방향에 상대적으로 위쪽 방향을 가져옵니다.
        /// </summary>
        public Vector3 up => Vector3.Transform(Vector3.UnitY, rotation);
        /// <summary>
        /// world space 에서 transform 이 바라보는 방향에 상대적으로 아래쪽 방향을 가져옵니다.
        /// </summary>
        public Vector3 down => Vector3.Transform(-Vector3.UnitY, rotation);
        /// <summary>
        /// world space 에서 transform 이 바라보는 방향을 가져옵니다.
        /// </summary>
        public Vector3 forward => Vector3.Transform(Vector3.UnitZ, rotation);
        /// <summary>
        /// world space 에서 transform 이 바라보는 반대 방향을 가져옵니다.
        /// </summary>
        public Vector3 Backward => Vector3.Transform(-Vector3.UnitZ, rotation);


        /// <summary>
        /// 부모 트랜스폼을 설정합니다.
        /// </summary>
        /// <param name="newParent"></param>
        public void SetParent(Transform newParent)
        {
            if (_parent != newParent)
            {
                if (_parent != null)
                    _parent.RemoveChild(this);
                _parent = newParent;
                if (newParent != null)
                    newParent.AddChild(this);
            }
        }

        public void internal_UpdateMatrix()
        {
            if (isLocalDirty)
            {
                _transformMatrix =
                    Matrix.CreateScale(_localScale) *         // 크기 적용
                    Matrix.CreateFromQuaternion(_localRotation) * // 회전 적용
                    Matrix.CreateTranslation(_localPosition); // 위치 적용
                _invTransformMatrix = Matrix.Invert(_transformMatrix);
                isLocalDirty = false;
            }
        }

        /// <summary>
        /// local space 에서 world space 로 변환하는 행렬을 가져옵니다.
        /// </summary>
        public Matrix localToWorldMatrix
        {
            get
            {
                internal_UpdateMatrix();

                if (_parent != null)
                    return _transformMatrix * _parent.localToWorldMatrix;

                return _transformMatrix;
            }
        }

        /// <summary>
        /// world space 에서 local space 로 변환하는 행렬을 가져옵니다.
        /// </summary>
        public Matrix worldToLocalMatrix
        {
            get
            {
                internal_UpdateMatrix();
                if (_parent != null)
                    return _parent.worldToLocalMatrix * _invTransformMatrix;

                return _invTransformMatrix;
            }
        }

        /// <summary>
        /// 자식 노드들
        /// </summary>
        public IReadOnlyList<Transform> Children => children.AsReadOnly();

        /// <summary>
        /// 자식 노드의 개수
        /// </summary>
        public int childCount => children.Count;

        /// <summary>
        /// 부모 노드 자식중 첫번째 자식으로 설정합니다.
        /// </summary>
        public void SetAsFirstSibling()
        {
            if (_parent == null) return;
            _parent.children.Remove(this);
            _parent.children.Insert(0, this);
        }

        /// <summary>
        /// 부모 노드 자식중 마지막 자식으로 설정합니다.
        /// </summary>
        public void SetAsLastSibling()
        {
            if (_parent == null) return;
            _parent.children.Remove(this);
            _parent.children.Add(this);
        }

        /// <summary>
        /// 부모 노드 자식중 몇 번째 인지 가져옵니다
        /// </summary>
        /// <returns></returns>
        public int GetSiblingIndex()
        {
            if (_parent == null) return -1;
            return _parent.children.IndexOf(this);
        }

        /// <summary>
        /// 자식 노드중 index 번 째의 자식을 가져옵니다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Transform GetChild(int index)
        {
            return children[index];
        }

        /// <summary>
        /// 자식 노드의 개수를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public int GetChildCount()
        {
            return children.Count;
        }

        private void AddChild(Transform child)
        {
            children.Add(child);
            child._parent = this;
        }

        private void RemoveChild(Transform child)
        {
            var index = children.IndexOf(child);
            //if (index != -1)
            //    children.RemoveAt(index);
            children.Remove(child);
            // zombie 제거
            //children = children.FindAll(x => x != null);
            child._parent = null;
        }

        /// <summary>
        /// 모든 자식 노드를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public Transform[] GetChildren()
        {
            return children.ToArray();
        }

        // Dirty 상태 설정
        private void SetLocalDirty()
        {
            isLocalDirty = true;
        }

        /// <summary>
        /// Transform class 를 순환 할 수 있도록 해줍니다.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Transform> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// point 하나를 local space 에서 world space 로 변환합니다.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 TransformPoint(Vector3 position)
        {
            return Vector3.Transform(position, localToWorldMatrix);
        }

        /// <summary>
        /// vector 하나를 local space 에서 world space 로 변환합니다.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public Vector3 TransformVector(Vector3 vector)
        {
            Matrix worldMatrix = localToWorldMatrix;
            worldMatrix.Translation = Vector3.Zero; // 위치 제거
            return Vector3.Transform(vector, worldMatrix);
        }

        /// <summary>
        /// 여러개의 point 를 local space 에서 world space 로 변환합니다.
        /// </summary>
        /// <param name="positions"></param>
        public void TransformPoints(Span<Vector3> positions)
        {
            Matrix worldMatrix = localToWorldMatrix;

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = Vector3.Transform(positions[i], worldMatrix);
            }
        }

        /// <summary>
        /// 여러개의 vector 를 local space 에서 world space 로 변환합니다.
        /// </summary>
        /// <param name="vectors"></param>
        public void TransformVectors(Span<Vector3> vectors)
        {
            Matrix worldMatrix = localToWorldMatrix;
            worldMatrix.Translation = Vector3.Zero;

            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i] = Vector3.Transform(vectors[i], worldMatrix);
            }
        }

        /// <summary>
        /// world space 에서 현재 position 을 기준으로 target 을 바라보도록 회전합니다.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="worldUp"></param>
        public void LookAt(Vector3 target, Vector3 worldUp)
        {
            Matrix mat = Matrix.CreateLookAt(position, target, worldUp);
            var mitInverse = Matrix.Invert(mat);
            mitInverse.Decompose(out var decomposedScale, out var decomposedRotation, out var decomposedTranslation);

            position = decomposedTranslation;
            rotation = decomposedRotation;
        }

        /// <summary>
        /// world space 에서 현재 position 을 기준으로 target 을 바라보도록 회전합니다.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="worldUp"></param>
        public void LookAt(Transform target, Vector3 worldUp)
        {
            LookAt(target.position, worldUp);
        }

        /// <summary>
        /// 자식 노드들을 *모두* 가져옵니다.
        /// 성능에 주의해야 합니다.
        /// </summary>
        /// <param name="collector"></param>
        public void GetDescendants(ref List<Transform> collector)
        {
            foreach (var child in children)
            {
                child.GetDescendants(ref collector);
            }
            collector.Add(this);
        }

        public override void FinalizeDeserialize(DeserializeContext context)
        {
            base.FinalizeDeserialize(context);
            _parent?.AddChild(this);
        }

        public override void internal_Invalidate()
        {
            SetLocalDirty();
        }

    }
}