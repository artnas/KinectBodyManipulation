using System;
using OpenTK;

namespace KBMGraphics
{
    public struct Vector2Int
    {
        public int X, Y;

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2Int(float x, float y)
        {
            X = (int) Math.Round(x);
            Y = (int) Math.Round(y);
        }

        public Vector2Int(Vector3 vector)
        {
            X = (int) Math.Round(vector.X);
            Y = (int) Math.Round(vector.Y);
        }
    }
}