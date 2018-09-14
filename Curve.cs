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

        public static Curve FromSin(float startRad, float endRad)
        {
            startRad *= (float)Math.PI;
            endRad *= (float)Math.PI;

            int length = 20;
            float[] values = new float[length];

            for (int i = 0; i < length; i++)
            {
                float progress = i / (float) length;
                values[i] = 1.0f + (float)Math.Sin(Utils.Interpolate(startRad, endRad, progress)) / 2f;
            }

            return new Curve(values);
        }

        public float Evaluate(float progress)
        {
            return Curve.Evaluate(values, progress);
        }

        public float Evaluate(float progress, float power)
        {
            var value = Curve.Evaluate(values, progress);

            return (float)Math.Pow(value, power);
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
