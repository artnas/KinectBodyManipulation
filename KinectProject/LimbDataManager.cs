using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Kinect;

namespace KinectBodyModification
{
    public class LimbDataManager
    {

        private readonly KinectSensor sensor;

        public LimbData limbData;

        public LimbDataManager(KinectSensor sensor)
        {
            this.sensor = sensor;
            this.limbData = new LimbData();
        }

        private int getBufferIndex(int x, int y)
        {
            return y * Configuration.width + x;
        }

        private void ClearBuffer()
        {
            limbData.activePixels.Clear();
            limbData.contourPixels.Clear();
            
            foreach(LimbDataPixel lpd in limbData.allPixels)
            {
                lpd.Clear();
            }
        }

        public void Update(Skeleton[] skeletons)
        {
            ClearBuffer();

            Queue<int> pixelsQueue = new Queue<int>();

            foreach (var skeleton in skeletons)
            {
                LimbDataSkeleton foundSkeleton = null;

                foreach (var limbDataSkeleton in limbData.limbDataSkeletons)
                {
                    if (limbDataSkeleton.skeleton == skeleton)
                    {
                        foundSkeleton = limbDataSkeleton;
                        break;
                    }
                }

                if (foundSkeleton == null)
                {
                    foundSkeleton = new LimbDataSkeleton(skeleton);
                    limbData.limbDataSkeletons.Add(foundSkeleton);
                }

                foreach (JointPair jointPair in Utils.SkeletonIterator(skeleton))
                {
                    //Console.WriteLine("para " + jointPair.a.JointType + " " + jointPair.b.JointType);
                    AssignPixelsBetweenJoints(foundSkeleton, jointPair, pixelsQueue);
                }
            }

            FloodFill(pixelsQueue);

            foreach (var index in limbData.activePixels)
            {
                int pointX = 0, pointY = 0;
                Utils.GetIndexCoordinates(index, ref pointX, ref pointY);

                var isContour = IsContour(index, pointX, pointY);
          
                if (isContour)
                {
                    limbData.allPixels[index].isContour = isContour;
                    limbData.contourPixels.Add(index);
                }
            }
        }

        private void AssignPixelsBetweenJoints(LimbDataSkeleton limbDataSkeleton, JointPair jointPair, Queue<int> pixelsQueue)
        {
            AssignPixelsBetweenJoints(limbDataSkeleton, jointPair.a, jointPair.b, pixelsQueue);
        }

