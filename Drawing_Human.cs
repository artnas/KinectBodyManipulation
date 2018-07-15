using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public static partial class Drawing
    {

        public static void DrawHuman()
        {

            var b = Utils.GetBoneHash(JointType.Head, JointType.ShoulderCenter);

            Parallel.For(0, colorBuffer.Length / 4, i =>
            {
                i *= 4;

                var limbPixel = limbDataManager.limbData.pixelData[i / 4];

                if (limbPixel.humanIndex != -1)
                {

                    //if (limbPixel.boneHash == b) return;

                    outputBuffer[i] = Utils.Interpolate(savedBackgroundColorBuffer[i], colorBuffer[i],
                        savedBackgroundColorBuffer[i + 3] / 255f);
                    outputBuffer[i + 1] = Utils.Interpolate(savedBackgroundColorBuffer[i], colorBuffer[i + 1],
                        savedBackgroundColorBuffer[i + 3] / 255f);
                    outputBuffer[i + 2] = Utils.Interpolate(savedBackgroundColorBuffer[i], colorBuffer[i + 2],
                        savedBackgroundColorBuffer[i + 3] / 255f);

                }
            });
        }

    }
}
