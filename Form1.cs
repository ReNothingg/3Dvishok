using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using _3Dvishok.Engine.Core;
using _3Dvishok.Engine.Input;
using _3Dvishok.Engine.Math;
using _3Dvishok.Engine.Rendering;
using _3Dvishok.Engine.Scene;

namespace _3Dvishok
{
    public partial class Form1 : Form
    {
        private readonly Scene scene;
        private readonly Stack<Model> spawnedModels = new Stack<Model>();

        private readonly Camera camera = new Camera();
        private readonly Light light = new Light();
        private readonly InputState input = new InputState();
        private readonly FrameStats frameStats = new FrameStats();

        private readonly SoftwareRenderer renderer = new SoftwareRenderer();
        private readonly RendererSettings rendererSettings = new RendererSettings();
        private readonly HudRenderer hudRenderer = new HudRenderer();
        private readonly Timer timer = new Timer();

        private readonly Font hudFont = new Font("Consolas", 10f);
        private readonly Random random = new Random();

        private bool autoRotate = true;
        private bool showHud = true;
        private bool rightMouseLook;
        private Point lastMousePos;

        public Form1()
        {
            InitializeComponent();

            scene = SceneFactory.CreateDefaultScene();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            KeyPreview = true;
            ClientSize = new Size(1280, 720);
            Text = "3Dvishok Engine Sandbox";

            ConfigureCamera();
            ConfigureLight();

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            Resize += Form1_Resize;
            MouseDown += Form1_MouseDown;
            MouseUp += Form1_MouseUp;
            MouseMove += Form1_MouseMove;
            MouseWheel += Form1_MouseWheel;

            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            frameStats.Start();
            timer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer.Stop();
            hudFont.Dispose();
            base.OnFormClosed(e);
        }

        private void ConfigureCamera()
        {
            camera.Position = new Vector3(0.0, 1.5, -3.0);
            camera.Yaw = 0.0;
            camera.Pitch = MathUtil.DegToRad(-10.0);
            camera.FOV = MathUtil.DegToRad(70.0);
            camera.AspectRatio = (double)ClientSize.Width / Math.Max(1, ClientSize.Height);
            camera.NearPlane = 0.1;
            camera.FarPlane = 300.0;
        }

        private void ConfigureLight()
        {
            light.Direction = new Vector3(0.55, -1.0, -0.6).Normalize();
            light.Intensity = 0.85;
            light.Ambient = 0.20;
        }

        private void ResetCamera()
        {
            camera.Position = new Vector3(0.0, 1.5, -3.0);
            camera.Yaw = 0.0;
            camera.Pitch = MathUtil.DegToRad(-10.0);
            camera.FOV = MathUtil.DegToRad(70.0);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            bool firstPress = input.SetDown(e.KeyCode);
            if (!firstPress)
            {
                return;
            }

            if (e.KeyCode == Keys.F1)
            {
                rendererSettings.RenderMode = (RenderMode)(((int)rendererSettings.RenderMode + 1) % 3);
            }
            else if (e.KeyCode == Keys.F2)
            {
                rendererSettings.BackfaceCulling = !rendererSettings.BackfaceCulling;
            }
            else if (e.KeyCode == Keys.F3)
            {
                autoRotate = !autoRotate;
            }
            else if (e.KeyCode == Keys.F4)
            {
                showHud = !showHud;
            }
            else if (e.KeyCode == Keys.R)
            {
                ResetCamera();
            }
            else if (e.KeyCode == Keys.N)
            {
                SpawnCubeInFrontOfCamera();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                RemoveLastSpawnedCube();
            }

            Invalidate();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            input.SetUp(e.KeyCode);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            camera.AspectRatio = (double)ClientSize.Width / Math.Max(1, ClientSize.Height);
            Invalidate();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                rightMouseLook = true;
                lastMousePos = e.Location;
                Cursor = Cursors.SizeAll;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                rightMouseLook = false;
                Cursor = Cursors.Default;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!rightMouseLook)
            {
                return;
            }

            int dx = e.X - lastMousePos.X;
            int dy = e.Y - lastMousePos.Y;
            lastMousePos = e.Location;

