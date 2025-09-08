
using System;

namespace MGAlienLib
{
    /// <summary>
    /// (WIP) 성능 관리자입니다.
    /// </summary>
    public sealed class PerformanceManager : ManagerBase
    {
        public int fps { get; private set; }
        private int frameCounter = 0;
        private DateTime lastfpscheck = DateTime.Now;
        public int drawcallCount { get; private set; }
        public int verticesCount { get; private set; }

        public PerformanceManager(GameBase owner) : base(owner)
        {
        }

        public override void OnPreUpdate()
        {
            frameCounter++;
            if (DateTime.Now.Subtract(lastfpscheck).TotalMilliseconds > 1000)
            {
                fps = frameCounter;
                //System.Diagnostics.Debug.WriteLine("fps: " + frameCounter);
                frameCounter = 0;
                lastfpscheck = DateTime.Now;
            }
        }

        public void ReportRenderStatistics(int drawcallCount, int verticesCount)
        {
            this.drawcallCount = drawcallCount;
            this.verticesCount = verticesCount;
        }

        public override string ToString()
        {
            return $"fps: {fps}, batchCount: {drawcallCount}, verticesCount: {verticesCount}";
        }

    }
}
