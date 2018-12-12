namespace KinectBodyModification
{
    public class Settings
    {
        public enum GlDrawModeEnum
        {
            Normal,
            Uvs,
            Lines
        }

        private static Settings _instance;

        private Settings()
        {
            HeadSize = 100;
            ArmScale = 100;
            LegScale = 100;
            DebugDrawSkeleton = false;
            DebugDrawJoints = false;
            DebugDrawSilhouette = false;
            DebugDrawOutline = false;
            DrawMorphs = true;
            DrawMode = GlDrawModeEnum.Normal;
            OutlineSegmentation = 4;
            TriangleAreaLimit = 50;
        }

        public static Settings Instance => _instance ?? (_instance = new Settings());

        public float HeadSize { get; set; }
        public float ArmScale { get; set; }
        public float LegScale { get; set; }
        public bool DebugDrawSkeleton { get; set; }
        public bool DebugDrawJoints { get; set; }
        public bool DebugDrawSilhouette { get; set; }
        public bool DebugDrawOutline { get; set; }
        public bool DrawMorphs { get; set; }
        public GlDrawModeEnum DrawMode { get; set; }
        public int OutlineSegmentation { get; set; }
        public int TriangleAreaLimit { get; set; }

        public bool ShouldDrawDebugOverlay()
        {
            return DebugDrawJoints || DebugDrawSilhouette || DebugDrawSkeleton || DebugDrawOutline;
        }
    }
}