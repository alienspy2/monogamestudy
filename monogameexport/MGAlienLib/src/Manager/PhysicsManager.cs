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

        public void AddStatic(MGAlienLib.Mesh mgaMesh)
        {
        }


        public BodyHandle test_addSphere(Microsoft.Xna.Framework.Vector3 pos, float radius)
        {
            //Drop a ball on a big static box.
            var sphere = new Sphere(radius/1.25f);
            var sphereInertia = sphere.ComputeInertia(1);
            BodyHandle sphereHandle = 
                simulation.Bodies.Add(
                    BodyDescription.CreateDynamic(
                        Convert(pos), 
                        sphereInertia, 
                        simulation.Shapes.Add(sphere), 
                        0.01f));

            return sphereHandle;
        }


        public void test_addStaticBox()
        {
            simulation.Statics.Add(new StaticDescription(new Vector3(0, 0, 0), simulation.Shapes.Add(new Box(500, 1, 500))));
            simulation.Statics.Add(new StaticDescription(new Vector3(0, 0, -5), simulation.Shapes.Add(new Box(500, 20, 1))));
            simulation.Statics.Add(new StaticDescription(new Vector3(0, 0, 5), simulation.Shapes.Add(new Box(500, 20, 1))));
            simulation.Statics.Add(new StaticDescription(new Vector3(-5, 0, 0), simulation.Shapes.Add(new Box(1, 20, 500))));
            simulation.Statics.Add(new StaticDescription(new Vector3(5, 0, 0), simulation.Shapes.Add(new Box(1, 20, 500))));
        }

        public StaticHandle AddStatic(Transform t, float _in_radius)
        {
            RigidPose pose = new RigidPose(Convert(t.position), Convert(t.rotation));
            var radius = _in_radius * (t.scale.X + t.scale.Y + t.scale.Z) / 3f;
            return simulation.Statics.Add(new StaticDescription(pose, simulation.Shapes.Add(new Sphere(radius))));
        }

        public StaticHandle AddStatic(Transform t, Microsoft.Xna.Framework.BoundingBox box)
            => AddOrUpdateStatic(null, t, box)!.Value;

        public void UpdateStatic(StaticHandle handle, Transform t, Microsoft.Xna.Framework.BoundingBox box)
            => AddOrUpdateStatic(handle, t, box);

        public StaticHandle? AddOrUpdateStatic(StaticHandle? handle, Transform t, Microsoft.Xna.Framework.BoundingBox box)
        {
            var center = (box.Min + box.Max) / 2f;
            RigidPose pose = new RigidPose(Convert(t.position + center), Convert(t.rotation));
            Vector3 size = Convert(box.Max - box.Min);
            size.X = size.X * t.scale.X;
            size.Y = size.Y * t.scale.Y;
            size.Z = size.Z * t.scale.Z;
            var desc = new StaticDescription(pose, simulation.Shapes.Add(new Box(size.X, size.Y, size.Z)));

            if (!handle.HasValue)
            {
                handle = simulation.Statics.Add(desc);
            }
            else
            {
                simulation.Statics.ApplyDescription(handle.Value, desc);
            }

            return handle;
        }

        public void UpdateStatic(StaticHandle handle, Transform t, float _in_radius)
        {
            RigidPose pose = new RigidPose(Convert(t.position), Convert(t.rotation));
            var radius = _in_radius * (t.scale.X + t.scale.Y + t.scale.Z) / 3f;
            simulation.Statics.ApplyDescription(handle, new StaticDescription(pose, simulation.Shapes.Add(new Sphere(radius))));
        }

        //public void UpdateStatic(StaticHandle handle, Transform t, Microsoft.Xna.Framework.BoundingBox box)
        //{
        //    var center = (box.Min + box.Max) / 2f;
        //    RigidPose pose = new RigidPose(Convert(t.position + center), Convert(t.rotation));
        //    Vector3 size = Convert(box.Max - box.Min);
        //    size.X = size.X * t.scale.X;
        //    size.Y = size.Y * t.scale.Y;
        //    size.Z = size.Z * t.scale.Z;
        //    var desc = new StaticDescription(pose, simulation.Shapes.Add(new Box(size.X, size.Y, size.Z)));

        //    if (handle.ha)
                
        //}

        public void RemoveStatic(StaticHandle handle)
        {
            simulation.Statics.Remove(handle);
        }

        public BodyHandle AddBody(Transform t, float _in_radius, float mass)
        {
            RigidPose pose = new RigidPose(Convert(t.position), Convert(t.rotation));
            var radius = _in_radius * (t.scale.X + t.scale.Y + t.scale.Z) / 3f;
            var sphere = new Sphere(radius);
            var inertia = sphere.ComputeInertia(mass);
            return simulation.Bodies.Add(BodyDescription.CreateDynamic(pose, inertia, simulation.Shapes.Add(sphere), 0.01f));
        }

        public StaticHandle AddBody(Transform t, Microsoft.Xna.Framework.BoundingBox box, float mass)
        {
            var center = (box.Min + box.Max) / 2f;
            var pose = new RigidPose(Convert(t.position + center), Convert(t.rotation));
            Vector3 size = Convert(box.Max - box.Min);
            var boxShape = new Box(size.X * t.scale.X, size.Y * t.scale.Y, size.Z * t.scale.Z);
            var inertia = boxShape.ComputeInertia(mass);
            return simulation.Statics.Add(new StaticDescription(pose, simulation.Shapes.Add(boxShape)));
        }

        public void RemoveBody(BodyHandle handle)
        {
            simulation.Bodies.Remove(handle);
        }

        public void GetBodyReference(BodyHandle handle, 
            ref Microsoft.Xna.Framework.Vector3 pos,
            ref Microsoft.Xna.Framework.Quaternion rot,
            ref Microsoft.Xna.Framework.Vector3 vel)
        {
            var bodyRef = simulation.Bodies.GetBodyReference(handle);

            // 현재 위치와 회전 가져오기
            pos = Convert(bodyRef.Pose.Position);
            rot = Convert(bodyRef.Pose.Orientation);
            vel = Convert(bodyRef.Velocity.Linear);
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
