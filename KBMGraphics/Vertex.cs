using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace KBMGraphics
{
    public struct Vertex
    {
        public Vector2 position;
        public Vector2 uvCoordinates;

        public Vertex(Vector2 position, Vector2 uvCoordinates)
        {
            this.position = position;
            this.uvCoordinates = uvCoordinates;
        }
    }
}
