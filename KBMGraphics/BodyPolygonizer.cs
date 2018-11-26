using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // var contour = new Contour(new[]
            // {
            //     new Vertex(0, 100), 
            //     new Vertex(100, 150), 
            //     new Vertex(200, 75), 
            //     new Vertex(0, 100), 
            // });
            var poly = new Polygon(contourIndices.Count);
            //
            poly.Add(contour);

            // var poly = new Polygon(contourIndices.Count);
            // foreach (var index in contourIndices)
            // {
            //     var x = index % width;
            //     var y = (index - x) / width;
            //
            //     poly.Add(new Vertex(x, y));
            // }

            try
            {
                var mesh = poly.Triangulate(
                    new ConstraintOptions {ConformingDelaunay = true, Convex = false, SegmentSplitting = 0},
                    new QualityOptions {MinimumAngle = 25}
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

            var segmentation = 4;

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
