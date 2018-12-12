using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KBMGraphics;
using OpenTK;
using KinectBodyModification;

namespace KinectBodyModification
{
    public class VertexQuadTree
    {

        public static readonly int quadSize = 16;
        public static readonly int quadSizeSquared = quadSize * quadSize;

        private readonly Dictionary<Vector2Int, QuadTreeZone> verticesDictionary;

        public VertexQuadTree()
        {
            verticesDictionary = new Dictionary<Vector2Int, QuadTreeZone>();

            for (var y = 0; y < Configuration.height; y += quadSize)
            {
                for (var x = 0; x < Configuration.width; x += quadSize)
                {
                    verticesDictionary.Add(new Vector2Int(x, y), new QuadTreeZone(new Vector2Int(x, y)));
                }
            }
        }

        public Vector2Int GetZoneCoordinates(float x, float y)
        {
            return GetZoneCoordinates((int) x, (int) y);
        }

        public Vector2Int GetZoneCoordinates(int x, int y)
        {
            return new Vector2Int(
                (int)((float)x / quadSize) * quadSize,
                (int)((float)y / quadSize) * quadSize
            );
        }

        public QuadTreeZone GetZone(Vector2Int coordinates)
        {
            return verticesDictionary.ContainsKey(coordinates) ? verticesDictionary[coordinates] : null;
        }

        public QuadTreeZone GetZone(int x, int y)
        {
            return GetZone(new Vector2Int(x, y));
        }

        public List<QuadTreeZone> GetSurroundingZones(Vector2Int coordinates)
        {
            return GetSurroundingZones(coordinates.X, coordinates.Y);
        }

        public List<QuadTreeZone> GetSurroundingZones(int x, int y)
        {
            var result = new List<QuadTreeZone>();
            var coordinates = GetZoneCoordinates(x, y);

            for (var yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (var xOffset = -1; xOffset <= 1; xOffset++)
                {
                    var offsetCoordinates = new Vector2Int(
                        coordinates.X + xOffset * quadSize, 
                        coordinates.Y + yOffset * quadSize
                        );

                    if (verticesDictionary.ContainsKey(offsetCoordinates))
                    {
                        result.Add(verticesDictionary[offsetCoordinates]);
                    }
                }
            }

            return result;
        }

        public void AssignVerticesToZones(LimbData limbData)
        {

            foreach (var entry in verticesDictionary)
            {
                entry.Value.list.Clear();
            }

            Parallel.ForEach(limbData.mesh.vertices, vertexPosition =>
            {
                var coordinates = GetZoneCoordinates(vertexPosition.X, vertexPosition.Y);

                var zone = GetZone(coordinates);

                if (zone != null)
                {
                    lock (zone.list)
                    {
                        zone.list.Add(vertexPosition);
                    }
                }
            });

        }

    }

    public class QuadTreeZone
    {
        public readonly List<Vector3> list;
        public readonly Vector2Int coordinates;

        public QuadTreeZone(Vector2Int coordinates)
        {
            this.coordinates = coordinates;

            list = new List<Vector3>();
        }
    }
}
