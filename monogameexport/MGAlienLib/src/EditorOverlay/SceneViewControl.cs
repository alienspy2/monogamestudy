using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MGAlienLib.Tweening;
using MGAlienLib.Utility;
using MonoGame.Extended;
using System;

namespace MGAlienLib
{
    public class SceneViewControl : ComponentBase
    {
        private const bool drawGridFlag = false;

        private MeshRenderer[] arrows;
        private GameObject _selectedObject;
        private Camera _targetCamera;

        private int grabAxis = -1; // 0:X, 1:Y, 2:Z
        private Vector3? oldDragPoint1 = null;
        private Vector3? oldDragPoint2 = null;

        private Vector3 virtualCameraLookAtTarget = Vector3.Zero;

        private float moveSpeed = 0.5f;
        private float rotateSpeed = 0.2f;
        private bool activated = true;

        public bool Activated => activated;

        private UIPanel sceneControlPanel;



        public void SetTargetCamera(Camera targetCamera)
        {
            _targetCamera = targetCamera;
        }


        public void Activate()
        {
            activated = true;
            sceneControlPanel?.gameObject.SetActive(true);
            if (_selectedObject != null)
            {
                transform.position = _selectedObject.transform.position;
                transform.localScale = Vector3.One;
            }
            else
            {
                transform.localScale = Vector3.Zero;
            }
        }

        public void Deactivate()
        {
            activated = false;
            sceneControlPanel?.gameObject.SetActive(false);
            transform.localScale = Vector3.Zero;
        }

        public override void Update()
        {
            base.Update();

            if (inputManager.WasPressedThisFrame(Keys.F2))
            {
                if (activated) Deactivate();
                else Activate();
            }

            if (activated)
            {
                UpdateTransformGizmo();
                UpdateSelectTargetWithBounds();
                UpdateFlyCam();

                if (drawGridFlag) DrawGrid();
            }
            else
            {
                grabAxis = -1;
                oldDragPoint1 = null;
                oldDragPoint2 = null;
            }
        }

        private void DrawGrid()
        {
            if (_targetCamera == null) return;
            if (_selectedObject == null) return;

            var gridCenter = _selectedObject.transform.position;

            // Calculate the distance from the camera to the virtualCameraLookAtTarget
            float distance = Vector3.Distance(_targetCamera.transform.position, gridCenter);

            // Snap grid size to 1m, 10m, or 100m based on distance
            float gridSize;
            if (distance <= 50f) // Use 10m grid for medium distances
            {
                gridSize = 10f;
            }
            else // Use 100m grid for far distances
            {
                gridSize = 100f;
            }

            // Determine grid spacing (1/10th of gridSize for 10 lines per side)
            float gridSpacing = gridSize * 0.1f; // Results in 0.1m, 1m, or 10m spacing
            int lineCount = 10; // Fixed at 10 lines per side to match 1/10th spacing

            // Draw grid lines along X-axis
            for (int i = -lineCount; i <= lineCount; i++)
            {
                float z = gridCenter.Z + i * gridSpacing;
                Vector3 start = new Vector3(gridCenter.X - gridSize, gridCenter.Y, z);
                Vector3 end = new Vector3(gridCenter.X + gridSize, gridCenter.Y, z);
                Color color = (i == 0) ? Color.White : Color.Gray; // Highlight Z-axis
                DebugDraw.DrawLine(start, end, color);
            }

            // Draw grid lines along Z-axis
            for (int i = -lineCount; i <= lineCount; i++)
            {
                float x = gridCenter.X + i * gridSpacing;
                Vector3 start = new Vector3(x, gridCenter.Y, gridCenter.Z - gridSize);
                Vector3 end = new Vector3(x, gridCenter.Y, gridCenter.Z + gridSize);
                Color color = (i == 0) ? Color.White : Color.Gray; // Highlight X-axis
                DebugDraw.DrawLine(start, end, color);
            }
        }

