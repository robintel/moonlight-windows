﻿namespace Limelight
{
    using System.Net;
    using System.Threading;
    using System.Windows;
    using Limelight_common_binding;
    using Microsoft.Phone.Controls;
    using System.Diagnostics;
    using Microsoft.Xna.Framework.Input;
    using System;
    using System.ComponentModel;
    using Microsoft.Phone.Shell;
    using Microsoft.Phone.Net.NetworkInformation;
    using System.Threading.Tasks; 

    /// <summary>
    /// UI Frame that contains the media element that streams Steam
    /// </summary>
    public partial class StreamFrame : PhoneApplicationPage
    {
        #region Class Variables

        /// <summary>
        /// Connection stage identifiers
        /// </summary>
        private const int STAGE_NONE = 0;
        private const int STAGE_PLATFORM_INIT = 1;
        private const int STAGE_HANDSHAKE = 2;
        private const int STAGE_CONTROL_STREAM_INIT = 3;
        private const int STAGE_VIDEO_STREAM_INIT = 4;
        private const int STAGE_AUDIO_STREAM_INIT = 5;
        private const int STAGE_INPUT_STREAM_INIT = 6;
        private const int STAGE_CONTROL_STREAM_START = 7;
        private const int STAGE_VIDEO_STREAM_START = 8;
        private const int STAGE_AUDIO_STREAM_START = 9;
        private const int STAGE_INPUT_STREAM_START = 10;
        private const int STAGE_MAX = 11;

        /// <summary>
        /// Width and height of the frame from the video source
        /// TODO Make these numbers less magic
        /// </summary>
        private int frameWidth = 1280;
        private int frameHeight = 720;

        /// <summary>
        /// Mouse input
        /// </summary>
        private bool hasMoved = false;

        /// <summary>
        /// Gets and sets the custom AV source
        /// </summary>
        internal AvStreamSource AvStream { get; private set; }

        /// <summary>
        /// Background worker and callbacks
        /// </summary>
        private BackgroundWorker bw = new BackgroundWorker();
        private String stageFailureText;

        #endregion Class Variables

        #region Init

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamFrame"/> class. 
        /// </summary>
        public StreamFrame()
        {
            InitializeComponent();

            // Set up the XML query object with serverinfo
            // TODO the real url


            // TODO Convince the app to run synchronously 

            AvStream = new AvStreamSource(frameWidth, frameHeight);
            StreamDisplay.SetSource(AvStream);
            StreamDisplay.AutoPlay = true;
            StreamDisplay.Play();

            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;

            bw.DoWork += new DoWorkEventHandler(bwDoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwRunWorkerCompleted);

            Waitgrid.Visibility = Visibility.Visible;
            currentStateText.Visibility = Visibility.Visible;
            bw.RunWorkerAsync();
        }
        #endregion Init

        #region Callbacks
        public void DrSetup(int width, int height, int redrawRate, int drFlags)
        {

        }

        public void DrStart()
        {

        }

        public void DrStop()
        {

        }

        public void DrRelease()
        {

        }

        public void DrSubmitDecodeUnit(byte[] data)
        {
            AvStream.EnqueueVideoSamples(data);
        }

        public void ArInit()
        {

        }

        public void ArStart()
        {

        }

        public void ArStop()
        {

        }

        public void ArRelease()
        {

        }

        public void ArPlaySample(byte[] data)
        {
            AvStream.EnqueueAudioSamples(data);
        }

        public void ClStageStarting(int stage)
        {
            String stateText = ""; 
            switch (stage)
            {
                case STAGE_PLATFORM_INIT:
                    stateText = "Initializing platform...";
                    break;
                case STAGE_HANDSHAKE:
                    stateText = "Starting handshake...";
                    break;
                case STAGE_CONTROL_STREAM_INIT:
                    stateText = "Initializing control stream...";
                    break;
                case STAGE_VIDEO_STREAM_INIT:
                    stateText = "Initializing video stream...";
                    break;
                case STAGE_AUDIO_STREAM_INIT:
                    stateText = "Initializing audio stream...";
                    break;
                case STAGE_INPUT_STREAM_INIT:
                    stateText = "Initializing input stream...";
                    break;
                case STAGE_CONTROL_STREAM_START:
                    stateText = "Starting control stream...";
                    break;
                case STAGE_VIDEO_STREAM_START:
                    stateText = "Starting video stream...";
                    break;
                case STAGE_AUDIO_STREAM_START:
                    stateText = "Starting audio stream...";
                    break;
                case STAGE_INPUT_STREAM_START:
                    stateText = "Starting input stream...";
                    break;
            }
            // Send the stage change to the UI thread. 
            // The dispatcher might not be quick enough for the user to see every stage
            Dispatcher.BeginInvoke(new Action(() => setStateText(stateText)));
        }

        public void ClStageComplete(int stage)
        {

        }

        public void ClStageFailed(int stage, int errorCode)
        {
            switch (stage)
            {
                case STAGE_PLATFORM_INIT:
                    stageFailureText = "Initializing platform failed. Error: " + errorCode.ToString();
                    break;
                case STAGE_HANDSHAKE:
                    stageFailureText = "Starting handshake failed. Error: " + errorCode.ToString();
                    break;
                case STAGE_CONTROL_STREAM_INIT:
                    stageFailureText = "Initializing control stream failed. Error: " + errorCode.ToString();
                    break;
                case STAGE_VIDEO_STREAM_INIT:
                    stageFailureText = "Initializing video stream failed. Error: " + errorCode.ToString();
                    break;
                case STAGE_AUDIO_STREAM_INIT:
                    stageFailureText = "Initializing audio stream failed. Error: " + errorCode.ToString();
                    break;
                case STAGE_INPUT_STREAM_INIT:
                    stageFailureText = "Initializing input stream failed. Error: " + errorCode.ToString();
                    break;
                case STAGE_CONTROL_STREAM_START:
                    stageFailureText = "Starting control stream failed. Error: " + errorCode.ToString();
                    break;
                case STAGE_VIDEO_STREAM_START:
                    stageFailureText = "Starting video stream failed. Error: " + errorCode.ToString();
                    break;
                case STAGE_AUDIO_STREAM_START:
                    stageFailureText = "Starting audio stream failed. Error: " + errorCode.ToString();
                    break;
                case STAGE_INPUT_STREAM_START:
                    stageFailureText = "Starting input stream failed. Error: " + errorCode.ToString();
                    break;
            }
        }

        public void ClConnectionStarted()
        {

        }

        public void ClConnectionTerminated(int errorCode)
        {
            Debug.WriteLine("Connection terminated: " + errorCode);
        }

        public void ClDisplayMessage(String message)
        {
        }

        public void ClDisplayTransientMessage(String message)
        {

        }

        #endregion Callbacks

        #region Background Worker

        /// <summary>
        /// Event handler for Background Worker's doWork event.
        /// </summary>
        private void bwDoWork(object sender, DoWorkEventArgs e)
        {

            String hostnameString = (String)PhoneApplicationService.Current.State["host"];
            Dispatcher.BeginInvoke(new Action(() => setStateText("Resolving hostname...")));
            NvHttp nv = new NvHttp(hostnameString);

            ServerInfo serverInfo = new ServerInfo(nv.baseUrl + "/serverinfo");

            // Set up callbacks
            LimelightStreamConfiguration streamConfig = new LimelightStreamConfiguration(frameWidth, frameHeight, 30, 10000, 1024); // TODO a magic number. Get FPS from the settings
            LimelightDecoderRenderer drCallbacks = new LimelightDecoderRenderer(DrSetup, DrStart, DrStop, DrRelease, DrSubmitDecodeUnit);
            LimelightAudioRenderer arCallbacks = new LimelightAudioRenderer(ArInit, ArStart, ArStop, ArRelease, ArPlaySample);
            LimelightConnectionListener clCallbacks = new LimelightConnectionListener(ClStageStarting, ClStageComplete, ClStageFailed,
            ClConnectionStarted, ClConnectionTerminated, ClDisplayMessage, ClDisplayTransientMessage);

            // Call into Common to start the connection
            Debug.WriteLine("Starting connection");
            LimelightCommonRuntimeComponent.StartConnection((uint)nv.resolvedHost.Address, streamConfig, clCallbacks, drCallbacks, arCallbacks);

            // If one of the stages failed, tell the background worker to cancel
            if(stageFailureText != null)
            {
                Debug.WriteLine("Stage failed - background worker cancelled");
                e.Cancel = true;
            }
        }

        // <summary>
        /// Runs once the background worker completes
        /// </summary>
        void bwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Waitgrid.Visibility = Visibility.Collapsed;
            this.currentStateText.Visibility = Visibility.Collapsed; 

            // Check to see if an error occurred in the background process.
            if (e.Error != null)
            {
                Debug.WriteLine("Error while performing background operation.");
                MessageBoxResult result = MessageBox.Show(e.Error.Message);
                if (result == MessageBoxResult.OK)
                {
                    // Return to the settings page
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    // TODO clean up
                }
            }

            // If the connection attempt was cancelled by a failed stage
            else if(e.Cancelled) 
            {
                // Inform the user of the failure via a message box
                MessageBoxResult result = MessageBox.Show(stageFailureText, "Failure Starting Connection",  MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                {
                    // Return to the settings page
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
            }
                
            // Everything completed normally - bring the user to the stream frame
            else
            {
                Debug.WriteLine("Background Worker Successfully Completed");

                StreamDisplay.Visibility = Visibility.Visible; 
            }

        }

        #endregion Background Worker

        #region Private Methods

        /// <summary>
        /// Touch event initiated
        /// </summary>
        private void touchDownEvent(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            MouseState ms = Mouse.GetState();
            hasMoved = false; 
        }

        private void touchUpEvent(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (!hasMoved)
            {
                // We haven't moved so send a click

                LimelightCommonRuntimeComponent.SendMouseButtonEvent((byte)MouseButtonAction.Press, (int)MouseButton.Left); 

                // Sleep here because some games do input detection by polling
                try
                {
                    Thread.Sleep(100);
                }
                catch (Exception ex) {
                    Debug.WriteLine("Thread.sleep threw exception " + ex.StackTrace);
                }

                // Raise the mouse button
                LimelightCommonRuntimeComponent.SendMouseButtonEvent((byte)MouseButtonAction.Release, (int)MouseButton.Left);
            }
        }

        private void touchMoveEvent(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            MouseState ms = Mouse.GetState();

            // If the user has moved
            if (ms.X != e.DeltaManipulation.Translation.X || ms.Y != e.DeltaManipulation.Translation.Y)
            {
                hasMoved = true;
                LimelightCommonRuntimeComponent.SendMouseMoveEvent((short)(ms.X - e.DeltaManipulation.Translation.X), (short)(ms.Y - e.DeltaManipulation.Translation.Y));
            }
        }

        /// <summary>
        /// Let the dispatcher set the state text on the progress bar
        /// </summary>
        /// <param name="stateText"></param>
        private void setStateText(string stateText)
        {
            currentStateText.Text = stateText; 
        }

        #endregion Private Methods
    }
}