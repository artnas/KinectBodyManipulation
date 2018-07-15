//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.Kinect.Toolkit.BackgroundRemoval;

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor sensor;

        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        private WriteableBitmap colorBitmap;
        private WriteableBitmap playerOpacityMaskImage = null;

        private DepthImagePixel[] depthBuffer;    
        private byte[] colorBuffer;
        private int colorToDepthDivisor;

        private byte[] outputBuffer;
        private int[] playerPixelData;
        private ColorImagePoint[] colorCoordinates;

        private bool hasSavedBackgroundColorFrame = false;
        private bool hasSavedBackgroundDepthFrame = false;
        private byte[] backgroundRemovedBuffer;
        private BackgroundRemovedColorStream backgroundRemovedColorStream;

        private byte[] savedBackgroundColorBuffer;
        private DepthImagePixel[] savedBackgroundDepthBuffer;

        private SkeletonStream skeletonStream;
        private Skeleton[] skeletons = new Skeleton[6];
        private int trackedPlayerId = -1;

        private byte[] normalBuffer = new byte[Configuration.size * 2];

        private int depthWidth, depthHeight;

        private int opaquePixelValue = -1;

        private LimbDataManager limbDataManager;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {

                this.sensor.ColorStream.Enable(ColorFormat); 
                this.sensor.DepthStream.Enable(DepthFormat); // Turn on the depth stream to receive depth frames
                this.sensor.SkeletonStream.Enable(); // Turn on to get player masks

                this.depthWidth = this.sensor.DepthStream.FrameWidth;
                this.depthHeight = this.sensor.DepthStream.FrameHeight;

                int colorWidth = this.sensor.ColorStream.FrameWidth;
                int colorHeight = this.sensor.ColorStream.FrameHeight;

                this.colorToDepthDivisor = colorWidth / this.depthWidth;

                // Allocate space to put the depth pixels we'll receive
                this.depthBuffer = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.colorBuffer = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.backgroundRemovedBuffer = new byte[this.colorBuffer.Length];
                this.outputBuffer = new byte[this.colorBuffer.Length];

                this.savedBackgroundColorBuffer = new byte[this.colorBuffer.Length];
                this.savedBackgroundDepthBuffer = new DepthImagePixel[this.depthBuffer.Length];

                this.playerPixelData = new int[this.sensor.DepthStream.FramePixelDataLength];

                this.colorCoordinates = new ColorImagePoint[this.sensor.DepthStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                //
                this.limbDataManager = new LimbDataManager(colorBuffer, depthBuffer, backgroundRemovedBuffer, sensor);

                this.backgroundRemovedColorStream = new BackgroundRemovedColorStream(this.sensor);
                this.backgroundRemovedColorStream.Enable(ColorFormat, DepthFormat);
                this.backgroundRemovedColorStream.SetTrackedPlayer(0);

                // Add an event handler to be called when the background removed color frame is ready, so that we can
                // composite the image and output to the app
                this.backgroundRemovedColorStream.BackgroundRemovedFrameReady += this.BackgroundRemovedFrameReadyHandler;

                // Set the image we display to point to the bitmap where we'll put the image data
                this.MaskedColor.Source = this.colorBitmap;

                Drawing.SetBuffers(depthBuffer, colorBuffer, outputBuffer, backgroundRemovedBuffer, limbDataManager, savedBackgroundColorBuffer, savedBackgroundDepthBuffer, normalBuffer);
                BoneProcessor.SetBuffers(depthBuffer, colorBuffer, outputBuffer, backgroundRemovedBuffer, limbDataManager, savedBackgroundColorBuffer, savedBackgroundDepthBuffer, normalBuffer);

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.AllFramesReady += this.SensorAllFramesReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }

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

                    Array.Copy(backgroundRemovedFrame.GetRawPixelData(), this.backgroundRemovedBuffer, this.backgroundRemovedBuffer.Length);

                }
            }

            //Array.Copy(this.backgroundRemovedBuffer, this.outputBuffer, this.outputBuffer.Length);

            limbDataManager.Update(skeletons);

            Drawing.DrawBackground();

            BoneProcessor.ProcessAllBones();

            
            //Drawing.DrawDebug(true, true, true, true);        
            //Drawing.DrawHuman();
            //Drawing.ProcessNormalDisplacement();

            DrawOutputBuffer();

        }

        private void DrawOutputBuffer()
        {

            this.colorBitmap.WritePixels(
                new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                this.outputBuffer,
                this.colorBitmap.PixelWidth * sizeof(int),
                0);

        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.backgroundRemovedColorStream)
            {
                this.backgroundRemovedColorStream.Dispose();
                this.backgroundRemovedColorStream = null;
            }

            if (null != this.sensor)
            {
                this.sensor.Stop();
                this.sensor = null;
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // in the middle of shutting down, so nothing to do
            if (null == this.sensor)
            {
                return;
            }

            bool depthReceived = false;
            bool colorReceived = false;
            bool skeletonsReceived = false;

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (null != depthFrame)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthBuffer);
                    this.backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);

                    depthReceived = true;
                }
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (null != colorFrame)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorBuffer);
                    this.backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);

                    colorReceived = true;

                    if (hasSavedBackgroundColorFrame == false)
                    {
                        hasSavedBackgroundColorFrame = true;
                        Array.Copy(colorBuffer, savedBackgroundColorBuffer, colorBuffer.Length);
                    }
                }
            }

            // do our processing outside of the using block
            // so that we return resources to the kinect as soon as possible
            if (true == depthReceived)
            {
                this.sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    DepthFormat,
                    this.depthBuffer,
                    ColorFormat,
                    this.colorCoordinates);

                if (hasSavedBackgroundDepthFrame == false)
                {
                    hasSavedBackgroundDepthFrame = true;
                    Array.Copy(depthBuffer, savedBackgroundDepthBuffer, depthBuffer.Length);
                }

                Array.Clear(this.playerPixelData, 0, this.playerPixelData.Length);

                // loop over each row and column of the depth
                for (int y = 0; y < this.depthHeight; ++y)
                {
                    for (int x = 0; x < this.depthWidth; ++x)
                    {
                        // calculate index into depth array
                        int depthIndex = x + (y * this.depthWidth);

                        DepthImagePixel depthPixel = this.depthBuffer[depthIndex];

                        int player = depthPixel.PlayerIndex;

                        // if we're tracking a player for the current pixel, sets it opacity to full
                        if (player > 0)
                        {
                            // retrieve the depth to color mapping for the current depth pixel
                            ColorImagePoint colorImagePoint = this.colorCoordinates[depthIndex];

                            // scale color coordinates to depth resolution
                            int colorInDepthX = colorImagePoint.X / this.colorToDepthDivisor;
                            int colorInDepthY = colorImagePoint.Y / this.colorToDepthDivisor;

                            // make sure the depth pixel maps to a valid point in color space
                            // check y > 0 and y < depthHeight to make sure we don't write outside of the array
                            // check x > 0 instead of >= 0 since to fill gaps we set opaque current pixel plus the one to the left
                            // because of how the sensor works it is more correct to do it this way than to set to the right
                            if (colorInDepthX > 0 && colorInDepthX < this.depthWidth && colorInDepthY >= 0 && colorInDepthY < this.depthHeight)
                            {
                                // calculate index into the player mask pixel array
                                int playerPixelIndex = colorInDepthX + (colorInDepthY * this.depthWidth);

                                // set opaque
                                this.playerPixelData[playerPixelIndex] = opaquePixelValue;

                                // compensate for depth/color not corresponding exactly by setting the pixel 
                                // to the left to opaque as well
                                this.playerPixelData[playerPixelIndex - 1] = opaquePixelValue;
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
                    this.backgroundRemovedColorStream.ProcessSkeleton(skeletons, skeletonFrame.Timestamp);

                    if (trackedPlayerId == -1)
                    {
                        foreach (var skeleton in skeletons)
                        {
                            if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                this.backgroundRemovedColorStream.SetTrackedPlayer(skeleton.TrackingId);
                                trackedPlayerId = skeleton.TrackingId;
                                Console.WriteLine("setting tracked id to " + trackedPlayerId);
                            }
                        }
                    }

                }
            }

        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            int colorWidth = this.sensor.ColorStream.FrameWidth;
            int colorHeight = this.sensor.ColorStream.FrameHeight;

            // create a render target that we'll render our controls to
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Pbgra32);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                // render the backdrop
                VisualBrush backdropBrush = new VisualBrush(Backdrop);
                dc.DrawRectangle(backdropBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));

                // render the color image masked out by players
                VisualBrush colorBrush = new VisualBrush(MaskedColor);
                dc.DrawRectangle(colorBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));
            }

            renderBitmap.Render(dv);
    
            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteSuccess, path);
            }
            catch (IOException)
            {
                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
            }
        }
        
        /// <summary>
        /// Handles the checking or unchecking of the near mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                // will not function on non-Kinect for Windows devices
                try
                {
                    if (this.checkBoxNearMode.IsChecked.GetValueOrDefault())
                    {
                        this.sensor.DepthStream.Range = DepthRange.Near;
                    }
                    else
                    {
                        this.sensor.DepthStream.Range = DepthRange.Default;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
    }
}