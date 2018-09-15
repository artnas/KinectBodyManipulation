using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public class Settings
    {

        private static Settings _instance;
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Settings();
                }

                return _instance;
            }
        }

        private Settings()
        {
            this.HeadSize = 100;
            this.ArmScale = 100;
            this.LegScale = 100;
        }

        public float HeadSize { get; set; }
        public float ArmScale { get; set; }
        public float LegScale { get; set; }
        public bool DebugDrawSkeleton { get; set; }
        public bool DebugDrawJoints { get; set; }
        public bool DebugDrawSilhouette { get; set; }

    }
}
