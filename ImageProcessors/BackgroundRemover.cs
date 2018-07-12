using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics.ImageProcessors
{
    public class BackgroundRemover : ImageProcessor
    {
        private List<byte[]> savedColorFrames;
        private List<DepthImagePixel[]> savedDepthFrames;

        private DepthImagePixel[] backgroundDepthBuffer;
        public byte[] backgroundColorBuffer;

        private bool hasBackgroundBuffers = false;

        private ColorImagePoint[] colorCoordinates;

        public BackgroundRemover(int colorWidth, int colorHeight, int depthWidth, int depthHeight, byte[] colorBuffer, DepthImagePixel[] depthBuffer, ColorImagePoint[] colorCoordinates) : base(colorWidth, colorHeight, depthWidth, depthHeight, colorBuffer, depthBuffer)
        {

            savedColorFrames = new List<byte[]>();
            savedDepthFrames = new List<DepthImagePixel[]>();

            this.colorCoordinates = colorCoordinates;

        }

        public override void Process()
        {
            base.Process();

            if (!hasBackgroundBuffers)
                return;

            Console.WriteLine("Process");

            var t = 240 * colorWidth + 320;
            Console.WriteLine(colorCoordinates[t].X + " " + colorCoordinates[t].Y);

            for(var j = 0; j < depthBuffer.Length; j ++)
            {

                int i = ( (colorCoordinates[j].Y * depthWidth) + colorCoordinates[j].X ) * 4;

                if (i < 0 || i > colorBuffer.Length)
                    continue;

                int depthDifference = Math.Abs(depthBuffer[i / 4].Depth - backgroundDepthBuffer[i / 4].Depth);



                var x = (i / 4) % colorWidth;
                var y = ((i / 4) - x) / colorWidth;

                if (x == 320 && y == 240)
                {
                    Console.WriteLine("old " + backgroundDepthBuffer[i / 4].Depth + ", new: " + depthBuffer[i / 4].Depth);
                    //Console.WriteLine(rDiff + " " + gDiff + " " + bDiff);
                    //Console.WriteLine((int)colorBuffer[i+2] + " " + (int)colorBuffer[i+1] + " " + (int)colorBuffer[i]);
                    //Console.WriteLine((int)backgroundColorBuffer[i+2] + " " + (int)backgroundColorBuffer[i+1] + " " + (int)backgroundColorBuffer[i]);
                }

                /*
                int bDiff = Math.Abs((int)backgroundColorBuffer[i] - (int)colorBuffer[i]);
                int gDiff = Math.Abs((int)backgroundColorBuffer[i+1] - (int)colorBuffer[i+1]);
                int rDiff = Math.Abs((int)backgroundColorBuffer[i+2] - (int)colorBuffer[i+2]);

                maxDifference = Math.Max(Math.Max(bDiff, gDiff), rDiff);

                

                */

                /*
                colorBuffer[i] = (byte)( ( (float)depthBuffer[i/4].Depth / short.MaxValue ) * 255);
                colorBuffer[i + 1] = (byte)( ((float)depthBuffer[i / 4].Depth / short.MaxValue) * 255);
                colorBuffer[i + 2] = (byte)(((float)depthBuffer[i / 4].Depth / short.MaxValue) * 255);
                */

                if (depthDifference <= 50)
                {
                    outputBuffer[i] = (byte)(((float)depthBuffer[i / 4].Depth / short.MaxValue) * 255);
                    outputBuffer[i + 1] = (byte)(((float)depthBuffer[i / 4].Depth / short.MaxValue) * 255);
                    outputBuffer[i + 2] = (byte)(((float)depthBuffer[i / 4].Depth / short.MaxValue) * 255);
                }
                else
                {
                    outputBuffer[i] = colorBuffer[i];
                    outputBuffer[i+1] = colorBuffer[i+1];
                    outputBuffer[i+2] = colorBuffer[i+2];
                    //colorBuffer[i] = (byte) ( (colorBuffer[i] + (byte)(((float)depthBuffer[i / 4].Depth / short.MaxValue) * 255)) / 2);
                    //colorBuffer[i + 1] = (byte)((colorBuffer[i+1] + (byte)(((float)depthBuffer[i / 4].Depth / short.MaxValue) * 255)) / 2);
                    //colorBuffer[i + 2] = (byte)((colorBuffer[i+2] + (byte)(((float)depthBuffer[i / 4].Depth / short.MaxValue) * 255)) / 2);
                }
            }

            Array.Copy(outputBuffer, colorBuffer, colorBuffer.Length);

        }

        public bool CheckBackgroundBufferState()
        {
            return hasBackgroundBuffers;
        }

        public void SaveBackground()
        {
            Console.WriteLine("Save background");

            var newColorBuffer = new byte[colorBuffer.Length];
            var newDepthBuffer = new DepthImagePixel[depthBuffer.Length];

            Array.Copy(colorBuffer, newColorBuffer, colorBuffer.Length);
            Array.Copy(depthBuffer, newDepthBuffer, depthBuffer.Length);

            savedColorFrames.Add(newColorBuffer);
            savedDepthFrames.Add(newDepthBuffer);

            if (savedColorFrames.Count >= 15)
            {
                hasBackgroundBuffers = true;

                backgroundColorBuffer = new byte[colorBuffer.Length];
                backgroundDepthBuffer = new DepthImagePixel[depthBuffer.Length];

                for (var i = 0; i < colorBuffer.Length; i += 4)
                {
                    int bSum = 0;
                    int gSum = 0;
                    int rSum = 0;

                    for (var j = 0; j < savedColorFrames.Count; j++)
                    {
                        bSum += (int)savedColorFrames[j][i];
                        gSum += (int)savedColorFrames[j][i+1];
                        rSum += (int)savedColorFrames[j][i+2];
                    }

                    byte b = (byte)((float)bSum / savedColorFrames.Count);
                    byte g = (byte)((float)gSum / savedColorFrames.Count);
                    byte r = (byte)((float)rSum / savedColorFrames.Count);

                    backgroundColorBuffer[i] = b;
                    backgroundColorBuffer[i + 1] = g;
                    backgroundColorBuffer[i + 2] = r;

                    var x = (i / 4) % colorWidth;
                    var y = ((i / 4) - x) / colorWidth;

                    if (x == 320 && y == 240)
                    {
                        Console.WriteLine((int)backgroundColorBuffer[i + 2] + " " + (int)backgroundColorBuffer[i + 1] + " " + (int)backgroundColorBuffer[i]);
                    }

                    int depthSum = 0;
                    for (var j = 0; j < savedDepthFrames.Count; j++)
                    {
                        depthSum += savedDepthFrames[j][i/4].Depth;
                    }

                    backgroundDepthBuffer[i / 4].Depth = (short)(depthSum / savedDepthFrames.Count);
                }

                //Array.Copy(colorBuffer, backgroundColorBuffer, colorBuffer.Length);
                //Array.Copy(depthBuffer, backgroundDepthBuffer, depthBuffer.Length);
            }
            
        }

    }
}
