﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Kinect;

using GB = KinectBodyModification.GlobalBuffers;
// using Vector2 = OpenTK.Vector2;

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

            foreach (var limbDataSkeleton in GB.limbDataManager.limbData.limbDataSkeletons)
            {
                foreach (var bone in limbDataSkeleton.bones)
                {
                    ProcessBone(bone);
                }
            }
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

        private static void ProcessBone(LimbDataBone bone)
        {
            if (bone.points.Count == 0)
                return;

            if (!bonePixelsDictionary.ContainsKey(bone.boneHash)
                || bonePixelsDictionary[bone.boneHash] == null
                || bonePixelsDictionary[bone.boneHash].vertexIndices.Count == 0)
                return;

            var bonePixelData = bonePixelsDictionary[bone.boneHash];

            switch (bone.boneHash)
            {
                case 35:    // head, shoulder center
                    ProcessBone_Grow(bone, bonePixelData, Settings.Instance.HeadSize / 100f - 1f);
                    break;
                case 152:   // arms
                case 84:
                case 169:
                case 101:
                    ProcessBone_Stretch(bone, bonePixelData, new StretchParameters
                    {
                        curve = Curves.steepHillCurve,
                        power = Settings.Instance.ArmScale / 100f
                    });
                    break;
                case 272:   // legs
                case 305:
                case 220:
                case 237:
                    ProcessBone_Stretch(bone, bonePixelData, new StretchParameters
                    {
                        curve = Curves.hillCurve,
                        power = Settings.Instance.LegScale / 100f
                    });
                    break;
                // case 186:   // hands, feet
                // case 118:
                // case 242:
                // case 254:
                //     ProcessBone_Bloat(bone, bonePixelData, Settings.Instance.HeadSize / 25f);
                //     break;
                default:
                    // ProcessBone_Normal(bone, bonePixelData);
                    break;
            }
        }

        private static void ProcessBone_Bloat(LimbDataBone bone, BonePixelsData bonePixelData, float scale)
        {
            var boneStartPoint = new OpenTK.Vector2(bone.startPoint.X, bone.startPoint.Y);
            var boneEndPoint = new OpenTK.Vector2(bone.endPoint.X, bone.endPoint.Y);

            var boneVector = OpenTK.Vector2.Normalize(boneEndPoint - boneStartPoint);

            var indices = bonePixelData.vertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.limbDataManager.limbData.mesh.vertices[vertexIndex];
                var vertexPoint = new OpenTK.Vector2((float)vertex.X, (float)vertex.Y);

                var distance = OpenTK.Vector2.Distance(vertexPoint, boneEndPoint);
                var directionVector = OpenTK.Vector2.Normalize(vertexPoint - boneStartPoint);

                vertex.X += (float)(distance * directionVector.X * scale) / 2f;
                vertex.Y += (float)(distance * directionVector.Y * scale) / 2f;
                vertex.Z = bone.boneHash;

                GB.limbDataManager.limbData.mesh.vertices[vertexIndex] = vertex;
            }
        }

        private static void ProcessBone_Grow(LimbDataBone bone, BonePixelsData bonePixelData, float scale)
        {
            var boneStartPoint = new OpenTK.Vector2(bone.startPoint.X, bone.startPoint.Y);
            var boneEndPoint = new OpenTK.Vector2(bone.endPoint.X, bone.endPoint.Y);

            var boneVector = OpenTK.Vector2.Normalize(boneEndPoint - boneStartPoint);

            var indices = bonePixelData.vertexIndices;

            foreach (var vertexIndex in indices)
            {
                var vertex = GB.limbDataManager.limbData.mesh.vertices[vertexIndex];
                var vertexPoint = new OpenTK.Vector2((float)vertex.X, (float)vertex.Y);

                var distance = OpenTK.Vector2.Distance(vertexPoint, boneEndPoint);
                var directionVector = OpenTK.Vector2.Normalize(vertexPoint - boneEndPoint);

                vertex.X += (float)(distance * directionVector.X * scale) / 2f;
                vertex.Y += (float)(distance * directionVector.Y * scale) / 2f;
                vertex.Z = bone.boneHash;

                GB.limbDataManager.limbData.mesh.vertices[vertexIndex] = vertex;
            }
        }

        private static void ProcessBone_Stretch(LimbDataBone bone, BonePixelsData bonePixelData, StretchParameters stretchParameters)
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

                var _closestPointOnLine = Utils.GetClosestPointOnLine(bone.startPoint, bone.endPoint, new Vector3(vertexPoint.X, vertexPoint.Y, 0));
                var closestPointOnLine = new OpenTK.Vector2(_closestPointOnLine.X, _closestPointOnLine.Y);

                var progressOnLine = OpenTK.Vector2.Distance(closestPointOnLine, boneStartPoint) / boneLength;

                if (progressOnLine < 0 || progressOnLine > 1) continue;

                var distance = OpenTK.Vector2.Distance(vertexPoint, closestPointOnLine);
                var directionVector = OpenTK.Vector2.Normalize(vertexPoint - closestPointOnLine);

                var distanceMultiplier = distance / 2f;

                var curveScale = 1f + stretchParameters.power * stretchParameters.curve.Evaluate(progressOnLine);

                vertex.X += (float)(curveScale * directionVector.X * stretchParameters.power * distanceMultiplier);
                vertex.Y += (float)(curveScale * directionVector.Y * stretchParameters.power * distanceMultiplier);
                vertex.Z = bone.boneHash;

                GB.limbDataManager.limbData.mesh.vertices[vertexIndex] = vertex;
            }
        }

        private struct StretchParameters
        {
            public Curve curve;
            public float power;
        }
    }
}