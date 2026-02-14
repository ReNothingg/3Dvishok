using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using _3Dvishok.Engine.Core;
using _3Dvishok.Engine.Math;
using SceneFace = _3Dvishok.Engine.Scene.Face;
using SceneGraph = _3Dvishok.Engine.Scene.Scene;
using SceneModel = _3Dvishok.Engine.Scene.Model;

namespace _3Dvishok.Engine.Rendering
{
    public class SoftwareRenderer
    {
        public RenderStats Render(Graphics g, Rectangle viewport, SceneGraph scene, Camera camera, Light light, RendererSettings settings)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            DrawBackground(g, viewport);

            Matrix4x4 viewMat = camera.GetViewMatrix();
            Matrix4x4 projMat = Matrix4x4.CreatePerspectiveFieldOfView(
                camera.FOV,
                camera.AspectRatio,
                camera.NearPlane,
                camera.FarPlane);

            List<FaceToDraw> facesToDraw = BuildDrawList(scene, camera, light, settings, viewMat, projMat, viewport);
            facesToDraw.Sort((a, b) => b.Depth.CompareTo(a.Depth));

            for (int i = 0; i < facesToDraw.Count; i++)
            {
                FaceToDraw face = facesToDraw[i];
                if (face.Fill)
                {
                    using (SolidBrush brush = new SolidBrush(face.FillColor))
                    {
                        g.FillPolygon(brush, face.Points);
                    }
                }

                if (face.DrawOutline)
                {
                    using (Pen pen = new Pen(face.OutlineColor, 1.2f))
                    {
                        g.DrawPolygon(pen, face.Points);
                    }
                }
            }

            if (settings.DrawWorldAxes)
            {
                DrawWorldAxes(g, viewMat, projMat, camera, viewport);
            }

            RenderStats stats = new RenderStats();
            stats.ObjectCount = scene.Models.Count;
            stats.FaceCount = facesToDraw.Count;
            return stats;
        }

