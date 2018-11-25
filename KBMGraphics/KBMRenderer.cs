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
using OpenTK.Platform;
using TriangleNet;
using All = OpenTK.Graphics.OpenGL.All;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;
using BufferTarget = OpenTK.Graphics.OpenGL.BufferTarget;
using BufferUsageHint = OpenTK.Graphics.OpenGL.BufferUsageHint;
using ClearBufferMask = OpenTK.Graphics.OpenGL.ClearBufferMask;
using EnableCap = OpenTK.Graphics.OpenGL.EnableCap;
using FramebufferTarget = OpenTK.Graphics.OpenGL.FramebufferTarget;
using GL = OpenTK.Graphics.OpenGL.GL;
using MatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using PixelInternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using PixelType = OpenTK.Graphics.OpenGL.PixelType;
using PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType;
using ReadBufferMode = OpenTK.Graphics.OpenGL.ReadBufferMode;
using RenderbufferTarget = OpenTK.Graphics.OpenGL.RenderbufferTarget;
using TextureParameterName = OpenTK.Graphics.OpenGL.TextureParameterName;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;
using VertexAttribPointerType = OpenTK.Graphics.OpenGL.VertexAttribPointerType;

// https://stackoverflow.com/questions/12157646/how-to-render-offscreen-on-opengl

namespace KBMGraphics
{
    public class KBMRenderer
    {
        public bool isInitialized = false;

        public readonly int width;
        public readonly int height;
        public readonly byte[] OutputBuffer;
        public readonly byte[] TextureBuffer;
        private KBMSceneData sceneData;

        private GraphicsContext context;

        private readonly int[] frameBuffer = {0}, renderBuffer = {0}, backgroundTexturePointer = {0}, foregroundTexturePointer = {0};
        private byte[] pixels;

        private Vector2[] screenEdges;
        private Vector2[] screenEdgeUvs;

