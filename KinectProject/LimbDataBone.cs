using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Kinect;

namespace KinectBodyModification
{
    public class LimbDataBone
    {

        public readonly Joint startJoint, endJoint;
        public readonly JointTypePair jointTypePair;
        public readonly int boneHash;

        public List<Vector3> points = new List<Vector3>();

        public Vector3 startPoint, endPoint;

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

        public LimbDataBone(Joint startJoint, Joint endJoint)
        {
            this.startJoint = startJoint;
            this.endJoint = endJoint;

            this.jointTypePair = GetJointTypePair();

            this.boneHash = Utils.GetBoneHash(jointTypePair.a, jointTypePair.b);
        }

    }
}
