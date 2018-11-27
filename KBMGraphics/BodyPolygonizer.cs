using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectBodyModification;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

namespace KBMGraphics
{
    public class BodyPolygonizer
    {

        public readonly int width, height;
        private readonly QualityOptions QualityOptions;
        private List<Vertex> pointsList = new List<Vertex>();

        public BodyPolygonizer(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.QualityOptions = new QualityOptions();
        }

        public Mesh GetMesh(HashSet<int> contourIndices)
        {
            if (contourIndices == null || contourIndices.Count < 4)
            {
                return null;
            }

            var contour = new Contour(GetContourPoints(contourIndices), 0, false);
            var poly = new Polygon(contourIndices.Count);

            poly.Add(contour);

            try
            {
                var mesh = poly.Triangulate(
                    new ConstraintOptions {ConformingDelaunay = true, Convex = false, SegmentSplitting = 0},
                    new QualityOptions {MinimumAngle = 25, MaximumArea = Settings.Instance.TriangleAreaLimit, VariableArea = true}
                ) as Mesh;

                return mesh;
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        private List<Vertex> GetContourPoints(HashSet<int> contourIndices)
        {
            pointsList.Clear();

            var segmentation = Settings.Instance.OutlineSegmentation;

            var counter = 0;
            foreach (var index in contourIndices)
            {
                if (contourIndices.Count > segmentation * 3 && counter % segmentation != 0)
                {
                    counter++;
                    continue;
                }
                else
                {
                    counter++;
                }

                var x = index % width;
                var y = (index - x) / width;

                pointsList.Add(new Vertex(x, y));
            }

            List<string> sList = new List<string>();
            foreach (var s in pointsList)
            {
                sList.Add(s.X + " " + s.Y);
            }

            // File.WriteAllLines("C:\\a.txt", sList);

            return pointsList;
        }

    }
}
