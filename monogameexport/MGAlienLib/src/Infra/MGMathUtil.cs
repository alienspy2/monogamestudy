
using Microsoft.Xna.Framework;
using System;

namespace MGAlienLib
{
    public static class MGColorExt
    {
        public static Color Dimming(this Color color, float dimmingFactor)
        {
            return new Color(color.R * dimmingFactor / 255f, 
                color.G * dimmingFactor / 255f, 
                color.B * dimmingFactor / 255f, 
                color.A);
        }

        public static bool IsDark(this Color color)
        {
            // RGB 값의 평균을 계산하여 어두운 색인지 판단
            float average = (color.R + color.G + color.B) / 3f;
            return average < 128; // 평균이 128보다 작으면 어두운 색으로 간주
        }
    }

    /// <summary>
    /// 수학 관련 유틸리티 클래스입니다.
    /// </summary>
    public static class MGMathUtil
    {
        public static float ToRadians(this float degrees)
        {
            return degrees * Mathf.Deg2Rad;
        }

        public static float ToDegrees(this float radians)
        {
            return radians * Mathf.Rad2Deg;
        }

        public static Vector3 ToVector3(this Vector2 v, float z = 0)
        {
            return new Vector3(v, z);
        }

        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector2 Normalized(this Vector2 v)
        {
            var newV = v;
            newV.Normalize();
            return newV;
        }

        public static Vector3 ToDegrees(this Vector3 radians)
        {
            return new Vector3(radians.X.ToDegrees(), radians.Y.ToDegrees(), radians.Z.ToDegrees());
        }

        public static Vector3 ToRadians(this Vector3 degrees)
        {
            return new Vector3(degrees.X.ToRadians(), degrees.Y.ToRadians(), degrees.Z.ToRadians());
        }

        public static Vector3 Normalized(this Vector3 v)
        {
            var newV = v;
            newV.Normalize();
            return newV;
        }

        public static System.Numerics.Vector3 ToSystemNumericVector3(this Vector3 v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }

        public static Vector3 ToMonogameVector3(System.Numerics.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        /// <summary>
        /// Extension method to convert a Quaternion to Euler angles using YXZ order
        /// </summary>
        /// <param name="q">The quaternion to convert</param>
        /// <returns>Vector3 containing Euler angles in YXZ order</returns>
        public static Vector3 ToEulerAngles(this Quaternion q)
        {
            // Quaternion → Rotation Matrix
            float xx = q.X * q.X;
            float yy = q.Y * q.Y;
            float zz = q.Z * q.Z;
            float xy = q.X * q.Y;
            float xz = q.X * q.Z;
            float yz = q.Y * q.Z;
            float wx = q.W * q.X;
            float wy = q.W * q.Y;
            float wz = q.W * q.Z;

            // Rotation Matrix elements
            float m00 = 1 - 2 * (yy + zz);
            float m01 = 2 * (xy - wz);
            float m02 = 2 * (xz + wy);
            float m10 = 2 * (xy + wz);
            float m11 = 1 - 2 * (xx + zz);
            float m12 = 2 * (yz - wx);
            float m20 = 2 * (xz - wy);
            float m21 = 2 * (yz + wx);
            float m22 = 1 - 2 * (xx + yy);

            Vector3 euler = new Vector3();

            // Pitch (X-axis)
            euler.X = MathF.Asin(Math.Clamp(m12, -1f, 1f));

            // Handle Gimbal Lock
            if (MathF.Abs(m12) < 0.9999f)
            {
                // Yaw (Y-axis)
                euler.Y = MathF.Atan2(-m02, m22);
                // Roll (Z-axis)
                euler.Z = MathF.Atan2(-m10, m11);
            }
            else
            {
                // Gimbal lock case
                euler.Y = MathF.Atan2(m20, m00);
                euler.Z = 0;
            }

            // Convert to degrees
            return new Vector3(
                -euler.X.ToDegrees(), // Pitch
                -euler.Y.ToDegrees(), // Yaw
                -euler.Z.ToDegrees()  // Roll
            );
        }

        /// <summary>
        /// Vector3 (Euler angles in degrees, YXZ order) to Quaternion
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static Quaternion FromEulerAnglesToQuaternion(this Vector3 from)
        {
            return Quaternion.CreateFromYawPitchRoll(from.Y.ToRadians(), from.X.ToRadians(), from.Z.ToRadians());
        }

        public static bool RayIntersectsPlane(Ray ray, Plane plane, out Vector3 intersectionPoint)
        {
            float denominator = Vector3.Dot(plane.Normal, ray.Direction);

            // 평면과 광선이 평행하면 교점이 없음
            if (Math.Abs(denominator) < 1e-6)
            {
                intersectionPoint = Vector3.Zero;
                return false;
            }

            // t 값 계산
            float t = -(Vector3.Dot(plane.Normal, ray.Position) + plane.D) / denominator;

            // 교점 좌표 계산
            intersectionPoint = ray.Position + t * ray.Direction;
            return true;
        }
    }

    public static class StringExt
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }

}

