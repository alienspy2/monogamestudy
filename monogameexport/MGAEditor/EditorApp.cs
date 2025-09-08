using MGAlienLib;
using Microsoft.Xna.Framework;
using System;

namespace MGAEditor
{
    public class EditorApp : GameBase
    {
        public EditorApp()
        {
            IsMouseVisible = true;
        }
        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            base.OnExiting(sender, args);
            Logger.Log("bye!");
        }

        protected override void OnInitialize()
        {
            Logger.Pipe += Logger.PipeToLogFile;

            //{
            //    Quaternion q = new Vector3(30, 0, 0).EulerToQuaternion();
            //    Vector3 e = q.ToEulerAngles();
            //    Logger.Log($"euler: {e}");
            //}

            //{
            //    Quaternion q = new Vector3(0, 30, 0).EulerToQuaternion();
            //    Vector3 e = q.ToEulerAngles();
            //    Logger.Log($"euler: {e}");
            //}

            //{
            //    Quaternion q = new Vector3(0, 0, 30).EulerToQuaternion();
            //    Vector3 e = q.ToEulerAngles();
            //    Logger.Log($"euler: {e}");
            //}

            //{
            //    //var yaw = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(10));
            //    //var pitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(20));
            //    //var roll = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(30));
            //    //var combine1 = yaw * pitch * roll;
            //    //var combine2 = yaw * roll * pitch;
            //    //var combine3 = roll * yaw * pitch;
            //    //var combine4 = roll * pitch * yaw;
            //    //var combine5 = pitch * yaw * roll;
            //    //var combine6 = pitch * roll * yaw;
            //    //Quaternion q = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(10), MathHelper.ToRadians(20), MathHelper.ToRadians(30));
            //    var q = new Vector3(10,20,30).FromEulerAnglesToQuaternion();
            //    //Logger.Log($"combine: {combine1.ToString()}");
            //    Logger.Log($"q: {q}");

            //    var v = Vector3.Transform(Vector3.UnitX, q);
            //    //var v2 = Vector3.Transform(Vector3.UnitX, q);

            //    //Logger.Log($"x 10 : {Vector3.Transform(Vector3.Right, Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(10)))}");
            //    //Logger.Log($"x 20 : {Vector3.Transform(Vector3.Right, Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(20)))}");
            //    //Logger.Log($"x 30 : {Vector3.Transform(Vector3.Right, Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(30)))}");
            //    //Logger.Log($"x 45 : {Vector3.Transform(Vector3.Right, Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(45)))}");

            //    //Quaternion q = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(10), MathHelper.ToRadians(20), MathHelper.ToRadians(30));
            //    //Vector3 e1 = ToEulerAngles(yaw);
            //    //Vector3 e2 = ToEulerAngles(pitch);
            //    //Vector3 e3 = ToEulerAngles(roll);
            //    //Vector3 e4 = ToEulerAngles(yaw * pitch);
            //    //Vector3 e5 = ToEulerAngles(pitch * roll);
            //    //Vector3 e6 = ToEulerAngles(yaw * pitch * roll);
            //    Vector3 e = q.ToEulerAngles();
            //    Logger.Log($"euler: {e} , v: {v}");

            //    //Vector3 ee1 = ToEulerAngles3(yaw);
            //    //Vector3 ee2 = ToEulerAngles3(pitch);
            //    //Vector3 ee3 = ToEulerAngles3(roll);
            //    //Vector3 ee4 = ToEulerAngles3(yaw * pitch);
            //    //Vector3 ee5 = ToEulerAngles3(pitch * roll);
            //    //Vector3 ee6 = ToEulerAngles3(yaw * pitch * roll);

            //    //Logger.Log("!");
            //}

            var entryObj = hierarchyManager.CreateGameObject("entry", null);
            entryObj.AddComponent<editorEntryPoint>();
            //Exit();
        }


        //public static Vector3 ToEulerAngles3(Quaternion q)
        //{
        //    // Quaternion → Rotation Matrix
        //    float xx = q.X * q.X;
        //    float yy = q.Y * q.Y;
        //    float zz = q.Z * q.Z;
        //    float xy = q.X * q.Y;
        //    float xz = q.X * q.Z;
        //    float yz = q.Y * q.Z;
        //    float wx = q.W * q.X;
        //    float wy = q.W * q.Y;
        //    float wz = q.W * q.Z;

        //    // Rotation Matrix elements
        //    float m00 = 1 - 2 * (yy + zz);
        //    float m01 = 2 * (xy - wz);
        //    float m02 = 2 * (xz + wy);
        //    float m10 = 2 * (xy + wz);
        //    float m11 = 1 - 2 * (xx + zz);
        //    float m12 = 2 * (yz - wx);
        //    float m20 = 2 * (xz - wy);
        //    float m21 = 2 * (yz + wx);
        //    float m22 = 1 - 2 * (xx + yy);

        //    Vector3 euler = new Vector3();

        //    // Pitch (X-axis)
        //    euler.Y = MathF.Asin(Math.Clamp(m12, -1f, 1f));

        //    // Handle Gimbal Lock
        //    if (MathF.Abs(m12) < 0.9999f)
        //    {
        //        // Yaw (Y-axis)
        //        euler.X = MathF.Atan2(-m02, m22);
        //        // Roll (Z-axis)
        //        euler.Z = MathF.Atan2(-m10, m11);
        //    }
        //    else
        //    {
        //        // Gimbal lock case
        //        euler.X = MathF.Atan2(m20, m00);
        //        euler.Z = 0;
        //    }

        //    // Convert to degrees
        //    return new Vector3(
        //        -euler.X.ToDegrees(), // Yaw
        //        -euler.Y.ToDegrees(), // Pitch
        //        -euler.Z.ToDegrees()  // Roll
        //    );
        //}
    }
}
