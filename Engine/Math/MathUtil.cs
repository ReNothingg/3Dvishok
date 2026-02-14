using System;

namespace _3Dvishok.Engine.Math
{
    public static class MathUtil
    {
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        public static double DegToRad(double degrees)
        {
            return degrees * System.Math.PI / 180.0;
        }

        public static double RadToDeg(double radians)
        {
            return radians * 180.0 / System.Math.PI;
        }
    }
}
