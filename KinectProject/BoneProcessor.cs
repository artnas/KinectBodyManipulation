using System;
using System.Collections.Generic;
using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static partial class BoneProcessor
    {
        private static readonly Dictionary<int, BonePixels> bonePixelsDictionary = new Dictionary<int, BonePixels>(20);

        public static void ProcessAllBones()
        {
            AssignBonePixelsToDictionaries();
            ProcessAllBoneWeights();

            if (Settings.Instance.DrawMorphs) MorphAllBones();
        }

        private static void AssignBonePixelsToDictionaries()
        {
            // wyczysc listy
            foreach (var entry in bonePixelsDictionary) entry.Value.vertexIndices.Clear();

            for (var i = 0; i < GB.limbDataManager.limbData.mesh.vertices.Count; i++)
            {
                var vertex = GB.limbDataManager.limbData.mesh.vertices[i];

                var x = (int) Math.Round(vertex.X);
                var y = (int) Math.Round(vertex.Y);

                var index = Utils.CoordinatesToIndex(x, y);

                var limbPixel = GB.limbDataManager.limbData.allPixels[index];

                if (limbPixel.humanIndex != -1)
                {
                    if (bonePixelsDictionary.ContainsKey(limbPixel.boneHash))
                    {
                        bonePixelsDictionary[limbPixel.boneHash].vertexIndices.Add(i);
                    }
                    else
                    {
                        bonePixelsDictionary.Add(limbPixel.boneHash, new BonePixels());
                        bonePixelsDictionary[limbPixel.boneHash].vertexIndices.Add(i);
                    }
                }
            }
        }

        private static BonePixels GetBonePixelsDataFromBoneDictionary(int boneHash)
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