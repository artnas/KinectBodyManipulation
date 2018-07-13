using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
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