        private void UpdateSelectTargetWithBounds()
        {
            if (_targetCamera == null) return;

            if (grabAxis != -1) return; // gizmo 조작중일 때는 선택 안함

            if (inputManager.InputIsConsumedByUI) return;

            if (inputManager.WasPressedThisFrame(eMouseButton.Left) 
                && !inputManager.IsPressed(eMouseButton.Right)
                && !inputManager.IsPressed(Keys.LeftAlt))
            {
                var mousePos = inputManager.GetMousePos();
                var p1 = _targetCamera.UnprojectAtZ(mousePos, +1000);
                var p2 = _targetCamera.UnprojectAtZ(mousePos, -1000);
                var ray = new Ray(p1, p2 - p1);
                GameObject closestObj = null;
                float? closestDist = null;

                foreach (var rdr in game.renderQueue.internal_Renderables)
                {
                    if (rdr == null) continue;
                    if (rdr.gameObject == null) continue;
                    if (rdr.gameObject.active == false) continue;
                    if (rdr is MeshRenderer meshRdr)
                    {
                        if (rdr.transform.IsSelectableInSceneView == false) continue;

                        float? hit = meshRdr.RaycastToBounds(ray);
                        if (hit.HasValue && !float.IsNaN(hit.Value))
                        {
                            if (closestDist == null || hit.Value < closestDist.Value)
                            {
                                closestDist = hit.Value;
                                closestObj = rdr.gameObject;
                            }
                        }   
                    }
                }

                if (closestObj != null)
                {
                    selectionManager.ClearSelection();
                    selectionManager.AddToSelection(closestObj);
                }
                else
                {
                    selectionManager.ClearSelection();
                }
            }
        }

