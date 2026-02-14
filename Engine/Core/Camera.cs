using _3Dvishok.Engine.Math;

namespace _3Dvishok.Engine.Core
{
    public class Camera
    {
        public Vector3 Position;
        public double Yaw;
        public double Pitch;
        public double FOV;
        public double AspectRatio;
        public double NearPlane;
        public double FarPlane;

        public Camera()
        {
            Position = Vector3.Zero;
            Yaw = 0.0;
            Pitch = 0.0;
            FOV = System.Math.PI / 3.0;
            AspectRatio = 16.0 / 9.0;
            NearPlane = 0.1;
            FarPlane = 100.0;
        }

        public void ClampPitch(double maxAbsPitch)
        {
            Pitch = System.Math.Max(-maxAbsPitch, System.Math.Min(maxAbsPitch, Pitch));
        }

        public Vector3 GetForwardVector()
        {
            double cp = System.Math.Cos(Pitch);
            return new Vector3(
                System.Math.Sin(Yaw) * cp,
                System.Math.Sin(Pitch),
                System.Math.Cos(Yaw) * cp).Normalize();
        }

        public Vector3 GetRightVector()
        {
            Vector3 right = Vector3.Cross(Vector3.UnitY, GetForwardVector());
            if (right.LengthSquared() < 0.0000001)
            {
                return Vector3.UnitX;
            }

            return right.Normalize();
        }

        public Matrix4x4 GetViewMatrix()
        {
            Vector3 target = Position + GetForwardVector();
            return Matrix4x4.CreateLookAt(Position, target, Vector3.UnitY);
        }
    }
}
