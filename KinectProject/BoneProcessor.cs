using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Kinect;

using GB = KinectBodyModification.GlobalBuffers;
using Vector2 = OpenTK.Vector2;

namespace KinectBodyModification
{
    public static class BoneProcessor
    {

        private static readonly Dictionary<int, BonePixelsData> bonePixelsDictionary = new Dictionary<int, BonePixelsData>(20);

        public static void ProcessAllBones()
        {
            if (!Settings.Instance.DrawMorphs)
            {
                return;
            }

            AssignBonePixelsToDictionaries();

            ProcessAllBoneWeights();

            MorphAllBones();
        }

        private static void AssignBonePixelsToDictionaries()
        {
            // wyczysc listy
            foreach (var entry in bonePixelsDictionary)
            {
                entry.Value.vertexIndices.Clear();
            }

            for (var i = 0; i < GB.limbDataManager.limbData.mesh.vertices.Count; i++)
            {
                var vertex = GB.limbDataManager.limbData.mesh.vertices[i];

                int x = (int)Math.Round(vertex.X);
                int y = (int)Math.Round(vertex.Y);

                int index = Utils.CoordinatesToIndex(x, y);

                var limbPixel = GB.limbDataManager.limbData.allPixels[index];

                if (limbPixel.humanIndex != -1)
                {
                    if (bonePixelsDictionary.ContainsKey(limbPixel.boneHash))
                    {
                        bonePixelsDictionary[limbPixel.boneHash].vertexIndices.Add(i);
                    }
                    else
                    {
                        bonePixelsDictionary.Add(limbPixel.boneHash, new BonePixelsData());
                        bonePixelsDictionary[limbPixel.boneHash].vertexIndices.Add(i);
                    }
                }
            }
        }

        private static void MorphAllBones()
        {
            foreach (var limbDataSkeleton in GB.limbDataManager.limbData.limbDataSkeletons)
            {
                foreach (var bone in limbDataSkeleton.bones)
                {
                    MorphBone(bone);
                }
            }
        }

        private static void MorphBone(LimbDataBone bone)
        {
            if (bone.points.Count == 0)
                return;

            var bonePixelData = GetBonePixelsDataFromBoneDictionary(bone.boneHash);

            if (bonePixelData == null)
                return;

            switch (bone.boneHash)
            {
                case 35:    // head, shoulder center
                    MorphBoneGrow(bone, bonePixelData, Settings.Instance.HeadSize / 100f - 1f);
                    break;
                case 152:   // arms
                case 84:
                case 169:
                case 101:
                    MorphBoneStretch(bone, bonePixelData, new StretchParameters
                    {
                        curve = Curves.armsCurve,
                        power = Settings.Instance.ArmScale / 100f
                    });
                    break;
                case 272:   // legs
                case 305:
                case 220:
                case 237:
                    MorphBoneStretch(bone, bonePixelData, new StretchParameters
                    {
                        curve = Curves.legsCurve,
                        power = Settings.Instance.LegScale / 100f
                    });
                    break;
                // case 186:   // hands, feet
                // case 118:
                // case 242:
                // case 254:
                //     MorphBoneBloat(bone, bonePixelData, Settings.Instance.HeadSize / 25f);
                //     break;
            }
        }

        private static void MorphBoneBloat(LimbDataBone bone, BonePixelsData bonePixelData, float scale)
        {
            var boneStartPoint = new OpenTK.Vector2(bone.startPoint.X, bone.startPoint.Y);
            var boneEndPoint = new OpenTK.Vector2(bone.endPoint.X, bone.endPoint.Y);

            var boneVector = OpenTK.Vector2.Normalize(boneEndPoint - boneStartPoint);

            var indices = bonePixelData.vertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.limbDataManager.limbData.mesh.vertices[vertexIndex];
                var vertexPoint = new OpenTK.Vector2((float)vertex.X, (float)vertex.Y);
                var vertexWeight = GB.limbDataManager.limbData.mesh.GetVertexWeight(vertex);

                var distance = OpenTK.Vector2.Distance(vertexPoint, boneEndPoint);
                var directionVector = OpenTK.Vector2.Normalize(vertexPoint - boneStartPoint);

                vertex.X += (float)(distance * directionVector.X * scale * vertexWeight) / 2f;
                vertex.Y += (float)(distance * directionVector.Y * scale * vertexWeight) / 2f;
                vertex.Z = bone.boneHash;

                GB.limbDataManager.limbData.mesh.vertices[vertexIndex] = vertex;
            }
        }

