using System.Collections.Generic;
using KBMGraphics;

namespace KinectBodyModification
{
    public class LimbData
    {

        public HashSet<int> activePixels;
        public HashSet<int> contourPixels;
        public LimbDataPixel[] allPixels;
        public List<LimbDataSkeleton> limbDataSkeletons = new List<LimbDataSkeleton>();

        public Mesh mesh;

        public LimbData()
        {
            activePixels = new HashSet<int>();
            contourPixels = new HashSet<int>();
            allPixels = new LimbDataPixel[Configuration.width * Configuration.height];
            for (var i = 0; i < Configuration.width * Configuration.height; i++)
            {
                allPixels[i] = new LimbDataPixel();
            }

            mesh = new Mesh();
        }

    }
}
