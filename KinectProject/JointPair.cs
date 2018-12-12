using Microsoft.Kinect;

namespace KinectBodyModification
{
    public struct JointPair
    {
        public Joint a;
        public Joint b;

        public JointPair(Joint a, Joint b)
        {
            this.a = a;
            this.b = b;
        }
    }
}