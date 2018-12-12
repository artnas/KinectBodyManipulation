using System.Collections.Generic;
using KBMGraphics;

namespace KinectBodyModification
{
    public class LimbData
    {
        public HashSet<int> activePixels;
        public LimbDataPixel[] allPixels;
        public HashSet<int> contourPixels;
        public List<LimbDataSkeleton> limbDataSkeletons = new List<LimbDataSkeleton>();

        public Mesh mesh;

        public LimbData()
        {
            activePixels = new HashSet<int>();
            contourPixels = new HashSet<int>();
            allPixels = new LimbDataPixel[Configuration.size];
            for (var i = 0; i < Configuration.size; i++) allPixels[i] = new LimbDataPixel();

            mesh = new Mesh();
        }
    }
}