using System;
using System.Collections.Generic;
using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    public static partial class BoneProcessor
    {
        private static readonly Dictionary<int, BonePixels> BonePixelsDictionary = new Dictionary<int, BonePixels>(20);

        public static void ProcessAllBones()
        {
            AssignBonePixelsToDictionaries();
            ProcessAllBoneWeights();

            if (Settings.Instance.DrawMorphs) MorphAllBones();
        }

        private static void AssignBonePixelsToDictionaries()
        {
            // wyczysc listy
            foreach (var entry in BonePixelsDictionary) entry.Value.VertexIndices.Clear();

            for (var i = 0; i < GB.LimbDataManager.LimbData.Mesh.Vertices.Count; i++)
            {
                var vertex = GB.LimbDataManager.LimbData.Mesh.Vertices[i];

                var x = (int) Math.Round(vertex.X);
                var y = (int) Math.Round(vertex.Y);

                var index = Utils.CoordinatesToIndex(x, y);

                var limbPixel = GB.LimbDataManager.LimbData.AllPixels[index];

                if (limbPixel.HumanIndex != -1)
                {
                    if (BonePixelsDictionary.ContainsKey(limbPixel.BoneHash))
                    {
                        BonePixelsDictionary[limbPixel.BoneHash].VertexIndices.Add(i);
                    }
                    else
                    {
                        BonePixelsDictionary.Add(limbPixel.BoneHash, new BonePixels());
                        BonePixelsDictionary[limbPixel.BoneHash].VertexIndices.Add(i);
                    }
                }
            }
        }

        private static BonePixels GetBonePixelsDataFromBoneDictionary(int boneHash)
        {
            if (!BonePixelsDictionary.ContainsKey(boneHash)
                || BonePixelsDictionary[boneHash] == null
                || BonePixelsDictionary[boneHash].VertexIndices.Count == 0)
                return null;

            return BonePixelsDictionary[boneHash];
        }

        private struct StretchParameters
        {
            public Curve Curve;
            public float Power;
        }
    }
}