        private void UpdateFlyCam()
        {
            if (_targetCamera == null) return;


            // 카메라 회전
            if (inputManager.IsPressed(eMouseButton.Right))
            {
                var distanceToVirtualCameraLookAtTarget = Vector3.Distance(_targetCamera.transform.position, virtualCameraLookAtTarget);
                var delta = inputManager.GetMousePosDelta();
                var angles = _targetCamera.transform.rotation.ToEulerAngles();
                angles.Y -= delta.X * rotateSpeed;
                angles.X -= delta.Y * rotateSpeed;
                angles.X = MathHelper.Clamp(angles.X, -89, +89);
                _targetCamera.transform.rotation = angles.FromEulerAnglesToQuaternion();
                virtualCameraLookAtTarget = _targetCamera.transform.position + _targetCamera.transform.forward * distanceToVirtualCameraLookAtTarget;
            }

            // orbit camera
            if (inputManager.IsPressed(Keys.LeftAlt))
            {
                if (inputManager.IsPressed(eMouseButton.Left))
                {
                    var delta = inputManager.GetMousePosDelta();

                    // Get the current direction from camera to target
                    var dirToTarget = virtualCameraLookAtTarget - _targetCamera.transform.position;
                    var dist = dirToTarget.Length();
                    dirToTarget = dirToTarget.Normalized();

                    // Calculate yaw (horizontal rotation) and pitch (vertical rotation)
                    var yaw = -delta.X * rotateSpeed; // Horizontal mouse movement
                    var pitch = -delta.Y * rotateSpeed; // Vertical mouse movement

                    // Current Euler angles of the camera
                    var angles = _targetCamera.transform.rotation.ToEulerAngles();

                    // Apply yaw and pitch rotations
                    angles.Y += yaw;
                    angles.X += pitch;
                    angles.X = MathHelper.Clamp(angles.X, -89, 89); // Prevent flipping

                    // Convert Euler angles back to quaternion for rotation
                    var newRotation = angles.FromEulerAnglesToQuaternion();

                    // Calculate new camera position by rotating the direction vector
                    var newDir = Vector3.Transform(Vector3.Forward, newRotation); // Assuming Forward is (0,0,1)
                    _targetCamera.transform.position = virtualCameraLookAtTarget - newDir * dist;

                    // Update camera rotation to look at the target
                    _targetCamera.transform.LookAt(virtualCameraLookAtTarget, Vector3.Up);
                }
            }


            // 이 뒷부분은 UI 에 마우스 오버 되어있을 때는 동작하지 않도록 해야함
            if (inputManager.InputIsConsumedByUI) return;

            float run = inputManager.IsPressed(Keys.LeftShift) ? 4f : 1f;
            Vector3 movement = Vector3.Zero;

            // 카메라 이동
            if (inputManager.IsPressed(Keys.W))
            {
                movement += _targetCamera.transform.forward * moveSpeed * run;
            }
            if (inputManager.IsPressed(Keys.S))
            {
                movement -= _targetCamera.transform.forward * moveSpeed * run;
            }
            if (inputManager.IsPressed(Keys.A))
            {
                movement -= _targetCamera.transform.right * moveSpeed * run;
            }
            if (inputManager.IsPressed(Keys.D))
            {
                movement += _targetCamera.transform.right * moveSpeed * run;
            }
            if (inputManager.IsPressed(Keys.Q))
            {
                movement -= _targetCamera.transform.up * moveSpeed * run;
            }
            if (inputManager.IsPressed(Keys.E))
            {
                movement += _targetCamera.transform.up * moveSpeed * run;
            }

            _targetCamera.transform.position += movement;
            virtualCameraLookAtTarget += movement;

            // focus to selected
            if (inputManager.WasPressedThisFrame(Keys.F))
            {
                // 카메라가 바라보는 가상의 목표점 을 대상의 pivot 으로 설정. 방향은 바꾸지 않는다
                if (_selectedObject != null)
                {
                    virtualCameraLookAtTarget = _selectedObject.transform.position;
                    var dir = _targetCamera.transform.forward;
                    var dist = Vector3.Distance(_targetCamera.transform.position, virtualCameraLookAtTarget);
                    _targetCamera.transform.DOMove(virtualCameraLookAtTarget - dir * dist, .1f).SetEase(eEasingType.EaseInOutQuad);
                }
            }

            // zoom in/out
            var scroll = inputManager.GetMouseWheelDelta();
            if (scroll != 0)
            {
                var dir = _targetCamera.transform.forward;
                var dist = Vector3.Distance(_targetCamera.transform.position, virtualCameraLookAtTarget);
                dist *= (1 - scroll * 0.001f);
                dist = MathHelper.Clamp(dist, 1, 1000);
                _targetCamera.transform.position = virtualCameraLookAtTarget - dir * dist;
            }

            // panning
            if (inputManager.IsPressed(eMouseButton.Middle))
            {
                var delta = inputManager.GetMousePosDelta();
                var right = _targetCamera.transform.right;
                var up = _targetCamera.transform.up;
                var distance = Vector3.Distance(_targetCamera.transform.position, virtualCameraLookAtTarget);
                var speed = 0.001f;

                _targetCamera.transform.position -= right * delta.X * speed * distance;
                _targetCamera.transform.position += up * delta.Y * speed * distance;
                virtualCameraLookAtTarget -= right * delta.X * speed * distance;
                virtualCameraLookAtTarget += up * delta.Y * speed * distance;
            }
        }

