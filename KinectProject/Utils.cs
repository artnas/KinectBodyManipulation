﻿using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Kinect;
using OpenTK;

namespace KinectBodyModification
{
    public static class Utils
    {
        public static readonly int[,] CardinalDirections =
        {
            {-1, 0}, // lewo
            {1, 0}, // prawo
            {0, -1}, // gora
            {0, 1} // dol
        };

        public static readonly int[,] OrdinalDirections =
        {
            {-1, 0}, // lewo
            {-1, -1}, // lewo gora
            {-1, 1}, // lewo dol
            {1, 0}, // prawo
            {1, -1}, // prawo gora
            {1, 1}, // prawo dol
            {0, -1}, // gora
            {0, 1} // dol 
        };

        /// <summary>
        ///     Offsety poszukiwania kolejnego punktu do obwodki
        /// </summary>
        public static readonly int[,] ContourSeekingDirections =
        {
            {0, 2},
            {1, 1},
            {1, 2},
            {2, 2},
            {2, 1},
            {2, 0},
            {1, -1},
            {2, -1},
            {2, -2},
            {1, -2},
            {0, -2},
            {-1, -2},
            {-1, -1},
            {-2, -2},
            {-2, -1},
            {-2, 0},
            {-2, 1},
            {-1, 1},
            {-2, 2},
            {-1, 2},
            {0, 3},
            {3, 0},
            {0, -3},
            {-3, 0}
        };

        public static readonly int ContourSeekingDirectionsCount = ContourSeekingDirections.GetLength(0);

        /// <summary>
        ///     Iteruje przez szkielet zwracajac wszystkie pary jointow w postaci: (joint nadrzedny, joint podrzedny)
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        public static IEnumerable SkeletonIterator(Skeleton skeleton)
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

        /// <summary>
        ///     Iteruje przez piksele pomiędzy punktami (from, to)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="onlyIncludePointsOnScreen"></param>
        /// <returns></returns>
        public static IEnumerable IteratePointsBetween(Vector3 from, Vector3 to, int width, int height,
            bool onlyIncludePointsOnScreen = true)
        {
            var v = to - from;

            var length = (int) Math.Ceiling(v.Length);

            v = Vector3.Normalize(v);

            var p = from;

            for (var i = 0; i < length; i++)
            {
                p += v;

                yield return new Vector3((int) p.X, (int) p.Y, (int) p.Z);
            }
        }

        public static List<Vector3> GetPointsBetween(Vector3 from, Vector3 to, int width, int height,
            bool onlyIncludePointsOnScreen = true)
        {
            var points = new List<Vector3>();

            foreach (Vector3 point in IteratePointsBetween(from, to, width, height, onlyIncludePointsOnScreen))
                points.Add(point);

            return points;
        }

        public static void GetPointsBetween(List<Vector3> list, Vector3 from, Vector3 to, int width, int height,
            bool onlyIncludePointsOnScreen = true)
        {
            list.Clear();

            foreach (Vector3 point in IteratePointsBetween(from, to, width, height, onlyIncludePointsOnScreen))
                list.Add(point);
        }

        // oblicza wektor prostopadly do prostej (a, b)
        public static Vector3 GetPerpendicularVector(Vector3 a, Vector3 b)
        {
            var v = b - a;

            return new Vector3(-v.Y, v.X, 0) / (float) Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        public static Vector3 GetClosestPointOnLine(Vector3 a, Vector3 b, Vector3 p)
        {
            var vVector1 = p - a;
            var vVector2 = Vector3.Normalize(b - a);

            var d = Vector3.Distance(a, b);
            var t = Vector3.Dot(vVector2, vVector1);

            if (t <= 0)
                return a;

            if (t >= d)
                return b;

            var vVector3 = vVector2 * t;

            var vClosestPoint = a + vVector3;

            return vClosestPoint;
        }

        /// <summary>
        ///     Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        public static Vector3 SkeletonPointToScreen(KinectSensor sensor, SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.

            var depthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, Configuration.DepthFormat);
            var colorPoint = sensor.CoordinateMapper.MapDepthPointToColorPoint(Configuration.DepthFormat, depthPoint, Configuration.ColorFormat);
            return new Vector3(colorPoint.X, colorPoint.Y, skelpoint.Z);
        }

        /// <summary>
        ///     Wylicza wartość będącą wynikiem interpolacji pomiędzy dwoma wartościami na podstawie podanych wag
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="aWeight"></param>
        /// <param name="bWeight"></param>
        /// <returns>obliczona wartość</returns>
        public static float Interpolate(float a, float b, float aWeight, float bWeight)
        {
            return (a * aWeight + b * bWeight) / (aWeight + bWeight);
        }

        public static float Interpolate(float a, float b, float v)
        {
            return Interpolate(a, b, 1f - v, v);
        }

        public static byte Interpolate(byte a, byte b, float v)
        {
            var value = Interpolate(a, b, v, 1f - v);

            if (value < 0)
                value = 0;
            if (value > 255)
                value = 255;

            return (byte) value;
        }

        public static int Interpolate(int a, int b, float v)
        {
            var value = Interpolate(a, b, v, 1f - v);

            return (int) value;
        }

        /// <summary>
        ///     Oblicza hash dla kosci sprecyzowanej przez jointy (a, b)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int GetBoneHash(JointType a, JointType b)
        {
            return (int) a | ((int) b << 4);
        }

        /// <summary>
        ///     Oblicza koordynaty piksela odpowiadajacemu podanemu indeksowi
        /// </summary>
        /// <param name="index"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void IndexToCoordinates(int index, ref int x, ref int y)
        {
            x = index % Configuration.Width;
            y = (index - x) / Configuration.Width;
        }

        /// <summary>
        ///     Oblicza indeks piksela odpowiadajcego podanym koordynatom
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int CoordinatesToIndex(int x, int y)
        {
            return x + y * Configuration.Width;
        }

        public static bool AreCoordinatesInBounds(int x, int y)
        {
            return x >= 0 && x < Configuration.Width && y >= 0 && y < Configuration.Height;
        }
    }
}