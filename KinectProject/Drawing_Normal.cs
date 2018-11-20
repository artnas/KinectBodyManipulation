using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static partial class Drawing
    {

        private static byte[] normalTestBuffer = new WriteableBitmap(new BitmapImage(new Uri("../../Images/test-normal.png", UriKind.Relative))).ToByteArray();

        public static void DrawNormalMap()
        {

            float scale = 2;

            Parallel.For(0, GB.outputBuffer.Length / 4, i =>
            {

                i *= 4;

                GB.outputBuffer[i] = 255;
                GB.outputBuffer[i + 1] = (byte)(128 + (GB.normalBuffer[i / 2] - 128) * scale);
                GB.outputBuffer[i + 2] = (byte)(128 + (GB.normalBuffer[i / 2 + 1] - 128) * scale);

            });

        }

        public static void ProcessNormalDisplacement()
        {

            Array.Copy(GB.outputBuffer, tempBuffer, GB.outputBuffer.Length);

            Parallel.For(0, GB.outputBuffer.Length / 4, i =>
            {

                int x = 0, y = 0;

                Utils.GetIndexCoordinates(i, ref x, ref y);

                x += (GB.normalBuffer[i*2 + 0] - 128);
                y += (GB.normalBuffer[i*2 + 1] - 128);

                i *= 4;

                if (x < 0 || x >= Configuration.width || y < 0 || y >= Configuration.height)
                {
                    return;
                }

                int index = Utils.GetIndexByCoordinates(x, y) * 4;

                GB.outputBuffer[i] = tempBuffer[index];
                GB.outputBuffer[i + 1] = tempBuffer[index + 1];
                GB.outputBuffer[i + 2] = tempBuffer[index + 2];
            });

        }

        public static void ProcessNormalGlassDisplacement()
        {

            Array.Copy(GB.outputBuffer, tempBuffer, GB.outputBuffer.Length);

            float scale = 0.5f;

            Parallel.For(0, GB.colorBuffer.Length / 4, i =>
            {

                int x = 0, y = 0;

                Utils.GetIndexCoordinates(i, ref x, ref y);

                x += (int)(((int)normalTestBuffer[i * 2 + 2] - 128) * scale);
                y += (int)(((int)normalTestBuffer[i * 2 + 1] - 128) * scale);

                i *= 4;

                if (x < 0 || x >= Configuration.width || y < 0 || y >= Configuration.height)
                {
                    return;
                }

                int index = Utils.GetIndexByCoordinates(x, y) * 4;

                GB.outputBuffer[i] = tempBuffer[index];
                GB.outputBuffer[i + 1] = tempBuffer[index + 1];
                GB.outputBuffer[i + 2] = tempBuffer[index + 2];
            });

        }

    }
}
