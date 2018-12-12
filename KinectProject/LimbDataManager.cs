using System;
using System.Collections.Generic;
using System.Linq;
using KBMGraphics;
using Microsoft.Kinect;
using OpenTK;

namespace KinectBodyModification
{
    public class LimbDataManager
    {
        private readonly OutlineTriangulator outlineTriangulator;

        private readonly KinectSensor sensor;
        private Vector2 lastContourPoint = new Vector2(0, 0);

        public LimbData limbData;

        public HashSet<int> sortedContour = new HashSet<int>();
        public HashSet<int> usedContourIndices = new HashSet<int>();

        public LimbDataManager(KinectSensor sensor)
        {
            this.sensor = sensor;
            outlineTriangulator = new OutlineTriangulator(Configuration.width, Configuration.height);

            limbData = new LimbData();
        }

        private int getBufferIndex(int x, int y)
        {
            return y * Configuration.width + x;
        }

        private void ClearBuffer()
        {
            limbData.activePixels.Clear();
            limbData.contourPixels.Clear();

            foreach (var lpd in limbData.allPixels) lpd.Clear();
        }

        public void Update(Skeleton[] skeletons)
        {
            ClearBuffer();

            var pixelsQueue = new Queue<int>();

            foreach (var skeleton in skeletons)
            {
                LimbDataSkeleton foundSkeleton = null;

                foreach (var limbDataSkeleton in limbData.limbDataSkeletons)
                    if (limbDataSkeleton.skeleton == skeleton)
                    {
                        foundSkeleton = limbDataSkeleton;
                        break;
                    }

                if (foundSkeleton == null)
                {
                    foundSkeleton = new LimbDataSkeleton(skeleton);
                    limbData.limbDataSkeletons.Add(foundSkeleton);
                }

                foreach (JointPair jointPair in Utils.SkeletonIterator(skeleton))
                    AssignPixelsBetweenJoints(foundSkeleton, jointPair, pixelsQueue);
            }

            FloodFill(pixelsQueue);

            foreach (var index in limbData.activePixels)
            {
                int pointX = 0, pointY = 0;
                Utils.IndexToCoordinates(index, ref pointX, ref pointY);

                var isContour = IsContour(pointX, pointY);

                if (isContour)
                {
                    limbData.allPixels[index].isContour = true;
                    limbData.contourPixels.Add(index);
                }
            }

            limbData.mesh.Update(outlineTriangulator.GetMesh(GetSortedContour()));
        }

        private void AssignPixelsBetweenJoints(LimbDataSkeleton limbDataSkeleton, JointPair jointPair,
            Queue<int> pixelsQueue)
        {
            AssignPixelsBetweenJoints(limbDataSkeleton, jointPair.a, jointPair.b, pixelsQueue);
        }

        private void AssignPixelsBetweenJoints(LimbDataSkeleton limbDataSkeleton, Joint a, Joint b,
            Queue<int> pixelsQueue)
        {
            if (a.TrackingState == JointTrackingState.NotTracked ||
                b.TrackingState == JointTrackingState.NotTracked) return;

            var bone = limbDataSkeleton.GetBoneByJointPair(a, b);

            var aPosition = Utils.SkeletonPointToScreen(sensor, a.Position);
            var bPosition = Utils.SkeletonPointToScreen(sensor, b.Position);

            bone.startPoint = aPosition;
            bone.endPoint = bPosition;

            var points = bone.points;
            Utils.GetPointsBetween(points, aPosition, bPosition, Configuration.width, Configuration.height);

            if (points.Count > 0) ProcessBone(limbDataSkeleton, bone, pixelsQueue);
        }

        private void ProcessBone(LimbDataSkeleton limbDataSkeleton, LimbDataBone bone, Queue<int> pixelsQueue)
        {
            var perpendicularVector = Utils.GetPerpendicularVector(bone.GetStartPoint(), bone.GetEndPoint());

            BoneConfiguration boneConfiguration = null;

            if (Configuration.boneConfigurationsDictionary.ContainsKey(bone.jointTypePair))
                boneConfiguration = Configuration.boneConfigurationsDictionary[bone.jointTypePair];

            ProcessBoneJoint(limbDataSkeleton, bone, pixelsQueue, perpendicularVector, true, boneConfiguration);
        }

