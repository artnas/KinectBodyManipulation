using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectBodyModification
{
    public static partial class Drawing
    {

        public static void DrawHuman()
        {

            var b = Utils.GetBoneHash(JointType.Head, JointType.ShoulderCenter);

            Parallel.For(0, GlobalBuffers.colorBuffer.Length / 4, i =>
            {
                i *= 4;

                var limbPixel = GlobalBuffers.limbDataManager.limbData.pixelData[i / 4];

                if (limbPixel.humanIndex != -1)
                {

                    //if (limbPixel.boneHash == b) return;

                    GlobalBuffers.outputBuffer[i] = Utils.Interpolate(GlobalBuffers.savedBackgroundColorBuffer[i], GlobalBuffers.backgroundRemovedBuffer[i],
                        1f - GlobalBuffers.backgroundRemovedBuffer[i + 3] / 255f);
                    GlobalBuffers.outputBuffer[i + 1] = Utils.Interpolate(GlobalBuffers.savedBackgroundColorBuffer[i], GlobalBuffers.backgroundRemovedBuffer[i + 1],
                        1f - GlobalBuffers.backgroundRemovedBuffer[i + 3] / 255f);
                    GlobalBuffers.outputBuffer[i + 2] = Utils.Interpolate(GlobalBuffers.savedBackgroundColorBuffer[i], GlobalBuffers.backgroundRemovedBuffer[i + 2],
                        1f - GlobalBuffers.backgroundRemovedBuffer[i + 3] / 255f);

                }
            });
        }

    }
}
