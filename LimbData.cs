using System.Collections.Generic;

namespace KinectBodyModification
{
    public class LimbData
    {

        public LimbDataPixel[] pixelData;
        public List<LimbDataSkeleton> limbDataSkeletons = new List<LimbDataSkeleton>();

        public LimbData()
        {
            pixelData = new LimbDataPixel[Configuration.width * Configuration.height];
            for (var i = 0; i < Configuration.width * Configuration.height; i++)
            {
                pixelData[i] = new LimbDataPixel();
            }
        }

    }
}
