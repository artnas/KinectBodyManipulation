using System.Threading.Tasks;
using Microsoft.Kinect;

using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static partial class Drawing
    {

        public static void DrawHuman()
        {

            var b = Utils.GetBoneHash(JointType.Head, JointType.ShoulderCenter);

            Parallel.For(0, GB.colorBuffer.Length / 4, i =>
            {
                i *= 4;

                var limbPixel = GB.limbDataManager.limbData.pixelData[i / 4];

                if (limbPixel.humanIndex != -1)
                {

                    //if (limbPixel.boneHash == b) return;

                    GB.outputBuffer[i] = Utils.Interpolate(GB.savedBackgroundColorBuffer[i], GB.backgroundRemovedBuffer[i],
                        1f - GB.backgroundRemovedBuffer[i + 3] / 255f);
                    GB.outputBuffer[i + 1] = Utils.Interpolate(GB.savedBackgroundColorBuffer[i], GB.backgroundRemovedBuffer[i + 1],
                        1f - GB.backgroundRemovedBuffer[i + 3] / 255f);
                    GB.outputBuffer[i + 2] = Utils.Interpolate(GB.savedBackgroundColorBuffer[i], GB.backgroundRemovedBuffer[i + 2],
                        1f - GB.backgroundRemovedBuffer[i + 3] / 255f);

                }
            });
        }

    }
}
