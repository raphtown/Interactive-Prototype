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
 * ***there is also a logic error, that is when it is before office hour page, do we need to do the 30s counting to pop up the help menu?
 * 
 * Comment from Ben: Seriously magic numbers?  It's bad enough when you use them for numeric
 * values, but when you use numbers for symbolic values it's awful.
 * Seriously, it's bad form to use magic numbers, even in your own personal projects.  If you use them in a 
 * project, abandon it then come back to it later you will have to relearn what those numbers mean.  If 
 * instead you just declare your constants at the beginning and only use those constants you will never
 * have to remember the meaning of the number.
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

        // Page values
        const int HELP_PAGE = 0;
        const int TIME_SELECT = 1;
        const int WAIT_SCREEN = 2;
        const int OH_SCREEN = 3;
        const int TIME_SELECT_DETAIL = 4;
        int previousPage; // Page we were on previously
        int currentPage;  // Page which we are currently on

        int sHour, sMin, eHour, eMin;
        System.Drawing.Point cursorPosition = new System.Drawing.Point(0, 0);//cursor control
        DispatcherTimer startOH = new DispatcherTimer();
        private Button currentFocus = null;
        ArrayList allRecognizers = new ArrayList();
        Button[] timeSelectorButtons = new Button[6];


        Dictionary<int, Canvas> mappings = new Dictionary<int, Canvas>();

        const double firstX = 10.0;
        const double secondX = 295.0;
        bool stu1quest = false;
        bool stu2quest = false;
        int framecount = 0;
        const int maxCount = 20;
        #endregion


        //

        public MainWindow()
        {
            InitializeComponent();
            t.Interval = TimeSpan.FromMilliseconds(30000);
            t.Tick += new EventHandler(dis_help);
            currentPage = HELP_PAGE;
            previousPage = currentPage;
            sHour = 11;
            sMin = 0;
            eHour = 12;
            eMin = 0;
            syncLabels();
            allRecognizers.Add(new RightHandPushRecognizer());
            mappings.Add(HELP_PAGE, initialPage);
            mappings.Add(TIME_SELECT, initialTimeSelection);
            mappings.Add(WAIT_SCREEN, beforeStart);
            mappings.Add(OH_SCREEN, OHStarted);
            mappings.Add(TIME_SELECT_DETAIL, timeSelector);

            timeSelectorButtons[0] = l1Button ;
            timeSelectorButtons[1] = l2Button;
            timeSelectorButtons[2] = l3Button;
            timeSelectorButtons[3] = l4Button;
            timeSelectorButtons[4] = l5Button;
            timeSelectorButtons[5] = l6Button;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
            //initialPage.Visibility = Visibility.Visible;
            System.Drawing.Point winPos = new System.Drawing.Point((int)this.Left, (int)this.Top);
            System.Drawing.Size winSize = new System.Drawing.Size((int)this.Width, (int)this.Height);
        }

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor oldSensor = (KinectSensor)e.OldValue;
            stopKinect(oldSensor);
            KinectSensor newSensor = (KinectSensor)e.NewValue;
            initialPage.Visibility = Visibility.Visible;
            newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.1f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            newSensor.SkeletonStream.Enable(parameters);
            //newSensor.SkeletonStream.Enable();
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

        Boolean gotHeadshot = false;
        void newSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            framecount--;
            if (closing)
            {
                return;
            }

            Skeleton sk = getFirstSke(e);
            //initialPage.Visibility = Visibility.Visible;
            
            if (sk != null)
            {
                if (currentPage == OH_SCREEN)
                {
                    Student1.Visibility = Visibility.Visible;
                    if (rightHandRaised(sk) && !stu1quest)
                    {
                        framecount = maxCount;
                        if (!stu2quest)
                        {
                            Student2.SetValue(Canvas.LeftProperty, secondX);
                            Student1.SetValue(Canvas.LeftProperty, firstX);
                        }
                        stu1quest = true;
                        Student1Back.Opacity = 1.0;
                        Student1Question.Visibility = Visibility.Visible;
                    }
                    else if (rightHandRaised(sk) && stu1quest)
                    {
                        framecount = maxCount;
                        if (stu2quest)
                        {
                            Student2.SetValue(Canvas.LeftProperty, firstX);
                            Student1.SetValue(Canvas.LeftProperty, secondX);
                        }
                        stu1quest = false;
                        Student1Back.Opacity = 0.0;
                        Student1Question.Visibility = Visibility.Collapsed;
                    }
                    double z = sk.Joints[JointType.Head].Position.Z;
                    if (Student1Pic.Source == null&&z>1.6&&z<1.8)
                    {
                        Student1Pic.Source = getHeadshot(sk, e.OpenColorImageFrame());
                    }
                    Skeleton sk2 = getSecondSke(e);
                    if (sk2 != null)
                    {

                        if (rightHandRaised(sk2) && !stu2quest)
                        {
                            framecount = maxCount;
                            if (!stu1quest)
                            {
                                Student2.SetValue(Canvas.LeftProperty, firstX);
                                Student1.SetValue(Canvas.LeftProperty, secondX);
                            }
                            stu2quest = true;
                            Student2Back.Opacity = 1.0;
                            Student2Question.Visibility = Visibility.Visible;
                        }
                        else if (rightHandRaised(sk2) && stu2quest)
                        {
                            framecount = maxCount;
                            if (stu1quest)
                            {
                                Student2.SetValue(Canvas.LeftProperty, secondX);
                                Student1.SetValue(Canvas.LeftProperty, firstX);
                            }
                            stu2quest = false;
                            Student2Back.Opacity = 0.0;
                            Student2Question.Visibility = Visibility.Collapsed;
                        }

                        Student2.Visibility = Visibility.Visible;
                        z = sk2.Joints[JointType.Head].Position.Z;
                        if (Student2Pic.Source == null && z > 1.6 && z < 1.8)
                        {

                            Student2.SetValue(Canvas.LeftProperty, firstX);
                            Student1.SetValue(Canvas.LeftProperty, secondX);
                            Student2Pic.Source = getHeadshot(sk2, e.OpenColorImageFrame());
                        }
                    }
                }
                /*if (true)
                {
                    gotHeadshot = true;
                    Student1Pic.Source = getHeadshot(sk, e.OpenColorImageFrame());
                }*/
                MoveMousePosition(sk);
                Dictionary<JointType, Point3D> normalizedJointData = NormalizeJoints(sk);
                RecognizeGesture(normalizedJointData);
            }

            if (currentPage == WAIT_SCREEN)
            {
                disTimeBeforeStart.Text = dis_time_before_OH();
            }
            else if (currentPage == OH_SCREEN)
            {
                disTimeOver.Text = dis_time_left();
            }
            
        }

        private Boolean leftHandRaised(Skeleton sk)
        {
            return sk.Joints[JointType.HandLeft].Position.Y>sk.Joints[JointType.Head].Position.Y;
        }

        private Boolean rightHandRaised(Skeleton sk)
        {
            return sk.Joints[JointType.HandRight].Position.Y>sk.Joints[JointType.Head].Position.Y&&(framecount<=0);
        }

        private ImageSource getHeadshot(Skeleton sk, ColorImageFrame colorFrame)
        {
            Joint head = sk.Joints[JointType.Head];
            // Magic numbers, head position seems to scale with depth, these values are calibrated for a specific depth
            Joint scaledHead = head.ScaleTo(640, 480, 1.25f, 0.9f);

            int x = (int)scaledHead.Position.X;
            int y = (int)scaledHead.Position.Y;
            if (colorFrame == null)
            {
                return null;
            }

            byte[] pixels = new byte[colorFrame.PixelDataLength];
            colorFrame.CopyPixelDataTo(pixels);

            int stride = colorFrame.Width * 4;

            BitmapSource frame = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            if (x - 32 >= 0 && y - 16 >= 0 && x - 32 < 640-65 && y - 16 < 480-65)
            {
                x = x - 32;
                y = y - 16;
                CroppedBitmap pic = new CroppedBitmap(frame, new Int32Rect(x, y, 65, 65));
                colorFrame.Dispose();
                return pic;
            }
            colorFrame.Dispose();
            return null;
        }

        private void setCurrentFocus(Button btn)
        {
            currentFocus = btn;
            if (btn != null) 
            {
                Keyboard.Focus(btn);
            }
        }

        private void RecognizeGesture(Dictionary<JointType, Point3D> normalizedJointData)
        {
            foreach (IGestureRecognizer recognizer in allRecognizers)
            {
                recognizer.UpdateJointData(normalizedJointData);
                if (recognizer.Recognized)
                {
                    if (recognizer.name == "RightHandPush" && currentFocus != null)
                    {
                        SystemSounds.Beep.Play();
                        currentFocus.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                }
            }
        }

        // Normalize all joint data by setting shoulder center to be origin
        // and shoulder width to be 1.0.
        private Dictionary<JointType, Point3D> NormalizeJoints(Skeleton sk)
        {
            Dictionary<JointType, Point3D> normalizedJointData = new Dictionary<JointType, Point3D>();

            // Make all joints' position relative to the shoulder center
            SkeletonPoint shouldCenter = sk.Joints[JointType.ShoulderCenter].Position;
            foreach (JointType jointType in Enum.GetValues(typeof(JointType)).Cast<JointType>())
            {
                Point3D newPoint = new Point3D();
                normalizedJointData.Add(jointType, newPoint);
                newPoint.X = sk.Joints[jointType].Position.X - shouldCenter.X;
                newPoint.Y = sk.Joints[jointType].Position.Y - shouldCenter.Y;
                newPoint.Z = sk.Joints[jointType].Position.Z - shouldCenter.Z;
            }

            // Normalize the distance between each point and the shoulder center
            double shouldWidth = Point3D.Dist(sk.Joints[JointType.ShoulderLeft].Position,
                                              sk.Joints[JointType.ShoulderRight].Position);
            foreach (JointType jointType in Enum.GetValues(typeof(JointType)).Cast<JointType>())
            {
                Point3D p = normalizedJointData[jointType];
                p.X /= shouldWidth;
                p.Y /= shouldWidth;
                p.Z /= shouldWidth;
            }

            return normalizedJointData;
        }

        private string dis_time_left()
        {
            int m = eMin - 1 - DateTime.Now.Minute;
            int s = 60 - DateTime.Now.Second;
            int h = 0;
            if (m < 0)
            {
                h = eHour - DateTime.Now.Hour - 1;
                m = m + 60;
            }
            else
                h = eHour - DateTime.Now.Hour;
            return h.ToString() + ":" + m.ToString() + ":" + s.ToString();
        }

        private string dis_time_before_OH()
        {
            int m = sMin-1-DateTime.Now.Minute;
            int s = 60 - DateTime.Now.Second;
            int h=0;
            if (m < 0)
            {
                h = sHour - DateTime.Now.Hour - 1;
                m = m + 60;
            }
            else
                h = sHour - DateTime.Now.Hour;

            return h.ToString() + ":" + m.ToString() + ":" + s.ToString();
            
        }

        private void MoveMousePosition(Skeleton sk)
        {
            Joint leftHand = sk.Joints[JointType.HandLeft];
            Joint hip = sk.Joints[JointType.HipCenter];
            Joint ls = sk.Joints[JointType.ShoulderLeft];
            Joint scaledLeftHand = leftHand.ScaleTo((int)this.Width, (int)this.Height, hip.Position.X - ls.Position.X, 0.25f);
            //Joint scaledHip = hip.ScaleTo((int)this.Width, (int)this.Height, 0.25f, 0.25f);
            double x = scaledLeftHand.Position.X;
            double y = scaledLeftHand.Position.Y;
            //double a = scaledHip.Position.X;
            //double b = scaledHip.Position.Y;
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

        Skeleton getSecondSke(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }

                skeletonFrameData.CopySkeletonDataTo(allSkeleton);
                int inc = 0;
                Skeleton second = null;
                foreach (Skeleton s in allSkeleton)
                {
                    if (s.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        inc++;
                        if (inc == 2)
                            second = s;
                    }
                }

                return second;
            }
        }

        private void c1_Click(object sender, RoutedEventArgs e)
        {

            transitionToPage(TIME_SELECT);
            t.Start(); // start the timer
        }

        private void c2_Click(object sender, RoutedEventArgs e)
        {
            t.Stop();
            //check if the time is legal
            if(checkTime())
            {
               int remaing = calTime();
               if (remaing < 0) warningMsg.Content = "Past time selction!";
               else
               {
                   transitionToPage(WAIT_SCREEN);
                   startOH.Interval = TimeSpan.FromMilliseconds(calTime());
                   startOH.Tick += new EventHandler(startedOH);
                   warningMsg.Content = "";
                   startOH.Start();
               }
               //t.Start();
            }
            else
            {
                warningMsg.Content = "Wrong time selection!";
            }
        }

        private void plus5_Click(object sender, RoutedEventArgs e)
        {
            eMin = eMin + 5;
            if (eMin > 55) 
            {
                eMin = eMin % 60;
                eHour++;
            }
        }

        private void min5_Click(object sender, RoutedEventArgs e)
        {
            int m = 0;
            m = eMin - 5;
            if (m < 0)
            {
                if ((eHour - DateTime.Now.Hour - 1) < 0) warningMsg.Content = "No enought time for subtraction!";
                else
                {
                    --eHour;
                    eMin = eMin + 55;
                    warningMsg.Content = "";
                }
            }
            else eMin = eMin - 5;
        }

        private int calTime()
        {
            int m = sMin - 1 - DateTime.Now.Minute;
            int s = 60 - DateTime.Now.Second;
            int h = 0;
            if (m < 0)
            {
                m = m + 60;
                h = (sHour - DateTime.Now.Hour - 1) % 24;
            }
            else
                h = sHour - DateTime.Now.Hour;
            return (h * 3600 + m * 60 + s) * 1000;
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
            if (currentPage == TIME_SELECT)
            {
                transitionToPage(HELP_PAGE);
            }
            DispatcherTimer at = (DispatcherTimer)sender;
            at.Stop();
        }

        private void startedOH(object sender, EventArgs e)
        {
            DispatcherTimer at = (DispatcherTimer)sender;
            at.Stop();
            t.Stop();
            transitionToPage(OH_SCREEN);    
        }

        private void cSpecial_Click(object sender, RoutedEventArgs e)
        {
            transitionToPage(previousPage);
             t.Start();
        }

        private void ct_Click(object sender, RoutedEventArgs e)
        {
            t.Stop();
            transitionToPage(TIME_SELECT);
            t.Start();
        }


        private void allMouseEnter(object sender, MouseEventArgs e)
        {
            setCurrentFocus((Button)sender);
        }

        #region timeSelectionDetailFunctionality
        // Detail view of time selection.  User scrolls through available times to choose from using up and down buttons, and selects appropriate time

        // Show next screenful of available times to select
        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < timeSelectorButtons.Length; i++)
            {
                if (selectorType == HOUR)
                {
                    timeSelectorButtons[i].Content = (int.Parse(timeSelectorButtons[i].Content.ToString()) + 18) % 24;
                }
                else
                {
                    timeSelectorButtons[i].Content = (int.Parse(timeSelectorButtons[i].Content.ToString()) + 30) % 60;
                }

            }
        }

        // Show next screenful of available times to select
        private void downButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < timeSelectorButtons.Length; i++)
            {
                if (selectorType == HOUR)
                {
                    timeSelectorButtons[i].Content = (int.Parse(timeSelectorButtons[i].Content.ToString()) + 6) % 24;
                }
                else
                {
                    timeSelectorButtons[i].Content = (int.Parse(timeSelectorButtons[i].Content.ToString()) + 30) % 60;
                }

            }
        }

        // User selected a time: update times and transition back to main screen
        private void lButton_Click(object sender, RoutedEventArgs e)
        {
            int result = int.Parse(((Button)sender).Content.ToString());
            if (selectorType == HOUR)
            {
                if (start)
                {
                    sHour = result;
                    eHour = sHour + 1;
                }
                else
                {
                    eHour = result;
                }
            }
            else
            {
                if (start)
                {
                  
                    sMin = result;
                    eMin = sMin;
                }
                else
                {
                    eMin = result;
                }
            }

            transitionToPage(TIME_SELECT);
            syncLabels();
            t.Start();
        }


        #endregion

        #region timeSelectionMainScreenFunctionality
        // Main time selection window, displaying both start and end times

        // Indicates Type of Time Selection
        int selectorType = 0;
        const int HOUR = 0;
        const int MINUTE = 1;

        bool start;


        private void endHour_Click(object sender, RoutedEventArgs e)
        {
            launchTimeSelector(HOUR, false);
        }

        private void endMin_Click(object sender, RoutedEventArgs e)
        {
            launchTimeSelector(MINUTE, false);
        }

        private void startHour_Click(object sender, RoutedEventArgs e)
        {
            launchTimeSelector(HOUR, true);
        }

        private void startMin_Click(object sender, RoutedEventArgs e)
        {
            launchTimeSelector(MINUTE, true);
        }

        private void launchTimeSelector(int selectorType, bool start)
        {
            t.Stop();
            initializeSelector(selectorType, start);
            transitionToPage(TIME_SELECT_DETAIL);
        }

        public void initializeSelector(int selectorType, bool start)
        {
            this.start = start;
            this.selectorType = selectorType;
            for (int i = 0; i < timeSelectorButtons.Length; i++)
            {
                if (selectorType == HOUR)
                {
                    timeSelectorButtons[i].Content = i;
                    if (start) timeSelectorLabel.Content = "Start Hour";
                    else timeSelectorLabel.Content = "End Hour";
                }
                else
                {
                    timeSelectorButtons[i].Content = 5 * i;
                    if (start) timeSelectorLabel.Content = "Start Minute";
                    else timeSelectorLabel.Content = "End Minute";
                }

            }
        }
        #endregion

        // Resync labels with our own set values (NOTE: if we actually learned C# this could probably be done via data binding)
        private void syncLabels()
        {
            startTime.Text =(sHour < 10 ? "0" : "") + sHour + ":" + (sMin < 10 ? "0" : "") + sMin;
            endTime.Text =(eHour < 10 ? "0" : "") + eHour + ":" + (eMin < 10 ? "0" : "") + eMin;
        }

        // Switches to a different page.  Either you disable previous page, or hide it entirely
        private void transitionToPage(int page, bool hidePreviousPage = false)
        {
            previousPage = currentPage;
            currentPage = page;
            if (hidePreviousPage)
            {
                mappings[previousPage].IsEnabled = false;
            }
            else
            {
                mappings[previousPage].Visibility = Visibility.Collapsed;
            }
            mappings[currentPage].IsEnabled = true;
            mappings[currentPage].Visibility = Visibility.Visible;
            
            setCurrentFocus(null);
        }

        // Just a shortcut to be able to start Office hours right away
        private void startOHNow_Click(object sender, RoutedEventArgs e)
        {
            startOH.Stop();
            t.Stop();    
            transitionToPage(OH_SCREEN);   
        }
    }
}
