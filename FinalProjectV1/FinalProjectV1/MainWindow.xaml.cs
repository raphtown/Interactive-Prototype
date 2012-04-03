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
using System.Collections;
using System.Media;
using System.Drawing;


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
        //a variable keep track which page we are on right now, so that when we switch to the timeouthelp page, we know that when we hit continue, when page should be visible
        int currentPage;  //define: initialTimeSelection page: 1, beforeOH: 2, 
        // time representation
        int sHour, sMin, eHour, eMin;
        System.Drawing.Point cursorPosition = new System.Drawing.Point(0, 0);//cursor control
        #endregion



        public MainWindow()
        {
            InitializeComponent();
            t.Interval = TimeSpan.FromMilliseconds(30000);
            t.Tick += new EventHandler(dis_help);
            currentPage = 0;
            sHour = 0;
            sMin = 0;
            eHour = 0;
            eMin = 0;

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

            Skeleton sk = getFirstSke(e);
            initialPage.Visibility = Visibility.Visible;
            if (sk != null)
            {
                MoveMousePosition(sk);
            }
            
        }

        private void MoveMousePosition(Skeleton sk)
        {
            Joint leftHand = sk.Joints[JointType.HandLeft];
            Joint scaledLeftHand = leftHand.ScaleTo((int)this.Width, (int)this.Height, 0.25f, 0.25f);

            double x = scaledLeftHand.Position.X;
            double y = scaledLeftHand.Position.Y;
            System.Windows.Point scnPt = this.PointToScreen(new System.Windows.Point(x, y));

            //Debug.WriteLine("   x=  " + x+ "   y=  " + y);
            cursorPosition.X = (int)scnPt.X;
            cursorPosition.Y = (int)scnPt.Y;
            System.Windows.Forms.Cursor.Position = cursorPosition;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            stopKinect(kinectSensorChooser1.Kinect);
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
            initialPage.Visibility = Visibility.Collapsed;
            initialTimeSelection.Visibility = Visibility.Visible;
            currentPage = 1;
            t.Start(); // start the timer
        }

        private void startHourPlus_Click(object sender, RoutedEventArgs e)
        {
            t.Stop();
            if (sHour < 23)
            {
                ++sHour;
                //update textbox
                if (sHour < 10)
                    startHour.Text = "0" + sHour.ToString();  //don't know if this works
                else
                    startHour.Text = sHour.ToString();
            }
            else
            {
                //do some error prevention here
            }
            t.Start();
        }

        private void startHourMin_Click(object sender, RoutedEventArgs e)
        {
            t.Stop();
            if (sHour > 0)
            {
                --sHour;
                //update textbox
                if (sHour < 10)
                    startHour.Text = "0" + sHour.ToString();  //don't know if this works
                else
                    startHour.Text = sHour.ToString();
            }
            else
            {
                //do some error prevention here
            }
            t.Start();
        }

        private void startMinPlus_Click(object sender, RoutedEventArgs e)
        {
            t.Stop();
            if (sMin < 59)
            {
                sMin+=5;
                //update textbox
                if (sMin < 10)
                    startMinute.Text = "0" + sMin.ToString();  //don't know if this works
                else
                    startMinute.Text = sMin.ToString();
            }
            else
            {
                //do some error prevention here
            }
            t.Start();
        }

        private void startMinMin_Click(object sender, RoutedEventArgs e)
        {
            t.Stop();
            if (sMin > 0)
            {
                sMin -= 5;
                //update textbox
                if (sMin < 10)
                    startMinute.Text = "0" + sMin.ToString();  //don't know if this works
                else
                    startMinute.Text = sMin.ToString();
            }
            else
            {
                //do some error prevention here
            }
            t.Start();
        }

        private void endHourPlus_Click(object sender, RoutedEventArgs e)
        {
            t.Stop();
            if (eHour < 23)
            {
                ++eHour;
                //update textbox
                if (eHour < 10)
                    endHour.Text = "0" + eHour.ToString();  //don't know if this works
                else
                    endHour.Text = eHour.ToString();
            }
            else
            {
                //do some error prevention here
            }
            t.Start();
        }

        private void endHourMin_Click(object sender, RoutedEventArgs e)
        {
            t.Stop();
            if (eHour > 0)
            {
                --eHour;
                //update textbox
                if (eHour < 10)
                    endHour.Text = "0" + eHour.ToString();  //don't know if this works
                else
                    endHour.Text = eHour.ToString();
            }
            else
            {
                //do some error prevention here
            }
            t.Start();
        }

        private void endMinPlus_Click(object sender, RoutedEventArgs e)
        {
            t.Stop();
            if (eMin < 59)
            {
                eMin += 5;
                //update textbox
                if (eMin < 10)
                    endMin.Text = "0" + eMin.ToString();  //don't know if this works
                else
                    endMin.Text = eMin.ToString();
            }
            else
            {
                //do some error prevention here
            }
            t.Start();
        }

        private void endMinMin_Click(object sender, RoutedEventArgs e)
        {
            t.Stop();
            if (eMin > 0)
            {
                eMin -= 5;
                //update textbox
                if (eMin < 10)
                    endMin.Text = "0" + eMin.ToString();  //don't know if this works
                else
                    endMin.Text = eMin.ToString();
            }
            else
            {
                //do some error prevention here
            }
            t.Start();
        }

        private void c2_Click(object sender, RoutedEventArgs e)
        {
            //check if the time is legal
            if(checkTime())
            {
                t.Stop();
                initialTimeSelection.Visibility = Visibility.Collapsed;
                beforeStart.Visibility = Visibility.Visible;
                currentPage = 2;
                t.Start();
            }
            else
            {
                //a notification to user about the wrong time
            }
        }


        private Boolean checkTime()
        {
            if (eHour > sHour)
            {
                return true;
            }
            else if (eHour == sHour)
            {
                if (eMin > sMin)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }


        //30 second no action event handler, we also have a separate page for this special help page, but it will be same as the first time user guide page
        private void dis_help(object sender, EventArgs e)
        {
            //checking current page 
            if (currentPage == 1)
            {
                initialTimeSelection.Visibility = Visibility.Collapsed;
                helpForTimeout.Visibility = Visibility.Visible;
            }
            else if (currentPage == 2)
            {
                beforeStart.Visibility = Visibility.Collapsed;
                helpForTimeout.Visibility = Visibility.Visible;
            }
            DispatcherTimer at = (DispatcherTimer)sender;
            at.Stop();
        }

        private void cSpecial_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage == 1)
            {
                helpForTimeout.Visibility = Visibility.Collapsed;
                initialTimeSelection.Visibility = Visibility.Visible;
                t.Start();
            }
            else if (currentPage == 2)
            {
                helpForTimeout.Visibility = Visibility.Collapsed;
                beforeStart.Visibility = Visibility.Visible;
                t.Start();
            }

        }
    }
}
