using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;

namespace KinectBodyModification
{
    /// <summary>
    /// GlobalBuffers - Global Buffers
    /// Klasa przechowywująca odniesienia do globalnych buforów wykorzystywanych w programie.
    /// </summary>
    public static class GlobalBuffers
    {
        public static DepthImagePixel[] depthBuffer;

        public static byte[] colorBuffer;
        public static byte[] savedBackgroundColorBuffer;

        public static byte[] outputBuffer;
        public static byte[] backgroundRemovedBuffer;

        public static int[] playerPixelData;

        public static DepthImagePixel[] savedBackgroundDepthBuffer;
        public static BackgroundRemovedColorStream backgroundRemovedColorStream;

        public static ColorImagePoint[] colorCoordinates;
        public static DepthImagePoint[] depthCoordinates;

        public static LimbDataManager limbDataManager;
    }
}
