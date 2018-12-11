﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KBMGraphics;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.BackgroundRemoval;
using OpenTK.Graphics.OpenGL;
using Drawing = KinectBodyModification.Drawing;

using GB = KinectBodyModification.GlobalBuffers;

namespace KinectBodyModification
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Renderer renderer;
        private SceneData sceneData;

        private bool isProcessingFrame = false;

        private KinectSensor sensor;

        private bool hasSavedBackgroundColorFrame = false;
        private bool hasSavedBackgroundDepthFrame = false;

        private readonly Skeleton[] skeletons = new Skeleton[6];
        private int trackedPlayerId = -1;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            DataContext = Settings.Instance;
            InitializeComponent();
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (renderer == null)
            {
                InitializeRenderer();
            }

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    sensor = potentialSensor;
                    break;
                }
            }

            if (null != sensor)
            {

                sensor.ColorStream.Enable(Configuration.ColorFormat); 
                sensor.DepthStream.Enable(Configuration.DepthFormat); // Turn on the depth stream to receive depth frames
                sensor.SkeletonStream.Enable(); // Turn on to get player masks

                // Allocate space to put the depth allPixels we'll receive
                GB.depthBuffer = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
                // Allocate space to put the color allPixels we'll create
                GB.colorBuffer = new byte[sensor.ColorStream.FramePixelDataLength];

                GB.backgroundRemovedBuffer = new byte[GB.colorBuffer.Length];
                GB.outputBuffer = new byte[GB.colorBuffer.Length];

                GB.savedBackgroundColorBuffer = new byte[GB.colorBuffer.Length];
                GB.savedBackgroundDepthBuffer = new DepthImagePixel[GB.depthBuffer.Length];

                GB.playerPixelData = new int[sensor.DepthStream.FramePixelDataLength];

                GB.colorCoordinates = new ColorImagePoint[sensor.DepthStream.FramePixelDataLength];
                GB.depthCoordinates = new DepthImagePoint[sensor.DepthStream.FramePixelDataLength];

                GB.limbDataManager = new LimbDataManager(sensor);

                GB.backgroundRemovedColorStream = new BackgroundRemovedColorStream(sensor);
                GB.backgroundRemovedColorStream.Enable(Configuration.ColorFormat, Configuration.DepthFormat);
                GB.backgroundRemovedColorStream.SetTrackedPlayer(0);

                // Add an event handler to be called when the background removed color frame is ready, so that we can
                // composite the image and output to the app
                GB.backgroundRemovedColorStream.BackgroundRemovedFrameReady += this.BackgroundRemovedFrameReadyHandler;

                // Add an event handler to be called whenever there is new depth frame data
                sensor.AllFramesReady += SensorAllFramesReady;

                // Start the sensor!
                try
                {
                    sensor.Start();
                }
                catch (IOException)
                {
                    sensor = null;
                }
            }

            statusBarText.Text = sensor == null ? Properties.Resources.NoKinectReady : $"Połączono z kontrolerem Kinect (id: ${sensor.UniqueKinectId}, connection: ${sensor.DeviceConnectionId})";
        }

        /// <summary>
        /// Handle the background removed color frame ready event. The frame obtained from the background removed
        /// color stream is in RGBA format.
        /// </summary>
        /// <param name="sender">object that sends the event</param>
        /// <param name="e">argument of the event</param>
        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {
            using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {
                    Array.Copy(backgroundRemovedFrame.GetRawPixelData(), GB.backgroundRemovedBuffer, GB.backgroundRemovedBuffer.Length);
                }
            }

            Draw();
        }

        private void Draw()
        {
            renderer.SetForegroundTexture(GB.backgroundRemovedBuffer);

            GB.limbDataManager.Update(skeletons);
            BoneProcessor.ProcessAllBones();

            PrepareRenderSceneData();

            if (Settings.Instance.ShouldDrawDebugOverlay())
            {
                Drawing.DrawDebug();
                renderer.SetDebugTexture(GB.outputBuffer);
            }

            RenderGL();
        }

        private void PrepareRenderSceneData()
        {
            sceneData.mesh = GB.limbDataManager.limbData.mesh;
        }

        private void RenderGL()
        {
            if (renderer != null && sceneData != null)
            {
                renderer.Draw();

                GL.Flush();
                RenderCanvas.SwapBuffers();
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != GB.backgroundRemovedColorStream)
            {
                GB.backgroundRemovedColorStream.Dispose();
                GB.backgroundRemovedColorStream = null;
            }

            if (null != sensor)
            {
                sensor.Stop();
                sensor = null;
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (isProcessingFrame)
            {
                return;
            }

            isProcessingFrame = true;
            
            // in the middle of shutting down, so nothing to do
            if (sensor == null)
            {
                return;
            }

            bool depthReceived = false;
            bool colorReceived = false;
            bool skeletonsReceived = false;

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(GB.depthBuffer);
                    GB.backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);

                    depthReceived = true;
                }
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(GB.colorBuffer);
                    GB.backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);

                    colorReceived = true;

                    if (hasSavedBackgroundColorFrame == false)
                    {
                        hasSavedBackgroundColorFrame = true;
                        Array.Copy(GB.colorBuffer, GB.savedBackgroundColorBuffer, GB.colorBuffer.Length);

                        if (renderer != null && renderer.isInitialized)
                        {
                            renderer.SetBackgroundTexture(GB.savedBackgroundColorBuffer, Configuration.width, Configuration.height);
                        }
                    }
                }
            }

            // do our processing outside of the using block
            // so that we return resources to the kinect as soon as possible
            if (depthReceived)
            {
                sensor.CoordinateMapper.MapDepthFrameToColorFrame(Configuration.DepthFormat,
                    GB.depthBuffer, Configuration.ColorFormat,
                    GB.colorCoordinates);

                sensor.CoordinateMapper.MapColorFrameToDepthFrame(Configuration.ColorFormat, Configuration.DepthFormat,
                    GB.depthBuffer,
                    GB.depthCoordinates);

                if (hasSavedBackgroundDepthFrame == false)
                {
                    hasSavedBackgroundDepthFrame = true;
                    Array.Copy(GB.depthBuffer, GB.savedBackgroundDepthBuffer, GB.depthBuffer.Length);
                }

                Array.Clear(GB.playerPixelData, 0, GB.playerPixelData.Length);

                // loop over each row and column of the depth
                for (int y = 0; y < Configuration.height; ++y)
                {
                    for (int x = 0; x < Configuration.width; ++x)
                    {
                        // calculate index into depth array
                        int depthIndex = x + (y * Configuration.width);

                        DepthImagePixel depthPixel = GB.depthBuffer[depthIndex];

                        int player = depthPixel.PlayerIndex;

                        // if we're tracking a player for the current pixel, sets it opacity to full
                        if (player > 0)
                        {
                            // retrieve the depth to color mapping for the current depth pixel
                            ColorImagePoint colorImagePoint = GB.colorCoordinates[depthIndex];

                            // scale color coordinates to depth resolution
                            int colorInDepthX = colorImagePoint.X;
                            int colorInDepthY = colorImagePoint.Y;

                            // make sure the depth pixel maps to a valid point in color space
                            // check y > 0 and y < depthHeight to make sure we don't write outside of the array
                            // check x > 0 instead of >= 0 since to fill gaps we set opaque current pixel plus the one to the left
                            // because of how the sensor works it is more correct to do it this way than to set to the right
                            if (colorInDepthX > 0 && colorInDepthX < Configuration.width && colorInDepthY >= 0 && colorInDepthY < Configuration.height)
                            {
                                // calculate index into the player mask pixel array
                                int playerPixelIndex = colorInDepthX + (colorInDepthY * Configuration.width);

                                // int opaquePixelValue = -1;

                                // set opaque
                                GB.playerPixelData[playerPixelIndex] = -1;

                                // compensate for depth/color not corresponding exactly by setting the pixel 
                                // to the left to opaque as well
                                GB.playerPixelData[playerPixelIndex - 1] = -1;
                            }
                        }
                    }
                }
            }

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (null != skeletonFrame)
                {

                    //skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    GB.backgroundRemovedColorStream.ProcessSkeleton(skeletons, skeletonFrame.Timestamp);

                    if (trackedPlayerId == -1)
                    {
                        foreach (var skeleton in skeletons)
                        {
                            if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                GB.backgroundRemovedColorStream.SetTrackedPlayer(skeleton.TrackingId);
                                trackedPlayerId = skeleton.TrackingId;
                                Console.WriteLine("setting tracked id to " + trackedPlayerId);
                            }
                        }
                    }

                }
            }

            isProcessingFrame = false;
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
            renderer = new Renderer(Configuration.width, Configuration.height);
            sceneData = new SceneData();

            renderer.SetSceneData(sceneData);
            renderer.Initialize();
        }

        private void WindowsFormsHost_Initialized(object sender, EventArgs e)
        {
            RenderCanvas.MakeCurrent();
        }

        private void renderCanvas_Load(object sender, EventArgs e)
        {
            if (renderer == null)
            {
                InitializeRenderer();
            }
        }

        private void renderCanvas_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // RenderCanvas.Width = RenderCanvas.Parent.Width;
            // RenderCanvas.Height = (int)(RenderCanvas.Width * 0.75f);

            RenderGL();
        }

        private void OpenGLDrawModeSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Instance.DrawMode = (Settings.GLDrawModeEnum)OpenGLDrawModeSelector.SelectedIndex;
        }

        /// <summary>
        /// Wywoływane przy zmianie wielkości okna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            int width = RenderCanvas.Width;
            int height = (int) (RenderCanvas.Width * 0.75f);

            if (height > RenderCanvas.Height)
            {
                height = (int) RenderCanvas.Height;
                width = (int) (height * 1.3333f);
            }

            RenderCanvas.Width = width;
            RenderCanvas.Height = height;
            
            renderer.ResizeViewport(width, height);
        }
    }
}