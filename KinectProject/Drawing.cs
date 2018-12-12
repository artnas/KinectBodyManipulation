using System;
using System.Windows.Media;
using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static partial class Drawing
    {
        private static readonly byte[] ClearBytes = GetClearBytes();

        private static byte[] GetClearBytes()
        {
            var clearBytes = new byte[Configuration.Size * 4];

            for (var i = 0; i < clearBytes.Length; i += 4)
            {
                clearBytes[i] = 0;
                clearBytes[i + 1] = 0;
                clearBytes[i + 2] = 0;
                clearBytes[i + 3] = 0;
            }

            return clearBytes;
        }

        public static void Clear()
        {
            Array.Copy(ClearBytes, GB.OutputBuffer, ClearBytes.Length);
        }

        private static void DrawThickDot(byte[] buffer, int index, int thickness, Color color)
        {
            for (var y = -thickness; y <= thickness; y++)
            for (var x = -thickness; x <= thickness; x++)
            {
                var offsetIndex = index + x * 4 + y * Configuration.Width * 4;

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
            Array.Copy(GB.SavedBackgroundColorBuffer, GB.OutputBuffer, GB.OutputBuffer.Length);
        }

        public static void DrawColorBuffer()
        {
            Array.Copy(GB.ColorBuffer, GB.OutputBuffer, GB.OutputBuffer.Length);
        }

        public static void DrawDepthBuffer()
        {
            for (var i = 0; i < GB.DepthBuffer.Length; i++)
            {
                var index = i * 4;
                GB.OutputBuffer[index + 0] = (byte) (GB.DepthBuffer[i].Depth % 256);
                GB.OutputBuffer[index + 1] = (byte) (GB.DepthBuffer[i].Depth % 256);
                GB.OutputBuffer[index + 2] = (byte) (GB.DepthBuffer[i].Depth % 256);
                GB.OutputBuffer[index + 3] = (byte) (GB.DepthBuffer[i].Depth % 256);
            }
        }

        public static void DrawDepthDifference()
        {
            for (var i = 0; i < GB.DepthBuffer.Length; i++)
            {
                var index = i * 4;
                var difference = (short) Math.Abs(GB.SavedBackgroundDepthBuffer[i].Depth - GB.DepthBuffer[i].Depth);

                if (difference > Configuration.DepthThreshold)
                {
                    GB.OutputBuffer[index + 0] = (byte) (difference % 256);
                    GB.OutputBuffer[index + 1] = (byte) (difference % 256);
                    GB.OutputBuffer[index + 2] = (byte) (difference % 256);
                    GB.OutputBuffer[index + 3] = (byte) (difference % 256);
                }
                else
                {
                    GB.OutputBuffer[index + 0] = 0;
                    GB.OutputBuffer[index + 1] = 0;
                    GB.OutputBuffer[index + 2] = 0;
                    GB.OutputBuffer[index + 3] = 0;
                }
            }
        }

        public static void DrawMappedDepthDifference()
        {
            for (var i = 0; i < GB.DepthBuffer.Length; i++)
            {
                var index = i * 4;

                var mappedDepthBufferIndex =
                    Utils.CoordinatesToIndex(GB.DepthCoordinates[i].X, GB.DepthCoordinates[i].Y);

                if (mappedDepthBufferIndex < 0 || mappedDepthBufferIndex >= GB.DepthBuffer.Length) continue;

                var difference = (short) Math.Abs(GB.SavedBackgroundDepthBuffer[mappedDepthBufferIndex].Depth -
                                                  GB.DepthBuffer[mappedDepthBufferIndex].Depth);

                if (difference > Configuration.DepthThreshold)
                {
                    GB.OutputBuffer[index + 0] = (byte) (difference % 256);
                    GB.OutputBuffer[index + 1] = (byte) (difference % 256);
                    GB.OutputBuffer[index + 2] = (byte) (difference % 256);
                    GB.OutputBuffer[index + 3] = (byte) (difference % 256);
                }
                else
                {
                    GB.OutputBuffer[index + 0] = GB.ColorBuffer[index + 0];
                    GB.OutputBuffer[index + 1] = GB.ColorBuffer[index + 1];
                    GB.OutputBuffer[index + 2] = GB.ColorBuffer[index + 2];
                    GB.OutputBuffer[index + 3] = 255;
                }
            }
        }
    }
}