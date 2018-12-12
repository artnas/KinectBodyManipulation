using System.Collections.Generic;
using System.Linq;
using KBMGraphics;
using OpenTK;

namespace KinectBodyModification
{
    public partial class LimbDataManager
    {
        private readonly OutlineTriangulator _outlineTriangulator;
        private Vector2 _lastContourPoint = new Vector2(0, 0);

        public HashSet<int> SortedContour = new HashSet<int>();
        public HashSet<int> UsedContourIndices = new HashSet<int>();

        private void ContourActivePixels()
        {
            foreach (var index in LimbData.ActivePixels)
            {
                int pointX = 0, pointY = 0;
                Utils.IndexToCoordinates(index, ref pointX, ref pointY);

                var isContour = IsContour(pointX, pointY);

                if (isContour)
                {
                    LimbData.AllPixels[index].IsContour = true;
                    LimbData.ContourPixels.Add(index);
                }
            }
        }

        private bool IsContour(int x, int y)
        {
            var emptyNeighbors = 0;

            for (var i = 0; i < 4; i++)
            {
                var directionX = Utils.CardinalDirections[i, 0];
                var directionY = Utils.CardinalDirections[i, 1];

                var offsetX = x + directionX;
                var offsetY = y + directionY;

                var neighborIndex = Utils.CoordinatesToIndex(offsetX, offsetY);

                if (!Utils.AreCoordinatesInBounds(offsetX, offsetY) ||
                    LimbData.AllPixels[neighborIndex].HumanIndex == -1) emptyNeighbors++;
            }

            if (emptyNeighbors == 1 || emptyNeighbors == 2) return true;

            return false;
        }

        public HashSet<int> GetSortedContour()
        {
            SortedContour.Clear();
            UsedContourIndices.Clear();
            _lastContourPoint = new Vector2(0, 0);

            if (LimbData.ContourPixels.Count > 0) AddPointToContourPoints(LimbData.ContourPixels.Last());

            return SortedContour;
        }

        private void AddPointToContourPoints(int index)
        {
            int pointX = 0, pointY = 0;
            Utils.IndexToCoordinates(index, ref pointX, ref pointY);
            var point = new Vector2(pointX, pointY);

            var distanceFromLastPoint = Vector2.Distance(point, _lastContourPoint);

            if (!(_lastContourPoint.X == 0 && _lastContourPoint.Y == 0) && distanceFromLastPoint > 10) return;

            SortedContour.Add(index);
            UsedContourIndices.Add(index);

            _lastContourPoint = point;

            for (var i = 0; i < 4; i++)
            {
                var directionX = Utils.CardinalDirections[i, 0];
                var directionY = Utils.CardinalDirections[i, 1];

                var neighborX = pointX + directionX;
                var neighborY = pointY + directionY;

                var neighborIndex = Utils.CoordinatesToIndex(neighborX, neighborY);
                if (!UsedContourIndices.Contains(neighborIndex)) UsedContourIndices.Add(neighborIndex);
            }

            for (var i = 0; i < Utils.ContourSeekingDirectionsCount; i++)
            {
                var directionX = Utils.ContourSeekingDirections[i, 0];
                var directionY = Utils.ContourSeekingDirections[i, 1];

                var neighborX = pointX + directionX;
                var neighborY = pointY + directionY;

                var neighborIndex = Utils.CoordinatesToIndex(neighborX, neighborY);
                if (!UsedContourIndices.Contains(neighborIndex) && LimbData.ContourPixels.Contains(neighborIndex))
                    AddPointToContourPoints(neighborIndex);
            }
        }
    }
}