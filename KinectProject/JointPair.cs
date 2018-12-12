using Microsoft.Kinect;

namespace KinectBodyModification
{
    public struct JointPair
    {
        public Joint A;
        public Joint B;

        public JointPair(Joint a, Joint b)
        {
            A = a;
            B = b;
        }
    }
}