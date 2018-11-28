using System;
using System.Windows.Media;
using Microsoft.Kinect;

using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static partial class Drawing
    {
        private static byte[] clearBytes = GetClearBytes();

        private static byte[] GetClearBytes()
        {
            byte[] clearBytes = new byte[Configuration.size * 4];

            for (var i = 0; i < clearBytes.Length; i += 4)
            {
                clearBytes[i] = 0;
                clearBytes[i+1] = 0;
                clearBytes[i+2] = 0;
                clearBytes[i+3] = 0;
            }

            return clearBytes;
        }

        public static void Clear()
        {
            Array.Copy(clearBytes, GB.outputBuffer, clearBytes.Length);
        }

        private static void DrawThickDot(byte[] buffer, int index, int thickness, Color color)
        {
            for (var y = -thickness; y <= thickness; y++)
            for (var x = -thickness; x <= thickness; x++)
            {
                var offsetIndex = index + x * 4 + y * Configuration.width * 4;

                if (offsetIndex < 0 || offsetIndex >= buffer.Length)
                    continue;

                buffer[offsetIndex] = color.B;
                buffer[offsetIndex + 1] = color.G;
                buffer[offsetIndex + 2] = color.R;
                buffer[offsetIndex + 3] = 255;
            }
        }

        public static void DrawBackground()
        {
            Array.Copy(GB.savedBackgroundColorBuffer, GB.outputBuffer, GB.outputBuffer.Length);
        }

        public static void DrawColorBuffer()
        {
            Array.Copy(GB.colorBuffer, GB.outputBuffer, GB.outputBuffer.Length);
        }

        public static void DrawDepthBuffer()
        {
            for (var i = 0; i < GB.depthBuffer.Length; i++)
            {
                var index = i * 4;
                GB.outputBuffer[index + 0] = (byte) (GB.depthBuffer[i].Depth % 256);
                GB.outputBuffer[index + 1] = (byte) (GB.depthBuffer[i].Depth % 256);
                GB.outputBuffer[index + 2] = (byte) (GB.depthBuffer[i].Depth % 256);
                GB.outputBuffer[index + 3] = (byte) (GB.depthBuffer[i].Depth % 256);
            }
        }

        public static void DrawDepthDifference()
        {
            for (var i = 0; i < GB.depthBuffer.Length; i++)
            {
                var index = i * 4;
                var difference = (short) Math.Abs(GB.savedBackgroundDepthBuffer[i].Depth - GB.depthBuffer[i].Depth);

                if (difference > Configuration.depthThreshold)
                {
                    GB.outputBuffer[index + 0] = (byte) (difference % 256);
                    GB.outputBuffer[index + 1] = (byte) (difference % 256);
                    GB.outputBuffer[index + 2] = (byte) (difference % 256);
                    GB.outputBuffer[index + 3] = (byte) (difference % 256);
                }
                else
                {
                    GB.outputBuffer[index + 0] = 0;
                    GB.outputBuffer[index + 1] = 0;
                    GB.outputBuffer[index + 2] = 0;
                    GB.outputBuffer[index + 3] = 0;
                }
            }
        }

        public static void DrawMappedDepthDifference()
        {
            for (var i = 0; i < GB.depthBuffer.Length; i++)
            {
                var index = i * 4;

                var mappedDepthBufferIndex =
                    Utils.CoordinatesToIndex(GB.depthCoordinates[i].X, GB.depthCoordinates[i].Y);

                if (mappedDepthBufferIndex < 0 || mappedDepthBufferIndex >= GB.depthBuffer.Length) continue;

                var difference = (short) Math.Abs(GB.savedBackgroundDepthBuffer[mappedDepthBufferIndex].Depth -
                                                  GB.depthBuffer[mappedDepthBufferIndex].Depth);

                if (difference > Configuration.depthThreshold)
                {
                    GB.outputBuffer[index + 0] = (byte) (difference % 256);
                    GB.outputBuffer[index + 1] = (byte) (difference % 256);
                    GB.outputBuffer[index + 2] = (byte) (difference % 256);
                    GB.outputBuffer[index + 3] = (byte) (difference % 256);
                }
                else
                {
                    GB.outputBuffer[index + 0] = GB.colorBuffer[index + 0];
                    GB.outputBuffer[index + 1] = GB.colorBuffer[index + 1];
                    GB.outputBuffer[index + 2] = GB.colorBuffer[index + 2];
                    GB.outputBuffer[index + 3] = 255;
                }
            }
        }
    }
}