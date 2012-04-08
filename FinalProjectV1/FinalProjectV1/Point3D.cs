using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalProjectV1
{
    public class Point3D
    {
        double _x = 0, _y = 0, _z = 0;

        public Point3D()
        {
        }

        public Point3D(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public double Z
        {
            get { return _z; }
            set { _z = value; }
        }

        public static double Dist(Point3D p1, Point3D p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) +
                             Math.Pow(p1.Y - p2.Y, 2) +
                             Math.Pow(p1.Z - p2.Z, 2));
        }

        public static double Dist(Microsoft.Kinect.SkeletonPoint p1, Microsoft.Kinect.SkeletonPoint p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) +
                             Math.Pow(p1.Y - p2.Y, 2) +
                             Math.Pow(p1.Z - p2.Z, 2));
        }

        public double Dist(Point3D p)
        {
            return Math.Sqrt(Math.Pow(_x - p.X, 2) +
                             Math.Pow(_y - p.Y, 2) +
                             Math.Pow(_z - p.Z, 2));
        }
    }
}
