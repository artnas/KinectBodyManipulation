using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using OpenTK;

namespace KinectBodyModification
{
    public class LimbDataBone
    {
        public readonly int BoneHash;
        public readonly JointTypePair JointTypePair;

        public readonly Joint StartJoint, EndJoint;

        public List<Vector3> Points = new List<Vector3>();

        public Vector3 StartPoint, EndPoint;

        public LimbDataBone(Joint startJoint, Joint endJoint)
        {
            this.StartJoint = startJoint;
            this.EndJoint = endJoint;

            JointTypePair = GetJointTypePair();

            BoneHash = Utils.GetBoneHash(JointTypePair.A, JointTypePair.B);
        }

        public Vector3 GetStartPoint()
        {
            return Points.First();
        }

        public Vector3 GetEndPoint()
        {
            return Points.Last();
        }

        private JointTypePair GetJointTypePair()
        {
            return new JointTypePair(StartJoint, EndJoint);
        }
    }
}