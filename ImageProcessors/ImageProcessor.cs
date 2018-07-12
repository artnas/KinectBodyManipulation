using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public class ImageProcessor
    {

        protected byte[] colorBuffer;
        protected DepthImagePixel[] depthBuffer;

        protected int colorWidth, colorHeight, depthWidth, depthHeight;

        protected byte[] outputBuffer;

        public ImageProcessor(int colorWidth, int colorHeight, int depthWidth, int depthHeight, byte[] colorBuffer, DepthImagePixel[] depthBuffer)
        {
            this.colorWidth = colorWidth;
            this.colorHeight = colorHeight;
            this.depthWidth = depthWidth;
            this.depthHeight = depthHeight;

            this.colorBuffer = colorBuffer;
            this.depthBuffer = depthBuffer;

            this.outputBuffer = new byte[colorBuffer.Length];
        }

        public virtual void Process()
        {

        }

    }
}
