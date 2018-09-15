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

        private static ColorImagePoint[] colorCoordinates;
        private static DepthImagePoint[] depthCoordinates;

        private static LimbDataManager limbDataManager;

        private static byte[] tempBuffer = new byte[Configuration.size * 4];

        public static void SetBuffers(
            DepthImagePixel[] depthBuffer, 
            byte[] colorBuffer, 
            byte[] outputBuffer, 
            byte[] backgroundRemovedBuffer, 
            LimbDataManager limbDataManager, 
            byte[] savedBackgroundColorBuffer, 
            DepthImagePixel[] savedBackgroundDepthBuffer, 
            byte[] normalBuffer, 
            ColorImagePoint[] colorCoordinates, 
            DepthImagePoint[] depthCoordinates)
        {
            Drawing.depthBuffer = depthBuffer;
            Drawing.colorBuffer = colorBuffer;
            Drawing.outputBuffer = outputBuffer;
            Drawing.backgroundRemovedBuffer = backgroundRemovedBuffer;
            Drawing.limbDataManager = limbDataManager;
            Drawing.savedBackgroundColorBuffer = savedBackgroundColorBuffer;
            Drawing.savedBackgroundDepthBuffer = savedBackgroundDepthBuffer;
            Drawing.normalBuffer = normalBuffer;
            Drawing.colorCoordinates = colorCoordinates;
            Drawing.depthCoordinates = depthCoordinates;
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

        public static void DrawColorBuffer()
        {

            Array.Copy(colorBuffer, outputBuffer, outputBuffer.Length);

        }

        public static void DrawDepthBuffer()
        {

            for (var i = 0; i < depthBuffer.Length; i++)
            {
                var index = i * 4;
                outputBuffer[index + 0] = (byte) (depthBuffer[i].Depth % 256);
                outputBuffer[index + 1] = (byte) (depthBuffer[i].Depth % 256);
                outputBuffer[index + 2] = (byte) (depthBuffer[i].Depth % 256);
                outputBuffer[index + 3] = (byte) (depthBuffer[i].Depth % 256);
            }

        }

        public static void DrawDepthDifference()
        {
            for (var i = 0; i < depthBuffer.Length; i++)
            {
                var index = i * 4;
                short difference = (short)Math.Abs(savedBackgroundDepthBuffer[i].Depth - depthBuffer[i].Depth);

                if (difference > Configuration.depthThreshold)
                {
                    outputBuffer[index + 0] = (byte)(difference % 256);
                    outputBuffer[index + 1] = (byte)(difference % 256);
                    outputBuffer[index + 2] = (byte)(difference % 256);
                    outputBuffer[index + 3] = (byte)(difference % 256);
                }
                else
                {
                    outputBuffer[index + 0] = 0;
                    outputBuffer[index + 1] = 0;
                    outputBuffer[index + 2] = 0;
                    outputBuffer[index + 3] = 0;
                }
            }
        }

        public static void DrawMappedDepthDifference()
        {
            for (var i = 0; i < depthBuffer.Length; i++)
            {
                var index = i * 4;

                var mappedDepthBufferIndex =
                    Utils.GetIndexByCoordinates(depthCoordinates[i].X, depthCoordinates[i].Y);

                if (mappedDepthBufferIndex < 0 || mappedDepthBufferIndex >= depthBuffer.Length)
                {
                    continue;
                }

                short difference = (short)Math.Abs(savedBackgroundDepthBuffer[mappedDepthBufferIndex].Depth - depthBuffer[mappedDepthBufferIndex].Depth);

                if (difference > Configuration.depthThreshold)
                {
                    outputBuffer[index + 0] = (byte)(difference % 256);
                    outputBuffer[index + 1] = (byte)(difference % 256);
                    outputBuffer[index + 2] = (byte)(difference % 256);
                    outputBuffer[index + 3] = (byte)(difference % 256);
                }
                else
                {
                    outputBuffer[index + 0] = colorBuffer[index + 0];
                    outputBuffer[index + 1] = colorBuffer[index + 1];
                    outputBuffer[index + 2] = colorBuffer[index + 2];
                    outputBuffer[index + 3] = 255;
                }
            }
        }

        public static void Draw()
        {

            DrawBackground();

            BoneProcessor.ProcessAllBones();
       
            DrawDebug();

        }

    }
}
