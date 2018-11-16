using System.Windows.Media;
using Microsoft.Kinect;

namespace KinectBodyModification
{
    public static partial class Drawing
    {

        public static void DrawDebug()
        {

            // cialo
            if (Settings.Instance.DebugDrawSilhouette)
            {
                for (int i = 0; i < GlobalBuffers.colorBuffer.Length; i += 4)
                {
                    var limbPixel = GlobalBuffers.limbDataManager.limbData.pixelData[i / 4];

                    if (limbPixel.humanIndex != -1 && limbPixel.boneHash != -1)
                    {

                        var color = Configuration.GetBoneColor(limbPixel.boneHash);

                        GlobalBuffers.outputBuffer[i] = Utils.Interpolate(GlobalBuffers.outputBuffer[i], color.B, 0.7f);
                        GlobalBuffers.outputBuffer[i + 1] = Utils.Interpolate(GlobalBuffers.outputBuffer[i + 1], color.G, 0.7f);
                        GlobalBuffers.outputBuffer[i + 2] = Utils.Interpolate(GlobalBuffers.outputBuffer[i + 2], color.R, 0.7f);

                    }
                }
            }

            // szkielet (kosci)
            if (Settings.Instance.DebugDrawSkeleton)
            {
                foreach (var limbDataSkeleton in GlobalBuffers.limbDataManager.limbData.limbDataSkeletons)
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
                            DrawThickDot(GlobalBuffers.outputBuffer, ((int) point.X + (int) point.Y * Configuration.width) * 4, 2,
                                color);
                        }
                    }
                }
            }

            // szkielet (jointy)
            if (Settings.Instance.DebugDrawJoints)
            {
                foreach (var limbDataSkeleton in GlobalBuffers.limbDataManager.limbData.limbDataSkeletons)
                {
                    foreach (var bone in limbDataSkeleton.bones)
                    {
                        if (bone.points.Count == 0)
                            continue;

                        // piksele poczatkowego i koncowego jointa tej kosci
                        DrawThickDot(GlobalBuffers.outputBuffer,
                            ((int) bone.GetStartPoint().X + (int) bone.GetStartPoint().Y * Configuration.width) * 4, 3,
                            Colors.Yellow);
                        DrawThickDot(GlobalBuffers.outputBuffer,
                            ((int) bone.GetEndPoint().X + (int) bone.GetEndPoint().Y * Configuration.width) * 4, 3,
                            Colors.Yellow);
                    }
                }
            }

            // szkielet(debug)
            // if (false)
            // {
            //     for (int i = 0; i < colorBuffer.Length; i += 4)
            //     {
            //         var limbPixel = limbDataManager.limbData.pixelData[i / 4];
            //
            //         if (limbPixel.debugDraw)
            //         {
            //
            //             outputBuffer[i] = 255;
            //             outputBuffer[i + 1] = 0;
            //             outputBuffer[i + 2] = 0;
            //
            //         }
            //     }
            // }

        }

    }
}
