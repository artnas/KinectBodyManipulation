using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public static partial class Drawing
    {

        private static byte[] normalTestBuffer = new WriteableBitmap(new BitmapImage(new Uri("../../Images/test-normal.png", UriKind.Relative))).ToByteArray();

        public static void ProcessNormalDisplacement()
        {

            Array.Copy(outputBuffer, tempBuffer, outputBuffer.Length);

            float scale = 0.5f;

            Parallel.For(0, colorBuffer.Length / 4, i =>
            {

                int x = 0, y = 0;

                Utils.GetIndexCoordinates(i, ref x, ref y);

                x += (int)(((int)normalBuffer[i*2 + 2] - 128) * scale);
                y += (int)(((int)normalBuffer[i*2 + 1] - 128) * scale);

                i *= 4;

                if (x < 0 || x >= Configuration.width || y < 0 || y >= Configuration.height)
                {
                    return;
                }

                int index = Utils.GetIndexByCoordinates(x, y) * 4;

                outputBuffer[i] = tempBuffer[index];
                outputBuffer[i + 1] = tempBuffer[index + 1];
                outputBuffer[i + 2] = tempBuffer[index + 2];
            });

        }

    }
}
