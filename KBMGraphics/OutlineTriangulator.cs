using System;
using System.Collections.Generic;
using KinectBodyModification;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

namespace KBMGraphics
{
    public class OutlineTriangulator
    {
        public readonly int Width, Height;
        private readonly QualityOptions _qualityOptions;
        private readonly List<Vertex> _pointsList = new List<Vertex>();

        public OutlineTriangulator(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            _qualityOptions = new QualityOptions();
        }

        public TriangleNet.Mesh GetMesh(HashSet<int> contourIndices)
        {
            if (contourIndices == null || contourIndices.Count < 4) return null;

            var contour = new Contour(GetContourPoints(contourIndices), 0, false);
            var poly = new Polygon(contourIndices.Count);

            poly.Add(contour);

            try
            {
                var mesh = poly.Triangulate(
                    new ConstraintOptions {ConformingDelaunay = true, Convex = false, SegmentSplitting = 0},
                    new QualityOptions
                    {
                        MinimumAngle = 25,
                        MaximumArea = Settings.Instance.TriangleAreaLimit,
                        VariableArea = true
                    }
                ) as TriangleNet.Mesh;

                return mesh;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }

        private List<Vertex> GetContourPoints(HashSet<int> contourIndices)
        {
            _pointsList.Clear();

            var segmentation = Settings.Instance.OutlineSegmentation;

            var counter = 0;
            foreach (var index in contourIndices)
            {
                if (contourIndices.Count > segmentation * 3 && counter % segmentation != 0)
                {
                    counter++;
                    continue;
                }

                counter++;

                var x = index % Width;
                var y = (index - x) / Width;

                _pointsList.Add(new Vertex(x, y));
            }

            var sList = new List<string>();
            foreach (var s in _pointsList) sList.Add(s.X + " " + s.Y);

            // File.WriteAllLines("C:\\a.txt", sList);

            return _pointsList;
        }
    }
}