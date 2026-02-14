using System.Diagnostics;

namespace _3Dvishok.Engine.Core
{
    public class FrameStats
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private double smoothFrameTime = 1.0 / 60.0;

        public double DeltaTime { get; private set; }
        public double Fps { get; private set; }

        public void Start()
        {
            stopwatch.Restart();
        }

        public void Tick()
        {
            double dt = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            if (dt <= 0.0)
            {
                dt = 0.001;
            }

            if (dt > 0.1)
            {
                dt = 0.1;
            }

            DeltaTime = dt;
            smoothFrameTime = smoothFrameTime * 0.90 + dt * 0.10;
            Fps = 1.0 / System.Math.Max(0.00001, smoothFrameTime);
        }
    }
}
