using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Kinect;

using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static class BoneProcessor
    {

        private static readonly LimbDataPixel[] oldLimbDataPixels = new LimbDataPixel[Configuration.size];

        private static readonly Dictionary<int, BonePixelsData> bonePixelsDictionary =
            new Dictionary<int, BonePixelsData>(20);

        public static void ProcessAllBones()
        {
            Array.Copy(GB.limbDataManager.limbData.allPixels, oldLimbDataPixels,
                GB.limbDataManager.limbData.allPixels.Length);
            AssignBonePixelsToDictionaries();

            for (var i = 0; i < GB.normalBuffer.Length; i++) GB.normalBuffer[i] = 128;

            foreach (var limbDataSkeleton in GB.limbDataManager.limbData.limbDataSkeletons)
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

            if (!bonePixelsDictionary.ContainsKey(bone.boneHash) || bonePixelsDictionary[bone.boneHash] == null ||
                bonePixelsDictionary[bone.boneHash].indices.Count == 0) return;

            var bonePixelData = bonePixelsDictionary[bone.boneHash];

            if (!Settings.Instance.DrawMorphs)
            {
                ProcessBone_Normal(bone, bonePixelData);
                return;
            }

            switch (bone.boneHash)
            {
                case 35: // head, shoulder center
                    ProcessBone_Size(bone, bonePixelData, Settings.Instance.HeadSize / 100f);
                    break;
                case 152:
                case 84:
                case 169:
                case 101:
                    ProcessBone_Stretch(bone, bonePixelData, new StretchParameters
                    {
                        curve = Curves.steepHillCurve,
                        power = Settings.Instance.ArmScale / 100f
                    });
                    break;
                case 272:
                case 305:
                case 220:
                case 237:
                    ProcessBone_Stretch(bone, bonePixelData, new StretchParameters
                    {
                        curve = Curves.hillCurve,
                        power = Settings.Instance.LegScale / 100f
                    });
                    break;
                default:
                    ProcessBone_Normal(bone, bonePixelData);
                    break;
            }
        }

        private static void ProcessBone_Size(LimbDataBone bone, BonePixelsData bonePixelData, float scale)
        {
            var boneVector = Vector3.Normalize(bone.endPoint - bone.startPoint);

            var indices = bonePixelData.indices;

            //ProcessBone_Normal(bone, indices);

            var originalStartX = bone.endPoint.X;
            var originalStartY = bone.endPoint.Y;

            var startX = originalStartX;
            var startY = originalStartY;

            var pixelsWidth = bonePixelData.maxX - bonePixelData.minX;
            var pixelsHeight = bonePixelData.maxY - bonePixelData.minY;

            var p = startY - bonePixelData.maxY;
            var p2 = startX - bonePixelData.maxX;

            // int pixelsOffsetX = (int)((pixelsWidth * scale - pixelsWidth) * boneVector.X) / 4;
            // int pixelsOffsetY = (int)((pixelsHeight * scale - pixelsHeight) * boneVector.Y) / 4;

            var pixelsOffsetX = (p2 / 2f * scale - p2 + p2 / 2f) * boneVector.X;
            var pixelsOffsetY = (p / 2f * scale - p + p / 2f) * boneVector.Y;

            Parallel.For(0L, oldLimbDataPixels.Length, i =>
            {
                var limbPixel = oldLimbDataPixels[i];

                int x = 0, y = 0;
                Utils.GetIndexCoordinates((int) i, ref x, ref y);

                var offsetX = startX - x;
                var offsetY = startY - y;

                var transformedX = startX - offsetX / scale;
                var transformedY = startY - offsetY / scale;

                //transformedX += pixelsOffsetX;
                transformedY -= pixelsOffsetY;

                if (transformedX < 0 || transformedX >= Configuration.width || transformedY < 0 ||
                    transformedY >= Configuration.height) return;

                var transformedIndex = Utils.GetIndexByCoordinates((int) transformedX, (int) transformedY);

                if (indices.Contains(transformedIndex))
                {
                    //var sourcePixelData = limbDataManager.limbData.allPixels[transformedIndex];

                    //limbPixel.boneHash = sourcePixelData.boneHash;
                    //limbPixel.humanIndex = sourcePixelData.humanIndex;

                    var targetIndex = (int) i * 4;
                    var sourceIndex = transformedIndex * 4;

                    //outputBuffer[transformedIndex] = backgroundRemovedBuffer

                    GB.outputBuffer[targetIndex] = Utils.Interpolate(GB.savedBackgroundColorBuffer[sourceIndex],
                        GB.backgroundRemovedBuffer[sourceIndex],
                        1f - GB.backgroundRemovedBuffer[sourceIndex + 3] / 255f);
                    GB.outputBuffer[targetIndex + 1] = Utils.Interpolate(GB.savedBackgroundColorBuffer[sourceIndex],
                        GB.backgroundRemovedBuffer[sourceIndex + 1],
                        1f - GB.backgroundRemovedBuffer[sourceIndex + 3] / 255f);
                    GB.outputBuffer[targetIndex + 2] = Utils.Interpolate(GB.savedBackgroundColorBuffer[sourceIndex],
                        GB.backgroundRemovedBuffer[sourceIndex + 2],
                        1f - GB.backgroundRemovedBuffer[sourceIndex + 3] / 255f);

                    // normalBuffer[targetIndex] = (byte) (-offsetX * scale + 128);
                    // normalBuffer[targetIndex + 1] = (byte) (-offsetY * scale + 128);
                }
            });
        }

        private static void ProcessBone_Normal(LimbDataBone bone, BonePixelsData bonePixelData)
        {
            var indicesList = bonePixelData.indices.ToList();

            Parallel.For(0, indicesList.Count, i =>
            {
                i = indicesList[i] * 4;

                GB.outputBuffer[i] = Utils.Interpolate(GB.savedBackgroundColorBuffer[i], GB.backgroundRemovedBuffer[i],
                    1f - GB.backgroundRemovedBuffer[i + 3] / 255f);
                GB.outputBuffer[i + 1] = Utils.Interpolate(GB.savedBackgroundColorBuffer[i], GB.backgroundRemovedBuffer[i + 1],
                    1f - GB.backgroundRemovedBuffer[i + 3] / 255f);
                GB.outputBuffer[i + 2] = Utils.Interpolate(GB.savedBackgroundColorBuffer[i], GB.backgroundRemovedBuffer[i + 2],
                    1f - GB.backgroundRemovedBuffer[i + 3] / 255f);

                // i = indicesList[i] * 4;
                //
                // normalBuffer[i] = (byte)(128);
                // normalBuffer[i + 1] = (byte)(128);
            });
        }

        private static void ProcessBone_Stretch(LimbDataBone bone, BonePixelsData bonePixelData,
            StretchParameters stretchParameters)
        {
            // List<int> indicesList = bonePixelData.indices.ToList();

            stretchParameters.power -= 1f;

            var boneVector = Vector3.Normalize(bone.endPoint - bone.startPoint);
            var perpendicularVector = Utils.GetPerpendicularVector(bone.GetStartPoint(), bone.GetEndPoint());

            var pointsBetween = Utils.GetPointsBetween(bone.startPoint, bone.endPoint, Configuration.width,
                Configuration.height);

            var usedIndicesFlagsArray = new bool[Configuration.size];

            Parallel.ForEach(pointsBetween, (point, parallelLoopState, index) =>
            {
                var progress = (float) index / pointsBetween.Count;
                var curveScale = 1f + stretchParameters.power * stretchParameters.curve.Evaluate(progress);

                // int index = Utils.GetIndexByCoordinates((int)point.X, (int)point.Y) * 4;

                for (var direction = -1; direction <= 1; direction += 2)
                for (var k = 0.1f;; k += 1f)
                {
                    var shouldBreak = false;

                    // szerokosc pedzla
                    for (var l = -2; l <= 2; l++)
                    {
                        var pointOffset = perpendicularVector * k * direction;
                        var perpendicularPoint = point + pointOffset + boneVector * l;
                        var samplingPoint = point + (pointOffset + boneVector * l) / curveScale;

                        if (perpendicularPoint.X < 0 || perpendicularPoint.X >= Configuration.width ||
                            perpendicularPoint.Y < 0 || perpendicularPoint.Y >= Configuration.height)
                        {
                            shouldBreak = true;
                            break;
                        }

                        var perpendicularPointIndex = Utils.GetIndexByCoordinates((int) perpendicularPoint.X,
                            (int) perpendicularPoint.Y);
                        var samplingIndex =
                            Utils.GetIndexByCoordinates((int) samplingPoint.X, (int) samplingPoint.Y);

                        if (!bonePixelData.indices.Contains(samplingIndex))
                        {
                            shouldBreak = true;
                            break;
                        }

                        usedIndicesFlagsArray[samplingIndex] = true;

                        samplingIndex *= 4;
                        var outputIndex = perpendicularPointIndex * 4;

                            GB.outputBuffer[outputIndex] = Utils.Interpolate(GB.savedBackgroundColorBuffer[samplingIndex],
                            GB.backgroundRemovedBuffer[samplingIndex],
                            1f - GB.backgroundRemovedBuffer[samplingIndex + 3] / 255f);
                            GB.outputBuffer[outputIndex + 1] = Utils.Interpolate(GB.savedBackgroundColorBuffer[samplingIndex],
                            GB.backgroundRemovedBuffer[samplingIndex + 1],
                            1f - GB.backgroundRemovedBuffer[samplingIndex + 3] / 255f);
                            GB.outputBuffer[outputIndex + 2] = Utils.Interpolate(GB.savedBackgroundColorBuffer[samplingIndex],
                            GB.backgroundRemovedBuffer[samplingIndex + 2],
                            1f - GB.backgroundRemovedBuffer[samplingIndex + 3] / 255f);
                    }

                    if (shouldBreak)
                        break;
                }
            });

            var usedIndices = new List<int>();
            for (var i = 0; i < usedIndicesFlagsArray.Length; i++)
                if (usedIndicesFlagsArray[i])
                    usedIndices.Add(i);

            var unusedIndices = bonePixelData.indices.Except(usedIndices).ToList();

            Parallel.For(0, unusedIndices.Count, i =>
            {
                i = unusedIndices[i] * 4;

                GB.outputBuffer[i] = Utils.Interpolate(GB.savedBackgroundColorBuffer[i], GB.backgroundRemovedBuffer[i],
                    1f - GB.backgroundRemovedBuffer[i + 3] / 255f);
                GB.outputBuffer[i + 1] = Utils.Interpolate(GB.savedBackgroundColorBuffer[i], GB.backgroundRemovedBuffer[i + 1],
                    1f - GB.backgroundRemovedBuffer[i + 3] / 255f);
                GB.outputBuffer[i + 2] = Utils.Interpolate(GB.savedBackgroundColorBuffer[i], GB.backgroundRemovedBuffer[i + 2],
                    1f - GB.backgroundRemovedBuffer[i + 3] / 255f);
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

            for (var i = 0; i < GB.limbDataManager.limbData.allPixels.Length; i++)
            {
                var limbPixel = GB.limbDataManager.limbData.allPixels[i];

                if (limbPixel.humanIndex != -1)
                {
                    int x = 0, y = 0;

                    Utils.GetIndexCoordinates(i, ref x, ref y);

                    if (bonePixelsDictionary.ContainsKey(limbPixel.boneHash))
                    {
                        bonePixelsDictionary[limbPixel.boneHash].indices.Add(i);

                        if (x < bonePixelsDictionary[limbPixel.boneHash].minX)
                            bonePixelsDictionary[limbPixel.boneHash].minX = x;
                        else if (x > bonePixelsDictionary[limbPixel.boneHash].maxX)
                            bonePixelsDictionary[limbPixel.boneHash].maxX = x;

                        if (y < bonePixelsDictionary[limbPixel.boneHash].minY)
                            bonePixelsDictionary[limbPixel.boneHash].minY = y;
                        else if (y > bonePixelsDictionary[limbPixel.boneHash].maxY)
                            bonePixelsDictionary[limbPixel.boneHash].maxY = y;
                    }
                    else
                    {
                        bonePixelsDictionary.Add(limbPixel.boneHash, new BonePixelsData());
                        bonePixelsDictionary[limbPixel.boneHash].indices.Add(i);
                        bonePixelsDictionary[limbPixel.boneHash].minX =
                            bonePixelsDictionary[limbPixel.boneHash].maxX = x;
                        bonePixelsDictionary[limbPixel.boneHash].minY =
                            bonePixelsDictionary[limbPixel.boneHash].maxY = y;
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

        private struct StretchParameters
        {
            public Curve curve;
            public float power;
        }
    }
}