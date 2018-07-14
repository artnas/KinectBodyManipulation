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

        public int boneHash = -1;
        public sbyte humanIndex = -1;
        public bool isBone = false;
        public bool isJoint = false;
        public bool debugDraw = false;
        
        public void Clear()
        {
            this.boneHash = -1;
            this.humanIndex = -1;
            this.isBone = false;
            this.isJoint = false;
            this.debugDraw = false;
        }

    }
}
