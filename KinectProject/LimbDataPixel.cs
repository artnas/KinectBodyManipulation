namespace KinectBodyModification
{
    public class LimbDataPixel
    {
        public int boneHash = -1;
        public bool debugDraw;
        public sbyte humanIndex = -1;
        public bool isBone;
        public bool isContour;
        public bool isJoint;

        public void Clear()
        {
            boneHash = -1;
            humanIndex = -1;
            isBone = false;
            isJoint = false;
            debugDraw = false;
            isContour = false;
        }
    }
}