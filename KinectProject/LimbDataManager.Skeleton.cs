using System;
using System.Collections.Generic;
using Microsoft.Kinect;
using OpenTK;
using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public partial class LimbDataManager
    {
        private void UpdateSkeletons(Skeleton[] skeletons)
        {
            Skeleton trackedSkeleton = null;

            foreach (var skeleton in skeletons)
                if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                {
                    trackedSkeleton = skeleton;
                    break;
                }

            if (trackedSkeleton != null)
            {
                if (LimbData.LimbDataSkeleton == null)
                {
                    GB.BackgroundRemovedColorStream.SetTrackedPlayer(trackedSkeleton.TrackingId);

                    LimbData.LimbDataSkeleton = new LimbDataSkeleton(trackedSkeleton);
                }
                else
                {
                    if (LimbData.LimbDataSkeleton.Skeleton != trackedSkeleton)
                        GB.BackgroundRemovedColorStream.SetTrackedPlayer(trackedSkeleton.TrackingId);

                    LimbData.LimbDataSkeleton.Update(trackedSkeleton);
                }

                foreach (JointPair jointPair in Utils.SkeletonIterator(LimbData.LimbDataSkeleton.Skeleton))
                    AssignPixelsBetweenJoints(LimbData.LimbDataSkeleton, jointPair, _floodFillPixelsQueue);
            }
        }

        private void AssignPixelsBetweenJoints(LimbDataSkeleton limbDataSkeleton, JointPair jointPair,
            Queue<int> pixelsQueue)
        {
            AssignPixelsBetweenJoints(limbDataSkeleton, jointPair.A, jointPair.B, pixelsQueue);
        }

        private void AssignPixelsBetweenJoints(LimbDataSkeleton limbDataSkeleton, Joint a, Joint b,
            Queue<int> pixelsQueue)
        {
            if (a.TrackingState == JointTrackingState.NotTracked ||
                b.TrackingState == JointTrackingState.NotTracked) return;

            var bone = limbDataSkeleton.GetBoneByJointPair(a, b);

            var aPosition = Utils.SkeletonPointToScreen(_sensor, a.Position);
            var bPosition = Utils.SkeletonPointToScreen(_sensor, b.Position);

            bone.StartPoint = aPosition;
            bone.EndPoint = bPosition;

            var points = bone.Points;
            Utils.GetPointsBetween(points, aPosition, bPosition, Configuration.Width, Configuration.Height);

            if (points.Count > 0) ProcessBone(limbDataSkeleton, bone, pixelsQueue);
        }

        private void ProcessBone(LimbDataSkeleton limbDataSkeleton, LimbDataBone bone, Queue<int> pixelsQueue)
        {
            var perpendicularVector = Utils.GetPerpendicularVector(bone.GetStartPoint(), bone.GetEndPoint());

            BoneConfiguration boneConfiguration = null;

            if (Configuration.BoneConfigurationsDictionary.ContainsKey(bone.JointTypePair))
                boneConfiguration = Configuration.BoneConfigurationsDictionary[bone.JointTypePair];

            ProcessBoneJoint(limbDataSkeleton, bone, pixelsQueue, perpendicularVector, true, boneConfiguration);
        }

        private void ProcessBoneJoint(LimbDataSkeleton limbDataSkeleton, LimbDataBone bone, Queue<int> pixelsQueue,
            Vector3 perpendicularVector, bool isStart, BoneConfiguration boneConfiguration)
        {
            if (bone.Points.Count == 0)
                return;

            var startIndex = (int) Math.Floor(bone.Points.Count * boneConfiguration.StartOffset) + 1;
            if (startIndex >= bone.Points.Count) startIndex = bone.Points.Count - 1;

            var endIndex = (int) Math.Ceiling(bone.Points.Count * (1f - boneConfiguration.EndOffset)) - 1;
            if (endIndex >= bone.Points.Count) endIndex = bone.Points.Count - 1;

            var startWidth = boneConfiguration.StartWidth;
            var endWidth = boneConfiguration.EndWidth;

            var length = endIndex - startIndex;

            for (var k = startIndex; k < endIndex; k += 3)
            {
                var point = bone.Points[k];

                var isOk = true;

                var progress = (float) (k - startIndex) / length;

                var width = Utils.Interpolate(startWidth, endWidth, progress);

                for (var i = 1; i < width; i++)
                {
                    if (!isOk)
                        break;

                    {
                        var bufferIndex = Utils.CoordinatesToIndex((int) point.X, (int) point.Y);

                        if (bufferIndex > 0 && bufferIndex < LimbData.AllPixels.Length)
                        {
                            var pixel = LimbData.AllPixels[bufferIndex];
                            pixel.HumanIndex = (sbyte) limbDataSkeleton.Skeleton.TrackingId;
                            pixel.BoneHash = bone.BoneHash;
                            pixel.IsBone = true;

                            LimbData.ActivePixels.Add(bufferIndex);

                            if (k == startIndex)
                                pixel.IsJoint = true;

                            pixelsQueue.Enqueue(bufferIndex);
                        }
                    }

                    for (var j = -1; j <= 1; j += 2)
                    {
                        var offsetPoint = point + perpendicularVector * i * j;

                        var x = (int) offsetPoint.X;
                        var y = (int) offsetPoint.Y;

                        if (x < 0 || x >= Configuration.Width || y < 0 || y >= Configuration.Height) continue;

                        var colorBufferIndex = (x + y * Configuration.Width) * 4;

                        if (GB.BackgroundRemovedBuffer[colorBufferIndex + 3] < Configuration.AlphaThreshold)
                        {
                            isOk = false;
                            break;
                        }

                        var limbDataPixelIndex = colorBufferIndex / 4;

                        var pixel = LimbData.AllPixels[limbDataPixelIndex];

                        if (pixel.HumanIndex != -1)
                            if (pixel.HumanIndex != limbDataSkeleton.Skeleton.TrackingId)
                            {
                                isOk = false;
                                break;
                            }

                        // test glebii

                        if (GB.DepthBuffer[limbDataPixelIndex].Depth != 0)
                        {
                            var difference = Math.Abs(GB.DepthBuffer[limbDataPixelIndex].Depth -
                                                      GB.SavedBackgroundDepthBuffer[limbDataPixelIndex]
                                                          .Depth);
                            if (difference < Configuration.DepthThreshold)
                            {
                                isOk = false;
                                break;
                            }
                        }

                        pixel.HumanIndex = (sbyte) limbDataSkeleton.Skeleton.TrackingId;
                        pixel.BoneHash = bone.BoneHash;
                        pixel.DebugDraw = true;

                        LimbData.ActivePixels.Add(limbDataPixelIndex);

                        pixelsQueue.Enqueue(limbDataPixelIndex);
                    }
                }
            }

            // usun punkty przed indeksem
            // if (startIndex != 0)
            // {
            //     bone.points.RemoveRange(0, startIndex);
            //     startIndex = 0;
            // }

            for (var i = 0; i < startIndex; i++)
            {
                var index = (int) (bone.Points[i].X + bone.Points[i].Y * Configuration.Width);
                if (index > 0 && index < LimbData.AllPixels.Length) LimbData.AllPixels[index].Clear();
            }

            for (var i = endIndex; i < bone.Points.Count; i++)
            {
                var index = (int) (bone.Points[i].X + bone.Points[i].Y * Configuration.Width);
                if (index > 0 && index < LimbData.AllPixels.Length) LimbData.AllPixels[index].Clear();
            }

            bone.Points = bone.Points.GetRange(startIndex, endIndex - startIndex);
        }

        private void FloodFillSkeletonPixels()
        {
            while (_floodFillPixelsQueue.Count > 0)
            {
                var index = _floodFillPixelsQueue.Dequeue();

                if (!LimbData.ActivePixels.Contains(index)) LimbData.ActivePixels.Add(index);

                if (index < 0 || index >= LimbData.AllPixels.Length)
                    continue;

                var alpha = GB.BackgroundRemovedBuffer[index * 4 + 3];
                var lpd = LimbData.AllPixels[index];

                if (lpd.HumanIndex == -1)
                    continue;

                if (alpha < Configuration.AlphaThreshold)
                    continue;

                var mappedDepthBufferIndex =
                    Utils.CoordinatesToIndex(GB.DepthCoordinates[index].X,
                        GB.DepthCoordinates[index].Y);

                if (mappedDepthBufferIndex >= 0 && mappedDepthBufferIndex < GB.DepthBuffer.Length)
                    if (GB.DepthBuffer[mappedDepthBufferIndex].Depth != 0)
                    {
                        var difference = Math.Abs(GB.DepthBuffer[mappedDepthBufferIndex].Depth -
                                                  GB.SavedBackgroundDepthBuffer[mappedDepthBufferIndex]
                                                      .Depth);
                        if (difference < Configuration.DepthThreshold) continue;
                    }

                // 8 stron
                for (var j = 0; j < 8; j++)
                {
                    var xOffset = Utils.OrdinalDirections[j, 0];
                    var yOffset = Utils.OrdinalDirections[j, 1];

                    var offset = xOffset + yOffset * Configuration.Width;

                    // pomijanie wylewania poza krawedzie obrazka
                    if (
                        index % Configuration.Width == 0 && xOffset == -1 ||
                        (index + 1) % Configuration.Width == 0 && xOffset == 1 ||
                        index / Configuration.Width == 0 && yOffset == 1
                        // (limbData.allPixels.Length - index - xOffset <= Configuration.width && yOffset == -1)
                    )
                        continue;

                    var offsetIndex = index + offset;

                    if (offsetIndex < 0 || offsetIndex >= LimbData.AllPixels.Length)
                        continue;

                    if (LimbData.AllPixels[offsetIndex].HumanIndex == -1)
                    {
                        LimbData.AllPixels[offsetIndex].HumanIndex = lpd.HumanIndex;
                        LimbData.AllPixels[offsetIndex].BoneHash = lpd.BoneHash;

                        _floodFillPixelsQueue.Enqueue(offsetIndex);
                    }
                }
            }
        }
    }
}