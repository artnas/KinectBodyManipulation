using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace KBMGraphics
{
    public struct Vector2Int
    {
        public int X, Y;

        public Vector2Int(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vector2Int(float x, float y)
        {
            this.X = (int)Math.Round(x);
            this.Y = (int)Math.Round(y);
        }

        public Vector2Int(Vector3 vector)
        {
            this.X = (int)Math.Round(vector.X);
            this.Y = (int)Math.Round(vector.Y);
        }
        
    }
}
