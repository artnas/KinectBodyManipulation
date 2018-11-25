using System;
using System.Collections.Generic;
using System.IO;
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
        public List<Vertex> vertices;
        public List<int> indices;
        public List<Vector2> uvs;

        private Dictionary<Vertex, int> verticesDictionary;

        public QuadraticMesh()
        {
            vertices = new List<Vertex>();
            indices = new List<int>();
            uvs = new List<Vector2>();
            verticesDictionary = new Dictionary<Vertex, int>();
        }
        
        public QuadraticMesh(Mesh mesh)
        {
            vertices = new List<Vertex>();
            indices = new List<int>();
            uvs = new List<Vector2>();
            verticesDictionary = new Dictionary<Vertex, int>();

            Update(mesh);
        }

        public void Update(Mesh mesh)
        {
            vertices.Clear();
            indices.Clear();
            uvs.Clear();
            verticesDictionary.Clear();

            if (mesh == null) return;

            mesh.Renumber();

            foreach (var vertex in mesh.Vertices)
            {
                verticesDictionary.Add(vertex, verticesDictionary.Count);
                vertices.Add(vertex);
                uvs.Add(new Vector2((float)(vertex.X / 640), (float)(vertex.Y / 480)));
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
        }

        public void ExportToObj(string path)
        {
            List<string> sList = new List<string>();

            foreach (var v in vertices)
            {
                sList.Add($"v {v.X} {v.Y} 0");
            }

            foreach (var u in uvs)
            {
                sList.Add($"vt {u.X} {u.Y} 0");
            }

            for (var i = 0; i < indices.Count; i+= 3)
            {
                sList.Add($"f {indices[i]+1} {indices[i+1]+1} {indices[i+2]+1}");
            }

            File.WriteAllLines(path, sList);
        }

    }
}
