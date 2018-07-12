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

        public static void DrawDebug(WriteableBitmap writeableBitmap, byte[] colorBuffer, LimbPixelData[] limbPixelData)
        {

            for (int i = 0; i < colorBuffer.Length; i+=4)
            {
                var limbPixel = limbPixelData[i/4];

                if (limbPixel.humanIndex != -1)
                {

                    var color = limbPixel.isBone ? Colors.White : Utils.limbColors[((int)limbPixel.startJointType) % Utils.limbColors.Length];

                    colorBuffer[i] = Utils.Interpolate(colorBuffer[i], color.B, 0.7f);
                    colorBuffer[i + 1] = Utils.Interpolate(colorBuffer[i+1], color.G, 0.7f);
                    colorBuffer[i + 2] = Utils.Interpolate(colorBuffer[i+2], color.R, 0.7f);

                }
            }

        }

    }
}
