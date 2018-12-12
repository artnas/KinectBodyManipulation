using System.Collections.Generic;

namespace KinectBodyModification
{
    public class BonePixels
    {
        public readonly HashSet<int> VertexIndices;

        public BonePixels()
        {
            VertexIndices = new HashSet<int>();
        }
    }
}