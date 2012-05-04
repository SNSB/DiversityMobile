using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DiversityPhone.Model.Geometry
{
    public class Vector
    {
        private double x;
        public double X { get {return x;} set{x=value;} }

        private double y;
        public double Y { get { return y; } set { y = value; } }

        public Vector()
        {
        }

        public Vector(Point origin, Point destination)
        {
            x = destination.X - origin.X;
            y = destination.Y - origin.Y;
        }



        public static bool isParallel(Vector v1, Vector v2)
        {
            double lambda = 0;
            if (v2.X != 0)
                lambda = v1.X / v2.X;
            else if (v1.X != 0)
                lambda = 0;
            else
                return true;
            if (v1.Y * lambda == v2.Y)
                return true;
            else return false;
        }
    }
}
