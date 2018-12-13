using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KBMGraphics;
using Microsoft.Kinect;
using OpenTK;
using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public partial class LimbDataManager
    {
        private readonly KinectSensor _sensor;
        private readonly Queue<int> _floodFillPixelsQueue = new Queue<int>();

        public LimbData LimbData;

        public LimbDataManager(KinectSensor sensor)
        {
            _sensor = sensor;
            _outlineTriangulator = new OutlineTriangulator(Configuration.Width, Configuration.Height);

            LimbData = new LimbData();
        }

        private void ClearBuffer()
        {
            LimbData.ActivePixels.Clear();
            LimbData.ContourPixels.Clear();

            _floodFillPixelsQueue.Clear();

            Parallel.ForEach(LimbData.AllPixels, pixel => { pixel.Clear(); });
        }

        public void Update(Skeleton[] skeletons)
        {
            ClearBuffer();

            UpdateSkeletons(skeletons);

            FloodFillSkeletonPixels();

            ContourActivePixels();

            var sortedContour = GetSortedContour();
            var contourMesh = _outlineTriangulator.GetMesh(sortedContour);

            LimbData.Mesh.Update(contourMesh);
        }
    }
}