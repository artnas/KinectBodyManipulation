namespace KinectBodyModification
{
    public static class Curves
    {

        public static Curve legsCurve = new Curve(new[] { 0f, 0.05f, 0.3f, 0.5f, 0.3f, 0.05f, 0f });
        public static Curve armsCurve = new Curve(new[] { 0f, 1f, 0f });
        public static Curve blockyCurve = new Curve(new[] { 0f, 0f, 0f, 1f, 0f, 0f, 0f });
        public static Curve weightsSmoothingCurve = new Curve(new[] { 0f, 0.8f, 1f });
        public static Curve sinHill = Curve.FromSin(0.0f, 1f);
        public static Curve inverseSinHill = Curve.FromSin(1.0f, 2f);

    }
}
