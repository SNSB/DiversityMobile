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
    public class Line
    {
        private Point basePoint;
        public Point BasePoint { get { return basePoint; } set { basePoint = value; } }
        private Vector direction;
        public Vector Direction { get { return direction; } set { direction = value; } }

        public Line()
        {
        }

        public Line(Point p, Vector v)
        {
            basePoint = p;
            direction = v;
        }

        public Point MoveOnLineFromBaseForUnits(double length)
        {
            double x = BasePoint.X + Direction.X* length;
            double y = BasePoint.Y + Direction.Y * length;
            return new Point(x, y);
        }

        public static Point? Intersection(Line l1, Line l2)
        {
            if (Vector.isParallel(l1.Direction, l2.Direction))
                return null;
            else
            {
               

                double a=l1.BasePoint.X;
                double b=l1.Direction.X;
                double c=l2.BasePoint.X;
                double d=l2.Direction.X;
                double e = l1.BasePoint.Y;
                double f = l1.Direction.Y;
                double g = l2.BasePoint.Y;
                double h = l2.Direction.Y;
                double mu = 0;
                  //with these definitions the solution can be found with the help og the
                //solution of  mu the following LES
                // 1. a + lamba*b=c+mu*d
                // 2. e + lambda*f=g+mu*h
                //
                if (b == 0)
                {
                    if (d == 0)
                        throw new ArithmeticException();//Lines do not cross (identity or parallel);
                    else
                    {
                        mu = a - c / d;
                    }
                }
                else if (h - d * f / b == 0)
                {
                    //Calculation of a unique mu is not possible
                    throw new ArithmeticException();
                }
                else
                    mu = (e - g + (c - a) * f / b) / (h - d * f / b);

                //Use mu to travel on l2 and find the solution
                double x=l2.BasePoint.X+mu*l2.Direction.X;
                double y=l2.BasePoint.Y+mu*l2.Direction.Y;
                return new Point(x, y);
            }
        }

    }
}