        private static void DrawBackground(Graphics g, Rectangle viewport)
        {
            using (LinearGradientBrush sky = new LinearGradientBrush(
                viewport,
                Color.FromArgb(32, 46, 68),
                Color.FromArgb(108, 148, 196),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(sky, viewport);
            }
        }

        private static List<FaceToDraw> BuildDrawList(
            SceneGraph scene,
            Camera camera,
            Light light,
            RendererSettings settings,
            Matrix4x4 viewMat,
            Matrix4x4 projMat,
            Rectangle viewport)
        {
            List<FaceToDraw> drawList = new List<FaceToDraw>();

            for (int modelIndex = 0; modelIndex < scene.Models.Count; modelIndex++)
            {
                SceneModel model = scene.Models[modelIndex];

                Matrix4x4 modelMat = Matrix4x4.CreateRotationXYZ(
                        MathUtil.DegToRad(model.Rotation.X),
                        MathUtil.DegToRad(model.Rotation.Y),
                        MathUtil.DegToRad(model.Rotation.Z))
                    * Matrix4x4.CreateTranslation(
                        model.Position.X,
                        model.Position.Y,
                        model.Position.Z);

                List<Vector3> worldVertices = new List<Vector3>(model.Vertices.Count);
                List<Vector3> viewVertices = new List<Vector3>(model.Vertices.Count);
                for (int i = 0; i < model.Vertices.Count; i++)
                {
                    Vector3 world = modelMat.Transform(model.Vertices[i]);
                    worldVertices.Add(world);
                    viewVertices.Add(viewMat.Transform(world));
                }

                for (int faceIndex = 0; faceIndex < model.Faces.Count; faceIndex++)
                {
                    SceneFace face = model.Faces[faceIndex];
                    if (face.Indices.Length < 3)
                    {
                        continue;
                    }

                    bool allInFront = true;
                    double avgDepth = 0.0;
                    Vector3 centerView = Vector3.Zero;
                    for (int idx = 0; idx < face.Indices.Length; idx++)
                    {
                        Vector3 vv = viewVertices[face.Indices[idx]];
                        if (vv.Z <= camera.NearPlane)
                        {
                            allInFront = false;
                            break;
                        }

                        avgDepth += vv.Z;
                        centerView += vv;
                    }

                    if (!allInFront)
                    {
                        continue;
                    }

                    avgDepth /= face.Indices.Length;
                    centerView /= face.Indices.Length;

                    Vector3 v0 = viewVertices[face.Indices[0]];
                    Vector3 v1 = viewVertices[face.Indices[1]];
                    Vector3 v2 = viewVertices[face.Indices[2]];
                    Vector3 normalView = Vector3.Cross(v1 - v0, v2 - v0).Normalize();

                    if (settings.BackfaceCulling && Vector3.Dot(normalView, centerView) >= 0.0)
                    {
                        continue;
                    }

                    Vector3 w0 = worldVertices[face.Indices[0]];
                    Vector3 w1 = worldVertices[face.Indices[1]];
                    Vector3 w2 = worldVertices[face.Indices[2]];
                    Vector3 normalWorld = Vector3.Cross(w1 - w0, w2 - w0).Normalize();

                    double diffuse = System.Math.Max(0.0, Vector3.Dot(normalWorld, -light.Direction));
                    double lightFactor = MathUtil.Clamp(light.Ambient + diffuse * light.Intensity, 0.0, 1.0);

                    PointF[] pts = new PointF[face.Indices.Length];
                    bool valid = true;
                    for (int i = 0; i < face.Indices.Length; i++)
                    {
                        Vector3 projected = projMat.Transform(viewVertices[face.Indices[i]]);
                        if (double.IsNaN(projected.X) || double.IsNaN(projected.Y))
                        {
                            valid = false;
                            break;
                        }

                        pts[i] = MapToScreen(projected, viewport);
                    }

                    if (!valid)
                    {
                        continue;
                    }

                    FaceToDraw drawFace = new FaceToDraw();
                    drawFace.Points = pts;
                    drawFace.Depth = avgDepth;
                    drawFace.FillColor = ApplyLighting(model.BaseColor, lightFactor);
                    drawFace.OutlineColor = Color.FromArgb(18, 18, 22);
                    drawFace.Fill = settings.RenderMode != RenderMode.Wireframe;
                    drawFace.DrawOutline = settings.RenderMode != RenderMode.Solid;
                    drawList.Add(drawFace);
                }
            }

            return drawList;
        }

        private static void DrawWorldAxes(Graphics g, Matrix4x4 viewMat, Matrix4x4 projMat, Camera camera, Rectangle viewport)
        {
            DrawAxisLine(g, viewMat, projMat, camera, viewport, Vector3.Zero, new Vector3(2.2, 0.0, 0.0), Color.FromArgb(215, 72, 68));
            DrawAxisLine(g, viewMat, projMat, camera, viewport, Vector3.Zero, new Vector3(0.0, 2.2, 0.0), Color.FromArgb(80, 190, 95));
            DrawAxisLine(g, viewMat, projMat, camera, viewport, Vector3.Zero, new Vector3(0.0, 0.0, 2.2), Color.FromArgb(72, 132, 224));
        }

        private static void DrawAxisLine(
            Graphics g,
            Matrix4x4 viewMat,
            Matrix4x4 projMat,
            Camera camera,
            Rectangle viewport,
            Vector3 fromWorld,
            Vector3 toWorld,
            Color color)
        {
            PointF fromPoint;
            PointF toPoint;
            if (!TryProjectPoint(fromWorld, viewMat, projMat, camera, viewport, out fromPoint)
                || !TryProjectPoint(toWorld, viewMat, projMat, camera, viewport, out toPoint))
            {
                return;
            }

            using (Pen pen = new Pen(color, 2f))
            {
                g.DrawLine(pen, fromPoint, toPoint);
            }
        }

        private static bool TryProjectPoint(
            Vector3 worldPoint,
            Matrix4x4 viewMat,
            Matrix4x4 projMat,
            Camera camera,
            Rectangle viewport,
            out PointF screenPoint)
        {
            screenPoint = PointF.Empty;
            Vector3 viewPoint = viewMat.Transform(worldPoint);
            if (viewPoint.Z <= camera.NearPlane)
            {
                return false;
            }

            Vector3 projected = projMat.Transform(viewPoint);
            if (double.IsNaN(projected.X) || double.IsNaN(projected.Y))
            {
                return false;
            }

            screenPoint = MapToScreen(projected, viewport);
            return true;
        }

        private static PointF MapToScreen(Vector3 projected, Rectangle viewport)
        {
            float x = (float)(viewport.X + (projected.X + 1.0) * 0.5 * viewport.Width);
            float y = (float)(viewport.Y + (1.0 - (projected.Y + 1.0) * 0.5) * viewport.Height);
            return new PointF(x, y);
        }

        private static Color ApplyLighting(Color baseColor, double lightFactor)
        {
            int r = (int)MathUtil.Clamp(baseColor.R * lightFactor, 0.0, 255.0);
            int g = (int)MathUtil.Clamp(baseColor.G * lightFactor, 0.0, 255.0);
            int b = (int)MathUtil.Clamp(baseColor.B * lightFactor, 0.0, 255.0);
            return Color.FromArgb(r, g, b);
        }
    }
}
