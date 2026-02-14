using _3Dvishok.Engine.Math;

namespace _3Dvishok.Engine.Rendering
{
    public class HudState
    {
        public double Fps;
        public RenderMode RenderMode;
        public bool BackfaceCulling;
        public bool AutoRotate;
        public Vector3 CameraPosition;
        public double CameraYaw;
        public double CameraPitch;
        public int ObjectCount;
        public int VisibleFaceCount;
        public int SpawnedObjectCount;
    }
}
