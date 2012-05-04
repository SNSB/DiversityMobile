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


        public static Point? Intersection(Line l1, Line l2)
        {
            if (Vector.isParallel(l1.Direction, l2.Direction))
                return null;
            else
            {
                Point p = new Point();
                double deltaX = l1.BasePoint.X - l2.BasePoint.X;
                double deltaY = l1.BasePoint.Y - l2.BasePoint.Y;
                double mu = 0;
                if (l1.Direction.X == 0)
                {
                    if (l2.Direction.X == 0)
                    {
                        if (l1.BasePoint.X == l2.BasePoint.X && l1.BasePoint.Y == l2.BasePoint.Y)
                            mu = 0;
                        else
                            throw new ArithmeticException("Directions are parallel");
                    }
                    else
                    {
                        mu = deltaX / l2.Direction.X;
                    }
                }
                else
                {
                    double rhs = deltaY + deltaX / l1.Direction.X;
                    double factor = l2.Direction.Y - l2.Direction.X / l1.Direction.X;
                    if (factor != 0)
                    {
                        mu = rhs / factor;
                    }
                    else if (rhs == 0)
                        mu = 0;
                    else
                        throw new ArithmeticException("Directions are parallel");

                }
                double x=l2.BasePoint.X+mu*l2.Direction.X;
                double y=l2.BasePoint.Y+mu*l2.Direction.Y;
                return new Point(x, y);
            }
        }

    }
}
