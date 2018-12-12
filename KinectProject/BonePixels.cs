using System.Collections.Generic;

namespace KinectBodyModification
{
    public class BonePixels
    {
        public readonly HashSet<int> vertexIndices;

        public BonePixels()
        {
            vertexIndices = new HashSet<int>();
        }
    }
}