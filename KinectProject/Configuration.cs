using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Kinect;

namespace KinectBodyModification
{
    public static class Configuration
    {
        public const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;
        public const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        public static readonly int Width = 640;
        public static readonly int Height = 480;
        public static readonly int Size = Width * Height;

        public static readonly byte AlphaThreshold = 120;
        public static readonly short DepthThreshold = 150;

        public static readonly Dictionary<JointTypePair, BoneConfiguration> BoneConfigurationsDictionary =
            new Dictionary<JointTypePair, BoneConfiguration>
            {
                {
                    new JointTypePair(JointType.Head, JointType.ShoulderCenter),
                    new BoneConfiguration(new JointTypePair(JointType.Head, JointType.ShoulderCenter), 0.1f, 0.4f, 20,
                        10, Colors.Black)
                },

                {
                    new JointTypePair(JointType.ShoulderCenter, JointType.ShoulderLeft),
                    new BoneConfiguration(new JointTypePair(JointType.ShoulderCenter, JointType.ShoulderLeft), 0.7f,
                        0.1f, 10, 10, Colors.Blue)
                },
                {
                    new JointTypePair(JointType.ShoulderCenter, JointType.ShoulderRight),
                    new BoneConfiguration(new JointTypePair(JointType.ShoulderCenter, JointType.ShoulderRight), 0.7f,
                        0.1f, 10, 10, Colors.Red)
                },

                {
                    new JointTypePair(JointType.ShoulderLeft, JointType.ElbowLeft),
                    new BoneConfiguration(new JointTypePair(JointType.ShoulderLeft, JointType.ElbowLeft), 0.2f, 0.1f,
                        20, 10, Colors.OrangeRed)
                },
                {
                    new JointTypePair(JointType.ShoulderRight, JointType.ElbowRight),
                    new BoneConfiguration(new JointTypePair(JointType.ShoulderRight, JointType.ElbowRight), 0.2f, 0.1f,
                        20, 10, Colors.CornflowerBlue)
                },

                {
                    new JointTypePair(JointType.ElbowLeft, JointType.WristLeft),
                    new BoneConfiguration(new JointTypePair(JointType.ElbowLeft, JointType.WristLeft), 0.15f, 0.1f, 20,
                        10, Colors.DarkMagenta)
                },
                {
                    new JointTypePair(JointType.ElbowRight, JointType.WristRight),
                    new BoneConfiguration(new JointTypePair(JointType.ElbowRight, JointType.WristRight), 0.15f, 0.1f,
                        20, 10, Colors.Crimson)
                },

                {
                    new JointTypePair(JointType.WristLeft, JointType.HandLeft),
                    new BoneConfiguration(new JointTypePair(JointType.WristLeft, JointType.HandLeft), 0.1f, 0.1f, 10,
                        10, Colors.Lime)
                },
                {
                    new JointTypePair(JointType.WristRight, JointType.HandRight),
                    new BoneConfiguration(new JointTypePair(JointType.WristRight, JointType.HandRight), 0.1f, 0.1f, 10,
                        10, Colors.LimeGreen)
                },

                {
                    new JointTypePair(JointType.ShoulderCenter, JointType.Spine),
                    new BoneConfiguration(new JointTypePair(JointType.ShoulderCenter, JointType.Spine), 0.0f, 0.1f, 40,
                        15, Colors.White)
                },

                {
                    new JointTypePair(JointType.Spine, JointType.HipCenter),
                    new BoneConfiguration(new JointTypePair(JointType.Spine, JointType.HipCenter), 0.05f, 0.1f, 35, 30,
                        Colors.DarkSlateGray)
                },

                {
                    new JointTypePair(JointType.HipCenter, JointType.HipLeft),
                    new BoneConfiguration(new JointTypePair(JointType.HipCenter, JointType.HipLeft), 0.2f, 0.1f, 15, 15,
                        Colors.Blue)
                },
                {
                    new JointTypePair(JointType.HipCenter, JointType.HipRight),
                    new BoneConfiguration(new JointTypePair(JointType.HipCenter, JointType.HipRight), 0.2f, 0.1f, 15,
                        15, Colors.Red)
                },

                {
                    new JointTypePair(JointType.HipLeft, JointType.KneeLeft),
                    new BoneConfiguration(new JointTypePair(JointType.HipLeft, JointType.KneeLeft), 0.2f, 0.1f, 15, 15,
                        Colors.OrangeRed)
                },
                {
                    new JointTypePair(JointType.HipRight, JointType.KneeRight),
                    new BoneConfiguration(new JointTypePair(JointType.HipRight, JointType.KneeRight), 0.2f, 0.1f, 15,
                        15, Colors.CornflowerBlue)
                },

                {
                    new JointTypePair(JointType.KneeLeft, JointType.AnkleLeft),
                    new BoneConfiguration(new JointTypePair(JointType.KneeLeft, JointType.AnkleLeft), 0.1f, 0.1f, 15,
                        15, Colors.DarkMagenta)
                },
                {
                    new JointTypePair(JointType.KneeRight, JointType.AnkleRight),
                    new BoneConfiguration(new JointTypePair(JointType.KneeRight, JointType.AnkleRight), 0.1f, 0.1f, 15,
                        15, Colors.Crimson)
                },

                {
                    new JointTypePair(JointType.AnkleLeft, JointType.FootLeft),
                    new BoneConfiguration(new JointTypePair(JointType.AnkleLeft, JointType.FootLeft), 0.1f, 0.1f, 15,
                        15, Colors.Lime)
                },
                {
                    new JointTypePair(JointType.AnkleRight, JointType.FootRight),
                    new BoneConfiguration(new JointTypePair(JointType.AnkleRight, JointType.FootLeft), 0.1f, 0.1f, 15,
                        15, Colors.LimeGreen)
                }
            };

        private static Dictionary<int, Color> _boneColorsDictionary;

        public static Color GetBoneColor(int boneHash)
        {
            if (_boneColorsDictionary == null)
            {
                _boneColorsDictionary = new Dictionary<int, Color>();

                foreach (var entry in BoneConfigurationsDictionary)
                {
                    var hash = Utils.GetBoneHash(entry.Key.A, entry.Key.B);

                    _boneColorsDictionary.Add(hash, entry.Value.Color);
                }
            }

            return _boneColorsDictionary[boneHash];
        }
    }

    public class BoneConfiguration
    {
        public Color Color;
        public JointTypePair JointTypePair;
        public float StartOffset, EndOffset;
        public int StartWidth, EndWidth;

        public BoneConfiguration(JointTypePair jointTypePair, float startOffset, float endOffset, int startWidth,
            int endWidth, Color color)
        {
            JointTypePair = jointTypePair;
            StartOffset = startOffset;
            EndOffset = endOffset;
            StartWidth = startWidth;
            EndWidth = endWidth;
            Color = color;
        }
    }
}