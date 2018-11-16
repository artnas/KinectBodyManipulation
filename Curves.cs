namespace KinectBodyModification
{
    public static class Curves
    {

        public static Curve hillCurve = new Curve(new[] { 1f, 1.05f, 1.3f, 1.5f, 1.3f, 1.05f, 1f });
        public static Curve test = new Curve(new[] { 0f, 0f, 1f, 0f, 0f });
        public static Curve sinHill = Curve.FromSin(0.0f, 1f);
        public static Curve inverseSinHill = Curve.FromSin(1.0f, 2f);

    }
}