            const double mouseLookSpeed = 0.0045;
            camera.Yaw += dx * mouseLookSpeed;
            camera.Pitch -= dy * mouseLookSpeed;
            camera.ClampPitch(MathUtil.DegToRad(89.0));
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            double step = MathUtil.DegToRad(2.5);
            camera.FOV += e.Delta > 0 ? -step : step;
            camera.FOV = MathUtil.Clamp(camera.FOV, MathUtil.DegToRad(35.0), MathUtil.DegToRad(100.0));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            frameStats.Tick();
            double dt = frameStats.DeltaTime;

            UpdateCamera(dt);
            UpdateScene(dt);
            Invalidate();
        }

        private void UpdateCamera(double dt)
        {
            double moveSpeed = input.IsAnyShiftDown() ? 9.0 : 4.0;
            double lookSpeed = MathUtil.DegToRad(90.0);

            if (input.IsDown(Keys.Left))
            {
                camera.Yaw -= lookSpeed * dt;
            }
            if (input.IsDown(Keys.Right))
            {
                camera.Yaw += lookSpeed * dt;
            }
            if (input.IsDown(Keys.Up))
            {
                camera.Pitch += lookSpeed * dt;
            }
            if (input.IsDown(Keys.Down))
            {
                camera.Pitch -= lookSpeed * dt;
            }
            camera.ClampPitch(MathUtil.DegToRad(89.0));

            Vector3 velocity = Vector3.Zero;
            Vector3 forward = camera.GetForwardVector();
            Vector3 right = camera.GetRightVector();

            if (input.IsDown(Keys.W))
            {
                velocity += forward;
            }
            if (input.IsDown(Keys.S))
            {
                velocity -= forward;
            }
            if (input.IsDown(Keys.D))
            {
                velocity += right;
            }
            if (input.IsDown(Keys.A))
            {
                velocity -= right;
            }
            if (input.IsDown(Keys.Q))
            {
                velocity += Vector3.UnitY;
            }
            if (input.IsDown(Keys.E))
            {
                velocity -= Vector3.UnitY;
            }

            if (velocity.LengthSquared() > 0.0000001)
            {
                camera.Position += velocity.Normalize() * (moveSpeed * dt);
            }
        }

        private void UpdateScene(double dt)
        {
            if (!autoRotate)
            {
                return;
            }

            for (int i = 0; i < scene.Models.Count; i++)
            {
                scene.Models[i].Rotation += scene.Models[i].RotationVelocity * dt;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            RenderStats renderStats = renderer.Render(e.Graphics, ClientRectangle, scene, camera, light, rendererSettings);
            if (!showHud)
            {
                return;
            }

            HudState hudState = new HudState();
            hudState.Fps = frameStats.Fps;
            hudState.RenderMode = rendererSettings.RenderMode;
            hudState.BackfaceCulling = rendererSettings.BackfaceCulling;
            hudState.AutoRotate = autoRotate;
            hudState.ObjectCount = renderStats.ObjectCount;
            hudState.VisibleFaceCount = renderStats.FaceCount;
            hudState.SpawnedObjectCount = spawnedModels.Count;
            hudState.CameraPosition = camera.Position;
            hudState.CameraYaw = camera.Yaw;
            hudState.CameraPitch = camera.Pitch;

            hudRenderer.Draw(e.Graphics, hudFont, hudState);
        }

        private void SpawnCubeInFrontOfCamera()
        {
            Vector3 forward = camera.GetForwardVector();
            Vector3 spawnPos = camera.Position + forward * 6.0;
            spawnPos += Vector3.UnitY * RandomRange(-0.8, 0.8);

            int r = (int)RandomRange(60, 240);
            int g = (int)RandomRange(60, 240);
            int b = (int)RandomRange(60, 240);

            Model cube = Model.CreateCube(RandomRange(0.6, 1.4), Color.FromArgb(r, g, b));
            cube.Position = spawnPos;
            cube.Rotation = new Vector3(RandomRange(0.0, 360.0), RandomRange(0.0, 360.0), RandomRange(0.0, 360.0));
            cube.RotationVelocity = new Vector3(
                RandomRange(-40.0, 40.0),
                RandomRange(-40.0, 40.0),
                RandomRange(-40.0, 40.0));

            scene.Add(cube);
            spawnedModels.Push(cube);
        }

        private void RemoveLastSpawnedCube()
        {
            if (spawnedModels.Count == 0)
            {
                return;
            }

            Model model = spawnedModels.Pop();
            scene.Remove(model);
        }

        private double RandomRange(double min, double max)
        {
            return min + random.NextDouble() * (max - min);
        }
    }
}
