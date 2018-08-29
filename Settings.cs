using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public static class Settings
    {

        public static float headSize = 1;

        public static void Update(MainWindow window)
        {
            headSize = (float)window.HeadSize.Value / 100f;
        }

    }
}