        private static void MorphBoneGrow(LimbDataBone bone, BonePixelsData bonePixelData, float scale)
        {
            var boneStartPoint = new OpenTK.Vector2(bone.startPoint.X, bone.startPoint.Y);
            var boneEndPoint = new OpenTK.Vector2(bone.endPoint.X, bone.endPoint.Y);

            var boneVector = OpenTK.Vector2.Normalize(boneEndPoint - boneStartPoint);

            var indices = bonePixelData.vertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.limbDataManager.limbData.mesh.vertices[vertexIndex];
                var vertexPoint = new OpenTK.Vector2((float)vertex.X, (float)vertex.Y);
                var vertexWeight = GB.limbDataManager.limbData.mesh.GetVertexWeight(vertex);

                var distance = OpenTK.Vector2.Distance(vertexPoint, boneEndPoint);
                var directionVector = OpenTK.Vector2.Normalize(vertexPoint - boneEndPoint);

                vertex.X += (float)(distance * directionVector.X * scale * vertexWeight) / 2f;
                vertex.Y += (float)(distance * directionVector.Y * scale * vertexWeight) / 2f;
                vertex.Z = bone.boneHash;

                GB.limbDataManager.limbData.mesh.vertices[vertexIndex] = vertex;
            }
        }

        private static void MorphBoneStretch(LimbDataBone bone, BonePixelsData bonePixelData, StretchParameters stretchParameters)
        {
            stretchParameters.power -= 1f;          

            var boneStartPoint = new OpenTK.Vector2(bone.startPoint.X, bone.startPoint.Y);
            var boneEndPoint = new OpenTK.Vector2(bone.endPoint.X, bone.endPoint.Y);
            var boneLength = OpenTK.Vector2.Distance(boneStartPoint, boneEndPoint);

            var boneVector = OpenTK.Vector2.Normalize(boneEndPoint - boneStartPoint);

            var _perpendicularVector = Utils.GetPerpendicularVector(bone.startPoint, bone.endPoint);
            var perpendicularVector = new OpenTK.Vector2(_perpendicularVector.X, _perpendicularVector.Y);

            var indices = bonePixelData.vertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.limbDataManager.limbData.mesh.vertices[vertexIndex];
                var vertexPoint = new OpenTK.Vector2((float)vertex.X, (float)vertex.Y);
                var vertexWeight = GB.limbDataManager.limbData.mesh.GetVertexWeight(vertex);

                var _closestPointOnLine = Utils.GetClosestPointOnLine(bone.startPoint, bone.endPoint, new Vector3(vertexPoint.X, vertexPoint.Y, 0));
                var closestPointOnLine = new OpenTK.Vector2(_closestPointOnLine.X, _closestPointOnLine.Y);

                var progressOnLine = OpenTK.Vector2.Distance(closestPointOnLine, boneStartPoint) / boneLength;

                if (progressOnLine < 0 || progressOnLine > 1) continue;

                var distance = OpenTK.Vector2.Distance(vertexPoint, closestPointOnLine);
                var directionVector = OpenTK.Vector2.Normalize(vertexPoint - closestPointOnLine);

                var distanceMultiplier = distance / 2f;

                var curveScale = 1f + stretchParameters.power * stretchParameters.curve.Evaluate(progressOnLine);

                vertex.X += (float)(curveScale * directionVector.X * stretchParameters.power * distanceMultiplier * vertexWeight);
                vertex.Y += (float)(curveScale * directionVector.Y * stretchParameters.power * distanceMultiplier * vertexWeight);
                vertex.Z = bone.boneHash;

                GB.limbDataManager.limbData.mesh.vertices[vertexIndex] = vertex;
            }
        }

        private static void ProcessAllBoneWeights()
        {
            foreach (var limbDataSkeleton in GB.limbDataManager.limbData.limbDataSkeletons)
            {
                foreach (var bone in limbDataSkeleton.bones)
                {
                    switch (bone.boneHash)
                    {
                        case 35:    // head, shoulder center
                            ProcessBoneWeights(bone, new List<BonePixelsData>{ GetBonePixelsDataFromBoneDictionary(130), GetBonePixelsDataFromBoneDictionary(66) }); break;
                        case 152:   // arms
                            ProcessBoneWeights(bone, new List<BonePixelsData> { GetBonePixelsDataFromBoneDictionary(130), GetBonePixelsDataFromBoneDictionary(18), GetBonePixelsDataFromBoneDictionary(35) }); break;
                        case 84:
                            ProcessBoneWeights(bone, new List<BonePixelsData> { GetBonePixelsDataFromBoneDictionary(66), GetBonePixelsDataFromBoneDictionary(18), GetBonePixelsDataFromBoneDictionary(35) }); break;
                        case 169:
                            ProcessBoneWeights(bone, new List<BonePixelsData> { GetBonePixelsDataFromBoneDictionary(186) }); break;
                        case 101:
                            ProcessBoneWeights(bone, new List<BonePixelsData> { GetBonePixelsDataFromBoneDictionary(118) }); break;
                        case 272:   // legs
                            ProcessBoneWeights(bone, new List<BonePixelsData> { GetBonePixelsDataFromBoneDictionary(256), GetBonePixelsDataFromBoneDictionary(192), GetBonePixelsDataFromBoneDictionary(220) }); break;
                        case 305:
                            ProcessBoneWeights(bone, new List<BonePixelsData> { GetBonePixelsDataFromBoneDictionary(242) }); break;
                        case 220:
                            ProcessBoneWeights(bone, new List<BonePixelsData> { GetBonePixelsDataFromBoneDictionary(192), GetBonePixelsDataFromBoneDictionary(256), GetBonePixelsDataFromBoneDictionary(272) }); break;
                        case 237:
                            ProcessBoneWeights(bone, new List<BonePixelsData> { GetBonePixelsDataFromBoneDictionary(254) }); break;
                    }
                }
            }
        }

        private static void ProcessBoneWeights(LimbDataBone bone, List<BonePixelsData> pixelListsToCheck)
        {
            if (bone.points.Count == 0)
                return;

            var bonePixelData = GetBonePixelsDataFromBoneDictionary(bone.boneHash);

            if (bonePixelData == null)
                return;

            const float distanceLimit = 32f;
            var indices = bonePixelData.vertexIndices;

            Parallel.ForEach(indices, vertexIndex =>
            {
                var thisVertex = GB.limbDataManager.limbData.mesh.vertices[vertexIndex];
                var thisVertex2D = new Vector2(thisVertex.X, thisVertex.Y);
                // var vertexWeight = GB.limbDataManager.limbData.mesh.GetVertexWeight(thisVertex);

                var minDistance = float.MaxValue;

                foreach (var whitelistPixelList in pixelListsToCheck)
                {
                    if (whitelistPixelList != null)
                    {
                        foreach (var otherVertexIndex in whitelistPixelList.vertexIndices)
                        {
                            var otherVertex = GB.limbDataManager.limbData.mesh.vertices[otherVertexIndex];
                            var otherVertex2D = new Vector2(otherVertex.X, otherVertex.Y);

                            var distance = Vector2.Distance(thisVertex2D, otherVertex2D);

                            if (distance < minDistance)
                            {
                                minDistance = distance;
                            }
                        }
                    }
                }

                if (minDistance < distanceLimit)
                {
                    var progress = minDistance / distanceLimit;
                    GB.limbDataManager.limbData.mesh.vertexWeightsDictionary[thisVertex] = Curves.weightsSmoothingCurve.Evaluate(progress);
                }
            });
        }

        private static BonePixelsData GetBonePixelsDataFromBoneDictionary(int boneHash)
        {
            if (!bonePixelsDictionary.ContainsKey(boneHash)
                || bonePixelsDictionary[boneHash] == null
                || bonePixelsDictionary[boneHash].vertexIndices.Count == 0)
                return null;

            return bonePixelsDictionary[boneHash];
        }

        private struct StretchParameters
        {
            public Curve curve;
            public float power;
        }
    }
}