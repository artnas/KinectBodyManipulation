using System;

namespace KinectBodyModification
{
    public class Curve
    {

        private readonly float[] values;

        public Curve(float[] values)
        {
            this.values = values;
        }

        public static Curve FromSin(float startRad, float endRad)
        {
            startRad *= (float)Math.PI;
            endRad *= (float)Math.PI;

            int length = 100;
            float[] values = new float[length];

            // for (int i = 0; i < length; i++)
            // {
            //     float progress = i / (float)(length-1);
            //     if (i == length - 2) progress = 1;
            //
            //     values[i] = (float)Math.Sin(Utils.Interpolate(startRad, endRad, progress)) / 2f;
            // }

            int halfLength = length / 2;
            for (int i = 0; i < halfLength; i++)
            {
                float progress = i / (float)(length - 1);

                float value = (float) Math.Sin(Utils.Interpolate(startRad, endRad, progress)) / 2f;

                values[i] = value;
                values[length - 1 - i] = value;
            }

            // values[length - 2] = values[1];
            // values[length - 1] = 0;

            return new Curve(values);
        }

        public float Evaluate(float progress)
        {
            return Evaluate(values, progress);
        }

        public float Evaluate(float progress, float power)
        {
            var value = Evaluate(values, progress);

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
