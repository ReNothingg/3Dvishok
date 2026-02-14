using System;

namespace _3Dvishok.Engine.Math
{
    public class Vector3
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Zero
        {
            get { return new Vector3(0.0, 0.0, 0.0); }
        }

        public static Vector3 UnitX
        {
            get { return new Vector3(1.0, 0.0, 0.0); }
        }

        public static Vector3 UnitY
        {
            get { return new Vector3(0.0, 1.0, 0.0); }
        }

        public static Vector3 UnitZ
        {
            get { return new Vector3(0.0, 0.0, 1.0); }
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.X, -a.Y, -a.Z);
        }

        public static Vector3 operator *(Vector3 a, double d)
        {
            return new Vector3(a.X * d, a.Y * d, a.Z * d);
        }

        public static Vector3 operator /(Vector3 a, double d)
        {
            return new Vector3(a.X / d, a.Y / d, a.Z / d);
        }

        public static Vector3 operator *(double d, Vector3 a)
        {
            return a * d;
        }

        public double Length()
        {
            return System.Math.Sqrt(LengthSquared());
        }

        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public Vector3 Normalize()
        {
            double len = Length();
            if (len <= 0.0000001)
            {
                return Zero;
            }

            return this / len;
        }

        public static double Dot(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }
    }
}
