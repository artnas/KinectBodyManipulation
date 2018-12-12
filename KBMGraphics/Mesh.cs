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
    public class Mesh
    {
        public List<Vector3> vertices;
        public List<int> indices;
        public List<Vector2> uvs;

        public Dictionary<Vector3, float> vertexWeightsDictionary;

        private Dictionary<Vector3, int> vertexIndicesDictionary;

        public Mesh()
        {
            vertices = new List<Vector3>();
            indices = new List<int>();
            uvs = new List<Vector2>();
            vertexIndicesDictionary = new Dictionary<Vector3, int>();
            vertexWeightsDictionary = new Dictionary<Vector3, float>();
        }
        
        public Mesh(TriangleNet.Mesh mesh)
        {
            vertices = new List<Vector3>();
            indices = new List<int>();
            uvs = new List<Vector2>();
            vertexIndicesDictionary = new Dictionary<Vector3, int>();
            vertexWeightsDictionary = new Dictionary<Vector3, float>();

            Update(mesh);
        }

        public void Update(TriangleNet.Mesh mesh)
        {
            vertices.Clear();
            indices.Clear();
            uvs.Clear();
            vertexIndicesDictionary.Clear();
            vertexWeightsDictionary.Clear();

            if (mesh == null) return;

            mesh.Renumber();

            foreach (var vertex in mesh.Vertices)
            {
                var vector = new Vector3((float)vertex.X, (float)vertex.Y, 0);
                vertexIndicesDictionary.Add(vector, vertexIndicesDictionary.Count);
                vertexWeightsDictionary.Add(vector, 1);
                vertices.Add(vector);
                uvs.Add(new Vector2((float)(vertex.X / 640), (float)(vertex.Y / 480)));
            }

            foreach (var meshTriangle in mesh.Triangles)
            {
                var va = meshTriangle.GetVertex(0);
                var vb = meshTriangle.GetVertex(1);
                var vc = meshTriangle.GetVertex(2);

                var vca = new Vector3((float)va.X, (float)va.Y, 0);
                var vcb = new Vector3((float)vb.X, (float)vb.Y, 0);
                var vcc = new Vector3((float)vc.X, (float)vc.Y, 0);

                var vai = vertexIndicesDictionary[vca];
                var vbi = vertexIndicesDictionary[vcb];
                var vci = vertexIndicesDictionary[vcc];

                indices.Add(vai);
                indices.Add(vbi);
                indices.Add(vci);
            }
        }

        public float GetVertexWeight(Vector3 vertex)
        {
            return vertexWeightsDictionary.ContainsKey(vertex) ? vertexWeightsDictionary[vertex] : 1;
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
