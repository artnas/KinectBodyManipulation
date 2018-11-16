using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;

namespace KinectBodyModification
{
    public static class GlobalBuffers
    {
        public static DepthImagePixel[] depthBuffer;

        public static byte[] colorBuffer;
        public static byte[] outputBuffer;
        public static byte[] backgroundRemovedBuffer;
        public static byte[] normalBuffer;

        public static int[] playerPixelData;

        public static byte[] savedBackgroundColorBuffer;
        public static DepthImagePixel[] savedBackgroundDepthBuffer;
        public static BackgroundRemovedColorStream backgroundRemovedColorStream;

        public static ColorImagePoint[] colorCoordinates;
        public static DepthImagePoint[] depthCoordinates;

        public static LimbDataManager limbDataManager;
    }
}
