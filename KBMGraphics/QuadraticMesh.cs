using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using TriangleNet;
using TriangleNet.Geometry;

namespace KBMGraphics
{
    public class QuadraticMesh
    {

        /// <summary>
        /// Gets the list of new vertices (edge midpoints).
        /// </summary>
        public List<Point> Vertices { get; set; }

        /// <summary>
        /// Gets the new vertex indices for each triangle.
        /// </summary>
        /// <remarks>
        /// For a triangle with index k, Indices[k, i](k,-i) (i = 0, 1, 2) corresponds to
        /// the vertex on the edge shared with the i-th neighbor, e.g. Indices[k, 0](k,-0)
        /// is the vertex shared by triangle k and it's neighbor N0 and lies on the
        /// side opposite of P0.
        /// </remarks>
        public int[,] Indices { get; set; }

        public List<Vertex> vertices;
        public List<int> indices;
        public List<Vector2> uvs;

        private Dictionary<Vertex, int> verticesDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadraticElements" /> class.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public QuadraticMesh(Mesh mesh)
        {
            vertices = new List<Vertex>();
            indices = new List<int>();
            uvs = new List<Vector2>();
            verticesDictionary = new Dictionary<Vertex, int>();
            QuadraticOrder(mesh);
        }

        private void QuadraticOrder(Mesh mesh)
        {
            mesh.Renumber();

            vertices.Clear();
            indices.Clear();
            uvs.Clear();
            verticesDictionary.Clear();

            foreach (var vertex in mesh.Vertices)
            {
                verticesDictionary.Add(vertex, verticesDictionary.Count);
                vertices.Add(vertex);
                uvs.Add(new Vector2((float)(vertex.X / 800.0), (float)(vertex.Y / 600.0)));
            }

            foreach (var meshTriangle in mesh.Triangles)
            {
                var va = meshTriangle.GetVertex(0);
                var vb = meshTriangle.GetVertex(1);
                var vc = meshTriangle.GetVertex(2);

                var vai = verticesDictionary[va];
                var vbi = verticesDictionary[vb];
                var vci = verticesDictionary[vc];

                indices.Add(vai);
                indices.Add(vbi);
                indices.Add(vci);
            }

            // mesh.Renumber();
            //
            // Vertices = new List<Point>(mesh.NumberOfEdges);
            // Indices = new int[mesh.Triangles.Count, 3];
            //
            // Point org, dest, middle;
            // ITriangle neighbor, neighbor2;
            // int nid, nid2;
            // int orient = 0, count = 0;
            //
            // foreach (var tri in mesh.Triangles)
            // {
            //     for (int i = 0; i < 3; i++)
            //     {
            //         // The neighbor opposite of vertex i.
            //         GetNeighbor(tri, i, out neighbor, out nid);
            //
            //         // Consider each edge only once.
            //         if ((tri.ID < nid) || (nid < 0))
            //         {
            //             // Get the edge endpoints.
            //             org = tri.GetVertex((i + 1) % 3);
            //             dest = tri.GetVertex((i + 2) % 3);
            //
            //             // Create a new node in the middle of the edge.
            //
            //             middle = new Point(0.5 * (org.X + dest.X), 0.5 * (org.Y + dest.Y));
            //                 // Math.Min(org.Boundary, dest.Boundary));
            //
            //             Vertices.Add(middle);
            //
            //             // Record the new node. 
            //             Indices[tri.ID, i] = count;
            //
            //             // Record the new node in the neighbor element. 
            //             if (nid != -1)
            //             {
            //                 GetNeighbor(neighbor, orient, out neighbor2, out nid2);
            //
            //                 // Get the neighbors orientation.
            //                 while (nid2 != tri.ID)
            //                 {
            //                     orient = (orient + 1) % 3;
            //                     GetNeighbor(neighbor, orient, out neighbor2, out nid2);
            //                 }
            //
            //                 Indices[neighbor.ID, orient] = count;
            //             }
            //
            //             count++;
            //         }
            //     }
            // }
        }

        private void GetNeighbor(ITriangle tri, int i, out ITriangle neighbor, out int nid)
        {
            neighbor = tri.GetNeighbor(i);
            nid = neighbor?.ID ?? -1;
        }

    }
}