        private void ProcessBoneJoint(LimbDataSkeleton limbDataSkeleton, LimbDataBone bone, Queue<int> pixelsQueue,
            Vector3 perpendicularVector, bool isStart, BoneConfiguration boneConfiguration)
        {
            if (bone.points.Count == 0)
                return;

            var startIndex = (int) Math.Floor(bone.points.Count * boneConfiguration.startOffset) + 1;
            if (startIndex >= bone.points.Count) startIndex = bone.points.Count - 1;

            var endIndex = (int) Math.Ceiling(bone.points.Count * (1f - boneConfiguration.endOffset)) - 1;
            if (endIndex >= bone.points.Count) endIndex = bone.points.Count - 1;

            var startWidth = boneConfiguration.startWidth;
            var endWidth = boneConfiguration.endWidth;

            var length = endIndex - startIndex;

            for (var k = startIndex; k < endIndex; k += 3)
            {
                var point = bone.points[k];

                var isOk = true;

                var progress = (float) (k - startIndex) / length;

                var width = Utils.Interpolate(startWidth, endWidth, progress);

                for (var i = 1; i < width; i++)
                {
                    if (!isOk)
                        break;

                    {
                        var bufferIndex = getBufferIndex((int) point.X, (int) point.Y);

                        if (bufferIndex > 0 && bufferIndex < limbData.allPixels.Length)
                        {
                            var pixel = limbData.allPixels[bufferIndex];
                            pixel.humanIndex = (sbyte) limbDataSkeleton.skeleton.TrackingId;
                            pixel.boneHash = bone.boneHash;
                            pixel.isBone = true;

                            limbData.activePixels.Add(bufferIndex);

                            if (k == startIndex)
                                pixel.isJoint = true;

                            pixelsQueue.Enqueue(bufferIndex);
                        }
                    }

                    for (var j = -1; j <= 1; j += 2)
                    {
                        var offsetPoint = point + perpendicularVector * i * j;

                        var x = (int) offsetPoint.X;
                        var y = (int) offsetPoint.Y;

                        if (x < 0 || x >= Configuration.width || y < 0 || y >= Configuration.height) continue;

                        var colorBufferIndex = (x + y * Configuration.width) * 4;

                        if (GlobalBuffers.backgroundRemovedBuffer[colorBufferIndex + 3] < Configuration.alphaThreshold)
                        {
                            isOk = false;
                            break;
                        }

                        var limbDataPixelIndex = colorBufferIndex / 4;

                        var pixel = limbData.allPixels[limbDataPixelIndex];

                        if (pixel.humanIndex != -1)
                            if (pixel.humanIndex != limbDataSkeleton.skeleton.TrackingId)
                            {
                                isOk = false;
                                break;
                            }

                        // test glebii

                        if (GlobalBuffers.depthBuffer[limbDataPixelIndex].Depth != 0)
                        {
                            var difference = Math.Abs(GlobalBuffers.depthBuffer[limbDataPixelIndex].Depth -
                                                      GlobalBuffers.savedBackgroundDepthBuffer[limbDataPixelIndex]
                                                          .Depth);
                            if (difference < Configuration.depthThreshold)
                            {
                                isOk = false;
                                break;
                            }
                        }

                        pixel.humanIndex = (sbyte) limbDataSkeleton.skeleton.TrackingId;
                        pixel.boneHash = bone.boneHash;
                        pixel.debugDraw = true;

                        limbData.activePixels.Add(limbDataPixelIndex);

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
                var index = (int) (bone.points[i].X + bone.points[i].Y * Configuration.width);
                if (index > 0 && index < limbData.allPixels.Length) limbData.allPixels[index].Clear();
            }

            for (var i = endIndex; i < bone.points.Count; i++)
            {
                var index = (int) (bone.points[i].X + bone.points[i].Y * Configuration.width);
                if (index > 0 && index < limbData.allPixels.Length) limbData.allPixels[index].Clear();
            }

            bone.points = bone.points.GetRange(startIndex, endIndex - startIndex);
        }

        private bool IsContour(int x, int y)
        {
            var emptyNeighbors = 0;

            for (var i = 0; i < 4; i++)
            {
                var directionX = Utils.cardinalDirections[i, 0];
                var directionY = Utils.cardinalDirections[i, 1];

                var offsetX = x + directionX;
                var offsetY = y + directionY;

                var neighborIndex = Utils.CoordinatesToIndex(offsetX, offsetY);

                if (!Utils.AreCoordinatesInBounds(offsetX, offsetY) ||
                    limbData.allPixels[neighborIndex].humanIndex == -1) emptyNeighbors++;
            }

            if (emptyNeighbors == 1 || emptyNeighbors == 2) return true;

            return false;
        }

        private void FloodFill(Queue<int> pixelsQueue)
        {
            if (pixelsQueue == null)
            {
                pixelsQueue = new Queue<int>();

                // wypelnianie stacka bazowymi pikselami
                for (var i = 0; i < limbData.allPixels.Length; i++)
                    if (limbData.allPixels[i].humanIndex != -1)
                        pixelsQueue.Enqueue(i);
            }

            // FloodFill
            while (pixelsQueue.Count > 0)
            {
                var index = pixelsQueue.Dequeue();

                if (!limbData.activePixels.Contains(index)) limbData.activePixels.Add(index);

                if (index < 0 || index >= limbData.allPixels.Length)
                    continue;

                var alpha = GlobalBuffers.backgroundRemovedBuffer[index * 4 + 3];
                var lpd = limbData.allPixels[index];

                if (lpd.humanIndex == -1)
                    continue;

                if (alpha < Configuration.alphaThreshold)
                    continue;

                var mappedDepthBufferIndex =
                    Utils.CoordinatesToIndex(GlobalBuffers.depthCoordinates[index].X,
                        GlobalBuffers.depthCoordinates[index].Y);

                if (mappedDepthBufferIndex >= 0 && mappedDepthBufferIndex < GlobalBuffers.depthBuffer.Length)
                    if (GlobalBuffers.depthBuffer[mappedDepthBufferIndex].Depth != 0)
                    {
                        var difference = Math.Abs(GlobalBuffers.depthBuffer[mappedDepthBufferIndex].Depth -
                                                  GlobalBuffers.savedBackgroundDepthBuffer[mappedDepthBufferIndex]
                                                      .Depth);
                        if (difference < Configuration.depthThreshold) continue;
                    }

                // 8 stron
                for (var j = 0; j < 8; j++)
                {
                    var xOffset = Utils.ordinalDirections[j, 0];
                    var yOffset = Utils.ordinalDirections[j, 1];

                    var offset = xOffset + yOffset * Configuration.width;

                    // pomijanie wylewania poza krawedzie obrazka
                    if (
                        index % Configuration.width == 0 && xOffset == -1 ||
                        (index + 1) % Configuration.width == 0 && xOffset == 1 ||
                        index / Configuration.width == 0 && yOffset == 1
                        // (limbData.allPixels.Length - index - xOffset <= Configuration.width && yOffset == -1)
                    )
                        continue;

                    var offsetIndex = index + offset;

                    if (offsetIndex < 0 || offsetIndex >= limbData.allPixels.Length)
                        continue;

                    if (limbData.allPixels[offsetIndex].humanIndex == -1)
                    {
                        limbData.allPixels[offsetIndex].humanIndex = lpd.humanIndex;
                        limbData.allPixels[offsetIndex].boneHash = lpd.boneHash;

                        pixelsQueue.Enqueue(offsetIndex);
                    }
                }
            }
        }

        public HashSet<int> GetSortedContour()
        {
            sortedContour.Clear();
            usedContourIndices.Clear();
            lastContourPoint = new Vector2(0, 0);

            if (limbData.contourPixels.Count > 0) AddPointToContourPoints(limbData.contourPixels.Last());

            return sortedContour;
        }

        private void AddPointToContourPoints(int index)
        {
            int pointX = 0, pointY = 0;
            Utils.IndexToCoordinates(index, ref pointX, ref pointY);
            var point = new Vector2(pointX, pointY);

            var distanceFromLastPoint = Vector2.Distance(point, lastContourPoint);

            if (!(lastContourPoint.X == 0 && lastContourPoint.Y == 0) && distanceFromLastPoint > 10) return;

            sortedContour.Add(index);
            usedContourIndices.Add(index);

            lastContourPoint = point;

            for (var i = 0; i < 4; i++)
            {
                var directionX = Utils.cardinalDirections[i, 0];
                var directionY = Utils.cardinalDirections[i, 1];

                var neighborX = pointX + directionX;
                var neighborY = pointY + directionY;

                var neighborIndex = Utils.CoordinatesToIndex(neighborX, neighborY);
                if (!usedContourIndices.Contains(neighborIndex)) usedContourIndices.Add(neighborIndex);
            }

            for (var i = 0; i < Utils.contourSeekingDirectionsCount; i++)
            {
                var directionX = Utils.contourSeekingDirections[i, 0];
                var directionY = Utils.contourSeekingDirections[i, 1];

                var neighborX = pointX + directionX;
                var neighborY = pointY + directionY;

                var neighborIndex = Utils.CoordinatesToIndex(neighborX, neighborY);
                if (!usedContourIndices.Contains(neighborIndex) && limbData.contourPixels.Contains(neighborIndex))
                    AddPointToContourPoints(neighborIndex);
            }
        }
    }
}