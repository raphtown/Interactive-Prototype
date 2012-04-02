/* the sequence:
 * 1. initialPage which contains the basic gesture help
 * 2. intitalTimeSelection which sets the start time and the end time 
 * 3. beforeStart which display the time until OH and offer change time method
 * 4. OHstarted which is the main challenge of this project, display many things
 * 5. ended which (suggestions)
 * 
 * 2.1.stSelection which allow use to select start time
 * 2.2.etSelection which allow use to set end time
 * ========== ignore the above ===================
*/
/*
 * buttons:
 * c1: is the continue button at initial first time user guide page
 * c2: is the continue button at new select time page
 * 
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.Windows.Threading;
using System.Windows.Threading;


namespace FinalProjectV1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region local variables
        bool closing = false;
        const int skeletonCount = 6;
        Skeleton[] allSkeleton = new Skeleton[skeletonCount];


        DispatcherTimer t = new DispatcherTimer();
        // startTime
        // endTime

        #endregion


        public MainWindow()
        {
            InitializeComponent();
            t.Interval = TimeSpan.FromMilliseconds(30000);
            t.Tick += new EventHandler(dis_help);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
        }

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor oldSensor = (KinectSensor)e.OldValue;
            stopKinect(oldSensor);
            KinectSensor newSensor = (KinectSensor)e.NewValue;

            newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            //newSensor.SkeletonStream.Enable(parameters);
            newSensor.SkeletonStream.Enable();
            newSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(newSensor_AllFramesReady);
            try
            {
                newSensor.Start();
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred();
            }
        }


        void newSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }

            Skeleton first = getFirstSke(e);
            initialPage.Visibility = Visibility.Visible;

            
        }




        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }




        void stopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }


                }
            }
        }


        Skeleton getFirstSke(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }

                skeletonFrameData.CopySkeletonDataTo(allSkeleton);
                Skeleton first = (from s in allSkeleton where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                return first;
            }
        }

        private void c1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void startHourPlus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void startHourMin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void startMinPlus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void startMinMin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void endHourPlus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void endHourMin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void endMinPlus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void endMinMin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void c2_Click(object sender, RoutedEventArgs e)
        {

        }




        //30 second no action event handler, we also have a separate page for this special help page, but it will be same as the first time user guide page
        private void dis_help(object sender, EventArgs e)
        {
            DispatcherTimer at = (DispatcherTimer)sender;
            at.Stop();
        }
    }
}
