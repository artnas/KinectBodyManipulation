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

        public static readonly Dictionary<JointTypePair, BoneConfiguration> boneConfigurationsDictionary = new Dictionary<JointTypePair, BoneConfiguration>()
        {
            { new JointTypePair(JointType.ShoulderCenter, JointType.ShoulderLeft), new BoneConfiguration(new JointTypePair(JointType.ShoulderCenter, JointType.ShoulderLeft), 0.7f, 0.1f, -1, -1) },
            { new JointTypePair(JointType.ShoulderCenter, JointType.ShoulderRight), new BoneConfiguration(new JointTypePair(JointType.ShoulderCenter, JointType.ShoulderRight), 0.7f, 0.1f, -1, -1) },

            { new JointTypePair(JointType.ShoulderLeft, JointType.ElbowLeft), new BoneConfiguration(new JointTypePair(JointType.ShoulderLeft, JointType.ElbowLeft), 0.2f, 0.1f, -1, -1) },
            { new JointTypePair(JointType.ShoulderRight, JointType.ElbowRight), new BoneConfiguration(new JointTypePair(JointType.ShoulderRight, JointType.ElbowRight), 0.2f, 0.1f, -1, -1) },

            { new JointTypePair(JointType.ElbowLeft, JointType.WristLeft), new BoneConfiguration(new JointTypePair(JointType.ElbowLeft, JointType.WristLeft), 0.15f, 0.1f, -1, -1) },
            { new JointTypePair(JointType.ElbowRight, JointType.WristRight), new BoneConfiguration(new JointTypePair(JointType.ElbowRight, JointType.WristRight), 0.15f, 0.1f, -1, -1) },

            { new JointTypePair(JointType.HipLeft, JointType.KneeLeft), new BoneConfiguration(new JointTypePair(JointType.HipLeft, JointType.KneeLeft), 0.2f, 0.1f, -1, -1) },
            { new JointTypePair(JointType.HipRight, JointType.KneeRight), new BoneConfiguration(new JointTypePair(JointType.HipRight, JointType.KneeRight), 0.2f, 0.1f, -1, -1) },

            { new JointTypePair(JointType.ShoulderCenter, JointType.Spine), new BoneConfiguration(new JointTypePair(JointType.ShoulderCenter, JointType.Spine), 0.1f, 0.1f, 30, -1) },
            { new JointTypePair(JointType.Spine, JointType.HipCenter), new BoneConfiguration(new JointTypePair(JointType.Spine, JointType.HipCenter), 0.1f, 0.1f, 30, -1) },

        };

        public static readonly BoneConfiguration boneConfigurationDefault =
            new BoneConfiguration(new JointTypePair(JointType.Head, JointType.Head), 0.1f, 0.1f, -1, -1);

    }

    public class BoneConfiguration
    {
        public JointTypePair jointTypePair;
        public float startOffset, endOffset;
        public int startWidth, endWidth;

        public BoneConfiguration(JointTypePair jointTypePair, float startOffset, float endOffset, int startWidth, int endWidth)
        {
            this.jointTypePair = jointTypePair;
            this.startOffset = startOffset;
            this.endOffset = endOffset;
            this.startWidth = startWidth;
            this.endWidth = endWidth;
        }
    }

}
