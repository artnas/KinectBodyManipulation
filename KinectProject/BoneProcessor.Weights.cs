using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK;
using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static partial class BoneProcessor
    {
        private static void ProcessAllBoneWeights()
        {
            foreach (var bone in GB.LimbDataManager.LimbData.LimbDataSkeleton.Bones)
                switch (bone.BoneHash)
                {
                    case 35: // head, shoulder center
                        ProcessBoneWeights(bone,
                            new List<BonePixels>
                            {
                                GetBonePixelsDataFromBoneDictionary(130),
                                GetBonePixelsDataFromBoneDictionary(66)
                            });
                        break;
                    case 152: // arms
                        ProcessBoneWeights(bone,
                            new List<BonePixels>
                            {
                                GetBonePixelsDataFromBoneDictionary(130),
                                GetBonePixelsDataFromBoneDictionary(18),
                                GetBonePixelsDataFromBoneDictionary(35)
                            });
                        break;
                    case 84:
                        ProcessBoneWeights(bone,
                            new List<BonePixels>
                            {
                                GetBonePixelsDataFromBoneDictionary(66),
                                GetBonePixelsDataFromBoneDictionary(18),
                                GetBonePixelsDataFromBoneDictionary(35)
                            });
                        break;
                    case 169:
                        ProcessBoneWeights(bone, new List<BonePixels> {GetBonePixelsDataFromBoneDictionary(186)});
                        break;
                    case 101:
                        ProcessBoneWeights(bone, new List<BonePixels> {GetBonePixelsDataFromBoneDictionary(118)});
                        break;
                    case 272: // legs
                        ProcessBoneWeights(bone,
                            new List<BonePixels>
                            {
                                GetBonePixelsDataFromBoneDictionary(256),
                                GetBonePixelsDataFromBoneDictionary(192),
                                GetBonePixelsDataFromBoneDictionary(220)
                            });
                        break;
                    case 305:
                        ProcessBoneWeights(bone, new List<BonePixels> {GetBonePixelsDataFromBoneDictionary(242)});
                        break;
                    case 220:
                        ProcessBoneWeights(bone,
                            new List<BonePixels>
                            {
                                GetBonePixelsDataFromBoneDictionary(192),
                                GetBonePixelsDataFromBoneDictionary(256),
                                GetBonePixelsDataFromBoneDictionary(272)
                            });
                        break;
                    case 237:
                        ProcessBoneWeights(bone, new List<BonePixels> {GetBonePixelsDataFromBoneDictionary(254)});
                        break;
                }
        }

        private static void ProcessBoneWeights(LimbDataBone bone, List<BonePixels> pixelListsToCheck)
        {
            if (bone.Points.Count == 0)
                return;

            var bonePixelData = GetBonePixelsDataFromBoneDictionary(bone.BoneHash);

            if (bonePixelData == null)
                return;

            const float distanceLimit = 32f;
            var indices = bonePixelData.VertexIndices;

            Parallel.ForEach(indices, vertexIndex =>
            {
                var thisVertex = GB.LimbDataManager.LimbData.Mesh.Vertices[vertexIndex];
                var thisVertex2D = new Vector2(thisVertex.X, thisVertex.Y);

                var minDistance = float.MaxValue;

                foreach (var whitelistPixelList in pixelListsToCheck)
                    if (whitelistPixelList != null)
                        foreach (var otherVertexIndex in whitelistPixelList.VertexIndices)
                        {
                            var otherVertex = GB.LimbDataManager.LimbData.Mesh.Vertices[otherVertexIndex];
                            var otherVertex2D = new Vector2(otherVertex.X, otherVertex.Y);

                            var distance = Vector2.Distance(thisVertex2D, otherVertex2D);

                            if (distance < minDistance) minDistance = distance;
                        }

                if (minDistance < distanceLimit)
                {
                    var progress = minDistance / distanceLimit;
                    GB.LimbDataManager.LimbData.Mesh.VertexWeightsDictionary[thisVertex] =
                        Curves.WeightsSmoothingCurve.Evaluate(progress);
                }
            });
        }
    }
}