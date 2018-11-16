using System;
using System.Windows.Media;
using Microsoft.Kinect;

namespace KinectBodyModification
{
    public static partial class Drawing
    {
        // private static DepthImagePixel[] depthBuffer;
        //
        // private static byte[] colorBuffer;
        // private static byte[] outputBuffer;
        // private static byte[] backgroundRemovedBuffer;
        // private static byte[] normalBuffer;
        //
        // private static byte[] savedBackgroundColorBuffer;
        // private static DepthImagePixel[] savedBackgroundDepthBuffer;
        //
        // private static ColorImagePoint[] colorCoordinates;
        // private static DepthImagePoint[] depthCoordinates;
        //
        // private static LimbDataManager limbDataManager;

        private static readonly byte[] tempBuffer = new byte[Configuration.size * 4];

        // public static void SetBuffers(
        //     DepthImagePixel[] depthBuffer,
        //     byte[] colorBuffer,
        //     byte[] outputBuffer,
        //     byte[] backgroundRemovedBuffer,
        //     LimbDataManager limbDataManager,
        //     byte[] savedBackgroundColorBuffer,
        //     DepthImagePixel[] savedBackgroundDepthBuffer,
        //     byte[] normalBuffer,
        //     ColorImagePoint[] colorCoordinates,
        //     DepthImagePoint[] depthCoordinates)
        // {
        //     Drawing.depthBuffer = depthBuffer;
        //     Drawing.colorBuffer = colorBuffer;
        //     Drawing.outputBuffer = outputBuffer;
        //     Drawing.backgroundRemovedBuffer = backgroundRemovedBuffer;
        //     Drawing.limbDataManager = limbDataManager;
        //     Drawing.savedBackgroundColorBuffer = savedBackgroundColorBuffer;
        //     Drawing.savedBackgroundDepthBuffer = savedBackgroundDepthBuffer;
        //     Drawing.normalBuffer = normalBuffer;
        //     Drawing.colorCoordinates = colorCoordinates;
        //     Drawing.depthCoordinates = depthCoordinates;
        // }

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
            }
        }

        public static void DrawBackground()
        {
            Array.Copy(GlobalBuffers.savedBackgroundColorBuffer, GlobalBuffers.outputBuffer, GlobalBuffers.outputBuffer.Length);
        }

        public static void DrawColorBuffer()
        {
            Array.Copy(GlobalBuffers.colorBuffer, GlobalBuffers.outputBuffer, GlobalBuffers.outputBuffer.Length);
        }

        public static void DrawDepthBuffer()
        {
            for (var i = 0; i < GlobalBuffers.depthBuffer.Length; i++)
            {
                var index = i * 4;
                GlobalBuffers.outputBuffer[index + 0] = (byte) (GlobalBuffers.depthBuffer[i].Depth % 256);
                GlobalBuffers.outputBuffer[index + 1] = (byte) (GlobalBuffers.depthBuffer[i].Depth % 256);
                GlobalBuffers.outputBuffer[index + 2] = (byte) (GlobalBuffers.depthBuffer[i].Depth % 256);
                GlobalBuffers.outputBuffer[index + 3] = (byte) (GlobalBuffers.depthBuffer[i].Depth % 256);
            }
        }

        public static void DrawDepthDifference()
        {
            for (var i = 0; i < GlobalBuffers.depthBuffer.Length; i++)
            {
                var index = i * 4;
                var difference = (short) Math.Abs(GlobalBuffers.savedBackgroundDepthBuffer[i].Depth - GlobalBuffers.depthBuffer[i].Depth);

                if (difference > Configuration.depthThreshold)
                {
                    GlobalBuffers.outputBuffer[index + 0] = (byte) (difference % 256);
                    GlobalBuffers.outputBuffer[index + 1] = (byte) (difference % 256);
                    GlobalBuffers.outputBuffer[index + 2] = (byte) (difference % 256);
                    GlobalBuffers.outputBuffer[index + 3] = (byte) (difference % 256);
                }
                else
                {
                    GlobalBuffers.outputBuffer[index + 0] = 0;
                    GlobalBuffers.outputBuffer[index + 1] = 0;
                    GlobalBuffers.outputBuffer[index + 2] = 0;
                    GlobalBuffers.outputBuffer[index + 3] = 0;
                }
            }
        }

        public static void DrawMappedDepthDifference()
        {
            for (var i = 0; i < GlobalBuffers.depthBuffer.Length; i++)
            {
                var index = i * 4;

                var mappedDepthBufferIndex =
                    Utils.GetIndexByCoordinates(GlobalBuffers.depthCoordinates[i].X, GlobalBuffers.depthCoordinates[i].Y);

                if (mappedDepthBufferIndex < 0 || mappedDepthBufferIndex >= GlobalBuffers.depthBuffer.Length) continue;

                var difference = (short) Math.Abs(GlobalBuffers.savedBackgroundDepthBuffer[mappedDepthBufferIndex].Depth -
                                                  GlobalBuffers.depthBuffer[mappedDepthBufferIndex].Depth);

                if (difference > Configuration.depthThreshold)
                {
                    GlobalBuffers.outputBuffer[index + 0] = (byte) (difference % 256);
                    GlobalBuffers.outputBuffer[index + 1] = (byte) (difference % 256);
                    GlobalBuffers.outputBuffer[index + 2] = (byte) (difference % 256);
                    GlobalBuffers.outputBuffer[index + 3] = (byte) (difference % 256);
                }
                else
                {
                    GlobalBuffers.outputBuffer[index + 0] = GlobalBuffers.colorBuffer[index + 0];
                    GlobalBuffers.outputBuffer[index + 1] = GlobalBuffers.colorBuffer[index + 1];
                    GlobalBuffers.outputBuffer[index + 2] = GlobalBuffers.colorBuffer[index + 2];
                    GlobalBuffers.outputBuffer[index + 3] = 255;
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