using System;
using KinectBodyModification;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

// https://stackoverflow.com/questions/12157646/how-to-render-offscreen-on-opengl

namespace KBMGraphics
{
    public class Renderer
    {
        public readonly int Height;

        private readonly int[]
            _renderBuffer = {0},
            _backgroundTexturePointer = {0},
            _foregroundTexturePointer = {0},
            _debugTexturePointer = {0};

        public readonly int Width;

        public bool IsInitialized;

        private SceneData _sceneData;

        private readonly Vector2[] _screenEdges;
        private readonly Vector2[] _screenEdgeUvs;

        public Renderer(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            // this.graphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 0, 0, 0);

            _screenEdges = new[]
            {
                new Vector2(0, height),
                new Vector2(0, 0),
                new Vector2(width, 0),

                new Vector2(0, height),
                new Vector2(width, height),
                new Vector2(width, 0)
            };

            _screenEdgeUvs = new[]
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
            // GL.DeleteRenderbuffers(1, renderBuffer);
        }

        public void Initialize()
        {
            if (IsInitialized)
                throw new Exception("KBM Renderer is already initialized");
            IsInitialized = true;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(0f, Width, Height, 0f, -1000f, 1000f);

            GL.GenRenderbuffers(1, _renderBuffer);
            GL.GenTextures(1, _backgroundTexturePointer);
            GL.GenTextures(1, _foregroundTexturePointer);
            GL.GenTextures(1, _debugTexturePointer);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _renderBuffer[0]);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, Width, Height);

