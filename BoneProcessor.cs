using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public static class BoneProcessor
    {

        private static DepthImagePixel[] depthBuffer;

        private static byte[] colorBuffer;
        private static byte[] outputBuffer;
        private static byte[] backgroundRemovedBuffer;
        private static byte[] normalBuffer;

        private static byte[] savedBackgroundColorBuffer;
        private static DepthImagePixel[] savedBackgroundDepthBuffer;

        private static LimbDataManager limbDataManager;

        private static LimbDataPixel[] oldLimbDataPixels = new LimbDataPixel[Configuration.size];

        private static Dictionary<int, HashSet<int>> bonePixelsDictionary = new Dictionary<int, HashSet<int>>(20);

        public static void SetBuffers(DepthImagePixel[] depthBuffer, byte[] colorBuffer, byte[] outputBuffer, byte[] backgroundRemovedBuffer, LimbDataManager limbDataManager, byte[] savedBackgroundColorBuffer, DepthImagePixel[] savedBackgroundDepthBuffer, byte[] normalBuffer)
        {
            BoneProcessor.depthBuffer = depthBuffer;
            BoneProcessor.colorBuffer = colorBuffer;
            BoneProcessor.outputBuffer = outputBuffer;
            BoneProcessor.backgroundRemovedBuffer = backgroundRemovedBuffer;
            BoneProcessor.limbDataManager = limbDataManager;
            BoneProcessor.savedBackgroundColorBuffer = savedBackgroundColorBuffer;
            BoneProcessor.savedBackgroundDepthBuffer = savedBackgroundDepthBuffer;
            BoneProcessor.normalBuffer = normalBuffer;
        }

        public static void ProcessAllBones()
        {
            Array.Copy(limbDataManager.limbData.pixelData, oldLimbDataPixels, limbDataManager.limbData.pixelData.Length);
            AssignBonePixelsToDictionaries();

            foreach (var i in normalBuffer)
            {
                normalBuffer[i] = 0;
            }

            foreach (var limbDataSkeleton in limbDataManager.limbData.limbDataSkeletons)
            {
                foreach (var bone in limbDataSkeleton.bones)
                {
                    ProcessBone(bone);
                }
            }
        }

        public static void ProcessBone(LimbDataBone bone)
        {

            if (bone.points.Count == 0)
                return;

            if (!bonePixelsDictionary.ContainsKey(bone.boneHash) || bonePixelsDictionary[bone.boneHash] == null || bonePixelsDictionary[bone.boneHash].Count == 0)
            {
                return;
            }

            HashSet<int> pixelIndices = bonePixelsDictionary[bone.boneHash];

            switch (bone.boneHash)
            {
                case 35:    // head, shoulder center
                    ProcessBone_Size(bone, pixelIndices);
                    break;
                default:
                    ProcessBone_Normal(bone, pixelIndices);
                    break;
            }

        }

        private static void ProcessBone_Size(LimbDataBone bone, HashSet<int> indices)
        {

            float scale = 1f;

            int originalStartX = (int)bone.startPoint.X;
            int originalStartY = (int)bone.startPoint.Y;

            int trimmedStartX = (int)bone.GetStartPoint().X;
            int trimmedStartY = (int)bone.GetStartPoint().Y;

            int startX = trimmedStartX;
            int startY = trimmedStartY;

            float startOffsetX = (originalStartX - trimmedStartX) / (scale);
            float startOffsetY = (originalStartY - trimmedStartY) / (scale);

            Console.WriteLine(startOffsetX + " " + startOffsetY);

            Parallel.For(0L, oldLimbDataPixels.Length, i =>
            {
                var limbPixel = oldLimbDataPixels[i];

                int x = 0, y = 0;
                Utils.GetIndexCoordinates((int)i, ref x, ref y);

                int offsetX = startX - x;
                int offsetY = startY - y;

                float transformedX = startX - offsetX / scale;
                float transformedY = startY - offsetY / scale;

                transformedX += startOffsetX;
                transformedY += startOffsetY;

                if (transformedX < 0 || transformedX >= Configuration.width || transformedY < 0 || transformedY >= Configuration.height)
                {
                    return;
                }

                int transformedIndex = Utils.GetIndexByCoordinates((int)transformedX, (int)transformedY);

                if (indices.Contains(transformedIndex))
                {
                    //var sourcePixelData = limbDataManager.limbData.pixelData[transformedIndex];

                    //limbPixel.boneHash = sourcePixelData.boneHash;
                    //limbPixel.humanIndex = sourcePixelData.humanIndex;

                    int targetIndex = (int)i * 4;
                    int sourceIndex = transformedIndex * 4;

                    //outputBuffer[transformedIndex] = backgroundRemovedBuffer

                    outputBuffer[targetIndex] = Utils.Interpolate(savedBackgroundColorBuffer[sourceIndex], backgroundRemovedBuffer[sourceIndex],
                        1f - backgroundRemovedBuffer[sourceIndex + 3] / 255f);
                    outputBuffer[targetIndex + 1] = Utils.Interpolate(savedBackgroundColorBuffer[sourceIndex], backgroundRemovedBuffer[sourceIndex + 1],
                        1f - backgroundRemovedBuffer[sourceIndex + 3] / 255f);
                    outputBuffer[targetIndex + 2] = Utils.Interpolate(savedBackgroundColorBuffer[sourceIndex], backgroundRemovedBuffer[sourceIndex + 2],
                        1f - backgroundRemovedBuffer[sourceIndex + 3] / 255f);
                }
            });

        }

        private static void ProcessBone_Normal(LimbDataBone bone, HashSet<int> indices)
        {

            List<int> indicesList = indices.ToList();

            Parallel.For(0, indicesList.Count, i =>
            {
                i = indicesList[i] * 4;

                outputBuffer[i] = Utils.Interpolate(savedBackgroundColorBuffer[i], backgroundRemovedBuffer[i],
                    1f - backgroundRemovedBuffer[i + 3] / 255f);
                outputBuffer[i + 1] = Utils.Interpolate(savedBackgroundColorBuffer[i], backgroundRemovedBuffer[i + 1],
                    1f - backgroundRemovedBuffer[i + 3] / 255f);
                outputBuffer[i + 2] = Utils.Interpolate(savedBackgroundColorBuffer[i], backgroundRemovedBuffer[i + 2],
                    1f - backgroundRemovedBuffer[i + 3] / 255f);
            });

        }

        private static void AssignBonePixelsToDictionaries()
        {
            // wyczysc listy
            foreach (var entry in bonePixelsDictionary)
            {
                entry.Value.Clear();
            }

            for (int i = 0; i < limbDataManager.limbData.pixelData.Length; i++)
            {
                var limbPixel = limbDataManager.limbData.pixelData[i];

                if (limbPixel.humanIndex != -1)
                {

                    if (bonePixelsDictionary.ContainsKey(limbPixel.boneHash))
                    {
                        bonePixelsDictionary[limbPixel.boneHash].Add(i);
                    }
                    else
                    {
                        bonePixelsDictionary.Add(limbPixel.boneHash, new HashSet<int>());
                        bonePixelsDictionary[limbPixel.boneHash].Add(i);
                    }

                }
            }

            // if (bonePixelsDictionary.ContainsKey(1))
            // {
            //     if (bonePixelsDictionary[1].Count > 0)
            //     {
            //         Console.WriteLine(bonePixelsDictionary[1].First());
            //         Console.WriteLine(bonePixelsDictionary[1].Last());
            //     }        
            // }

        }

    }
}
