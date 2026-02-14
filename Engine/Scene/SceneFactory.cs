using System.Drawing;
using _3Dvishok.Engine.Math;

namespace _3Dvishok.Engine.Scene
{
    public static class SceneFactory
    {
        public static Scene CreateDefaultScene()
        {
            Scene scene = new Scene();

            Model cubeA = Model.CreateCube(1.8, Color.FromArgb(235, 98, 67));
            cubeA.Position = new Vector3(0.2, 1.0, 4.6);
            cubeA.Rotation = new Vector3(16.0, 25.0, 0.0);
            cubeA.RotationVelocity = new Vector3(18.0, 35.0, 0.0);

            Model cubeB = Model.CreateCube(1.2, Color.FromArgb(63, 153, 233));
            cubeB.Position = new Vector3(-2.7, 0.7, 7.3);
            cubeB.Rotation = new Vector3(0.0, -20.0, 6.0);
            cubeB.RotationVelocity = new Vector3(9.0, -23.0, 17.0);

            Model cubeC = Model.CreateCube(1.0, Color.FromArgb(126, 205, 84));
            cubeC.Position = new Vector3(2.8, 0.55, 9.1);
            cubeC.Rotation = new Vector3(8.0, 12.0, -7.0);
            cubeC.RotationVelocity = new Vector3(15.0, 19.0, 12.0);

            Model floor = Model.CreatePlane(16.0, 16.0, Color.FromArgb(160, 160, 160));
            floor.Position = new Vector3(0.0, -0.2, 7.0);

            scene.Add(floor);
            scene.Add(cubeA);
            scene.Add(cubeB);
            scene.Add(cubeC);

            return scene;
        }
    }
}
