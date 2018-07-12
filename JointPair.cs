using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public struct JointPair
    {

        public Joint a;
        public Joint b;

        public JointPair(Joint a, Joint b)
        {
            this.a = a;
            this.b = b;
        }

    }
}
