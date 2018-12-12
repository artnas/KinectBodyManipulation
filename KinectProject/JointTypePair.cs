using Microsoft.Kinect;

namespace KinectBodyModification
{
    public struct JointTypePair
    {
        public JointType a;
        public JointType b;

        public JointTypePair(Joint a, Joint b)
        {
            this.a = a.JointType;
            this.b = b.JointType;
        }

        public JointTypePair(JointType a, JointType b)
        {
            this.a = a;
            this.b = b;
        }
    }
}