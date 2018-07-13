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
                lpd.humanIndex = -1;
                lpd.isBone = false;
                lpd.isJoint = false;
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

            //var aPosition = new Vector3(a.Position.X, a.Position.Y, a.Position.Z);
            var aPosition = Utils.SkeletonPointToScreen(a.Position);
            var bPosition = Utils.SkeletonPointToScreen(b.Position);

            List<Vector3> points = bone.points;
            Utils.GetPointsBetween(points, aPosition, bPosition, Configuration.width, Configuration.height);

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

                pixelsQueue.Enqueue(bufferIndex);

            }

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

                if (alpha == 0)
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
                        limbData.pixelData[offsetIndex].startJointType = lpd.startJointType;
                        limbData.pixelData[offsetIndex].endJointType = lpd.endJointType;

                        pixelsQueue.Enqueue(offsetIndex);
                    }
                }
            }

        }

    }
}
