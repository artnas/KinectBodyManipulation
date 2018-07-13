using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public class LimbData
    {

        public LimbDataPixel[] pixelData = new LimbDataPixel[Configuration.size];
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