        public KBMRenderer(int width, int height, byte[] outputBuffer, byte[] textureBuffer)
        {
            this.width = width;
            this.height = height;
            this.OutputBuffer = outputBuffer;
            this.TextureBuffer = textureBuffer;

            // this.graphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0);

            screenEdges = new[]
            {
                new Vector2(0, height), 
                new Vector2(0, 0), 
                new Vector2(width, 0),

                new Vector2(0, height),
                new Vector2(width, height),
                new Vector2(width, 0)
            };

            screenEdgeUvs = new[]
            {
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),

                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0)
            };
        }

        ~KBMRenderer()
        {
            // GL.DeleteFramebuffers(1, frameBuffer);
            // GL.DeleteRenderbuffers(1, renderBuffer);
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                throw new Exception("KBM Renderer is already initialized");
            }
            else
            {
                isInitialized = true;
            }

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(0f, width, height, 0f, -1000f, 1000f);

            // GL.MatrixMode(MatrixMode.Projection);

            // GL.ReadBuffer(ReadBufferMode.Back);
            // GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, outputBuffer);



            GL.GenFramebuffers(1, frameBuffer);
            GL.GenRenderbuffers(1, renderBuffer);
            GL.GenTextures(1, backgroundTexturePointer);
            GL.GenTextures(1, foregroundTexturePointer);

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

            SetBackgroundTexture(new byte[]{
                0, 0, 0, 255,   255, 5, 255, 255,
                255, 77, 255, 255,   0, 0, 0, 255
            }, 2, 2);
        }

        public void SetForegroundTexture(byte[] foregroundColorBuffer)
        {
            GL.BindTexture(TextureTarget.Texture2D, foregroundTexturePointer[0]);

            unsafe
            {
                fixed (byte* ptr = &foregroundColorBuffer[0])
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra,
                        PixelType.UnsignedByte, new IntPtr(ptr));
                }
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetBackgroundTexture(byte[] backgroundColorBuffer, int width, int height)
        {
            GL.BindTexture(TextureTarget.Texture2D, backgroundTexturePointer[0]);

            unsafe
            {
                fixed (byte* ptr = &backgroundColorBuffer[0])
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra,
                        PixelType.UnsignedByte, new IntPtr(ptr));
                }
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetSceneData(KBMSceneData sceneData)
        {
            this.sceneData = sceneData;
        }

        private void OnBeforeDraw()
        {
            // GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frameBuffer[0]);
        }

        private void OnAfterDraw()
        {
            // GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            // GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, this.OutputBuffer);
            // GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            
        }

        public void Draw()
        {
            OnBeforeDraw();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 modelView = Matrix4.LookAt(new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            // draw background

            GL.BindTexture(TextureTarget.Texture2D, backgroundTexturePointer[0]);
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Triangles);

            for (var i = 0; i < screenEdges.Length; i++)
            {
                GL.TexCoord2(screenEdgeUvs[i].X, screenEdgeUvs[i].Y);
                GL.Vertex2(screenEdges[i].X, screenEdges[i].Y);
            }

            GL.End();

            // draw foreground
            // if (sceneData.mesh != null)
            // {
            //     GL.Begin(PrimitiveType.Polygon);
            //     GL.BindTexture(TextureTarget.Texture2D, foregroundTexturePointer[0]);
            //     GL.Enable(EnableCap.Texture2D);
            //
            //     for (var i = 0; i < sceneData.mesh.indices.Count; i ++)
            //     {
            //         var a = sceneData.mesh.indices[i];
            //
            //         GL.TexCoord2(sceneData.mesh.uvs[a].X, sceneData.mesh.uvs[a].Y);
            //         GL.Vertex2(sceneData.mesh.vertices[a].X, sceneData.mesh.vertices[a].Y);
            //     }
            //
            //     GL.End();
            // }

             if (sceneData.mesh != null)
             {
                 GL.BindTexture(TextureTarget.Texture2D, foregroundTexturePointer[0]);
                 GL.Enable(EnableCap.Texture2D);
                 GL.Begin(PrimitiveType.Triangles);
            
                 for (var i = 0; i < sceneData.mesh.indices.Count; i += 3)
                 {
                     var a = sceneData.mesh.indices[i];
                     var b = sceneData.mesh.indices[i + 1];
                     var c = sceneData.mesh.indices[i + 2];
            
                     GL.TexCoord2(sceneData.mesh.uvs[a].X, sceneData.mesh.uvs[a].Y);
                     GL.Vertex2(sceneData.mesh.vertices[a].X, sceneData.mesh.vertices[a].Y);
            
                     GL.TexCoord2(sceneData.mesh.uvs[b].X, sceneData.mesh.uvs[b].Y);
                     GL.Vertex2(sceneData.mesh.vertices[b].X, sceneData.mesh.vertices[b].Y);
            
                     GL.TexCoord2(sceneData.mesh.uvs[c].X, sceneData.mesh.uvs[c].Y);
                     GL.Vertex2(sceneData.mesh.vertices[c].X, sceneData.mesh.vertices[c].Y);
                 }
            
                 GL.End();
             }

            // float[] vertices =
            // {
            //     test, 0f,
            //     200f, 0f,
            //     400f, 400f
            // };
            //
            // int[] vertexBuffer = {0};
            // GL.GenBuffers(1, vertexBuffer);
            //
            // GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer[0]);
            // GL.BufferData(BufferTarget.ArrayBuffer, 4 * vertices.Length, vertices, BufferUsageHint.StaticDraw);
            //
            // GL.EnableVertexAttribArray(0);
            // // GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer[0]);
            //
            // GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            //
            // // GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            // // GL.DrawArrays(PrimitiveType.Polygon, 0, vertices.Length/2);
            // GL.DisableVertexAttribArray(0);

            // GL.Begin(PrimitiveType.Triangles);

            // GL.Vertex2(200, 100);
            // GL.Vertex2(400, 100);
            // GL.Vertex2(550, 300);
            // GL.Vertex2(400, 480);
            // GL.Vertex2(200, 480);
            // GL.Vertex2(50, 300);

            // GL.Vertex2(0, 0);
            // GL.Vertex2(800, 0);
            // GL.Vertex2(0, 600);

            Random r = new Random();

            // for (var i = 0; i < sceneData.vertices.Length; i++)
            // {
            //     GL.TexCoord2(r.NextDouble(), r.NextDouble());
            //     GL.Vertex2(sceneData.vertices[i].X, sceneData.vertices[i].Y);
            // }

              // for (var triangleIndex = 0; triangleIndex < mesh.indices.Count; triangleIndex+=3)
              // {
              //     var a = mesh.indices[triangleIndex];
              //     var b = mesh.indices[triangleIndex+1];
              //     var c = mesh.indices[triangleIndex+2];
              //
              //     // GL.TexCoord2(mesh.vertices[a].X / width, mesh.vertices[a].Y / height);
              //     GL.TexCoord2(r.NextDouble(), r.NextDouble());
              //     GL.Vertex2(mesh.vertices[a].X, mesh.vertices[a].Y);
              //
              //     // GL.TexCoord2(mesh.vertices[b].X / width, mesh.vertices[b].Y / height);
              //     GL.TexCoord2(r.NextDouble(), r.NextDouble());
              //     GL.Vertex2(mesh.vertices[b].X, mesh.vertices[b].Y);
              //
              //     // GL.TexCoord2(mesh.vertices[c].X / width, mesh.vertices[c].Y / height);
              //     GL.TexCoord2(r.NextDouble(), r.NextDouble());
              //     GL.Vertex2(mesh.vertices[c].X, mesh.vertices[c].Y);
              // }

            // for (var triangleIndex = 0; triangleIndex < mesh.Indices.GetLength(0); triangleIndex++)
            // {
            //     var a = mesh.Indices[triangleIndex, 0];
            //     var b = mesh.Indices[triangleIndex, 1];
            //     var c = mesh.Indices[triangleIndex, 2];
            //
            //     GL.TexCoord2(mesh.Vertices[a].X / width, mesh.Vertices[a].Y / height);
            //     GL.Vertex2(mesh.Vertices[a].X, mesh.Vertices[a].Y);
            //
            //     GL.TexCoord2(mesh.Vertices[b].X / width, mesh.Vertices[b].Y / height);
            //     GL.Vertex2(mesh.Vertices[b].X, mesh.Vertices[b].Y);
            //
            //     GL.TexCoord2(mesh.Vertices[c].X / width, mesh.Vertices[c].Y / height);
            //     GL.Vertex2(mesh.Vertices[c].X, mesh.Vertices[c].Y);
            // }

            GL.End();

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
