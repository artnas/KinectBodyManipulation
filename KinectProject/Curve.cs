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
            startRad *= (float) Math.PI;
            endRad *= (float) Math.PI;

            var length = 100;
            var values = new float[length];

            var halfLength = length / 2;
            for (var i = 0; i < halfLength; i++)
            {
                var progress = i / (float) (length - 1);

                var value = (float) Math.Sin(Utils.Interpolate(startRad, endRad, progress)) / 2f;

                values[i] = value;
                values[length - 1 - i] = value;
            }

            return new Curve(values);
        }

        public float Evaluate(float progress)
        {
            return Evaluate(values, progress);
        }

        public float Evaluate(float progress, float power)
        {
            var value = Evaluate(values, progress);

            return (float) Math.Pow(value, power);
        }

        public static float Evaluate(float[] values, float progress)
        {
            if (progress < 0)
                progress = 0;
            else if (progress > 1)
                progress = 1;

            progress *= values.Length - 1;

            var floorIndex = (int) Math.Floor(progress);
            var ceilingIndex = (int) Math.Ceiling(progress);

            return Utils.Interpolate(values[floorIndex], values[ceilingIndex], progress % 1f);
        }
    }
}