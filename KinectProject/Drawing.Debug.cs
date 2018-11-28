using System.Windows.Media;
using Microsoft.Kinect;

using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static partial class Drawing
    {

        public static void DrawDebug()
        {
            Clear();

            // cialo
            if (Settings.Instance.DebugDrawSilhouette)
            {
                for (int i = 0; i < GB.colorBuffer.Length; i += 4)
                {
                    var limbPixel = GB.limbDataManager.limbData.allPixels[i / 4];

                    if (limbPixel.humanIndex != -1 && limbPixel.boneHash != -1)
                    {
                        var color = Configuration.GetBoneColor(limbPixel.boneHash);

                        GB.outputBuffer[i] = Utils.Interpolate(GB.outputBuffer[i], color.B, 0.7f);
                        GB.outputBuffer[i + 1] = Utils.Interpolate(GB.outputBuffer[i + 1], color.G, 0.7f);
                        GB.outputBuffer[i + 2] = Utils.Interpolate(GB.outputBuffer[i + 2], color.R, 0.7f);
                        GB.outputBuffer[i + 3] = 255;
                    }
                }
            }

            // szkielet (kosci)
            if (Settings.Instance.DebugDrawSkeleton)
            {
                foreach (var limbDataSkeleton in GB.limbDataManager.limbData.limbDataSkeletons)
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
                            DrawThickDot(GB.outputBuffer, ((int) point.X + (int) point.Y * Configuration.width) * 4, 2,
                                color);
                        }
                    }
                }
            }

            // szkielet (jointy)
            if (Settings.Instance.DebugDrawJoints)
            {
                foreach (var limbDataSkeleton in GB.limbDataManager.limbData.limbDataSkeletons)
                {
                    foreach (var bone in limbDataSkeleton.bones)
                    {
                        if (bone.points.Count == 0)
                            continue;

                        // piksele poczatkowego i koncowego jointa tej kosci
                        DrawThickDot(GB.outputBuffer,
                            ((int) bone.GetStartPoint().X + (int) bone.GetStartPoint().Y * Configuration.width) * 4, 3,
                            Colors.Yellow);
                        DrawThickDot(GB.outputBuffer,
                            ((int) bone.GetEndPoint().X + (int) bone.GetEndPoint().Y * Configuration.width) * 4, 3,
                            Colors.Yellow);
                    }
                }
            }

            // szkielet(debug)
            // for (int i = 0; i < GB.colorBuffer.Length; i += 4)
            // {
            //     var limbPixel = GB.limbDataManager.limbData.allPixels[i / 4];
            //
            //     if (limbPixel.debugDraw)
            //     {
            //
            //         GB.outputBuffer[i] = 255;
            //         GB.outputBuffer[i + 1] = 0;
            //         GB.outputBuffer[i + 2] = 0;
            //
            //     }
            // }

            if (Settings.Instance.DebugDrawOutline)
            {

                for (int i = 0; i < GB.colorBuffer.Length; i += 4)
                {
                    var limbPixel = GB.limbDataManager.limbData.allPixels[i / 4];

                    if (limbPixel.isContour)
                    {
                        GB.outputBuffer[i] = 0;
                        GB.outputBuffer[i + 1] = 255;
                        GB.outputBuffer[i + 2] = 0;
                        GB.outputBuffer[i + 3] = 255;
                    }
                }

                foreach (var i in GB.limbDataManager.usedContourIndices)
                {
                    var _i = i * 4;
                    if (_i < 0 || _i > GB.outputBuffer.Length) continue;
                    GB.outputBuffer[_i] = 0;
                    GB.outputBuffer[_i + 1] = 0;
                    GB.outputBuffer[_i + 2] = 255;
                    GB.outputBuffer[_i + 3] = 255;
                }

                foreach (var i in GB.limbDataManager.sortedContour)
                {
                    var _i = i * 4;
                    if (_i < 0 || _i > GB.outputBuffer.Length) continue;
                    GB.outputBuffer[_i] = 255;
                    GB.outputBuffer[_i + 1] = 0;
                    GB.outputBuffer[_i + 2] = 0;
                    GB.outputBuffer[_i + 3] = 255;
                }

            }

        }

    }
}
