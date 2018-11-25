using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace KinectBodyModification
{
    public class BonePixelsData
    {
        public readonly HashSet<int> indices;
        public int minX, maxX, minY, maxY;

        private readonly HashSet<int> usedContourIndices;
        private readonly HashSet<int> contourPoints;

        public BonePixelsData()
        {
            indices = new HashSet<int>();

            contourPoints = new HashSet<int>();
            usedContourIndices = new HashSet<int>();
        }

        public HashSet<int> GetContourPoints()
        {
            contourPoints.Clear();
            usedContourIndices.Clear();

            if (indices.Count > 0)
            {
                AddPointToContourPoints(indices.First());
            }

            return contourPoints;
        }

        private void AddPointToContourPoints(int index)
        {
            int pointX = 0, pointY = 0;

            contourPoints.Add(index);
            usedContourIndices.Add(index);

            Utils.GetIndexCoordinates(index, ref pointX, ref pointY);

            for (var i = 0; i < 8; i++)
            {
                var directionX = Utils.ordinalDirections[i, 0];
                var directionY = Utils.ordinalDirections[i, 1];

                int neighborX = pointX + directionX;
                int neighborY = pointY + directionY;

                int neighborIndex = Utils.GetIndexByCoordinates(neighborX, neighborY);
                if (!usedContourIndices.Contains(neighborIndex) && indices.Contains(neighborIndex))
                {
                    AddPointToContourPoints(neighborIndex);
                }
            }
        }
    }
}