using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectBodyModification;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Platform;
using TriangleNet;
using All = OpenTK.Graphics.OpenGL.All;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;
using BlendingFactor = OpenTK.Graphics.OpenGL.BlendingFactor;
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
    public class Renderer
    {
        public bool isInitialized = false;

        public readonly int width;
        public readonly int height;
        private SceneData sceneData;

        private GraphicsContext context;

        private readonly int[] 
            frameBuffer = {0}, 
            renderBuffer = {0}, 
            backgroundTexturePointer = {0}, 
            foregroundTexturePointer = {0},
            debugTexturePointer = {0};

        private byte[] pixels;

        private Vector2[] screenEdges;
        private Vector2[] screenEdgeUvs;

        public Renderer(int width, int height)
        {
            this.width = width;
            this.height = height;

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

        ~Renderer()
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

            GL.GenFramebuffers(1, frameBuffer);
            GL.GenRenderbuffers(1, renderBuffer);
            GL.GenTextures(1, backgroundTexturePointer);
            GL.GenTextures(1, foregroundTexturePointer);
            GL.GenTextures(1, debugTexturePointer);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderBuffer[0]);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, width, height);

            SetBackgroundTexture(new byte[]{
                200, 200, 200, 255,  200, 200, 200, 255,
                200, 200, 200, 255,  200, 200, 200, 255
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

        public void SetDebugTexture(byte[] debugColorBuffer)
        {
            GL.BindTexture(TextureTarget.Texture2D, debugTexturePointer[0]);

            unsafe
            {
                fixed (byte* ptr = &debugColorBuffer[0])
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra,
                        PixelType.UnsignedByte, new IntPtr(ptr));
                }
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetSceneData(SceneData sceneData)
        {
            this.sceneData = sceneData;
        }

        public void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 modelView = Matrix4.LookAt(new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            DrawBackground();
            DrawForeground();

            if (Settings.Instance.ShouldDrawDebugOverlay())
            {
                DrawDebugOverlay();
            }
        }

        private void DrawBackground()
        {         
            GL.BindTexture(TextureTarget.Texture2D, backgroundTexturePointer[0]);
            GL.Enable(EnableCap.Texture2D);

            GL.Begin(PrimitiveType.Triangles);  

            GL.Color3(1f, 1f, 1f);

            for (var i = 0; i < screenEdges.Length; i++)
            {
                GL.TexCoord2(screenEdgeUvs[i].X, screenEdgeUvs[i].Y);
                GL.Vertex2(screenEdges[i].X, screenEdges[i].Y);
            }

            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void DrawForeground()
        {
            switch (Settings.Instance.DrawMode)
            {
                case Settings.GLDrawModeEnum.Normal:
                    if (sceneData.mesh != null)
                    {
                        GL.BindTexture(TextureTarget.Texture2D, foregroundTexturePointer[0]);
                        GL.Enable(EnableCap.Texture2D);

                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                        GL.Begin(PrimitiveType.Triangles);
                        GL.Color3(1f, 1f, 1f);

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

                        GL.Disable(EnableCap.Blend);
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    }
                    break;
                case Settings.GLDrawModeEnum.Uvs:
                    if (sceneData.mesh != null)
                    {
                        GL.Begin(PrimitiveType.Triangles);

                        for (var i = 0; i < sceneData.mesh.indices.Count; i += 3)
                        {
                            var a = sceneData.mesh.indices[i];
                            var b = sceneData.mesh.indices[i + 1];
                            var c = sceneData.mesh.indices[i + 2];

                            GL.Color3(sceneData.mesh.uvs[a].X, sceneData.mesh.uvs[a].Y, 1f);
                            GL.Vertex2(sceneData.mesh.vertices[a].X, sceneData.mesh.vertices[a].Y);

                            GL.Color3(sceneData.mesh.uvs[b].X, sceneData.mesh.uvs[b].Y, 1f);
                            GL.Vertex2(sceneData.mesh.vertices[b].X, sceneData.mesh.vertices[b].Y);

                            GL.Color3(sceneData.mesh.uvs[c].X, sceneData.mesh.uvs[c].Y, 1f);
                            GL.Vertex2(sceneData.mesh.vertices[c].X, sceneData.mesh.vertices[c].Y);
                        }

                        GL.End();

                        DrawTriangleLines();
                    }
                    break;
                case Settings.GLDrawModeEnum.Lines:
                    if (sceneData.mesh != null)
                    {
                        GL.Begin(PrimitiveType.Triangles);
                        GL.Color4(1f, 1f, 1f, 1f);

                        for (var i = 0; i < sceneData.mesh.indices.Count; i += 3)
                        {
                            var a = sceneData.mesh.indices[i];
                            var b = sceneData.mesh.indices[i + 1];
                            var c = sceneData.mesh.indices[i + 2];

                            GL.Vertex2(sceneData.mesh.vertices[a].X, sceneData.mesh.vertices[a].Y);
                            GL.Vertex2(sceneData.mesh.vertices[b].X, sceneData.mesh.vertices[b].Y);
                            GL.Vertex2(sceneData.mesh.vertices[c].X, sceneData.mesh.vertices[c].Y);
                        }

                        GL.End();
                        
                        DrawTriangleLines();
                    }
                    break;
            }
        }

        private void DrawTriangleLines()
        {
            for (var i = 0; i < sceneData.mesh.indices.Count; i += 3)
            {
                GL.Begin(PrimitiveType.LineLoop);
                GL.Color3(0f, 0f, 0f);

                var a = sceneData.mesh.indices[i];
                var b = sceneData.mesh.indices[i + 1];
                var c = sceneData.mesh.indices[i + 2];

                GL.Vertex2(sceneData.mesh.vertices[a].X, sceneData.mesh.vertices[a].Y);
                GL.Vertex2(sceneData.mesh.vertices[b].X, sceneData.mesh.vertices[b].Y);
                GL.Vertex2(sceneData.mesh.vertices[c].X, sceneData.mesh.vertices[c].Y);
                GL.End();
            }
        }

        private void DrawDebugOverlay()
        {
            GL.BindTexture(TextureTarget.Texture2D, debugTexturePointer[0]);
            GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Begin(PrimitiveType.Triangles);

            GL.Color3(1f, 1f, 1f);

            for (var i = 0; i < screenEdges.Length; i++)
            {
                GL.TexCoord2(screenEdgeUvs[i].X, screenEdgeUvs[i].Y);
                GL.Vertex2(screenEdges[i].X, screenEdges[i].Y);
            }

            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void ResizeViewport(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
        }
    }
}
