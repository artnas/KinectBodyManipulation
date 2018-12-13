using System.Threading.Tasks;
using OpenTK;
using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static partial class BoneProcessor
    {
        private static void MorphAllBones()
        {
            Parallel.ForEach(GB.LimbDataManager.LimbData.LimbDataSkeleton.Bones, MorphBone);
        }

        private static void MorphBone(LimbDataBone bone)
        {
            if (bone.Points.Count == 0)
                return;

            var bonePixelData = GetBonePixelsDataFromBoneDictionary(bone.BoneHash);

            if (bonePixelData == null)
                return;

            switch (bone.BoneHash)
            {
                case 35: // head, shoulder center
                    MorphBoneGrow(bone, bonePixelData, Settings.Instance.HeadSize / 100f - 1f);
                    break;
                case 152: // arms
                case 84:
                case 169:
                case 101:
                    MorphBoneStretch(bone, bonePixelData, new StretchParameters
                    {
                        Curve = Curves.ArmsCurve,
                        Power = Settings.Instance.ArmScale / 100f - 1f
                    });
                    break;
                case 272: // legs
                case 305:
                case 220:
                case 237:
                    MorphBoneStretch(bone, bonePixelData, new StretchParameters
                    {
                        Curve = Curves.LegsCurve,
                        Power = Settings.Instance.LegScale / 100f - 1f
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
            var boneStartPoint = new Vector2(bone.StartPoint.X, bone.StartPoint.Y);
            var boneEndPoint = new Vector2(bone.EndPoint.X, bone.EndPoint.Y);

            var indices = bonePixel.VertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.LimbDataManager.LimbData.Mesh.Vertices[vertexIndex];
                var vertexPoint = new Vector2(vertex.X, vertex.Y);
                var vertexWeight = GB.LimbDataManager.LimbData.Mesh.GetVertexWeight(vertex);

                var distance = Vector2.Distance(vertexPoint, boneEndPoint);
                var directionVector = Vector2.Normalize(vertexPoint - boneStartPoint);

                vertex.X += distance * directionVector.X * scale * vertexWeight / 2f;
                vertex.Y += distance * directionVector.Y * scale * vertexWeight / 2f;
                vertex.Z = bone.BoneHash;

                GB.LimbDataManager.LimbData.Mesh.Vertices[vertexIndex] = vertex;
            }
        }

        private static void MorphBoneGrow(LimbDataBone bone, BonePixels bonePixel, float scale)
        {
            var boneStartPoint = new Vector2(bone.StartPoint.X, bone.StartPoint.Y);
            var boneEndPoint = new Vector2(bone.EndPoint.X, bone.EndPoint.Y);

            var indices = bonePixel.VertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.LimbDataManager.LimbData.Mesh.Vertices[vertexIndex];
                var vertexPoint = new Vector2(vertex.X, vertex.Y);
                var vertexWeight = GB.LimbDataManager.LimbData.Mesh.GetVertexWeight(vertex);

                var distance = Vector2.Distance(vertexPoint, boneEndPoint);
                var directionVector = Vector2.Normalize(vertexPoint - boneEndPoint);

                vertex.X += distance * directionVector.X * scale * vertexWeight / 2f;
                vertex.Y += distance * directionVector.Y * scale * vertexWeight / 2f;
                vertex.Z = bone.BoneHash;

                GB.LimbDataManager.LimbData.Mesh.Vertices[vertexIndex] = vertex;
            }
        }

        private static void MorphBoneStretch(LimbDataBone bone, BonePixels bonePixel,
            StretchParameters stretchParameters)
        {
            var boneStartPoint = new Vector2(bone.StartPoint.X, bone.StartPoint.Y);
            var boneEndPoint = new Vector2(bone.EndPoint.X, bone.EndPoint.Y);
            var boneLength = Vector2.Distance(boneStartPoint, boneEndPoint);

            var indices = bonePixel.VertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.LimbDataManager.LimbData.Mesh.Vertices[vertexIndex];
                var vertexPoint = new Vector2(vertex.X, vertex.Y);
                var vertexWeight = GB.LimbDataManager.LimbData.Mesh.GetVertexWeight(vertex);

                var closestPointOnLine3D = Utils.GetClosestPointOnLine(bone.StartPoint, bone.EndPoint,
                    new Vector3(vertexPoint.X, vertexPoint.Y, 0));
                var closestPointOnLine2D = new Vector2(closestPointOnLine3D.X, closestPointOnLine3D.Y);

                var progressOnLine = Vector2.Distance(closestPointOnLine2D, boneStartPoint) / boneLength;

                if (progressOnLine < 0 || progressOnLine > 1)
                    continue;

                var distance = Vector2.Distance(vertexPoint, closestPointOnLine2D);
                var directionVector = Vector2.Normalize(vertexPoint - closestPointOnLine2D);

                var distanceMultiplier = distance / 2f;

                var curveScale = 1f + stretchParameters.Power * stretchParameters.Curve.Evaluate(progressOnLine);

                vertex.X += curveScale * directionVector.X * stretchParameters.Power * distanceMultiplier *
                            vertexWeight;
                vertex.Y += curveScale * directionVector.Y * stretchParameters.Power * distanceMultiplier *
                            vertexWeight;
                vertex.Z = bone.BoneHash;

                GB.LimbDataManager.LimbData.Mesh.Vertices[vertexIndex] = vertex;
            }
        }
    }
}