using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public static partial class Drawing
    {

        private static DepthImagePixel[] depthBuffer;

        private static byte[] colorBuffer;
        private static byte[] outputBuffer;
        private static byte[] backgroundRemovedBuffer;
        private static byte[] normalBuffer;

        private static byte[] savedBackgroundColorBuffer;
        private static DepthImagePixel[] savedBackgroundDepthBuffer;

        private static LimbDataManager limbDataManager;

        private static byte[] tempBuffer = new byte[Configuration.size * 4];

        public static void SetBuffers(DepthImagePixel[] depthBuffer, byte[] colorBuffer, byte[] outputBuffer, byte[] backgroundRemovedBuffer, LimbDataManager limbDataManager, byte[] savedBackgroundColorBuffer, DepthImagePixel[] savedBackgroundDepthBuffer, byte[] normalBuffer)
        {
            Drawing.depthBuffer = depthBuffer;
            Drawing.colorBuffer = colorBuffer;
            Drawing.outputBuffer = outputBuffer;
            Drawing.backgroundRemovedBuffer = backgroundRemovedBuffer;
            Drawing.limbDataManager = limbDataManager;
            Drawing.savedBackgroundColorBuffer = savedBackgroundColorBuffer;
            Drawing.savedBackgroundDepthBuffer = savedBackgroundDepthBuffer;
            Drawing.normalBuffer = normalBuffer;
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

        public static void DrawBackground()
        {

            Array.Copy(savedBackgroundColorBuffer, outputBuffer, outputBuffer.Length);

        }

    }
}
