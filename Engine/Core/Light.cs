using _3Dvishok.Engine.Math;

namespace _3Dvishok.Engine.Core
{
    public class Light
    {
        public Vector3 Direction;
        public double Intensity;
        public double Ambient;

        public Light()
        {
            Direction = new Vector3(0.0, -1.0, -1.0);
            Intensity = 1.0;
            Ambient = 0.15;
        }
    }
}
