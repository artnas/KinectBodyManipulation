using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Media;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    public static class Utils
    {

        public static KinectSensor sensor;

        public static readonly Color[] limbColors = {
            Color.FromRgb(255,255,255),

            Color.FromRgb(255,255,0),
            Color.FromRgb(255,0,0),

            Color.FromRgb(0,255,0),
            Color.FromRgb(255,255,0),

            Color.FromRgb(0,0,255),
            Color.FromRgb(0,255,255),
        };

        public static readonly int[,] cardinalDirections =
        {
            { -1, 0 },  // lewo
            { 1, 0 },   // prawo
            { 0, -1 },  // gora
            { 0, 1 },   // dol
        };

        public static readonly int[,] ordinalDirections =
        {
            { -1, 0 },      // lewo
            { -1, 1 },      // lewo gora
            { -1, -1 },     // lewo dol
            { 1, 0 },       // prawo
            { 1, 1 },       // prawo gora
            { 1, -1 },      // prawo dol
            { 0, 1 },      // gora
            { 0, 1 },       // dol 
        };

        /// <summary>
        /// Iteruje przez szkielet zwracajac wszystkie pary jointow w postaci: (joint nadrzedny, joint podrzedny)
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        public static System.Collections.IEnumerable SkeletonIterator(Skeleton skeleton)
        {
            // glowa, tors
            yield return new JointPair(skeleton.Joints[JointType.Head], skeleton.Joints[JointType.ShoulderCenter]);
            yield return new JointPair(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft]);
            yield return new JointPair(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight]);
            yield return new JointPair(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Spine]);
            yield return new JointPair(skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.HipCenter]);
            yield return new JointPair(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipLeft]);
            yield return new JointPair(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipRight]);

            // lewe ramie
            yield return new JointPair(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft]);
            yield return new JointPair(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft]);
            yield return new JointPair(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);

            // prawe ramie
            yield return new JointPair(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight]);
            yield return new JointPair(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]);
            yield return new JointPair(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight]);

            // lewa noga
            yield return new JointPair(skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft]);
            yield return new JointPair(skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.AnkleLeft]);
            yield return new JointPair(skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.FootLeft]);

            // prawa noga
            yield return new JointPair(skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight]);
            yield return new JointPair(skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.AnkleRight]);
            yield return new JointPair(skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.FootRight]);
        }

        public static System.Collections.IEnumerable IteratePointsBetween(Vector3 from, Vector3 to, int width, int height, bool onlyIncludePointsOnScreen = true)
        {

            float a = (float)(to.Y - from.Y) / (to.X - from.X);
            float b = (from.Y) - a * from.X;

            if (from.X > to.X)
            {
                var temp = from.X;
                from.X = to.X;
                to.X = temp;
            }

            if (from.Y > to.Y)
            {
                var temp = from.Y;
                from.Y = to.Y;
                to.Y = temp;
            }

            float xDistance = to.X - from.X;
            float yDistance = to.Y - from.Y;
            float zDistance = to.Z - from.Z;

            for (int x = (int)Math.Floor(from.X); x <= to.X; x++)
            {

                int y = (int)(a * x + b);

                if (onlyIncludePointsOnScreen && (x < 0 || x >= width || y < 0 || y >= height))
                    continue;

                float progress = (to.X - x) / xDistance;
                float z = Vector3.Lerp(from, to, progress).Z;

                yield return new Vector3(x, y, z);

            }

            for (int y = (int)Math.Floor(from.Y); y <= to.Y; y++)
            {

                int x;

                if (from.X != to.X)
                    x = (int)((y - b) / a);
                else
                    x = (int)(from.X);

                if (onlyIncludePointsOnScreen && (x < 0 || x >= width || y < 0 || y >= height))
                    continue;

                float progress = (to.Y - y) / yDistance;
                float z = Vector3.Lerp(from, to, progress).Z;

                yield return new Vector3(x, y, z);

            }

        }

        public static List<Vector3> GetPointsBetween(Vector3 from, Vector3 to, int width, int height, bool onlyIncludePointsOnScreen = true)
        {
            List<Vector3> points = new List<Vector3>();

            foreach (Vector3 point in IteratePointsBetween(from, to, width, height, onlyIncludePointsOnScreen))
            {
                points.Add(point);
            }

            return points;
        }

        public static void GetPointsBetween(List<Vector3> list, Vector3 from, Vector3 to, int width, int height, bool onlyIncludePointsOnScreen = true)
        {
            List<Vector3> points = list;
            list.Clear();

            foreach (Vector3 point in IteratePointsBetween(from, to, width, height, onlyIncludePointsOnScreen))
            {
                points.Add(point);
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        public static Vector3 SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            var depthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            var colorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution640x480Fps30, depthPoint, ColorImageFormat.RgbResolution640x480Fps30);
            return new Vector3(colorPoint.X, colorPoint.Y, skelpoint.Z);
        }

        public static float Interpolate(float a, float b, float aWeight, float bWeight)
        {
            return (a * aWeight + b * bWeight) / (aWeight + bWeight);
        }

        public static float Interpolate(float a, float b, float v)
        {
            return Interpolate(a, b, v, 1f - v);
        }

        public static byte Interpolate(byte a, byte b, float v)
        {
            float value = Interpolate((float)a, (float)b, v, 1f - v);

            if (value < 0)
                value = 0;
            if (value > 255)
                value = 255;

            return (byte) value;
        }

    }
}
