using System;
using System.Collections.Generic;
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

        private readonly QualityOptions QualityOptions;

        public BodyPolygonizer()
        {
            this.QualityOptions = new QualityOptions();
        }

        public Mesh GetMesh()
        {
            var poly = new Polygon();
            poly.Add(new Contour(new[]
            {
                new Vertex(0, 0),
                new Vertex(100, 0),
                new Vertex(100, 100),
                new Vertex(50, 75),
                new Vertex(25, 90),
                new Vertex(0, 100)
            }));

            var mesh = poly.Triangulate(
                new ConstraintOptions { ConformingDelaunay = true },
                new QualityOptions { MinimumAngle = 25.0 }
            ) as Mesh;

            return mesh;
        }

        private QuadraticMesh GetQuadraticMesh(Mesh mesh)
        {
            return new QuadraticMesh(mesh);
        }

    }
}
