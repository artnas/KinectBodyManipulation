using System.Collections.Generic;
using Microsoft.Kinect;

namespace KinectBodyModification
{
    public class LimbDataSkeleton
    {
        public List<LimbDataBone> Bones;
        public Skeleton Skeleton;

        public LimbDataSkeleton(Skeleton skeleton)
        {
            Skeleton = skeleton;

            Bones = new List<LimbDataBone>();

            foreach (JointPair jointPair in Utils.SkeletonIterator(skeleton))
                Bones.Add(new LimbDataBone(jointPair.A, jointPair.B));
        }

        public void Update(Skeleton skeleton)
        {
            Skeleton = skeleton;

            Bones = new List<LimbDataBone>();

            foreach (JointPair jointPair in Utils.SkeletonIterator(skeleton))
                Bones.Add(new LimbDataBone(jointPair.A, jointPair.B));
        }

        public LimbDataBone GetBoneByJointPair(JointPair jointPair)
        {
            return GetBoneByJointPair(jointPair.A.JointType, jointPair.B.JointType);
        }

        public LimbDataBone GetBoneByJointPair(Joint a, Joint b)
        {
            return GetBoneByJointPair(a.JointType, b.JointType);
        }

        public LimbDataBone GetBoneByJointPair(JointType a, JointType b)
        {
            foreach (var bone in Bones)
                if (bone.StartJoint.JointType == a && bone.EndJoint.JointType == b)
                    return bone;

            return null;
        }
    }
}