namespace _3Dvishok.Engine.Math
{
    public class Matrix4x4
    {
        public double[,] M = new double[4, 4];

        public static Matrix4x4 Identity()
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.M[0, 0] = 1.0;
            matrix.M[1, 1] = 1.0;
            matrix.M[2, 2] = 1.0;
            matrix.M[3, 3] = 1.0;
            return matrix;
        }

        public static Matrix4x4 CreateTranslation(double x, double y, double z)
        {
            Matrix4x4 mat = Identity();
            mat.M[0, 3] = x;
            mat.M[1, 3] = y;
            mat.M[2, 3] = z;
            return mat;
        }

        public static Matrix4x4 CreateRotationX(double angle)
        {
            Matrix4x4 mat = Identity();
            double c = System.Math.Cos(angle);
            double s = System.Math.Sin(angle);
            mat.M[1, 1] = c;
            mat.M[1, 2] = -s;
            mat.M[2, 1] = s;
            mat.M[2, 2] = c;
            return mat;
        }

        public static Matrix4x4 CreateRotationY(double angle)
        {
            Matrix4x4 mat = Identity();
            double c = System.Math.Cos(angle);
            double s = System.Math.Sin(angle);
            mat.M[0, 0] = c;
            mat.M[0, 2] = s;
            mat.M[2, 0] = -s;
            mat.M[2, 2] = c;
            return mat;
        }

        public static Matrix4x4 CreateRotationZ(double angle)
        {
            Matrix4x4 mat = Identity();
            double c = System.Math.Cos(angle);
            double s = System.Math.Sin(angle);
            mat.M[0, 0] = c;
            mat.M[0, 1] = -s;
            mat.M[1, 0] = s;
            mat.M[1, 1] = c;
            return mat;
        }

        public static Matrix4x4 CreateRotationXYZ(double ax, double ay, double az)
        {
            return CreateRotationX(ax) * CreateRotationY(ay) * CreateRotationZ(az);
        }

        public static Matrix4x4 CreatePerspectiveFieldOfView(double fov, double aspect, double zn, double zf)
        {
            Matrix4x4 mat = new Matrix4x4();
            double yScale = 1.0 / System.Math.Tan(fov * 0.5);
            double xScale = yScale / aspect;

            mat.M[0, 0] = xScale;
            mat.M[1, 1] = yScale;
            mat.M[2, 2] = zf / (zf - zn);
            mat.M[2, 3] = (-zn * zf) / (zf - zn);
            mat.M[3, 2] = 1.0;
            mat.M[3, 3] = 0.0;

            return mat;
        }

        public static Matrix4x4 CreateLookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 zaxis = (target - eye).Normalize();
            Vector3 xaxis = Vector3.Cross(up, zaxis).Normalize();
            Vector3 yaxis = Vector3.Cross(zaxis, xaxis).Normalize();

            Matrix4x4 view = Identity();
            view.M[0, 0] = xaxis.X;
            view.M[0, 1] = xaxis.Y;
            view.M[0, 2] = xaxis.Z;
            view.M[1, 0] = yaxis.X;
            view.M[1, 1] = yaxis.Y;
            view.M[1, 2] = yaxis.Z;
            view.M[2, 0] = zaxis.X;
            view.M[2, 1] = zaxis.Y;
            view.M[2, 2] = zaxis.Z;

            view.M[0, 3] = -Vector3.Dot(xaxis, eye);
            view.M[1, 3] = -Vector3.Dot(yaxis, eye);
            view.M[2, 3] = -Vector3.Dot(zaxis, eye);

            return view;
        }

        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 r = new Matrix4x4();
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    r.M[row, col] =
                        a.M[row, 0] * b.M[0, col] +
                        a.M[row, 1] * b.M[1, col] +
                        a.M[row, 2] * b.M[2, col] +
                        a.M[row, 3] * b.M[3, col];
                }
            }

            return r;
        }

        public Vector3 Transform(Vector3 v)
        {
            double x = v.X * M[0, 0] + v.Y * M[0, 1] + v.Z * M[0, 2] + M[0, 3];
            double y = v.X * M[1, 0] + v.Y * M[1, 1] + v.Z * M[1, 2] + M[1, 3];
            double z = v.X * M[2, 0] + v.Y * M[2, 1] + v.Z * M[2, 2] + M[2, 3];
            double w = v.X * M[3, 0] + v.Y * M[3, 1] + v.Z * M[3, 2] + M[3, 3];

            if (System.Math.Abs(w) > 0.0000001)
            {
                x /= w;
                y /= w;
                z /= w;
            }

            return new Vector3(x, y, z);
        }
    }
}
