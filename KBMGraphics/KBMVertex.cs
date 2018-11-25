﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace KBMGraphics
{
    public struct KBMVertex
    {
        public Vector2 position;
        public Vector2 uvCoordinates;

        public KBMVertex(Vector2 position, Vector2 uvCoordinates)
        {
            this.position = position;
            this.uvCoordinates = uvCoordinates;
        }
    }
}
