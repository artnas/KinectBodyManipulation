using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public class Curve
    {

        private float[] values;

        public Curve(float[] values)
        {
            this.values = values;
        }

        public float Evaluate(float progress)
        {
            return Curve.Evaluate(values, progress);
        }

        public static float Evaluate(float[] values, float progress)
        {
            if (progress < 0)
                progress = 0;
            else if (progress > 1)
                progress = 1;

            progress *= values.Length - 1;

            int floorIndex = (int)Math.Floor(progress);
            int ceilingIndex = (int)Math.Ceiling(progress);

            return Utils.Interpolate(values[floorIndex], values[ceilingIndex], progress % 1f);
        }

    }
}
