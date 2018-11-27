﻿namespace KinectBodyModification
{
    public class Settings
    {

        private static Settings _instance;
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Settings();
                }

                return _instance;
            }
        }

        public float HeadSize { get; set; }
        public float ArmScale { get; set; }
        public float LegScale { get; set; }
        public bool DebugDrawSkeleton { get; set; }
        public bool DebugDrawJoints { get; set; }
        public bool DebugDrawSilhouette { get; set; }
        public bool DrawMorphs { get; set; }
        public GLDrawModeEnum DrawMode { get; set; }
        public int OutlineSegmentation { get; set; }
        public int TriangleAreaLimit { get; set; }

        private Settings()
        {
            this.HeadSize = 100;
            this.ArmScale = 100;
            this.LegScale = 100;
            this.DebugDrawSkeleton = false;
            this.DebugDrawJoints = false;
            this.DebugDrawSilhouette = false;
            this.DrawMorphs = true;
            this.DrawMode = GLDrawModeEnum.Normal;
            this.OutlineSegmentation = 4;
            this.TriangleAreaLimit = 50;
        }

        public enum GLDrawModeEnum
        {
            Normal,
            Uvs,
            Lines
        }

    }
}
