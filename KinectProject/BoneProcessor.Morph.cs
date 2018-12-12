using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK;
using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static partial class BoneProcessor
    {

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
                    //     MorphBoneBloat(bone, bonePixel, Settings.Instance.HeadSize / 25f);
                    //     break;
            }
        }

        private static void MorphBoneBloat(LimbDataBone bone, BonePixels bonePixel, float scale)
        {
            var boneStartPoint = new Vector2(bone.startPoint.X, bone.startPoint.Y);
            var boneEndPoint = new Vector2(bone.endPoint.X, bone.endPoint.Y);

            var indices = bonePixel.vertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.limbDataManager.limbData.mesh.vertices[vertexIndex];
                var vertexPoint = new Vector2(vertex.X, vertex.Y);
                var vertexWeight = GB.limbDataManager.limbData.mesh.GetVertexWeight(vertex);

                var distance = Vector2.Distance(vertexPoint, boneEndPoint);
                var directionVector = Vector2.Normalize(vertexPoint - boneStartPoint);

                vertex.X += distance * directionVector.X * scale * vertexWeight / 2f;
                vertex.Y += distance * directionVector.Y * scale * vertexWeight / 2f;
                vertex.Z = bone.boneHash;

                GB.limbDataManager.limbData.mesh.vertices[vertexIndex] = vertex;
            }
        }

        private static void MorphBoneGrow(LimbDataBone bone, BonePixels bonePixel, float scale)
        {
            var boneStartPoint = new Vector2(bone.startPoint.X, bone.startPoint.Y);
            var boneEndPoint = new Vector2(bone.endPoint.X, bone.endPoint.Y);

            var indices = bonePixel.vertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.limbDataManager.limbData.mesh.vertices[vertexIndex];
                var vertexPoint = new Vector2(vertex.X, vertex.Y);
                var vertexWeight = GB.limbDataManager.limbData.mesh.GetVertexWeight(vertex);

                var distance = Vector2.Distance(vertexPoint, boneEndPoint);
                var directionVector = Vector2.Normalize(vertexPoint - boneEndPoint);

                vertex.X += distance * directionVector.X * scale * vertexWeight / 2f;
                vertex.Y += distance * directionVector.Y * scale * vertexWeight / 2f;
                vertex.Z = bone.boneHash;

                GB.limbDataManager.limbData.mesh.vertices[vertexIndex] = vertex;
            }
        }

        private static void MorphBoneStretch(LimbDataBone bone, BonePixels bonePixel, StretchParameters stretchParameters)
        {
            stretchParameters.power -= 1f;

            var boneStartPoint = new Vector2(bone.startPoint.X, bone.startPoint.Y);
            var boneEndPoint = new Vector2(bone.endPoint.X, bone.endPoint.Y);
            var boneLength = Vector2.Distance(boneStartPoint, boneEndPoint);

            var indices = bonePixel.vertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.limbDataManager.limbData.mesh.vertices[vertexIndex];
                var vertexPoint = new Vector2(vertex.X, vertex.Y);
                var vertexWeight = GB.limbDataManager.limbData.mesh.GetVertexWeight(vertex);

                var closestPointOnLine3D = Utils.GetClosestPointOnLine(bone.startPoint, bone.endPoint, new Vector3(vertexPoint.X, vertexPoint.Y, 0));
                var closestPointOnLine2D = new Vector2(closestPointOnLine3D.X, closestPointOnLine3D.Y);

                var progressOnLine = Vector2.Distance(closestPointOnLine2D, boneStartPoint) / boneLength;

                if (progressOnLine < 0 || progressOnLine > 1)
                    continue;

                var distance = Vector2.Distance(vertexPoint, closestPointOnLine2D);
                var directionVector = Vector2.Normalize(vertexPoint - closestPointOnLine2D);

                var distanceMultiplier = distance / 2f;

                var curveScale = 1f + stretchParameters.power * stretchParameters.curve.Evaluate(progressOnLine);

                vertex.X += curveScale * directionVector.X * stretchParameters.power * distanceMultiplier * vertexWeight;
                vertex.Y += curveScale * directionVector.Y * stretchParameters.power * distanceMultiplier * vertexWeight;
                vertex.Z = bone.boneHash;

                GB.limbDataManager.limbData.mesh.vertices[vertexIndex] = vertex;
            }
        }

    }
}
