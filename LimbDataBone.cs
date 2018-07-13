using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public class LimbDataBone
    {

        public Joint startJoint, endJoint;

        public List<Vector3> points = new List<Vector3>();

        public Vector3 GetStartPoint()
        {
            return points.First();
        }

        public Vector3 GetEndPoint()
        {
            return points.Last();
        }

        public LimbDataBone(Joint startJoint, Joint endJoint)
        {
            this.startJoint = startJoint;
            this.endJoint = endJoint;
        }

    }
}
