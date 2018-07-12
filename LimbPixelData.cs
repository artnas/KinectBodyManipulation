using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public class LimbPixelData
    {

        public JointType startJointType;
        public JointType endJointType;
        public bool isBone = false;
        public sbyte humanIndex = -1;

    }
}
