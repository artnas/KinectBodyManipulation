using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.CoordinateMappingBasics.Properties;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public static partial class Drawing
    {

        public static void DrawDebug(bool drawBody, bool drawBones, bool drawJoints, bool drawDebug)
        {

            // cialo
            if (drawBody)
            {
                for (int i = 0; i < colorBuffer.Length; i += 4)
                {
                    var limbPixel = limbDataManager.limbData.pixelData[i / 4];

                    if (limbPixel.humanIndex != -1 && limbPixel.boneHash != -1)
                    {

                        var color = Configuration.GetBoneColor(limbPixel.boneHash);

                        colorBuffer[i] = Utils.Interpolate(colorBuffer[i], color.B, 0.7f);
                        colorBuffer[i + 1] = Utils.Interpolate(colorBuffer[i + 1], color.G, 0.7f);
                        colorBuffer[i + 2] = Utils.Interpolate(colorBuffer[i + 2], color.R, 0.7f);

                    }
                }
            }

            // szkielet (kosci)
            if (drawBones)
            {
                foreach (var limbDataSkeleton in limbDataManager.limbData.limbDataSkeletons)
                {
                    foreach (var bone in limbDataSkeleton.bones)
                    {
                        if (bone.points.Count == 0)
                            continue;

                        var color = Colors.White;

                        if (bone.startJoint.TrackingState == JointTrackingState.Inferred ||
                            bone.endJoint.TrackingState == JointTrackingState.Inferred)
                        {
                            color = Colors.Red;
                        }
                        else if (bone.startJoint.TrackingState == JointTrackingState.NotTracked ||
                                 bone.endJoint.TrackingState == JointTrackingState.NotTracked)
                        {
                            color = Colors.Black;
                        }

                        // piksele kosci
                        foreach (var point in bone.points)
                        {
                            DrawThickDot(colorBuffer, ((int) point.X + (int) point.Y * Configuration.width) * 4, 2,
                                color);
                        }
                    }
                }
            }

            // szkielet (jointy)
            if (drawJoints)
            {
                foreach (var limbDataSkeleton in limbDataManager.limbData.limbDataSkeletons)
                {
                    foreach (var bone in limbDataSkeleton.bones)
                    {
                        if (bone.points.Count == 0)
                            continue;

                        // piksele poczatkowego i koncowego jointa tej kosci
                        DrawThickDot(colorBuffer,
                            ((int) bone.GetStartPoint().X + (int) bone.GetStartPoint().Y * Configuration.width) * 4, 3,
                            Colors.Yellow);
                        DrawThickDot(colorBuffer,
                            ((int) bone.GetEndPoint().X + (int) bone.GetEndPoint().Y * Configuration.width) * 4, 3,
                            Colors.Yellow);
                    }
                }
            }

            // szkielet(debug)
            if (drawDebug)
            {
                for (int i = 0; i < colorBuffer.Length; i += 4)
                {
                    var limbPixel = limbDataManager.limbData.pixelData[i / 4];

                    if (limbPixel.debugDraw)
                    {

                        colorBuffer[i] = 255;
                        colorBuffer[i + 1] = 0;
                        colorBuffer[i + 2] = 0;

                    }
                }
            }

        }

    }
}
