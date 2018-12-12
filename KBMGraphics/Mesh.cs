using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace KBMGraphics
{
    public class Mesh
    {
        public List<Vector3> Vertices;
        public List<int> Indices;
        public List<Vector2> Uvs;

        public Dictionary<Vector3, float> VertexWeightsDictionary;

        private readonly Dictionary<Vector3, int> _vertexIndicesDictionary;

        public Mesh()
        {
            Vertices = new List<Vector3>();
            Indices = new List<int>();
            Uvs = new List<Vector2>();
            VertexWeightsDictionary = new Dictionary<Vector3, float>();

            _vertexIndicesDictionary = new Dictionary<Vector3, int>();
        }

        public Mesh(TriangleNet.Mesh mesh) : this()
        {
            Update(mesh);
        }

        public void Update(TriangleNet.Mesh mesh)
        {
            Vertices.Clear();
            Indices.Clear();
            Uvs.Clear();
            VertexWeightsDictionary.Clear();

            _vertexIndicesDictionary.Clear();

            if (mesh == null) return;

            mesh.Renumber();

            foreach (var vertex in mesh.Vertices)
            {
                var vector = new Vector3((float) vertex.X, (float) vertex.Y, 0);
                _vertexIndicesDictionary.Add(vector, _vertexIndicesDictionary.Count);
                VertexWeightsDictionary.Add(vector, 1);
                Vertices.Add(vector);
                Uvs.Add(new Vector2((float) (vertex.X / 640), (float) (vertex.Y / 480)));
            }

            foreach (var meshTriangle in mesh.Triangles)
            {
                var va = meshTriangle.GetVertex(0);
                var vb = meshTriangle.GetVertex(1);
                var vc = meshTriangle.GetVertex(2);

                var vca = new Vector3((float) va.X, (float) va.Y, 0);
                var vcb = new Vector3((float) vb.X, (float) vb.Y, 0);
                var vcc = new Vector3((float) vc.X, (float) vc.Y, 0);

                var vai = _vertexIndicesDictionary[vca];
                var vbi = _vertexIndicesDictionary[vcb];
                var vci = _vertexIndicesDictionary[vcc];

                Indices.Add(vai);
                Indices.Add(vbi);
                Indices.Add(vci);
            }
        }

        public float GetVertexWeight(Vector3 vertex)
        {
            return VertexWeightsDictionary.ContainsKey(vertex) ? VertexWeightsDictionary[vertex] : 1;
        }

        public void ExportToObj(string path)
        {
            var sList = new List<string>();

            foreach (var v in Vertices) sList.Add($"v {v.X} {v.Y} 0");

            foreach (var u in Uvs) sList.Add($"vt {u.X} {u.Y} 0");

            for (var i = 0; i < Indices.Count; i += 3)
                sList.Add($"f {Indices[i] + 1} {Indices[i + 1] + 1} {Indices[i + 2] + 1}");

            File.WriteAllLines(path, sList);
        }
    }
}