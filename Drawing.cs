using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public static class Drawing
    {

        public static void DrawDebug(WriteableBitmap writeableBitmap, byte[] colorBuffer, LimbData limbData)
        {

            // cialo
            for (int i = 0; i < colorBuffer.Length; i+=4)
            {
                var limbPixel = limbData.pixelData[i/4];

                if (limbPixel.humanIndex != -1)
                {

                    var color = Utils.limbColors[((int)limbPixel.startJointType) % Utils.limbColors.Length];

                    colorBuffer[i] = Utils.Interpolate(colorBuffer[i], color.B, 0.7f);
                    colorBuffer[i + 1] = Utils.Interpolate(colorBuffer[i+1], color.G, 0.7f);
                    colorBuffer[i + 2] = Utils.Interpolate(colorBuffer[i+2], color.R, 0.7f);

                }
            }

            // szkielet (kosci)
            foreach (var limbDataSkeleton in limbData.limbDataSkeletons)
            {
                foreach (var bone in limbDataSkeleton.bones)
                {
                    if (bone.points.Count == 0)
                        continue;

                    var color = Colors.White;

                    if (bone.startJoint.TrackingState == JointTrackingState.Inferred || bone.endJoint.TrackingState == JointTrackingState.Inferred)
                    {
                        color = Colors.Red;
                    }
                    else if (bone.startJoint.TrackingState == JointTrackingState.NotTracked || bone.endJoint.TrackingState == JointTrackingState.NotTracked)
                    {
                        color = Colors.Black;
                    }

                    // piksele kosci
                    foreach (var point in bone.points)
                    {
                        DrawThickDot(colorBuffer, ( (int)point.X + (int)point.Y * Configuration.width ) * 4, 2, color);
                    }  
                }
            }

            // szkielet (jointy)
            foreach (var limbDataSkeleton in limbData.limbDataSkeletons)
            {
                foreach (var bone in limbDataSkeleton.bones)
                {
                    if (bone.points.Count == 0)
                        continue;

                    // piksele poczatkowego i koncowego jointa tej kosci
                    DrawThickDot(colorBuffer, ((int)bone.GetStartPoint().X + (int)bone.GetStartPoint().Y * Configuration.width) * 4, 3, Colors.Yellow);
                    DrawThickDot(colorBuffer, ((int)bone.GetEndPoint().X + (int)bone.GetEndPoint().Y * Configuration.width) * 4, 3, Colors.Yellow);
                }
            }

        }

        private static void DrawThickDot(byte[] buffer, int index, int thickness, Color color)
        {

            for (int y = -thickness; y <= thickness; y++)
            {
                for (int x = -thickness; x <= thickness; x++)
                {

                    int offsetIndex = index + x * 4 + y * Configuration.width * 4;

                    if (offsetIndex < 0 || offsetIndex >= buffer.Length)
                        continue;

                    buffer[offsetIndex] = color.B;
                    buffer[offsetIndex+1] = color.G;
                    buffer[offsetIndex+2] = color.R;

                }
            }

        }

    }
}
