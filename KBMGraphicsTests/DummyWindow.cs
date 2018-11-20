using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KBMGraphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace KBMGraphicsTests
{
    class DummyWindow : GameWindow
    {
        private static readonly int width = 400;
        private static readonly int height = 300;
        byte[] outputBuffer = new byte[width * height * 4];
        byte[] textureBuffer = new byte[width * height * 4];
        private KBMRenderer renderer;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(0, 0, 0, 1);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 modelView = Matrix4.LookAt(new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

            GL.MatrixMode(MatrixMode.Modelview);

            GL.LoadMatrix(ref modelView);

            Render();

            // GL.Begin(PrimitiveType.Triangles);
            //
            // GL.Vertex3(10f, 10f, 0);
            // GL.Vertex3(50f, 50f, 0);
            // GL.Vertex3(0f, 70f, 0);

            // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            GL.Begin(PrimitiveType.Polygon);

            GL.Vertex2(200, 100);
            GL.Vertex2(400, 100);
            GL.Vertex2(550, 300);
            GL.Vertex2(400, 480);
            GL.Vertex2(200, 480);
            GL.Vertex2(50, 300);

            GL.End();

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(0f, ClientRectangle.Width, ClientRectangle.Height, 0f, -1000f, 1000f);

            GL.MatrixMode(MatrixMode.Projection);
        }

        private void Render()
        {         
            if (renderer == null)
            {
                renderer = new KBMRenderer(width, height, outputBuffer, textureBuffer);
            }

            renderer.Draw();
        }
    }
}
