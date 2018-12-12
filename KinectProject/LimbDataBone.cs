using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using OpenTK;

namespace KinectBodyModification
{
    public class LimbDataBone
    {
        public readonly int boneHash;
        public readonly JointTypePair jointTypePair;

        public readonly Joint startJoint, endJoint;

        public List<Vector3> points = new List<Vector3>();

        public Vector3 startPoint, endPoint;

        public LimbDataBone(Joint startJoint, Joint endJoint)
        {
            this.startJoint = startJoint;
            this.endJoint = endJoint;

            jointTypePair = GetJointTypePair();

            boneHash = Utils.GetBoneHash(jointTypePair.a, jointTypePair.b);
        }

        public Vector3 GetStartPoint()
        {
            return points.First();
        }

        public Vector3 GetEndPoint()
        {
            return points.Last();
        }

        private JointTypePair GetJointTypePair()
        {
            return new JointTypePair(startJoint, endJoint);
        }
    }
}