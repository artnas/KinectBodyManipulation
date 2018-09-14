using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        private static Dictionary<int, BonePixelsData> bonePixelsDictionary = new Dictionary<int, BonePixelsData>(20);

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

            for (int i = 0; i < normalBuffer.Length; i++)
            {
                normalBuffer[i] = 128;
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

            if (!bonePixelsDictionary.ContainsKey(bone.boneHash) || bonePixelsDictionary[bone.boneHash] == null || bonePixelsDictionary[bone.boneHash].indices.Count == 0)
            {
                return;
            }

            var bonePixelData = bonePixelsDictionary[bone.boneHash];

            switch (bone.boneHash)
            {
                case 35:    // head, shoulder center
                    ProcessBone_Size(bone, bonePixelData);
                    break;
                case 152:    
                case 84:    
                case 169:    
                case 101:    
                    ProcessBone_Stretch(bone, bonePixelData, new StretchParameters()
                    {
                        curve = Curves.sinHill,
                        power = 1,
                    });
                    break;
                case 272:
                case 305:
                case 220:
                case 237:
                    ProcessBone_Stretch(bone, bonePixelData, new StretchParameters()
                    {
                        curve = Curves.test,
                        power = 0.25f,
                    });
                    break;
                default:
                    ProcessBone_Normal(bone, bonePixelData);
                    break;
            }

        }

        private static void ProcessBone_Size(LimbDataBone bone, BonePixelsData bonePixelData)
        {

            Vector3 boneVector = Vector3.Normalize(bone.endPoint - bone.startPoint);

            HashSet<int> indices = bonePixelData.indices;

            //ProcessBone_Normal(bone, indices);

            float scale = Settings.headSize;

            float originalStartX = bone.endPoint.X;
            float originalStartY = bone.endPoint.Y;

            float startX = originalStartX;
            float startY = originalStartY;

            int pixelsWidth = bonePixelData.maxX - bonePixelData.minX;
            int pixelsHeight = bonePixelData.maxY - bonePixelData.minY;

            float p = startY - bonePixelData.maxY;
            float p2 = startX - bonePixelData.maxX;

            // int pixelsOffsetX = (int)((pixelsWidth * scale - pixelsWidth) * boneVector.X) / 4;
            // int pixelsOffsetY = (int)((pixelsHeight * scale - pixelsHeight) * boneVector.Y) / 4;

            float pixelsOffsetX = (((p2 / 2f) * scale - p2 + p2 / 2f) * boneVector.X);
            float pixelsOffsetY = (((p / 2f) * scale - p + p / 2f) * boneVector.Y);

            Parallel.For(0L, oldLimbDataPixels.Length, i =>
            {
                var limbPixel = oldLimbDataPixels[i];

                int x = 0, y = 0;
                Utils.GetIndexCoordinates((int)i, ref x, ref y);

                float offsetX = startX - x;
                float offsetY = startY - y;

                float transformedX = startX - offsetX / scale;
                float transformedY = startY - offsetY / scale;

                //transformedX += pixelsOffsetX;
                transformedY -= pixelsOffsetY;

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

                    // normalBuffer[targetIndex] = (byte) (-offsetX * scale + 128);
                    // normalBuffer[targetIndex + 1] = (byte) (-offsetY * scale + 128);
                }
            });

        }

        private static void ProcessBone_Normal(LimbDataBone bone, BonePixelsData bonePixelData)
        {

            List<int> indicesList = bonePixelData.indices.ToList();

            Parallel.For(0, indicesList.Count, i =>
            {

                 i = indicesList[i] * 4;
                
                 outputBuffer[i] = Utils.Interpolate(savedBackgroundColorBuffer[i], backgroundRemovedBuffer[i],
                     1f - backgroundRemovedBuffer[i + 3] / 255f);
                 outputBuffer[i + 1] = Utils.Interpolate(savedBackgroundColorBuffer[i], backgroundRemovedBuffer[i + 1],
                     1f - backgroundRemovedBuffer[i + 3] / 255f);
                 outputBuffer[i + 2] = Utils.Interpolate(savedBackgroundColorBuffer[i], backgroundRemovedBuffer[i + 2],
                     1f - backgroundRemovedBuffer[i + 3] / 255f);

                // i = indicesList[i] * 4;
                //
                // normalBuffer[i] = (byte)(128);
                // normalBuffer[i + 1] = (byte)(128);

            });

        }

        private struct StretchParameters
        {
            public Curve curve;
            public float power;
        }

        private static void ProcessBone_Stretch(LimbDataBone bone, BonePixelsData bonePixelData, StretchParameters stretchParameters)
        {

            // List<int> indicesList = bonePixelData.indices.ToList();

            // Vector3 boneVector = Vector3.Normalize(bone.endPoint - bone.startPoint);
            Vector3 perpendicularVector = Utils.GetPerpendicularVector(bone.GetStartPoint(), bone.GetEndPoint());

            var pointsBetween = Utils.GetPointsBetween(bone.startPoint, bone.endPoint, Configuration.width, Configuration.height);

            Parallel.ForEach(pointsBetween, (point, parallelLoopState, index) =>
            {

                var progress = (float) index / pointsBetween.Count;
                var curveScale = stretchParameters.curve.Evaluate(progress, stretchParameters.power);

                // int index = Utils.GetIndexByCoordinates((int)point.X, (int)point.Y) * 4;

                for (var direction = -1; direction <= 1; direction += 2)
                {

                    bool reachedNormalWidth = false;
                    float scaledLineWidth = 0;

                    for (var k = 0.1f; ; k+=0.05f)
                    {

                        Vector3 pointOffset = perpendicularVector * k * direction;
                        Vector3 perpendicularPoint = point + pointOffset;

                        if (perpendicularPoint.X < 0 || perpendicularPoint.X >= Configuration.width ||
                            perpendicularPoint.Y < 0 || perpendicularPoint.Y >= Configuration.height)
                        {
                            break;
                        }

                        int perpendicularPointIndex = Utils.GetIndexByCoordinates((int)perpendicularPoint.X, (int)perpendicularPoint.Y);

                        if (!reachedNormalWidth && !bonePixelData.indices.Contains(perpendicularPointIndex))
                        {
                            scaledLineWidth = k * curveScale;
                            reachedNormalWidth = true;
                        }

                        if (reachedNormalWidth && k >= scaledLineWidth)
                        {
                            break;
                        }

                        Vector3 samplingPoint = point + (pointOffset / curveScale);

                        int samplingIndex = Utils.GetIndexByCoordinates((int) samplingPoint.X, (int) samplingPoint.Y) * 4;

                        int outputIndex = perpendicularPointIndex * 4;

                        outputBuffer[outputIndex] = Utils.Interpolate(savedBackgroundColorBuffer[samplingIndex], backgroundRemovedBuffer[samplingIndex],
                            1f - backgroundRemovedBuffer[samplingIndex + 3] / 255f);
                        outputBuffer[outputIndex + 1] = Utils.Interpolate(savedBackgroundColorBuffer[samplingIndex], backgroundRemovedBuffer[samplingIndex + 1],
                            1f - backgroundRemovedBuffer[samplingIndex + 3] / 255f);
                        outputBuffer[outputIndex + 2] = Utils.Interpolate(savedBackgroundColorBuffer[samplingIndex], backgroundRemovedBuffer[samplingIndex + 2],
                            1f - backgroundRemovedBuffer[samplingIndex + 3] / 255f);

                    }

                }
          
            });

            // Parallel.For(0, indicesList.Count, i =>
            // {
            //
            //     i = indicesList[i] * 4;
            //
            //     // outputBuffer[i] = Utils.Interpolate(savedBackgroundColorBuffer[i], backgroundRemovedBuffer[i],
            //     //     1f - backgroundRemovedBuffer[i + 3] / 255f);
            //     // outputBuffer[i + 1] = Utils.Interpolate(savedBackgroundColorBuffer[i], backgroundRemovedBuffer[i + 1],
            //     //     1f - backgroundRemovedBuffer[i + 3] / 255f);
            //     // outputBuffer[i + 2] = Utils.Interpolate(savedBackgroundColorBuffer[i], backgroundRemovedBuffer[i + 2],
            //     //     1f - backgroundRemovedBuffer[i + 3] / 255f);
            //
            // });

        }

        private static void AssignBonePixelsToDictionaries()
        {
            // wyczysc listy
            foreach (var entry in bonePixelsDictionary)
            {
                entry.Value.indices.Clear();
                entry.Value.minX = entry.Value.maxX = entry.Value.minY = entry.Value.maxY = 0;
            }

            for (int i = 0; i < limbDataManager.limbData.pixelData.Length; i++)
            {
                var limbPixel = limbDataManager.limbData.pixelData[i];

                if (limbPixel.humanIndex != -1)
                {

                    int x = 0, y = 0;

                    Utils.GetIndexCoordinates(i, ref x, ref y);

                    if (bonePixelsDictionary.ContainsKey(limbPixel.boneHash))
                    {
                        bonePixelsDictionary[limbPixel.boneHash].indices.Add(i);

                        if (x < bonePixelsDictionary[limbPixel.boneHash].minX)
                        {
                            bonePixelsDictionary[limbPixel.boneHash].minX = x;
                        }
                        else if (x > bonePixelsDictionary[limbPixel.boneHash].maxX)
                        {
                            bonePixelsDictionary[limbPixel.boneHash].maxX = x;
                        }

                        if (y < bonePixelsDictionary[limbPixel.boneHash].minY)
                        {
                            bonePixelsDictionary[limbPixel.boneHash].minY = y;
                        }
                        else if (y > bonePixelsDictionary[limbPixel.boneHash].maxY)
                        {
                            bonePixelsDictionary[limbPixel.boneHash].maxY = y;
                        }
                    }
                    else
                    {
                        bonePixelsDictionary.Add(limbPixel.boneHash, new BonePixelsData());
                        bonePixelsDictionary[limbPixel.boneHash].indices.Add(i);
                        bonePixelsDictionary[limbPixel.boneHash].minX = bonePixelsDictionary[limbPixel.boneHash].maxX = x;
                        bonePixelsDictionary[limbPixel.boneHash].minY = bonePixelsDictionary[limbPixel.boneHash].maxY = y;
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

        private class BonePixelsData
        {
            public int minX, maxX, minY, maxY;
            public HashSet<int> indices;

            public BonePixelsData()
            {
                indices = new HashSet<int>();
            }
        }

    }
}
