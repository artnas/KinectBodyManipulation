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
        }

        private void WindowsFormsHost_Initialized(object sender, EventArgs e)
        {
            RenderCanvas.MakeCurrent();
        }

        private void renderCanvas_Load(object sender, EventArgs e)
        {        
            canDraw = true;

            renderer = new KBMRenderer(640, 480);
            sceneData = new KBMSceneData();

            renderer.SetSceneData(sceneData);
        }

        private void renderCanvas_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Random r = new Random();

            // for (var i = 0; i < sceneData.vertices.Length; i++)
            // {
            //     sceneData.vertices[i].X += (float) r.NextDouble() * 2 - 1;
            //     sceneData.vertices[i].Y += (float) r.NextDouble() * 2 - 1;
            // }

            renderer.Draw();

            GL.Flush();
            RenderCanvas.SwapBuffers();
        }

    }
}
