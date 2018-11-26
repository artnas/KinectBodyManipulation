using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace KinectBodyModification
{
    public class BonePixelsData
    {
        // public readonly HashSet<int> pixelIndices;
        public readonly HashSet<int> vertexIndices;

        public BonePixelsData()
        {
            // pixelIndices = new HashSet<int>();
            vertexIndices = new HashSet<int>();
        }
    }
}