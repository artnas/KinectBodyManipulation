using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;

namespace KinectBodyModification
{
    /// <summary>
    ///     GlobalBuffers - Global Buffers
    ///     Klasa przechowywująca odniesienia do globalnych buforów wykorzystywanych w programie.
    /// </summary>
    public static class GlobalBuffers
    {
        public static DepthImagePixel[] DepthBuffer;

        public static byte[] ColorBuffer;
        public static byte[] SavedBackgroundColorBuffer;

        public static byte[] OutputBuffer;
        public static byte[] BackgroundRemovedBuffer;

        public static DepthImagePixel[] SavedBackgroundDepthBuffer;
        public static BackgroundRemovedColorStream BackgroundRemovedColorStream;

        public static ColorImagePoint[] ColorCoordinates;
        public static DepthImagePoint[] DepthCoordinates;

        public static LimbDataManager LimbDataManager;
    }
}