using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _3Dvishok
{
    public partial class Form1 : Form
    {
        Model cube;
        Camera camera;
        Light light;

        private HashSet<Keys> keysPressed = new HashSet<Keys>();
        private Timer timer;

        public Form1()
        {
            this.DoubleBuffered = true;
            this.Width = 800;
            this.Height = 600;
            this.Text = "Moving Camera + Static Cube";

            cube = Model.CreateCube(1.0);
            cube.Position = new Vector3(0, 0, 0);
            cube.Rotation = new Vector3(20, 30, 0);

            camera = new Camera();
            camera.Position = new Vector3(0, 0, -5);
            camera.Target = new Vector3(0, 0, 0);
            camera.Up = new Vector3(0, 1, 0);
            camera.FOV = Math.PI / 4;
            camera.AspectRatio = (double)this.ClientSize.Width / this.ClientSize.Height;
            camera.NearPlane = 0.1;
            camera.FarPlane = 100;

            light = new Light();
            light.Direction = new Vector3(1, -1, -1).Normalize();
            light.Intensity = 1.0;

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            timer = new Timer();
            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            keysPressed.Add(e.KeyCode);
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            keysPressed.Remove(e.KeyCode);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            double dt = 0.016;
            double moveSpeed = 2.0;

            if (keysPressed.Contains(Keys.W))
                camera.Position.Z += moveSpeed * dt;
            if (keysPressed.Contains(Keys.S))
                camera.Position.Z -= moveSpeed * dt;

            if (keysPressed.Contains(Keys.A))
                camera.Position.X -= moveSpeed * dt;
            if (keysPressed.Contains(Keys.D))
                camera.Position.X += moveSpeed * dt;

            if (keysPressed.Contains(Keys.Q))
                camera.Position.Y += moveSpeed * dt;
            if (keysPressed.Contains(Keys.E))
                camera.Position.Y -= moveSpeed * dt;

            this.Invalidate();
        }

        // Отрисовка
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(Color.CornflowerBlue);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Матрицы: модель, вид, проекция
            Matrix4x4 modelMat = Matrix4x4.CreateRotationXYZ(
                                    DegToRad(cube.Rotation.X),
                                    DegToRad(cube.Rotation.Y),
                                    DegToRad(cube.Rotation.Z))
                              * Matrix4x4.CreateTranslation(
                                    cube.Position.X,
                                    cube.Position.Y,
                                    cube.Position.Z);

            Matrix4x4 viewMat = camera.GetViewMatrix();
            Matrix4x4 projMat = Matrix4x4.CreatePerspectiveFieldOfView(
                                    camera.FOV,
                                    camera.AspectRatio,
                                    camera.NearPlane,
                                    camera.FarPlane);

            // Преобразуем вершины куба в видовые координаты
            List<Vector3> transformed = new List<Vector3>();
            foreach (var vtx in cube.Vertices)
            {
                Vector3 v = modelMat.Transform(vtx);
                v = viewMat.Transform(v);
                transformed.Add(v);
            }

            List<FaceToDraw> facesToDraw = new List<FaceToDraw>();
            foreach (var face in cube.Faces)
            {
                Vector3 v0 = transformed[face.Indices[0]];
                Vector3 v1 = transformed[face.Indices[1]];
                Vector3 v2 = transformed[face.Indices[2]];
                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).Normalize();
                double avgZ = 0;
                foreach (int idx in face.Indices)
                    avgZ += transformed[idx].Z;
                avgZ /= face.Indices.Length;

                double lightFactor = Math.Max(0, Vector3.Dot(normal, -light.Direction)) * light.Intensity;
                int shade = Math.Max(0, Math.Min(255, (int)(lightFactor * 255)));
                Color faceColor = Color.FromArgb(shade, shade, shade);

                PointF[] pts = new PointF[face.Indices.Length];
                for (int i = 0; i < face.Indices.Length; i++)
                {
                    Vector3 proj = projMat.Transform(transformed[face.Indices[i]]);
                    float x = (float)((proj.X + 1) * 0.5 * this.ClientSize.Width);
                    float y = (float)((1 - (proj.Y + 1) * 0.5) * this.ClientSize.Height);
                    pts[i] = new PointF(x, y);
                }

                facesToDraw.Add(new FaceToDraw { Points = pts, AvgZ = avgZ, Color = faceColor });
            }

            facesToDraw.Sort((a, b) => b.AvgZ.CompareTo(a.AvgZ));

            foreach (var face in facesToDraw)
            {
                using (SolidBrush br = new SolidBrush(face.Color))
                {
                    g.FillPolygon(br, face.Points);
                    g.DrawPolygon(Pens.Black, face.Points);
                }
            }
        }

        private double DegToRad(double deg)
        {
            return deg * Math.PI / 180.0;
        }
    }
    public class FaceToDraw
    {
        public PointF[] Points;
        public double AvgZ;
        public Color Color;
    }

    public class Model
    {
        public List<Vector3> Vertices;
        public List<Face> Faces;
        public Vector3 Position;
        public Vector3 Rotation;

        public Model()
        {
            Vertices = new List<Vector3>();
            Faces = new List<Face>();
            Position = new Vector3(0, 0, 0);
            Rotation = new Vector3(0, 0, 0);
        }

        public static Model CreateCube(double size)
        {
            Model m = new Model();
            double h = size / 2.0;

            m.Vertices.Add(new Vector3(-h, -h, -h)); // 0
            m.Vertices.Add(new Vector3(h, -h, -h)); // 1
            m.Vertices.Add(new Vector3(h, h, -h)); // 2
            m.Vertices.Add(new Vector3(-h, h, -h)); // 3
            m.Vertices.Add(new Vector3(-h, -h, h)); // 4
            m.Vertices.Add(new Vector3(h, -h, h)); // 5
            m.Vertices.Add(new Vector3(h, h, h)); // 6
            m.Vertices.Add(new Vector3(-h, h, h)); // 7

            // 6 граней
            m.Faces.Add(new Face(new int[] { 0, 1, 2, 3 }));
            m.Faces.Add(new Face(new int[] { 4, 5, 6, 7 }));
            m.Faces.Add(new Face(new int[] { 0, 4, 7, 3 }));
            m.Faces.Add(new Face(new int[] { 1, 5, 6, 2 }));
            m.Faces.Add(new Face(new int[] { 3, 2, 6, 7 }));
            m.Faces.Add(new Face(new int[] { 0, 1, 5, 4 }));

            return m;
        }
    }

    public class Face
    {
        public int[] Indices;
        public Face(int[] indices) => Indices = indices;
    }

    public class Camera
    {
        public Vector3 Position;
        public Vector3 Target;
        public Vector3 Up;
        public double FOV;
        public double AspectRatio;
        public double NearPlane;
        public double FarPlane;

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(Position, Target, Up);
        }
    }

    public class Light
    {
        public Vector3 Direction;
        public double Intensity;
    }

    public class Vector3
    {
        public double X, Y, Z;
        public Vector3(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
            => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3 operator -(Vector3 a, Vector3 b)
            => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3 operator *(Vector3 a, double d)
            => new Vector3(a.X * d, a.Y * d, a.Z * d);

        public static Vector3 operator /(Vector3 a, double d)
            => new Vector3(a.X / d, a.Y / d, a.Z / d);

        public static Vector3 operator -(Vector3 a)
            => new Vector3(-a.X, -a.Y, -a.Z);

        public double Length()
            => Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3 Normalize()
        {
            double len = Length();
            return (len == 0) ? new Vector3(0, 0, 0) : (this / len);
        }

        public static double Dot(Vector3 a, Vector3 b)
            => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        public static Vector3 operator *(double d, Vector3 a)
            => a * d;
    }

    public class Matrix4x4
    {
        public double[,] M = new double[4, 4];

        public Matrix4x4() { }

        public static Matrix4x4 Identity()
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.M[0, 0] = 1;
            matrix.M[1, 1] = 1;
            matrix.M[2, 2] = 1;
            matrix.M[3, 3] = 1;
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
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            mat.M[1, 1] = c; mat.M[1, 2] = -s;
            mat.M[2, 1] = s; mat.M[2, 2] = c;
            return mat;
        }

        public static Matrix4x4 CreateRotationY(double angle)
        {
            Matrix4x4 mat = Identity();
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            mat.M[0, 0] = c; mat.M[0, 2] = s;
            mat.M[2, 0] = -s; mat.M[2, 2] = c;
            return mat;
        }

        public static Matrix4x4 CreateRotationZ(double angle)
        {
            Matrix4x4 mat = Identity();
            double c = Math.Cos(angle);
            double s = Math.Sin(angle);
            mat.M[0, 0] = c; mat.M[0, 1] = -s;
            mat.M[1, 0] = s; mat.M[1, 1] = c;
            return mat;
        }

        public static Matrix4x4 CreateRotationXYZ(double ax, double ay, double az)
        {
            return CreateRotationX(ax) * CreateRotationY(ay) * CreateRotationZ(az);
        }

        public static Matrix4x4 CreatePerspectiveFieldOfView(double fov, double aspect, double zn, double zf)
        {
            Matrix4x4 mat = new Matrix4x4();
            double yScale = 1.0 / Math.Tan(fov / 2);
            double xScale = yScale / aspect;
            mat.M[0, 0] = xScale;
            mat.M[1, 1] = yScale;
            mat.M[2, 2] = zf / (zf - zn);
            mat.M[2, 3] = -zn * zf / (zf - zn);
            mat.M[3, 2] = 1;
            mat.M[3, 3] = 0;
            return mat;
        }

        public static Matrix4x4 CreateLookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            //Направление вперёд
            Vector3 zaxis = (target - eye).Normalize();
            //Вправо
            Vector3 xaxis = Vector3.Cross(up, zaxis).Normalize();
            //Вверх
            Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

            Matrix4x4 view = Identity();
            view.M[0, 0] = xaxis.X; view.M[0, 1] = xaxis.Y; view.M[0, 2] = xaxis.Z;
            view.M[1, 0] = yaxis.X; view.M[1, 1] = yaxis.Y; view.M[1, 2] = yaxis.Z;
            view.M[2, 0] = zaxis.X; view.M[2, 1] = zaxis.Y; view.M[2, 2] = zaxis.Z;

            view.M[0, 3] = -Vector3.Dot(xaxis, eye);
            view.M[1, 3] = -Vector3.Dot(yaxis, eye);
            view.M[2, 3] = -Vector3.Dot(zaxis, eye);

            return view;
        }

        public static Matrix4x4 operator *(Matrix4x4 A, Matrix4x4 B)
        {
            Matrix4x4 R = new Matrix4x4();
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    R.M[r, c] = A.M[r, 0] * B.M[0, c]
                              + A.M[r, 1] * B.M[1, c]
                              + A.M[r, 2] * B.M[2, c]
                              + A.M[r, 3] * B.M[3, c];
                }
            }
            return R;
        }

        public Vector3 Transform(Vector3 v)
        {
            double x = v.X * M[0, 0] + v.Y * M[0, 1] + v.Z * M[0, 2] + M[0, 3];
            double y = v.X * M[1, 0] + v.Y * M[1, 1] + v.Z * M[1, 2] + M[1, 3];
            double z = v.X * M[2, 0] + v.Y * M[2, 1] + v.Z * M[2, 2] + M[2, 3];
            double w = v.X * M[3, 0] + v.Y * M[3, 1] + v.Z * M[3, 2] + M[3, 3];

            //не близко к 0
            if (Math.Abs(w) > 1e-7)
            {
                x /= w;
                y /= w;
                z /= w;
            }
            return new Vector3(x, y, z);
        }
    }
}
