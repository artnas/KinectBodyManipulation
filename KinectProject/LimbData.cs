using System.Collections.Generic;
using KBMGraphics;

namespace KinectBodyModification
{
    public class LimbData
    {
        public HashSet<int> ActivePixels;
        public LimbDataPixel[] AllPixels;
        public HashSet<int> ContourPixels;
        public LimbDataSkeleton LimbDataSkeleton;

        public Mesh Mesh;

        public LimbData()
        {
            ActivePixels = new HashSet<int>();
            ContourPixels = new HashSet<int>();
            AllPixels = new LimbDataPixel[Configuration.Size];
            for (var i = 0; i < Configuration.Size; i++) AllPixels[i] = new LimbDataPixel();

            Mesh = new Mesh();
        }
    }
}