using System.Drawing;
using _3Dvishok.Engine.Math;

namespace _3Dvishok.Engine.Rendering
{
    public class HudRenderer
    {
        public void Draw(Graphics g, Font font, HudState state)
        {
            string[] lines =
            {
                string.Format("FPS: {0:0.0}", state.Fps),
                string.Format("Render: {0}", state.RenderMode),
                string.Format("Backface Culling: {0}", state.BackfaceCulling ? "ON" : "OFF"),
                string.Format("Auto Rotate: {0}", state.AutoRotate ? "ON" : "OFF"),
                string.Format("Objects: {0} (spawned: {1})", state.ObjectCount, state.SpawnedObjectCount),
                string.Format("Visible Faces: {0}", state.VisibleFaceCount),
                string.Format("Camera Pos: {0:0.00}, {1:0.00}, {2:0.00}", state.CameraPosition.X, state.CameraPosition.Y, state.CameraPosition.Z),
                string.Format("Camera Rot: yaw {0:0.0} deg | pitch {1:0.0} deg", MathUtil.RadToDeg(state.CameraYaw), MathUtil.RadToDeg(state.CameraPitch)),
                "",
                "Controls:",
                "WASD - move, Q/E - up/down, Shift - boost",
                "Arrows or RMB drag - look around",
                "Mouse wheel - zoom (FOV), R - reset camera",
                "N - spawn cube, Delete - remove spawned cube",
                "F1 - render mode, F2 - culling, F3 - autorotate, F4 - HUD"
            };

            float x = 12f;
            float y = 12f;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Length == 0)
                {
                    y += 8f;
                    continue;
                }

                g.DrawString(line, font, Brushes.Black, x + 1f, y + 1f);
                g.DrawString(line, font, Brushes.WhiteSmoke, x, y);
                y += 17f;
            }
        }
    }
}
