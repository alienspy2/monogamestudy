

using System;

namespace MGAlienLib
{
    /// <summary>
    /// 수학 함수를 제공합니다.
    /// </summary>
    public static class Mathf
    {
        public static float PI = 3.14159274f;
        public static float PIOver2 = 1.57079637f;
        public static float Deg2Rad = 0.0174532924f;
        public static float Rad2Deg = 57.29578f;
        public static float Sin(float f)
        {
            return (float)System.Math.Sin(f);
        }
        public static float Cos(float f)
        {
            return (float)System.Math.Cos(f);
        }
        public static float Tan(float f)
        {
            return (float)System.Math.Tan(f);
        }
        public static float Asin(float f)
        {
            return (float)System.Math.Asin(f);
        }
        public static float Acos(float f)
        {
            return (float)System.Math.Acos(f);
        }
        public static float Atan(float f)
        {
            return (float)System.Math.Atan(f);
        }
        public static float Atan2(float y, float x)
        {
            return (float)System.Math.Atan2(y, x);
        }
        public static float Sqrt(float f)
        {
            return (float)System.Math.Sqrt(f);
        }
        public static float Abs(float f)
        {
            return System.Math.Abs(f);
        }
        public static int Abs(int value)
        {
            return System.Math.Abs(value);
        }
        public static float Min(float a, float b)
        {
            return a < b ? a : b;
        }
        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }
        public static float Pow(float f, float p)
        {
            return (float)System.Math.Pow(f, p);
        }
        public static float Exp(float power)
        {
            return (float)System.Math.Exp(power);
        }
        public static float Log(float f, float p)
        {
            return (float)System.Math.Log(f, p);
        }
        public static float Log(float f)
        {
            return (float)System.Math.Log(f);
        }
        public static float Ceil(float f)
        {
            return (float)System.Math.Ceiling(f);
        }
        public static float Floor(float f)
        {
            return (float)System.Math.Floor(f);
        }
        public static float Round(float f)
        {
            return (float)System.Math.Round(f);
        }
        public static int CeilToInt(float f)
        {
            return (int)System.Math.Ceiling(f);
        }

        public static float Lerp(float v1, float v2, float ratio)
        {
            return v1 + (v2 - v1) * ratio;
        }

        internal static float Clamp(float value, float v1, float v2)
        {
            return value < v1 ? v1 : value > v2 ? v2 : value;
        }

        internal static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }
    }
}
