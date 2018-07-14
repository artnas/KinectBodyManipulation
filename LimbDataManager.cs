using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public class LimbDataManager
    {
     
        private byte[] colorBuffer;
        private DepthImagePixel[] depthBuffer;
        private byte[] backgroundRemovedBuffer;

        public LimbData limbData;

        public LimbDataManager(byte[] colorBuffer, DepthImagePixel[] depthBuffer, byte[] backgroundRemovedBuffer)
        {
            this.colorBuffer = colorBuffer;
            this.depthBuffer = depthBuffer;
            this.backgroundRemovedBuffer = backgroundRemovedBuffer;

            this.limbData = new LimbData();
        }

        private int getBufferIndex(int x, int y)
        {
            return y * Configuration.width + x;
        }

        private void ClearBuffer()
        {
            foreach(LimbDataPixel lpd in limbData.pixelData)
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

            var aPosition = Utils.SkeletonPointToScreen(a.Position);
            var bPosition = Utils.SkeletonPointToScreen(b.Position);

            List<Vector3> points = bone.points;
            Utils.GetPointsBetween(points, aPosition, bPosition, Configuration.width, Configuration.height);

            /*

            for (int i = 0; i < points.Count; i++)
            {             

                Vector3 position = points[i];

                int bufferIndex = getBufferIndex((int)position.X, (int)position.Y);

                LimbDataPixel pixel = limbData.pixelData[bufferIndex];
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

                        if (bufferIndex > 0 && bufferIndex < limbData.pixelData.Length)
                        {
                            LimbDataPixel pixel = limbData.pixelData[bufferIndex];
                            pixel.humanIndex = (sbyte)limbDataSkeleton.skeleton.TrackingId;
                            pixel.boneHash = bone.boneHash;
                            pixel.isBone = true;

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

                        if (backgroundRemovedBuffer[colorBufferIndex + 3] < Configuration.alphaThreshold)
                        {
                            isOk = false;
                            break;
                        }
                        else
                        {
                            int limbDataPixelIndex = colorBufferIndex / 4;

                            LimbDataPixel pixel = limbData.pixelData[limbDataPixelIndex];

                            if (pixel.humanIndex != -1)
                            {
                                if (pixel.humanIndex != limbDataSkeleton.skeleton.TrackingId)
                                {
                                    isOk = false;
                                    break;
                                }
                            }

                            pixel.humanIndex = (sbyte)limbDataSkeleton.skeleton.TrackingId;
                            pixel.boneHash = bone.boneHash;
                            pixel.debugDraw = true;

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
                if (index > 0 && index < limbData.pixelData.Length)
                {
                    limbData.pixelData[index].Clear();
                }
            }
            for (int i = endIndex; i < bone.points.Count; i++)
            {
                int index = (int)(bone.points[i].X + bone.points[i].Y * Configuration.width);
                if (index > 0 && index < limbData.pixelData.Length)
                {
                    limbData.pixelData[index].Clear();
                }
            }

            bone.points = bone.points.GetRange(startIndex, (endIndex - startIndex));

        }

        private void FloodFill(Queue<int> pixelsQueue)
        {

            if (pixelsQueue == null)
            {
                pixelsQueue = new Queue<int>();

                // wypelnianie stacka bazowymi pikselami
                for (int i = 0; i < limbData.pixelData.Length; i++)
                {
                    if (limbData.pixelData[i].humanIndex != -1)
                        pixelsQueue.Enqueue(i);
                }
            }       

            // FloodFill
            while (pixelsQueue.Count > 0)
            {
                int index = pixelsQueue.Dequeue();

                if (index < 0 || index >= limbData.pixelData.Length)
                    continue;

                byte alpha = backgroundRemovedBuffer[index * 4 + 3];
                LimbDataPixel lpd = limbData.pixelData[index];

                if (lpd.humanIndex == -1)
                    continue;

                if (alpha < Configuration.alphaThreshold)
                    continue;

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
                        // (limbData.pixelData.Length - index - xOffset <= Configuration.width && yOffset == -1)
                    ){
                        continue;
                    }   

                    int offsetIndex = index + offset;

                    if (offsetIndex < 0 || offsetIndex >= limbData.pixelData.Length)
                        continue;

                    if (limbData.pixelData[offsetIndex].humanIndex == -1)
                    {
                        limbData.pixelData[offsetIndex].humanIndex = lpd.humanIndex;
                        limbData.pixelData[offsetIndex].boneHash = lpd.boneHash;

                        pixelsQueue.Enqueue(offsetIndex);
                    }
                }
            }

        }

    }
}
