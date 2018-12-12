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
                for (var i = 0; i < GB.ColorBuffer.Length; i += 4)
                {
                    var limbPixel = GB.LimbDataManager.LimbData.AllPixels[i / 4];

                    if (limbPixel.HumanIndex != -1 && limbPixel.BoneHash != -1)
                    {
                        var color = Configuration.GetBoneColor(limbPixel.BoneHash);

                        GB.OutputBuffer[i] = Utils.Interpolate(GB.OutputBuffer[i], color.B, 0.7f);
                        GB.OutputBuffer[i + 1] = Utils.Interpolate(GB.OutputBuffer[i + 1], color.G, 0.7f);
                        GB.OutputBuffer[i + 2] = Utils.Interpolate(GB.OutputBuffer[i + 2], color.R, 0.7f);
                        GB.OutputBuffer[i + 3] = 255;
                    }
                }

            // szkielet (kosci)
            if (Settings.Instance.DebugDrawSkeleton)
                foreach (var bone in GB.LimbDataManager.LimbData.LimbDataSkeleton.Bones)
                {
                    if (bone.Points.Count == 0)
                        continue;

                    var color = Colors.White;

                    if (bone.StartJoint.TrackingState == JointTrackingState.Inferred ||
                        bone.EndJoint.TrackingState == JointTrackingState.Inferred)
                        color = Colors.Red;
                    else if (bone.StartJoint.TrackingState == JointTrackingState.NotTracked ||
                             bone.EndJoint.TrackingState == JointTrackingState.NotTracked)
                        color = Colors.Black;

                    // piksele kosci
                    foreach (var point in bone.Points)
                        DrawThickDot(GB.OutputBuffer, ((int) point.X + (int) point.Y * Configuration.Width) * 4, 2,
                            color);
                }

            // szkielet (jointy)
            if (Settings.Instance.DebugDrawJoints)
                foreach (var bone in GB.LimbDataManager.LimbData.LimbDataSkeleton.Bones)
                {
                    if (bone.Points.Count == 0)
                        continue;

                    // piksele poczatkowego i koncowego jointa tej kosci
                    DrawThickDot(GB.OutputBuffer,
                        ((int) bone.GetStartPoint().X + (int) bone.GetStartPoint().Y * Configuration.Width) * 4, 3,
                        Colors.Yellow);
                    DrawThickDot(GB.OutputBuffer,
                        ((int) bone.GetEndPoint().X + (int) bone.GetEndPoint().Y * Configuration.Width) * 4, 3,
                        Colors.Yellow);
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
                for (var i = 0; i < GB.ColorBuffer.Length; i += 4)
                {
                    var limbPixel = GB.LimbDataManager.LimbData.AllPixels[i / 4];

                    if (limbPixel.IsContour)
                    {
                        GB.OutputBuffer[i] = 0;
                        GB.OutputBuffer[i + 1] = 255;
                        GB.OutputBuffer[i + 2] = 0;
                        GB.OutputBuffer[i + 3] = 255;
                    }
                }

                foreach (var index in GB.LimbDataManager.UsedContourIndices)
                {
                    var i = index * 4;
                    if (i < 0 || i > GB.OutputBuffer.Length) continue;
                    GB.OutputBuffer[i] = 0;
                    GB.OutputBuffer[i + 1] = 0;
                    GB.OutputBuffer[i + 2] = 255;
                    GB.OutputBuffer[i + 3] = 255;
                }

                foreach (var index in GB.LimbDataManager.SortedContour)
                {
                    var i = index * 4;
                    if (i < 0 || i > GB.OutputBuffer.Length) continue;
                    GB.OutputBuffer[i] = 255;
                    GB.OutputBuffer[i + 1] = 0;
                    GB.OutputBuffer[i + 2] = 0;
                    GB.OutputBuffer[i + 3] = 255;
                }
            }
        }
    }
}