using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using KBMGraphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace KBMGraphicsTests
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool canDraw = false;
        private int program;
        private int nVertices;

        private KBMRenderer renderer;
        private KBMSceneData sceneData;

        public MainWindow()
        {
            InitializeComponent();

            // using (DummyWindow dummyWindow = new DummyWindow())
            // {
            //     dummyWindow.Run(30.0);
            // }

            // WFH.Handle

            // Render();
        }

        private void WindowsFormsHost_Initialized(object sender, EventArgs e)
        {
            RenderCanvas.MakeCurrent();
        }

        private void renderCanvas_Load(object sender, EventArgs e)
        {        
            canDraw = true;

            byte[] outputBuffer = new byte[800 * 600 * 4];
            byte[] textureBuffer = new byte[800 * 600 * 4];

            renderer = new KBMRenderer(800, 600, outputBuffer, textureBuffer);
            sceneData = new KBMSceneData();
            sceneData.vertices = new Vector2[]
            {
                new Vector2(200, 100),
                new Vector2(400, 100),
                new Vector2(550, 300),
                new Vector2(400, 480),
                new Vector2(200, 480),
                new Vector2(50, 300)
            };

            renderer.SetSceneData(sceneData);
        }

        private void renderCanvas_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // GL.Viewport(0, 0, RenderCanvas.Width, RenderCanvas.Height);
            //
            // // Clear the render canvas with the current color
            // GL.Clear(ClearBufferMask.ColorBufferBit);
            //
            // if (canDraw)
            // {
            //     // Draw a triangle
            //     GL.DrawArrays(PrimitiveType.Triangles, 0, nVertices);
            // }
            //

            Random r = new Random();

            while (true)
            {

                for (var i = 0; i < sceneData.vertices.Length; i++)
                {
                    sceneData.vertices[i].X += (float) r.NextDouble() * 2 - 1;
                    sceneData.vertices[i].Y += (float) r.NextDouble() * 2 - 1;
                }

                renderer.Draw();

                GL.Flush();
                RenderCanvas.SwapBuffers();

                Thread.Sleep(30);

            }
        }

    }
}