        private void AssignPixelsBetweenJoints(LimbDataSkeleton limbDataSkeleton, Joint a, Joint b, Queue<int> pixelsQueue)
        {
            if (a.TrackingState == JointTrackingState.NotTracked || b.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            LimbDataBone bone = limbDataSkeleton.GetBoneByJointPair(a, b);

            var aPosition = Utils.SkeletonPointToScreen(sensor, a.Position);
            var bPosition = Utils.SkeletonPointToScreen(sensor, b.Position);

            bone.startPoint = aPosition;
            bone.endPoint = bPosition;

            List<Vector3> points = bone.points;
            Utils.GetPointsBetween(points, aPosition, bPosition, Configuration.width, Configuration.height);

            /*

            for (int i = 0; i < points.Count; i++)
            {             

                Vector3 position = points[i];

                int bufferIndex = getBufferIndex((int)position.X, (int)position.Y);

                LimbDataPixel pixel = limbData.allPixels[bufferIndex];
                pixel.humanIndex = (sbyte)limbDataSkeleton.skeleton.TrackingId;
                pixel.startJointType = a.JointType;
                pixel.endJointType = b.JointType;
                pixel.isBone = true;

                if (i == 0)
                    pixel.isJoint = true;

                if (i <= points.Count / 4)
                    pixel.debugDraw = true;

                pixelsQueue.Enqueue(bufferIndex);

            }

            */

            if (points.Count > 0)
            {

                ProcessBone(limbDataSkeleton, bone, pixelsQueue);
                
            }
        }

        private void ProcessBone(LimbDataSkeleton limbDataSkeleton, LimbDataBone bone, Queue<int> pixelsQueue)
        {
            Vector3 perpendicularVector = Utils.GetPerpendicularVector(bone.GetStartPoint(), bone.GetEndPoint());

            BoneConfiguration boneConfiguration = null;

            if (Configuration.boneConfigurationsDictionary.ContainsKey(bone.jointTypePair))
            {
                boneConfiguration = Configuration.boneConfigurationsDictionary[bone.jointTypePair];
            }

            ProcessBoneJoint(limbDataSkeleton, bone, pixelsQueue, perpendicularVector, true, boneConfiguration);
        }

        private void ProcessBoneJoint(LimbDataSkeleton limbDataSkeleton, LimbDataBone bone, Queue<int> pixelsQueue, Vector3 perpendicularVector, bool isStart, BoneConfiguration boneConfiguration)
        {
            if (bone.points.Count == 0)
                return;

            int startIndex = (int)Math.Floor(bone.points.Count * boneConfiguration.startOffset) + 1;
            if (startIndex >= bone.points.Count)
            {
                startIndex = bone.points.Count - 1;
            }

            int endIndex = (int)Math.Ceiling(bone.points.Count * (1f - boneConfiguration.endOffset) ) - 1;
            if (endIndex >= bone.points.Count)
            {
                endIndex = bone.points.Count - 1;
            }

            int startWidth = boneConfiguration.startWidth;
            int endWidth = boneConfiguration.endWidth;

            int length = endIndex - startIndex;

            for (int k = startIndex; k < endIndex; k+=3)
            {

                Vector3 point = bone.points[k];

                bool isOk = true;

                float progress = (float)(k - startIndex) / length;

                int width = Utils.Interpolate(startWidth, endWidth, progress);

                for (int i = 1; i < width; i++)
                {

                    if (!isOk)
                        break;

                    {
                        int bufferIndex = getBufferIndex((int)point.X, (int)point.Y);

                        if (bufferIndex > 0 && bufferIndex < limbData.allPixels.Length)
                        {
                            LimbDataPixel pixel = limbData.allPixels[bufferIndex];
                            pixel.humanIndex = (sbyte)limbDataSkeleton.skeleton.TrackingId;
                            pixel.boneHash = bone.boneHash;
                            pixel.isBone = true;

                            limbData.activePixels.Add(bufferIndex);

                            if (k == startIndex)
                                pixel.isJoint = true;

                            pixelsQueue.Enqueue(bufferIndex);
                        }
                    }

                    for (int j = -1; j <= 1; j += 2)
                    {

                        Vector3 offsetPoint = point + perpendicularVector * i * j;

                        int x = (int)offsetPoint.X;
                        int y = (int)offsetPoint.Y;

                        if (x < 0 || x >= Configuration.width || y < 0 || y >= Configuration.height)
                        {
                            continue;
                        }

                        int colorBufferIndex = (x + y * Configuration.width) * 4;

                        if (GlobalBuffers.backgroundRemovedBuffer[colorBufferIndex + 3] < Configuration.alphaThreshold)
                        {
                            isOk = false;
                            break;
                        }
                        else
                        {
                            int limbDataPixelIndex = colorBufferIndex / 4;

                            LimbDataPixel pixel = limbData.allPixels[limbDataPixelIndex];

                            if (pixel.humanIndex != -1)
                            {
                                if (pixel.humanIndex != limbDataSkeleton.skeleton.TrackingId)
                                {
                                    isOk = false;
                                    break;
                                }
                            }

                            // test glebii

                            if (GlobalBuffers.depthBuffer[limbDataPixelIndex].Depth != 0)
                            {
                                var difference = Math.Abs(GlobalBuffers.depthBuffer[limbDataPixelIndex].Depth - GlobalBuffers.savedBackgroundDepthBuffer[limbDataPixelIndex].Depth);
                                if (difference < Configuration.depthThreshold)
                                {
                                    isOk = false;
                                    break;
                                }              
                            }

                            pixel.humanIndex = (sbyte)limbDataSkeleton.skeleton.TrackingId;
                            pixel.boneHash = bone.boneHash;
                            pixel.debugDraw = true;

                            limbData.activePixels.Add(limbDataPixelIndex);

                            pixelsQueue.Enqueue(limbDataPixelIndex);
                        }

                    }

                }

            }

            // usun punkty przed indeksem
            // if (startIndex != 0)
            // {
            //     bone.points.RemoveRange(0, startIndex);
            //     startIndex = 0;
            // }
        
            for (int i = 0; i < startIndex; i++)
            {
                int index = (int) (bone.points[i].X + bone.points[i].Y * Configuration.width);
                if (index > 0 && index < limbData.allPixels.Length)
                {
                    limbData.allPixels[index].Clear();
                }
            }
            for (int i = endIndex; i < bone.points.Count; i++)
            {
                int index = (int)(bone.points[i].X + bone.points[i].Y * Configuration.width);
                if (index > 0 && index < limbData.allPixels.Length)
                {
                    limbData.allPixels[index].Clear();
                }
            }

            bone.points = bone.points.GetRange(startIndex, (endIndex - startIndex));
        }

        private bool IsContour(int index, int x, int y)
        {
            for (var i = 0; i < 4; i++)
            {
                var directionX = Utils.cardinalDirections[i, 0];
                var directionY = Utils.cardinalDirections[i, 1];

                var offsetX = x + directionX;
                var offsetY = y + directionY;

                int neighborIndex = Utils.GetIndexByCoordinates(offsetX, offsetY);

                if (!Utils.AreCoordinatesInBounds(offsetX, offsetY) || limbData.allPixels[neighborIndex].humanIndex == -1)
                {
                    return true;
                }
            }

            return false;
        }

        private void FloodFill(Queue<int> pixelsQueue)
        {

            if (pixelsQueue == null)
            {
                pixelsQueue = new Queue<int>();

                // wypelnianie stacka bazowymi pikselami
                for (int i = 0; i < limbData.allPixels.Length; i++)
                {
                    if (limbData.allPixels[i].humanIndex != -1)
                        pixelsQueue.Enqueue(i);
                }
            }       

            // FloodFill
            while (pixelsQueue.Count > 0)
            {
                int index = pixelsQueue.Dequeue();

                if (!limbData.activePixels.Contains(index))
                {
                    limbData.activePixels.Add(index);
                }

                if (index < 0 || index >= limbData.allPixels.Length)
                    continue;

                byte alpha = GlobalBuffers.backgroundRemovedBuffer[index * 4 + 3];
                LimbDataPixel lpd = limbData.allPixels[index];

                if (lpd.humanIndex == -1)
                    continue;

                if (alpha < Configuration.alphaThreshold)
                    continue;

                 var mappedDepthBufferIndex = 
                     Utils.GetIndexByCoordinates(GlobalBuffers.depthCoordinates[index].X, GlobalBuffers.depthCoordinates[index].Y);
                
                 if (mappedDepthBufferIndex >= 0 && mappedDepthBufferIndex < GlobalBuffers.depthBuffer.Length)
                 {
                     if (GlobalBuffers.depthBuffer[mappedDepthBufferIndex].Depth != 0)
                     {
                         var difference = Math.Abs(GlobalBuffers.depthBuffer[mappedDepthBufferIndex].Depth -
                                                   GlobalBuffers.savedBackgroundDepthBuffer[mappedDepthBufferIndex].Depth);
                         if (difference < Configuration.depthThreshold)
                         {
                             continue;
                         }
                     }
                 }

                // 8 stron
                for (int j = 0; j < 8; j++)
                {
                    int xOffset = Utils.ordinalDirections[j, 0];
                    int yOffset = Utils.ordinalDirections[j, 1];

                    int offset = xOffset + yOffset * Configuration.width;

                    // pomijanie wylewania poza krawedzie obrazka
                    if (
                        (index % Configuration.width == 0 && xOffset == -1) ||
                        ((index + 1) % Configuration.width == 0 && xOffset == 1) ||
                        (index / Configuration.width == 0 && yOffset == 1)
                        // (limbData.allPixels.Length - index - xOffset <= Configuration.width && yOffset == -1)
                    )
                    {
                        continue;
                    }

                    int offsetIndex = index + offset;

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

    }
}
