using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public class LimbDataPixel
    {

        public JointType startJointType;
        public JointType endJointType;
        public bool isBone = false;
        public bool isJoint = false;
        public sbyte humanIndex = -1;

    }
}
