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

        public LimbPixelData[] limbPixelData;

        public LimbDataManager(byte[] colorBuffer, DepthImagePixel[] depthBuffer, byte[] backgroundRemovedBuffer)
        {
            this.colorBuffer = colorBuffer;
            this.depthBuffer = depthBuffer;
            this.backgroundRemovedBuffer = backgroundRemovedBuffer;

            limbPixelData = new LimbPixelData[Configuration.width * Configuration.height];
            for (var i = 0; i < Configuration.width * Configuration.height; i++)
            {
                limbPixelData[i] = new LimbPixelData();
            }
        }

        private int getBufferIndex(int x, int y)
        {
            return y * Configuration.width + x;
        }

        private void ClearBuffer()
        {
            foreach(LimbPixelData lpd in limbPixelData)
            {
                lpd.humanIndex = -1;
                lpd.isBone = false;
            }
        }

        public void Update(Skeleton[] skeletons)
        {

            ClearBuffer();

            foreach(var skeleton in skeletons)
            {
                foreach (JointPair jointPair in Utils.SkeletonIterator(skeleton))
                {
                    //Console.WriteLine("para " + jointPair.a.JointType + " " + jointPair.b.JointType);
                    AssignPixelsBetweenJoints(skeleton, jointPair);
                }
            }

            FloodFill();
            
        }

        private void AssignPixelsBetweenJoints(Skeleton skeleton, JointPair jointPair)
        {
            AssignPixelsBetweenJoints(skeleton, jointPair.a, jointPair.b);
        }

        private void AssignPixelsBetweenJoints(Skeleton skeleton, Joint a, Joint b)
        {

            if (a.TrackingState == JointTrackingState.NotTracked || b.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            //var aPosition = new Vector3(a.Position.X, a.Position.Y, a.Position.Z);
            var aPosition = Utils.SkeletonPointToScreen(a.Position);
            var bPosition = Utils.SkeletonPointToScreen(b.Position);

            foreach (Vector3 position in Utils.IteratePointsBetween(aPosition, bPosition, Configuration.width, Configuration.height))
            {

                int bufferIndex = getBufferIndex((int)position.X, (int)position.Y);

                LimbPixelData pixel = limbPixelData[bufferIndex];
                pixel.humanIndex = (sbyte)skeleton.TrackingId;
                pixel.startJointType = a.JointType;
                pixel.endJointType = b.JointType;
                pixel.isBone = true;

            }

        }

        private void FloodFill()
        {

            Queue<int> pixelsToDillate = new Queue<int>();

            // wypelnianie stacka bazowymi pikselami
            for (int i = 0; i < limbPixelData.Length; i++)
            {
                if (limbPixelData[i].humanIndex != -1)
                    pixelsToDillate.Enqueue(i);              
            }

            // FloodFill
            while (pixelsToDillate.Count > 0)
            {
                int index = pixelsToDillate.Dequeue();

                if (index < 0 || index >= limbPixelData.Length)
                    continue;

                byte alpha = backgroundRemovedBuffer[index * 4 + 3];
                LimbPixelData lpd = limbPixelData[index];

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
                        // (limbPixelData.Length - index - xOffset <= Configuration.width && yOffset == -1)
                    ){
                        continue;
                    }   

                    int offsetIndex = index + offset;

                    if (offsetIndex < 0 || offsetIndex >= limbPixelData.Length)
                        continue;

                    if (limbPixelData[offsetIndex].humanIndex == -1)
                    {
                        limbPixelData[offsetIndex].humanIndex = lpd.humanIndex;
                        limbPixelData[offsetIndex].startJointType = lpd.startJointType;
                        limbPixelData[offsetIndex].endJointType = lpd.endJointType;

                        pixelsToDillate.Enqueue(offsetIndex);
                    }
                }
            }

        }

    }
}
