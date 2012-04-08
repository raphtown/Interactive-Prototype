using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace FinalProjectV1
{
    interface IGestureRecognizer
    {
        // gesture name
        string name
        {
            get;
        }

        // update skeleton data to the recognizer
        void UpdateJointData(Dictionary<JointType, Point3D> jointData);

        // Indicates if the guesture has been recognized
        // with the last data update. If the Recognized
        // property is set to true, the recognizer will
        // not process new data until the Recognized state
        // is read. Once it is read, the recognizer will
        // be reset and start the recongnition process again.
        bool Recognized
        {
            get;
        }
    }
}
