//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using KBMGraphics;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;
using OpenTK.Graphics.OpenGL;
using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Skeleton[] _skeletons = new Skeleton[6];

        private bool _hasSavedBackgroundColorFrame;
        private bool _hasSavedBackgroundDepthFrame;

        private bool _isProcessingFrame;

        private Renderer _renderer;
        private SceneData _sceneData;

        private KinectSensor _sensor;

        public MainWindow()
        {
            DataContext = Settings.Instance;
            InitializeComponent();
        }

        /// <summary>
        ///     Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (_renderer == null) InitializeRenderer();

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    _sensor = potentialSensor;
                    break;
                }

            if (null != _sensor)
            {
                _sensor.ColorStream.Enable(Configuration.ColorFormat);
                _sensor.DepthStream.Enable(Configuration.DepthFormat);
                _sensor.SkeletonStream.Enable(); // Turn on to get player masks

                // Allocate space to put the depth allPixels we'll receive
                GB.DepthBuffer = new DepthImagePixel[_sensor.DepthStream.FramePixelDataLength];
                // Allocate space to put the color allPixels we'll create
                GB.ColorBuffer = new byte[_sensor.ColorStream.FramePixelDataLength];

                GB.BackgroundRemovedBuffer = new byte[GB.ColorBuffer.Length];
                GB.OutputBuffer = new byte[GB.ColorBuffer.Length];

                GB.SavedBackgroundColorBuffer = new byte[GB.ColorBuffer.Length];
                GB.SavedBackgroundDepthBuffer = new DepthImagePixel[GB.DepthBuffer.Length];

                GB.ColorCoordinates = new ColorImagePoint[_sensor.DepthStream.FramePixelDataLength];
                GB.DepthCoordinates = new DepthImagePoint[_sensor.DepthStream.FramePixelDataLength];

                GB.LimbDataManager = new LimbDataManager(_sensor);

                GB.BackgroundRemovedColorStream = new BackgroundRemovedColorStream(_sensor);
                GB.BackgroundRemovedColorStream.Enable(Configuration.ColorFormat, Configuration.DepthFormat);
                GB.BackgroundRemovedColorStream.SetTrackedPlayer(0);

                // Add an event handler to be called when the background removed color frame is ready, so that we can
                // composite the image and output to the app
                GB.BackgroundRemovedColorStream.BackgroundRemovedFrameReady += BackgroundRemovedFrameReadyHandler;

                // Add an event handler to be called whenever there is new depth frame data
                _sensor.AllFramesReady += SensorAllFramesReady;

                // Start the sensor!
                try
                {
                    _sensor.Start();
                }
                catch (IOException)
                {
                    _sensor = null;
                }
            }

            statusBarText.Text = _sensor == null
                ? Properties.Resources.NoKinectReady
                : $"Połączono z kontrolerem Kinect (id: ${_sensor.UniqueKinectId}, connection: ${_sensor.DeviceConnectionId})";
        }

        /// <summary>
        ///     Handle the background removed color frame ready event. The frame obtained from the background removed
        ///     color stream is in RGBA format.
        /// </summary>
        /// <param name="sender">object that sends the event</param>
        /// <param name="e">argument of the event</param>
        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {
            using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                    Array.Copy(backgroundRemovedFrame.GetRawPixelData(), GB.BackgroundRemovedBuffer,
                        GB.BackgroundRemovedBuffer.Length);
            }

            Draw();
        }

        private void Draw()
        {
            _renderer.SetForegroundTexture(GB.BackgroundRemovedBuffer);

            GB.LimbDataManager.Update(_skeletons);

            if (GB.LimbDataManager.LimbData.LimbDataSkeleton != null)
            {
                BoneProcessor.ProcessAllBones();
            }

            PrepareRenderSceneData();

            if (Settings.Instance.ShouldDrawDebugOverlay())
            {
                Drawing.DrawDebug();
                _renderer.SetDebugTexture(GB.OutputBuffer);
            }

            RenderGL();
        }

        private void PrepareRenderSceneData()
        {
            _sceneData.Mesh = GB.LimbDataManager.LimbData.Mesh;
        }

        private void RenderGL()
        {
            if (_renderer != null && _sceneData != null)
            {
                _renderer.Draw();

                GL.Flush();
                RenderCanvas.SwapBuffers();
            }
        }

        /// <summary>
        ///     Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (null != GB.BackgroundRemovedColorStream)
            {
                GB.BackgroundRemovedColorStream.Dispose();
                GB.BackgroundRemovedColorStream = null;
            }

            if (null != _sensor)
            {
                _sensor.Stop();
                _sensor = null;
            }
        }

        /// <summary>
        ///     Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // trwa przetwarzanie poprzedniej klatki...
            if (_isProcessingFrame) return;
            _isProcessingFrame = true;

            // sensor w trakcie wyłączania...
            if (_sensor == null) return;

            var depthReceived = false;
            var colorReceived = false;

            using (var depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    depthReceived = true;

                    depthFrame.CopyDepthImagePixelDataTo(GB.DepthBuffer);
                    GB.BackgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);            
                }
            }

            using (var colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    colorReceived = true;

                    colorFrame.CopyPixelDataTo(GB.ColorBuffer);
                    GB.BackgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
                }
            }

            using (var skeletonFrame = e.OpenSkeletonFrame())
            {
                if (null != skeletonFrame)
                {
                    skeletonFrame.CopySkeletonDataTo(_skeletons);
                    GB.BackgroundRemovedColorStream.ProcessSkeleton(_skeletons, skeletonFrame.Timestamp);
                }
            }

            // bloki przetwarzania glębi i koloru są pod spodem, żeby jak najszybciej zwolnić zasoby kinecta

            if (depthReceived)
            {
                _sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    Configuration.DepthFormat, GB.DepthBuffer,
                    Configuration.ColorFormat, GB.ColorCoordinates);

                _sensor.CoordinateMapper.MapColorFrameToDepthFrame(
                    Configuration.ColorFormat, Configuration.DepthFormat,
                    GB.DepthBuffer, GB.DepthCoordinates);

                if (_hasSavedBackgroundDepthFrame == false)
                {
                    _hasSavedBackgroundDepthFrame = true;
                    Array.Copy(GB.DepthBuffer, GB.SavedBackgroundDepthBuffer, GB.DepthBuffer.Length);
                }
            }

            if (colorReceived)
            {
                if (_hasSavedBackgroundColorFrame == false)
                {
                    _hasSavedBackgroundColorFrame = true;
                    Array.Copy(GB.ColorBuffer, GB.SavedBackgroundColorBuffer, GB.ColorBuffer.Length);

                    if (_renderer != null && _renderer.IsInitialized)
                        _renderer.SetBackgroundTexture(GB.SavedBackgroundColorBuffer, Configuration.Width,
                            Configuration.Height);
                }
            }

            _isProcessingFrame = false;
        }

        // /// <summary>
        // /// Handles the user clicking on the screenshot button
        // /// </summary>
        // /// <param name="sender">object sending the event</param>
        // /// <param name="e">event arguments</param>
        // private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        // {
        //     if (null == this.sensor)
        //     {
        //         this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
        //         return;
        //     }
        //
        //     int colorWidth = this.sensor.ColorStream.FrameWidth;
        //     int colorHeight = this.sensor.ColorStream.FrameHeight;
        //
        //     // create a render target that we'll render our controls to
        //     RenderTargetBitmap renderBitmap = new RenderTargetBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Pbgra32);
        //
        //     DrawingVisual dv = new DrawingVisual();
        //     using (DrawingContext dc = dv.RenderOpen())
        //     {
        //         // render the backdrop
        //         VisualBrush backdropBrush = new VisualBrush(Backdrop);
        //         dc.DrawRectangle(backdropBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));
        //
        //         // render the color image masked out by players
        //         VisualBrush colorBrush = new VisualBrush(MaskedColor);
        //         dc.DrawRectangle(colorBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));
        //     }
        //
        //     renderBitmap.RenderGL(dv);
        //
        //     // create a png bitmap encoder which knows how to save a .png file
        //     BitmapEncoder encoder = new PngBitmapEncoder();
        //
        //     // create frame from the writable bitmap and add to encoder
        //     encoder.Frames.Add(BitmapFrame.Create(renderBitmap)); 
        //
        //     string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);
        //
        //     string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        //
        //     string path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");
        //
        //     // write the new file to disk
        //     try
        //     {
        //         using (FileStream fs = new FileStream(path, FileMode.Create))
        //         {
        //             encoder.Save(fs);
        //         }
        //
        //         this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
        //     }
        //     catch (IOException)
        //     {
        //         this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
        //     }
        // }

        // /// <summary>
        // /// Handles the checking or unchecking of the near mode combo box
        // /// </summary>
        // /// <param name="sender">object sending the event</param>
        // /// <param name="e">event arguments</param>
        // private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
        // {
        //     if (this.sensor != null)
        //     {
        //         // will not function on non-Kinect for Windows devices
        //         try
        //         {
        //             if (this.checkBoxNearMode.IsChecked.GetValueOrDefault())
        //             {
        //                 this.sensor.DepthStream.Range = DepthRange.Near;
        //             }
        //             else
        //             {
        //                 this.sensor.DepthStream.Range = DepthRange.Default;
        //             }
        //         }
        //         catch (InvalidOperationException)
        //         {
        //         }
        //     }
        // }

        private void InitializeRenderer()
        {
            _renderer = new Renderer(Configuration.Width, Configuration.Height);
            _sceneData = new SceneData();

            _renderer.SetSceneData(_sceneData);
            _renderer.Initialize();
        }

        private void WindowsFormsHost_Initialized(object sender, EventArgs e)
        {
            RenderCanvas.MakeCurrent();
        }

        private void renderCanvas_Load(object sender, EventArgs e)
        {
            if (_renderer == null) InitializeRenderer();
        }

        private void renderCanvas_Paint(object sender, PaintEventArgs e)
        {
            RenderGL();
        }

        private void OpenGLDrawModeSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Instance.DrawMode = (Settings.GlDrawModeEnum) OpenGLDrawModeSelector.SelectedIndex;
        }

        /// <summary>
        ///     Wywoływane przy zmianie wielkości okna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = RenderCanvas.Width;
            var height = (int) (RenderCanvas.Width * 0.75f);

            if (height > RenderCanvas.Height)
            {
                height = RenderCanvas.Height;
                width = (int) (height * 1.3333f);
            }

            RenderCanvas.Width = width;
            RenderCanvas.Height = height;

            _renderer.ResizeViewport(width, height);
        }
    }
}