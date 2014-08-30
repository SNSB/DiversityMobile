using DiversityPhone.Model;
using System;
using System.Windows;

namespace DiversityPhone.Services
{
    public static class MapProjection
    {
        private class Line
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
                double x = BasePoint.X + Direction.X * length;
                double y = BasePoint.Y + Direction.Y * length;
                return new Point(x, y);
            }

            public static Point? Intersection(Line l1, Line l2)
            {
                if (Vector.isParallel(l1.Direction, l2.Direction))
                    return null;
                else
                {
                    double a = l1.BasePoint.X;
                    double b = l1.Direction.X;
                    double c = l2.BasePoint.X;
                    double d = l2.Direction.X;
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
                    double x = l2.BasePoint.X + mu * l2.Direction.X;
                    double y = l2.BasePoint.Y + mu * l2.Direction.Y;
                    return new Point(x, y);
                }
            }
        }

        private class Vector
        {
            private double x;

            public double X { get { return x; } set { x = value; } }

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

        public static Point? PercentilePositionOnMap(this Map This, ILocalizable loc)
        {
            if (This == null || loc == null || !loc.Latitude.HasValue || !loc.Longitude.HasValue)
                return null;

            Point? p = PercentilePositionOnMapImpl(This, loc.Latitude.Value, loc.Longitude.Value);
            if (p == null || p.Value.X < 0 || p.Value.X > 1 || p.Value.Y < 0 || p.Value.Y > 1)
                return null;
            return p;
        }

        //The corner coordinates of the map dont define an exact rectangle in all cases. Generally, a convex quadrangle is defined.
        //The represetation on the screen will be in an rectangle. Hence, the corresponding position is calculated. To perform This,
        //2 lines are definend: One Going from the upper-left corner to the upper right corner of the map. The other one from the lower left corner to the lower right corner of the map.
        //A cohort of lines is definend by the connection between those lines parametrized by the percentual scale between the cornerpoint of the 2 lines definend in This way.
        //Analogously 2 lines are definend in the y-Direction which also define a cohort of lines between them. The GPS-Value is at a specific intersection of these cohorts.
        //These specific lines of the cohorts can be found by solving a quadratic equation. Theses lines define with their parameters the position of the GPS-Value on the map (On a percentual basis).
        private static Point? PercentilePositionOnMapImpl(Map This, double GPSLatitude, double GPSLongitude)
        {
            double a, b, c, d, e, f, g, h; //This-specific values derived form the position of the cornerpoints. a and e are dependent on addtional values and calculated in the position calculation method
            bool specialCase;//Criterion for the special case mu=-b/d (which leads to a division by zero in the lambda calculus)

            a = -GPSLongitude + This.NWLong;
            b = -This.NWLong + This.NELong;
            c = -This.NWLong + This.SWLong;
            d = This.NWLong - This.NELong + This.SELong - This.SWLong;

            e = -GPSLatitude + This.NWLat;
            f = -This.NWLat + This.NELat;
            g = -This.NWLat + This.SWLat;
            h = This.NWLat - This.NELat + This.SELat - This.SWLat;

            if (b == 0 || g == 0)
            {
                //This coordinates are corrupted
                return null;
            }

            //Check d==0
            if (d != 0)
            {
                if (a - c * b / d == 0)
                    specialCase = true;
                else
                    specialCase = false;
            }
            else
                specialCase = false;
            if (specialCase == false) //=>mu!=-b/d
            {
                double alpha, beta, gamma;//Coeffizients for the Mitternachts-Equation
                double discrim;//Discriminant in the Mitternachts-Equation
                double mu1, mu2, lambda1, lambda2;//Solution-Parameters. As the equation is quadratic two possible solutions exist.
                alpha = g * d - h * c;
                beta = e * d - c * f + g * b - a * h;
                gamma = -a * f + e * b;
                if (alpha != 0)
                {
                    discrim = beta * beta - 4 * alpha * gamma;
                    if (discrim < 0) //Equation unsovable
                        return null;
                    mu1 = (-beta + Math.Sqrt(discrim)) / (2 * alpha);
                    mu2 = (-beta - Math.Sqrt(discrim)) / (2 * alpha);
                    lambda1 = -(a + c * mu1) / (b + d * mu1);
                    lambda2 = -(a + c * mu2) / (b + d * mu2);

                    Point p1 = new Point(lambda1, mu1);
                    Point p2 = new Point(lambda2, mu2);
                    if (p1.X > 0 && p1.X < 1 && p1.Y > 0 && p1.Y < 1)
                        return p1;
                    if (p2.X > 0 && p2.X < 1 && p2.Y > 0 && p2.Y < 1)
                        return p2;
                    return null;
                }
                else
                {
                    if (beta != 0)
                    {
                        mu1 = -gamma / beta;
                        lambda1 = -(a + c * mu1) / (b + d * mu1);
                        Point p1 = new Point(lambda1, mu1);
                        if (p1.X > 0 && p1.X < 1 && p1.Y > 0 && p1.Y < 1)
                            return p1;
                        return null;
                    }
                    else
                    {
                        //No mu-Percentile can be calculated as the equation is either unsovable or there are no restriction to mu.
                        //In both cases no representation on the This can be given
                        return null;
                    }
                }
            }
            else //=> mu=-b/d
            {
                double lambda, mu;
                mu = -b / d;
                if (b * h - f * c != 0)
                {
                    lambda = -(e * d - b * g) / (b * h - f * c);//Div by Zero
                    Point p = new Point(lambda, mu);
                    return p;
                }
                else
                {
                    //No mu-Percentile can be calculated as the equation is either unsovable or there are no restriction to mu.
                    //In both cases no representation on the This can be given
                    return null;
                }
            }
        }

        public static Coordinate GPSFromPercentilePosition(this Map This, Point pos)
        {
            double percX = pos.X;
            double percY = pos.Y;
            Point A = new Point(This.NWLong, This.NWLat);
            Point B = new Point(This.NELong, This.NELat);
            Point C = new Point(This.SELong, This.SELat);
            Point D = new Point(This.SWLong, This.SWLat);
            Line AB = new Line(A, new Vector(A, B));
            Line DC = new Line(D, new Vector(D, C));
            Line AD = new Line(A, new Vector(A, D));
            Line BC = new Line(B, new Vector(B, C));

            Point X0 = AB.MoveOnLineFromBaseForUnits(percX);
            Point X1 = DC.MoveOnLineFromBaseForUnits(percX);
            Point Y0 = AD.MoveOnLineFromBaseForUnits(percY);
            Point Y1 = BC.MoveOnLineFromBaseForUnits(percY);

            Line X0X1 = new Line(X0, new Vector(X0, X1));
            Line Y0Y1 = new Line(Y0, new Vector(Y0, Y1));

            Point? p = Line.Intersection(X0X1, Y0Y1);

            if (p != null)
            {
                Coordinate gps = new Coordinate();
                gps.Longitude = p.Value.X;
                gps.Latitude = p.Value.Y;
                gps.Altitude = double.NaN;
                return gps;
            }
            else
            {
                throw new ArithmeticException("Lines do not cross. Mapdata are corrupted");
            }
        }
    }
}