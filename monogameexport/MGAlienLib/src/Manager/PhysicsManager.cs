using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;


namespace MGAlienLib
{
    public partial class PhysicsManager : ManagerBase
    {
        public const float TimestepDuration = 1 / 60f;



        public Simulation simulation { get; protected set; }
        public BufferPool bufferPool { get; private set; }
        public ThreadDispatcher ThreadDispatcher { get; private set; }

        public PhysicsManager(GameBase owner) : base(owner)
        {

        }

        public override void OnPreLoadContent()
        {
            base.OnPostLoadContent();

            bufferPool = new BufferPool();
            var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new ThreadDispatcher(targetThreadCount);
            simulation = Simulation.Create(
                bufferPool,
                new NarrowPhaseCallbacks(),
                new PoseIntegratorCallbacks(new System.Numerics.Vector3(0, -9.81f, 0)), // 중력
                new SolveDescription(8, 1)
            );

        }

        public override void OnPostUpdate()
        {
            base.OnPostUpdate();
            simulation.Timestep(TimestepDuration, ThreadDispatcher);

        }


        #region STATIC


        // sphere
        public StaticHandle? AddOrUpdateStatic(StaticHandle? handle, Transform t, float _in_radius)
        {
            RigidPose pose = new RigidPose(Convert(t.position), Convert(t.rotation));
            var radius = _in_radius * (t.scale.X + t.scale.Y + t.scale.Z) / 3f;
            var shapeIndex = simulation.Shapes.Add(new Sphere(radius));
            var desc = new StaticDescription(pose, simulation.Shapes.Add(new Sphere(radius)));

            if (handle.HasValue && simulation.Statics.StaticExists(handle.Value))
            {
                // 기존 핸들이 있으면 이전 Shape 제거
                var staticReference = simulation.Statics[handle.Value];
                var oldShapeIndex = staticReference.Shape;

                // 업데이트 적용
                simulation.Statics.ApplyDescription(handle.Value, desc);

                // 이전 Shape 제거
                simulation.Shapes.Remove(oldShapeIndex);
            }
            else
            {
                handle = simulation.Statics.Add(desc);
            }

            return handle;
        }


        // box
        public StaticHandle? AddOrUpdateStatic(StaticHandle? handle, Transform t, Microsoft.Xna.Framework.BoundingBox box)
        {
            var center = (box.Min + box.Max) / 2f;
            RigidPose pose = new RigidPose(Convert(t.position + center), Convert(t.rotation));
            Vector3 size = Convert(box.Max - box.Min);
            size.X = size.X * t.scale.X;
            size.Y = size.Y * t.scale.Y;
            size.Z = size.Z * t.scale.Z;
            var shapeIndex = simulation.Shapes.Add(new Box(size.X, size.Y, size.Z));
            var desc = new StaticDescription(pose, shapeIndex);

            if (handle.HasValue && simulation.Statics.StaticExists(handle.Value))
            {
                // 기존 핸들이 있으면 이전 Shape 제거
                var staticReference = simulation.Statics[handle.Value];
                var oldShapeIndex = staticReference.Shape;

                simulation.Statics.ApplyDescription(handle.Value, desc);

                // 이전 Shape 제거
                simulation.Shapes.Remove(oldShapeIndex);
            }
            else
            {
                handle = simulation.Statics.Add(desc);
            }

            return handle;
        }


        public void RemoveStatic(StaticHandle? handle)
        {
            if (handle == null) return;

            // StaticReference 가져오기 (인덱서 사용)
            var staticReference = simulation.Statics[handle.Value];

            // ShapeIndex 추출
            var shapeIndex = staticReference.Shape;

            // StaticHandle 제거
            simulation.Statics.Remove(handle.Value);

            // Shape 제거 (참조 카운트가 0이 되면 실제로 삭제됨)
            simulation.Shapes.Remove(shapeIndex);
        }
        #endregion

        #region DYNAMIC

