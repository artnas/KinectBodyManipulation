namespace KinectBodyModification
{
    public static class Curves
    {
        public static Curve LegsCurve = new Curve(new[] {0f, 0.05f, 0.3f, 0.5f, 0.3f, 0.05f, 0f});
        public static Curve ArmsCurve = new Curve(new[] {0f, 1f, 0f});
        public static Curve BlockyCurve = new Curve(new[] {0f, 0f, 0f, 1f, 0f, 0f, 0f});
        public static Curve WeightsSmoothingCurve = new Curve(new[] {0f, 0.8f, 1f});
        public static Curve SinHill = Curve.FromSin(0.0f, 1f);
        public static Curve InverseSinHill = Curve.FromSin(1.0f, 2f);
    }
}