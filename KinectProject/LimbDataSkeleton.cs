using System.Collections.Generic;
using Microsoft.Kinect;

namespace KinectBodyModification
{
    public class LimbDataSkeleton
    {
        public List<LimbDataBone> bones;
        public Skeleton skeleton;

        public LimbDataSkeleton(Skeleton skeleton)
        {
            this.skeleton = skeleton;

            bones = new List<LimbDataBone>();

            foreach (JointPair jointPair in Utils.SkeletonIterator(skeleton))
                bones.Add(new LimbDataBone(jointPair.a, jointPair.b));
        }

        public LimbDataBone GetBoneByJointPair(JointPair jointPair)
        {
            return GetBoneByJointPair(jointPair.a.JointType, jointPair.b.JointType);
        }

        public LimbDataBone GetBoneByJointPair(Joint a, Joint b)
        {
            return GetBoneByJointPair(a.JointType, b.JointType);
        }

        public LimbDataBone GetBoneByJointPair(JointType a, JointType b)
        {
            foreach (var bone in bones)
                if (bone.startJoint.JointType == a && bone.endJoint.JointType == b)
                    return bone;

            return null;
        }
    }
}