        public BodyHandle? AddOrUpdateBody(BodyHandle? handle, Transform t, float _in_radius, float mass)
        {
            RigidPose pose = new RigidPose(Convert(t.position), Convert(t.rotation));
            var radius = _in_radius * (t.scale.X + t.scale.Y + t.scale.Z) / 3f;
            var sphere = new Sphere(radius);
            var inertia = sphere.ComputeInertia(mass);
            var shapeIndex = simulation.Shapes.Add(sphere);
            var newShapeIndex = simulation.Shapes.Add(sphere);

            if (handle.HasValue && simulation.Bodies.BodyExists(handle.Value))
            {
                // 기존 BodyHandle이 있으면 이전 Shape 제거
                var bodyRef = simulation.Bodies.GetBodyReference(handle.Value);

                // 이전 ShapeIndex 가져오기
                var oldShapeIndex = bodyRef.Collidable.Shape;

                // 이전 Shape 제거
                simulation.Shapes.Remove(oldShapeIndex);

                // 새로운 BodyDescription 생성 (기존 속성 재사용)
                var desc = new BodyDescription
                {
                    Pose = pose, // 새로운 위치와 회전
                    Velocity = bodyRef.Velocity, // 기존 속도 유지
                    LocalInertia = inertia, // 새로운 관성
                    Collidable = new CollidableDescription(shapeIndex, 0.01f), // 새로운 Shape
                };

                // 업데이트 적용
                simulation.Bodies.ApplyDescription(handle.Value, desc);
                bodyRef.Awake = true;
            }
            else
            {
                // 새로운 Body 추가
                var desc = BodyDescription.CreateDynamic(pose, inertia, shapeIndex, 0.01f);
                handle = simulation.Bodies.Add(desc);
            }

            return handle;
        }

        public BodyHandle? AddOrUpdateBody(BodyHandle? handle, Transform t, Microsoft.Xna.Framework.BoundingBox box, float mass)
        {
            var center = (box.Min + box.Max) / 2f;
            var pose = new RigidPose(Convert(t.position + center), Convert(t.rotation));
            Vector3 size = Convert(box.Max - box.Min);
            var boxShape = new Box(size.X * t.scale.X, size.Y * t.scale.Y, size.Z * t.scale.Z);
            var inertia = boxShape.ComputeInertia(mass);
            var shapeIndex = simulation.Shapes.Add(boxShape);

            if (handle.HasValue && simulation.Bodies.BodyExists(handle.Value))
            {
                // 기존 BodyHandle이 있으면 이전 Shape 제거
                var bodyRef = simulation.Bodies.GetBodyReference(handle.Value);
                var oldShapeIndex = bodyRef.Collidable.Shape;

                // 새로운 BodyDescription 생성
                var desc = BodyDescription.CreateDynamic(
                    pose,                       // 새로운 위치와 회전
                    bodyRef.Velocity,          // 기존 속도 유지
                    inertia,                   // 새로운 관성
                    new CollidableDescription(shapeIndex, 0.01f), // 새로운 Shape
                    bodyRef.Activity.SleepThreshold // 기존 SleepThreshold 유지
                );

                // 업데이트 적용
                simulation.Bodies.ApplyDescription(handle.Value, desc);
                bodyRef.Awake = true; // 명시적으로 깨우기

                // 이전 Shape 제거
                simulation.Shapes.Remove(oldShapeIndex);
            }
            else
            {
                // 새로운 Body 추가
                var desc = BodyDescription.CreateDynamic(pose, inertia, shapeIndex, 0.01f);
                handle = simulation.Bodies.Add(desc);
            }

            return handle;
        }

        public void RemoveBody(BodyHandle? handle)
        {
            if (handle == null) return;

            // BodyHandle이 존재하는지 확인
            if (simulation.Bodies.BodyExists(handle.Value))
            {
                // BodyReference 가져오기
                var bodyRef = simulation.Bodies.GetBodyReference(handle.Value);

                // ShapeIndex 가져오기
                var shapeIndex = bodyRef.Collidable.Shape;

                // BodyHandle 제거
                simulation.Bodies.Remove(handle.Value);

                // Shape 제거
                simulation.Shapes.Remove(shapeIndex);
            }
        }

        public void GetBodyReference(BodyHandle? handle, 
            ref Microsoft.Xna.Framework.Vector3 pos,
            ref Microsoft.Xna.Framework.Quaternion rot,
            ref Microsoft.Xna.Framework.Vector3 vel)
        {
            if (handle == null) return;

            var bodyRef = simulation.Bodies.GetBodyReference(handle.Value);

            // 현재 위치와 회전 가져오기
            pos = Convert(bodyRef.Pose.Position);
            rot = Convert(bodyRef.Pose.Orientation);
            vel = Convert(bodyRef.Velocity.Linear);
        }

        #endregion

        public string GetStatistics()
        {
            var desc = "";
            desc += $"Bodies: {simulation.Bodies.ActiveSet.Count}\n";
            desc += $"Statics: {simulation.Statics.Count}\n";
            return desc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Microsoft.Xna.Framework.Vector3 Convert(System.Numerics.Vector3 v)
        {
            return new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private System.Numerics.Vector3 Convert(Microsoft.Xna.Framework.Vector3 v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Microsoft.Xna.Framework.Quaternion Convert(System.Numerics.Quaternion q)
        {
            return new Microsoft.Xna.Framework.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private System.Numerics.Quaternion Convert(Microsoft.Xna.Framework.Quaternion q)
        {
            return new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W);
        }
    }
}