            SetBackgroundTexture(new byte[]
            {
                200, 200, 200, 255, 200, 200, 200, 255,
                200, 200, 200, 255, 200, 200, 200, 255
            }, 2, 2);
        }

        public void SetForegroundTexture(byte[] foregroundColorBuffer)
        {
            GL.BindTexture(TextureTarget.Texture2D, _foregroundTexturePointer[0]);

            unsafe
            {
                fixed (byte* ptr = &foregroundColorBuffer[0])
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                        PixelFormat.Bgra,
                        PixelType.UnsignedByte, new IntPtr(ptr));
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) All.Linear);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetBackgroundTexture(byte[] backgroundColorBuffer, int width, int height)
        {
            GL.BindTexture(TextureTarget.Texture2D, _backgroundTexturePointer[0]);

            unsafe
            {
                fixed (byte* ptr = &backgroundColorBuffer[0])
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                        PixelFormat.Bgra,
                        PixelType.UnsignedByte, new IntPtr(ptr));
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) All.Linear);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetDebugTexture(byte[] debugColorBuffer)
        {
            GL.BindTexture(TextureTarget.Texture2D, _debugTexturePointer[0]);

            unsafe
            {
                fixed (byte* ptr = &debugColorBuffer[0])
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                        PixelFormat.Bgra,
                        PixelType.UnsignedByte, new IntPtr(ptr));
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) All.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) All.Nearest);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetSceneData(SceneData sceneData)
        {
            this._sceneData = sceneData;
        }

        public void Draw()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var modelView = Matrix4.LookAt(new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            DrawBackground();
            DrawForeground();

            if (Settings.Instance.ShouldDrawDebugOverlay()) DrawDebugOverlay();
        }

        private void DrawBackground()
        {
            GL.BindTexture(TextureTarget.Texture2D, _backgroundTexturePointer[0]);
            GL.Enable(EnableCap.Texture2D);

            GL.Begin(PrimitiveType.Triangles);

            GL.Color3(1f, 1f, 1f);

            for (var i = 0; i < _screenEdges.Length; i++)
            {
                GL.TexCoord2(_screenEdgeUvs[i].X, _screenEdgeUvs[i].Y);
                GL.Vertex2(_screenEdges[i].X, _screenEdges[i].Y);
            }

            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void DrawForeground()
        {
            switch (Settings.Instance.DrawMode)
            {
                case Settings.GlDrawModeEnum.Normal:
                    if (_sceneData.Mesh != null)
                    {
                        GL.BindTexture(TextureTarget.Texture2D, _foregroundTexturePointer[0]);
                        GL.Enable(EnableCap.Texture2D);

                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                        GL.Begin(PrimitiveType.Triangles);
                        GL.Color3(1f, 1f, 1f);

                        for (var i = 0; i < _sceneData.Mesh.Indices.Count; i += 3)
                        {
                            var a = _sceneData.Mesh.Indices[i];
                            var b = _sceneData.Mesh.Indices[i + 1];
                            var c = _sceneData.Mesh.Indices[i + 2];

                            GL.TexCoord2(_sceneData.Mesh.Uvs[a].X, _sceneData.Mesh.Uvs[a].Y);
                            GL.Vertex3(_sceneData.Mesh.Vertices[a].X, _sceneData.Mesh.Vertices[a].Y,
                                _sceneData.Mesh.Vertices[a].Z);

                            GL.TexCoord2(_sceneData.Mesh.Uvs[b].X, _sceneData.Mesh.Uvs[b].Y);
                            GL.Vertex3(_sceneData.Mesh.Vertices[b].X, _sceneData.Mesh.Vertices[b].Y,
                                _sceneData.Mesh.Vertices[b].Z);

                            GL.TexCoord2(_sceneData.Mesh.Uvs[c].X, _sceneData.Mesh.Uvs[c].Y);
                            GL.Vertex3(_sceneData.Mesh.Vertices[c].X, _sceneData.Mesh.Vertices[c].Y,
                                _sceneData.Mesh.Vertices[c].Z);
                        }

                        GL.End();

                        GL.Disable(EnableCap.Blend);
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    }

                    break;
                case Settings.GlDrawModeEnum.Uvs:
                    if (_sceneData.Mesh != null)
                    {
                        GL.Begin(PrimitiveType.Triangles);

                        for (var i = 0; i < _sceneData.Mesh.Indices.Count; i += 3)
                        {
                            var a = _sceneData.Mesh.Indices[i];
                            var b = _sceneData.Mesh.Indices[i + 1];
                            var c = _sceneData.Mesh.Indices[i + 2];

                            // var color = (sceneData.mesh.vertices[a].Z % 255) / 255f;
                            var colorA = _sceneData.Mesh.VertexWeightsDictionary.ContainsKey(_sceneData.Mesh.Vertices[a])
                                ? _sceneData.Mesh.VertexWeightsDictionary[_sceneData.Mesh.Vertices[a]]
                                : 0;
                            var colorB = _sceneData.Mesh.VertexWeightsDictionary.ContainsKey(_sceneData.Mesh.Vertices[b])
                                ? _sceneData.Mesh.VertexWeightsDictionary[_sceneData.Mesh.Vertices[b]]
                                : 0;
                            var colorC = _sceneData.Mesh.VertexWeightsDictionary.ContainsKey(_sceneData.Mesh.Vertices[c])
                                ? _sceneData.Mesh.VertexWeightsDictionary[_sceneData.Mesh.Vertices[c]]
                                : 0;

                            // GL.Color3(sceneData.mesh.uvs[a].X, sceneData.mesh.uvs[a].Y, colorA);
                            GL.Color3(colorA, colorA, colorA);
                            GL.Vertex2(_sceneData.Mesh.Vertices[a].X, _sceneData.Mesh.Vertices[a].Y);

                            // GL.Color3(sceneData.mesh.uvs[b].X, sceneData.mesh.uvs[b].Y, colorB);
                            GL.Color3(colorB, colorB, colorB);
                            GL.Vertex2(_sceneData.Mesh.Vertices[b].X, _sceneData.Mesh.Vertices[b].Y);

                            // GL.Color3(sceneData.mesh.uvs[c].X, sceneData.mesh.uvs[c].Y, colorC);
                            GL.Color3(colorC, colorC, colorC);
                            GL.Vertex2(_sceneData.Mesh.Vertices[c].X, _sceneData.Mesh.Vertices[c].Y);
                        }

                        GL.End();

                        DrawTriangleLines();
                    }

                    break;
                case Settings.GlDrawModeEnum.Lines:
                    if (_sceneData.Mesh != null)
                    {
                        GL.Begin(PrimitiveType.Triangles);
                        GL.Color4(1f, 1f, 1f, 1f);

                        for (var i = 0; i < _sceneData.Mesh.Indices.Count; i += 3)
                        {
                            var a = _sceneData.Mesh.Indices[i];
                            var b = _sceneData.Mesh.Indices[i + 1];
                            var c = _sceneData.Mesh.Indices[i + 2];

                            GL.Vertex2(_sceneData.Mesh.Vertices[a].X, _sceneData.Mesh.Vertices[a].Y);
                            GL.Vertex2(_sceneData.Mesh.Vertices[b].X, _sceneData.Mesh.Vertices[b].Y);
                            GL.Vertex2(_sceneData.Mesh.Vertices[c].X, _sceneData.Mesh.Vertices[c].Y);
                        }

                        GL.End();

                        DrawTriangleLines();
                    }

                    break;
            }
        }

        private void DrawTriangleLines()
        {
            for (var i = 0; i < _sceneData.Mesh.Indices.Count; i += 3)
            {
                GL.Begin(PrimitiveType.LineLoop);
                GL.Color3(0f, 0f, 0f);

                var a = _sceneData.Mesh.Indices[i];
                var b = _sceneData.Mesh.Indices[i + 1];
                var c = _sceneData.Mesh.Indices[i + 2];

                GL.Vertex2(_sceneData.Mesh.Vertices[a].X, _sceneData.Mesh.Vertices[a].Y);
                GL.Vertex2(_sceneData.Mesh.Vertices[b].X, _sceneData.Mesh.Vertices[b].Y);
                GL.Vertex2(_sceneData.Mesh.Vertices[c].X, _sceneData.Mesh.Vertices[c].Y);
                GL.End();
            }
        }

        private void DrawDebugOverlay()
        {
            GL.BindTexture(TextureTarget.Texture2D, _debugTexturePointer[0]);
            GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Begin(PrimitiveType.Triangles);

            GL.Color3(1f, 1f, 1f);

            for (var i = 0; i < _screenEdges.Length; i++)
            {
                GL.TexCoord2(_screenEdgeUvs[i].X, _screenEdgeUvs[i].Y);
                GL.Vertex2(_screenEdges[i].X, _screenEdges[i].Y);
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