        private void UpdateTransformGizmo()
        {
            // 선택된 오브젝트가 없으면 gizmo 도 숨김
            if (selectionManager.count > 0)
            {
                var obj = selectionManager.gameObjects[^1];
                if (obj != _selectedObject)
                {
                    _selectedObject = selectionManager.gameObjects[^1];
                    transform.position = _selectedObject.transform.position;
                    transform.localScale = Vector3.One;
                }
            }
            else
            {
                _selectedObject = null;
                transform.localScale = Vector3.Zero;
            }

            // selected object position 이동
            if (_targetCamera != null)
            {
                if (grabAxis == -1)
                {
                    if (inputManager.WasPressedThisFrame(eMouseButton.Left))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            var mousePos = inputManager.GetMousePos();
                            // todo : 범위를 실시간으로 계산
                            // todo : 현재 바라보는 축에 따라 projection 을 바꿔야 한다.
                            var p1 = _targetCamera.UnprojectAtZ(mousePos, +1000); 
                            var p2 = _targetCamera.UnprojectAtZ(mousePos, -1000);
                            float? result = arrows[i].RaycastToBounds(new Ray(p1, p2 - p1));
                            if (result.HasValue && !float.IsNaN(result.Value))
                            {
                                grabAxis = i;
                                break;
                            }
                        }
                    }

                    if (inputManager.WasReleasedThisFrame(eMouseButton.Left))
                    {
                        grabAxis = -1;
                        oldDragPoint1 = null;
                        oldDragPoint2 = null;
                    }
                }
                else
                {
                    if (inputManager.IsPressed(eMouseButton.Left) && grabAxis != -1)
                    {
                        var mousePos = inputManager.GetMousePos();
                        var dragPoint1 = _targetCamera.UnprojectAtZ(mousePos, transform.position.Z);
                        var dragPoint2 = _targetCamera.UnprojectAtX(mousePos, transform.position.X);

                        if (oldDragPoint1.HasValue && (grabAxis != 2))
                        {
                            var delta = dragPoint1 - oldDragPoint1.Value;
                            if (grabAxis == 0) delta = new Vector3(delta.X, 0, 0);
                            else if (grabAxis == 1) delta = new Vector3(0, delta.Y, 0);
                            transform.position += delta;
                            if (_selectedObject != null)
                            {
                                _selectedObject.transform.position = transform.position;
                            }
                        }

                        if (oldDragPoint2.HasValue && (grabAxis == 2))
                        {
                            var delta = dragPoint2 - oldDragPoint2.Value;
                            if (grabAxis == 2) delta = new Vector3(0, 0, delta.Z);
                            transform.position += delta;
                            if (_selectedObject != null)
                            {
                                _selectedObject.transform.position = transform.position;
                            }
                        }

                        oldDragPoint1 = dragPoint1;
                        oldDragPoint2 = dragPoint2;
                    }
                    else
                    {
                        grabAxis = -1;
                        oldDragPoint1 = null;
                        oldDragPoint2 = null;
                    }
                }
            }
        }

        public override void Awake()
        {
            base.Awake();
            transform.hideInHierarchy = true;


            if (arrows == null)
            {
                arrows = new MeshRenderer[3];

                var XArrowObj = CreateGameObject("XArrow", transform);
                var XArrow = XArrowObj.AddComponent<MeshRenderer>();
                XArrow.Load("raw://EditorResources/arrow.glb");
                XArrow.LoadMaterial("MG/3D/Lit");
                XArrow.BreakMaterialSharing();
                XArrow.material.asset.SetColor("_BaseColor", Color.Red);
                XArrow.material.asset.zWrite = false;
                XArrow.material.asset.zTest = false;
                XArrow.material.asset.renderPriority = 100;
                // +X 방향으로 회전
                XArrowObj.transform.rotation = (Vector3.Up * 90).FromEulerAnglesToQuaternion();
                arrows[0] = XArrow;

                var YArrowObj = CreateGameObject("YArrow", transform);
                var YArrow = YArrowObj.AddComponent<MeshRenderer>();
                YArrow.Load("raw://EditorResources/arrow.glb");
                YArrow.LoadMaterial("MG/3D/Lit");
                YArrow.BreakMaterialSharing();
                YArrow.material.asset.SetColor("_BaseColor", Color.Green);
                YArrow.material.asset.zWrite = false;
                YArrow.material.asset.zTest = false;
                YArrow.material.asset.renderPriority = 100;
                // +Y 방향으로 회전
                YArrowObj.transform.rotation = (Vector3.Right * -90).FromEulerAnglesToQuaternion();
                arrows[1] = YArrow;

                var ZArrowObj = CreateGameObject("ZArrow", transform);
                var ZArrow = ZArrowObj.AddComponent<MeshRenderer>();
                ZArrow.Load("raw://EditorResources/arrow.glb");
                ZArrow.LoadMaterial("MG/3D/Lit");
                ZArrow.BreakMaterialSharing();
                ZArrowObj.transform.rotation = (Vector3.Up * 180).FromEulerAnglesToQuaternion();
                ZArrow.material.asset.SetColor("_BaseColor", Color.Blue);
                ZArrow.material.asset.zWrite = false;
                ZArrow.material.asset.zTest = false;
                ZArrow.material.asset.renderPriority = 100;
                // +Z 방향으로 회전
                arrows[2] = ZArrow;
            }

            if (sceneControlPanel == null)
            {
                sceneControlPanel = UISceneViewControlPanel.Build(uiman.uiRoot);
            }
        }

    }
}
