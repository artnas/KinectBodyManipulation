using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace KinectBodyModification
{
    public class BonePixelsData
    {
        public readonly HashSet<int> vertexIndices;

        public BonePixelsData()
        {
            vertexIndices = new HashSet<int>();
        }
    }
}