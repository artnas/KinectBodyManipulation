using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using BufferTarget = OpenTK.Graphics.OpenGL.BufferTarget;
using BufferUsageHint = OpenTK.Graphics.OpenGL.BufferUsageHint;
using FramebufferTarget = OpenTK.Graphics.OpenGL.FramebufferTarget;
using GL = OpenTK.Graphics.OpenGL.GL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using PixelType = OpenTK.Graphics.OpenGL.PixelType;
using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;
using ReadBufferMode = OpenTK.Graphics.OpenGL.ReadBufferMode;
using RenderbufferTarget = OpenTK.Graphics.OpenGL.RenderbufferTarget;
using VertexAttribPointerType = OpenTK.Graphics.OpenGL.VertexAttribPointerType;

// https://stackoverflow.com/questions/12157646/how-to-render-offscreen-on-opengl

namespace KBMGraphics
{
    public class KBMRenderer
    {

        public readonly int width;
        public readonly int height;
        public readonly byte[] OutputBuffer;
        public readonly byte[] TextureBuffer;

        private GraphicsContext context;

        private readonly int[] frameBuffer = {0}, renderBuffer = {0};

        public KBMRenderer(int width, int height, byte[] outputBuffer, byte[] textureBuffer)
        {
            this.width = width;
            this.height = height;
            this.OutputBuffer = outputBuffer;
            this.TextureBuffer = textureBuffer;
            // this.graphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0);

            

            // GL.ReadBuffer(ReadBufferMode.Back);
            // GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, outputBuffer);

            GL.GenFramebuffers(1, frameBuffer);
            GL.GenRenderbuffers(1, renderBuffer);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer[0]);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height);
            // GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, renderBuffer[0]);

            

            // var framebuffer = GL.GenFramebuffer();
            // GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            //
            // var windowInfo = OpenTK.Platform.Utilities.CreateDummyWindowInfo();
            // new GraphicsContext()
            //
            // GL.GenTextures(1, out textureColorBuffer);
        }

        ~KBMRenderer()
        {
            // GL.DeleteFramebuffers(1, frameBuffer);
            // GL.DeleteRenderbuffers(1, renderBuffer);
        }

        private void OnBeforeDraw()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBuffer[0]);
        }

        private void OnAfterDraw()
        {
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, this.OutputBuffer);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            
        }

        private int test = 0;

        public void Draw()
        {

            test++;

            OnBeforeDraw();

            float[] vertices =
            {
                test, 0f,
                200f, 0f,
                400f, 400f
            };

            int[] vertexBuffer = {0};
            GL.GenBuffers(1, vertexBuffer);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * vertices.Length, vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer[0]);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);

            // GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            // GL.DrawArrays(PrimitiveType.Polygon, 0, vertices.Length/2);
            GL.DisableVertexAttribArray(0);

            // GL.Begin(PrimitiveType.Polygon);
            //
            // GL.Vertex2(200, 100);
            // GL.Vertex2(400, 100);
            // GL.Vertex2(550, 300);
            // GL.Vertex2(400, 480);
            // GL.Vertex2(200, 480);
            // GL.Vertex2(50, 300);
            //
            // GL.End();

            // GL.End();

            OnAfterDraw();
        }

        // public GraphicsContext GetContext()
        // {
        //
        // }
        //
        // protected override void OnLoad(EventArgs e)
        // {
        //     base.OnLoad(e);
        //
        //     GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
        //     GL.Enable(EnableCap.DepthTest);
        // }
        //
        // protected override void OnResize(EventArgs e)
        // {
        //     base.OnResize(e);
        //
        //     GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        //
        //     Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
        //     GL.MatrixMode(MatrixMode.Projection);
        //     GL.LoadMatrix(ref projection);
        // }
        //
        // protected override void OnUpdateFrame(FrameEventArgs e)
        // {
        //     base.OnUpdateFrame(e);
        //
        //     if (Keyboard[Key.Escape])
        //         Exit();
        // }
        //
        // protected override void OnRenderFrame(FrameEventArgs e)
        // {
        //     base.OnRenderFrame(e);
        //
        //     GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        //
        //     Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
        //     GL.MatrixMode(MatrixMode.Modelview);
        //     GL.LoadMatrix(ref modelview);
        //
        //     GL.Begin(BeginMode.Triangles);
        //
        //     GL.Color3(1.0f, 1.0f, 0.0f); GL.Vertex3(-1.0f, -1.0f, 4.0f);
        //     GL.Color3(1.0f, 0.0f, 0.0f); GL.Vertex3(1.0f, -1.0f, 4.0f);
        //     GL.Color3(0.2f, 0.9f, 1.0f); GL.Vertex3(0.0f, 1.0f, 4.0f);
        //
        //     GL.End();
        //
        //     SwapBuffers();
        // }

        // [STAThread]
        // static void Main()
        // {
        //     // The 'using' idiom guarantees proper resource cleanup.
        //     // We request 30 UpdateFrame events per second, and unlimited
        //     // RenderFrame events (as fast as the computer can handle).
        //     using (Game game = new Game())
        //     {
        //         game.Run(30.0);
        //     }
        // }

    }
}
