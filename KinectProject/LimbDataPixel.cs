namespace KinectBodyModification
{
    public class LimbDataPixel
    {
        public int BoneHash = -1;
        public bool DebugDraw;
        public sbyte HumanIndex = -1;
        public bool IsBone;
        public bool IsContour;
        public bool IsJoint;

        public void Clear()
        {
            BoneHash = -1;
            HumanIndex = -1;
            IsBone = false;
            IsJoint = false;
            DebugDraw = false;
            IsContour = false;
        }
    }
}