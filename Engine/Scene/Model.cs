using System.Collections.Generic;
using System.Drawing;
using _3Dvishok.Engine.Math;

namespace _3Dvishok.Engine.Scene
{
    public class Model
    {
        public List<Vector3> Vertices;
        public List<Face> Faces;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 RotationVelocity;
        public Color BaseColor;

        public Model()
        {
            Vertices = new List<Vector3>();
            Faces = new List<Face>();
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            RotationVelocity = Vector3.Zero;
            BaseColor = Color.White;
        }

        public static Model CreateCube(double size, Color color)
        {
            Model m = new Model();
            m.BaseColor = color;

            double h = size / 2.0;
            m.Vertices.Add(new Vector3(-h, -h, -h));
            m.Vertices.Add(new Vector3(h, -h, -h));
            m.Vertices.Add(new Vector3(h, h, -h));
            m.Vertices.Add(new Vector3(-h, h, -h));
            m.Vertices.Add(new Vector3(-h, -h, h));
            m.Vertices.Add(new Vector3(h, -h, h));
            m.Vertices.Add(new Vector3(h, h, h));
            m.Vertices.Add(new Vector3(-h, h, h));

            m.Faces.Add(new Face(new[] { 0, 3, 2, 1 }));
            m.Faces.Add(new Face(new[] { 4, 5, 6, 7 }));
            m.Faces.Add(new Face(new[] { 0, 4, 7, 3 }));
            m.Faces.Add(new Face(new[] { 1, 2, 6, 5 }));
            m.Faces.Add(new Face(new[] { 3, 7, 6, 2 }));
            m.Faces.Add(new Face(new[] { 0, 1, 5, 4 }));

            return m;
        }

        public static Model CreatePlane(double width, double depth, Color color)
        {
            Model m = new Model();
            m.BaseColor = color;

            double w = width * 0.5;
            double d = depth * 0.5;

            m.Vertices.Add(new Vector3(-w, 0.0, -d));
            m.Vertices.Add(new Vector3(w, 0.0, -d));
            m.Vertices.Add(new Vector3(w, 0.0, d));
            m.Vertices.Add(new Vector3(-w, 0.0, d));

            m.Faces.Add(new Face(new[] { 0, 3, 2, 1 }));

            return m;
        }
    }
}
