using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public static class Configuration
    {

        public static readonly int width = 640;
        public static readonly int height = 480;
        public static readonly int size = width * height;

        public static Dictionary<JointTypePair, float> boneOffsetDictionary = new Dictionary<JointTypePair, float>()
        {
            { new JointTypePair(JointType.ShoulderCenter, JointType.ShoulderLeft), 0.7f },
            { new JointTypePair(JointType.ShoulderCenter, JointType.ShoulderRight), 0.7f },

            { new JointTypePair(JointType.ShoulderLeft, JointType.ElbowLeft), 0.2f },
            { new JointTypePair(JointType.ShoulderRight, JointType.ElbowRight), 0.2f },

            { new JointTypePair(JointType.ElbowLeft, JointType.WristLeft), 0.15f },
            { new JointTypePair(JointType.ElbowRight, JointType.WristRight), 0.15f },

            { new JointTypePair(JointType.HipLeft, JointType.KneeLeft), 0.2f },
            { new JointTypePair(JointType.HipRight, JointType.KneeRight), 0.2f },
        };

    }
}
