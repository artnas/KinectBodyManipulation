using Microsoft.Kinect;

namespace KinectBodyModification
{
    public struct JointTypePair
    {
        public JointType A;
        public JointType B;

        public JointTypePair(Joint a, Joint b)
        {
            A = a.JointType;
            B = b.JointType;
        }

        public JointTypePair(JointType a, JointType b)
        {
            A = a;
            B = b;
        }
    }
}