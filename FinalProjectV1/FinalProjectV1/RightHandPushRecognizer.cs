using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;

namespace FinalProjectV1
{
    class RightHandPushRecognizer : IGestureRecognizer
    {
        private bool _recoginized = false;
        private bool _gestureStart = false;
        private Point3D lastRightHandPos = null;
        private Point3D startRightHandPos;

        string IGestureRecognizer.name
        {
            get { return "RightHandPush"; }
        }

        void IGestureRecognizer.UpdateJointData(Dictionary<Microsoft.Kinect.JointType, Point3D> jointData)
        {
            if (_recoginized)
                return;

            Point3D rightHand = jointData[JointType.HandRight];
            //Debug.WriteLine("z = " + rightHand.Z);

            if (!_gestureStart)
            {
                // if the guesture has not started, and we found the right hand
                // is about the height of the right shoulder, and it's Z is also
                // close to the right shoulder, we will say the gesture starts.
                double zDiff = lastRightHandPos != null ? lastRightHandPos.Z - rightHand.Z : 0;
                //Debug.WriteLine("zDiff = " + zDiff);
                if (zDiff > 0.005)
                {
                    //Debug.WriteLine("\n*** START ***");
                    _gestureStart = true;
                    startRightHandPos = rightHand;
                }
            }
            else
            {
                double zDiff = startRightHandPos.Z - rightHand.Z;

                // the gesture has started, we need to track the hand position 
                // to see if the guesture has completed.

                if (zDiff < -0.005)
                {
                    //Debug.WriteLine("=== ABORT!");
                    _gestureStart = false;
                    return;
                }

                if (zDiff > 0.3)
                {
                    _recoginized = true;
                    //Debug.WriteLine("\n!!!! RECOGNIZED!!! !!!");
                }
            }
            lastRightHandPos = rightHand;
        }

        bool IGestureRecognizer.Recognized
        {
            get
            {
                // Reset recognized after it is read.
                bool r = _recoginized;
                if (r)
                {
                    _recoginized = false;
                    _gestureStart = false;
                }
                return r;
            }
        }
    }
}
