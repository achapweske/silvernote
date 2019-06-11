/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace SilverNote.Editor
{
    public class CubicBezier : Shape
    {
        #region Constructors

        public CubicBezier()
        {
            Initialize();
        }

        public CubicBezier(Point point1, Point point2, Point point3, Point point4)
        {
            Initialize();

            X1 = point1.X;
            Y1 = point1.Y;
            X2 = point2.X;
            Y2 = point2.Y;
            X3 = point3.X;
            Y3 = point3.Y;
            X4 = point4.X;
            Y4 = point4.Y;
        }

        public CubicBezier(CubicBezier copy)
            : base(copy)
        {
            X1 = copy.X1;
            Y1 = copy.Y1;
            X2 = copy.X2;
            Y2 = copy.Y2;
            X3 = copy.X3;
            Y3 = copy.Y3;
            X4 = copy.X4;
            Y4 = copy.Y4;

            if (copy.MarkerStart != null)
            {
                MarkerStart = (Marker)copy.MarkerStart.Clone();
            }

            if (copy.MarkerEnd != null)
            {
                MarkerEnd = (Marker)copy.MarkerEnd.Clone();
            }
        }

        void Initialize()
        {
            X1 = 0;
            Y1 = 0;
            X2 = 0;
            Y2 = 0;
            X3 = 0;
            Y3 = 0;
            X4 = 0;
            Y4 = 0;

            MarkerStart = null;
            MarkerEnd = null;
        }

        #endregion

        #region Attributes

        public double X1 { get; set; }

        public double Y1 { get; set; }

        public double X2 { get; set; }

        public double Y2 { get; set; }

        public double X3 { get; set; }

        public double Y3 { get; set; }

        public double X4 { get; set; }

        public double Y4 { get; set; }

        private Marker markerStart = null;
        public Marker MarkerStart
        {
            get { return markerStart; }
            set
            {
                if (markerStart != null)
                    Children.Remove(markerStart);
                markerStart = value;
                if (markerStart != null)
                    Children.Add(markerStart);
            }
        }

        private Marker markerEnd = null;
        public Marker MarkerEnd
        {
            get { return markerEnd; }
            set
            {
                if (markerEnd != null)
                    Children.Remove(markerEnd);
                markerEnd = value;
                if (markerEnd != null)
                    Children.Add(markerEnd);
            }
        }

        #endregion

        #region NDrawing

        public override Shape ThumbLarge
        {
            get
            {
                var result = (CubicBezier)base.ThumbLarge;
                result.X1 = 0;
                result.Y1 = 20;
                result.X2 = 5;
                result.Y2 = 5;
                result.X3 = 15;
                result.Y3 = 15;
                result.X4 = 20;
                result.Y4 = 0;
                return result;
            }
        }

        public override Shape ThumbSmall
        {
            get
            {
                var result = (CubicBezier)base.ThumbSmall;
                result.X1 = 0;
                result.Y1 = 16;
                result.X2 = 4;
                result.Y2 = 4;
                result.X3 = 12;
                result.Y3 = 12;
                result.X4 = 16;
                result.Y4 = 0;
                return result;
            }
        }

        public override void Place(Point position)
        {
            X1 = X2 = X3 = X4 = position.X;
            Y1 = Y2 = Y3 = Y4 = position.Y;
        }

        public override void Draw(Point point)
        {
            X4 = point.X;
            Y4 = point.Y;

            X2 = (X1 + X4) / 3.0;
            Y2 = (Y1 + Y4) / 3.0;

            X3 = (X1 + X3) * 2.0 / 3.0;
            Y3 = (Y1 + Y3) * 2.0 / 3.0;
        }

        public override int HandleCount
        {
            get { return 4; }
        }

        protected override Point GetHandleInternal(int index)
        {
            switch (index)
            {
                case 0:
                    return new Point(X1, Y1);
                case 1:
                    return new Point(X2, Y2);
                case 2:
                    return new Point(X3, Y3);
                case 3:
                    return new Point(X4, Y4);
                default:
                    return new Point();
            }
        }

        protected override void SetHandleInternal(int index, Point value)
        {
            switch (index)
            {
                case 0:
                    X1 = value.X;
                    Y1 = value.Y;
                    break;
                case 1:
                    X2 = value.X;
                    Y2 = value.Y;
                    break;
                case 2:
                    X3 = value.X;
                    Y3 = value.Y;
                    break;
                case 3:
                    X4 = value.X;
                    Y4 = value.Y;
                    break;
                default:
                    break;
            }
        }

        protected override Rect Bounds
        {
            get
            {
                Point point1 = new Point(X1, Y1);
                Point point2 = new Point(X2, Y2);
                Point point3 = new Point(X3, Y3);
                Point point4 = new Point(X4, Y4);

                var geometry = new StreamGeometry();
                var gc = geometry.Open();
                gc.BeginFigure(point1, false, false);
                gc.BezierTo(point2, point3, point4, true, true);
                gc.Close();

                return geometry.Bounds;
            }
        }

        public override PathGeometry ToPathGeometry()
        {
            var segment = new BezierSegment();
            segment.Point1 = new Point(X2, Y2);
            segment.Point2 = new Point(X3, Y3);
            segment.Point3 = new Point(X4, Y4);
            segment.IsStroked = true;

            var figure = new PathFigure();
            figure.StartPoint = new Point(X1, Y1);
            figure.Segments.Add(segment);

            var path = new PathGeometry();
            path.Figures.Add(figure);

            return path;
        }

        public override StreamGeometry ToStreamGeometry()
        {
            var geometry = new StreamGeometry();
            var gc = geometry.Open();
            gc.BeginFigure(new Point(X1, Y1), FillBrush != null, false);
            gc.BezierTo(new Point(X2, Y2), new Point(X3, Y3), new Point(X4, Y4), StrokeBrush != null, false);
            gc.Close();

            return geometry;
        }

        protected override void OnRenderVisual(DrawingContext dc)
        {
            base.OnRenderVisual(dc);

            if (MarkerStart != null)
            {
                Point point1 = GeometryTransform.Transform(new Point(X1, Y1));
                Point point2 = GeometryTransform.Transform(new Point(X2, Y2));

                MarkerStart.Offset = new Vector(point1.X, point1.Y);
                MarkerStart.Orient = Math.Atan2(point1.Y - point2.Y, point1.X - point2.X) * 180.0 / Math.PI;
                MarkerStart.Redraw();
            }

            if (MarkerEnd != null)
            {
                Point point3 = GeometryTransform.Transform(new Point(X3, Y3));
                Point point4 = GeometryTransform.Transform(new Point(X4, Y4));

                MarkerEnd.Offset = new Vector(point4.X, point4.Y);
                MarkerEnd.Orient = Math.Atan2(point4.Y - point3.Y, point4.X - point3.X) * 180.0 / Math.PI;
                MarkerEnd.Redraw();
            }
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new CubicBezier(this);
        }

        #endregion

        #region Algorithms

        /// <summary>
        /// Evaluate a quadratic bezier at offset t
        /// </summary>
        public static Point Evaluate(Point p0, Point p1, Point p2, Point p3, double t)
        {
            return new Point(
                Cubic(p0.X, p1.X, p2.X, p3.X, t),
                Cubic(p0.Y, p1.Y, p2.Y, p3.Y, t)
            );
        }

        public static double Cubic(double c0, double c1, double c2, double c3, double x)
        {
            // y = c0*(1-x)^3 + 3*c1*x*(1-x)^2 + 3*c2*(1-x)*x^2 + c3*x^3

            double x2 = x * x;
            double x3 = x2 * x;
            double xp = 1 - x;
            double xp2 = xp * xp;
            double xp3 = xp2 * xp;

            return c0 * xp3 + 3 * c1 * x * xp2 + 3 * c2 * x2 * xp + c3 * x3;
        }

        #region Curve Fitting

        public static Point[] FitToBezier(Vector startTangent, Vector endTangent, IList<Point> s, IList<double> u = null)
        {
            // A cubic bezier is defined as:
            //
            //   B(t) = C0*(1-t)^3 + 3*C1*t*(1-t)^2 + 3*C2*t^2*(1-t) + C3*t^3 where 0 <= t <= 1
            //
            // Where C0-3 are the "control points".
            //
            // This can be expressed in terms of Bernstein polynomials:
            //
            //   B(t) = Sum(Ci*Bi(t), i=0 to n) where 0 <= t <= 1
            //
            //          B0(t) = (1-t)^3 
            //          B1(t) = 3*t*(1-t)^2
            //          B2(t) = 3*t^2*(1-t)
            //          B3(t) = t^3
            //
            // Our goal is to fit a set of sample points p[0] ... p[n].
            //
            // The outer control points are simply the first and last sample points:
            //
            //   C0 = p[0]
            //   C3 = p[n]
            //
            // Each inner control point lies on a line that intersects an end sample point
            // and is tangent to that sample:
            //
            //   C1 = p[0] + alpha1 * tan1
            //   C2 = p[n] + alpha2 * tan2
            // 
            // We'll use the following error function:
            //
            //   H = Sum((p[i] - B(u[i]))^2, i=1 to n)
            //
            // where u[i] is a parameterization of p[i]
            //
            // The problem, then is to find alpha1 and alpha2 that minimize H: 
            //
            //   d(H)/d(alpha1) = 0
            //   d(H)/d(alpha2) = 0
            //
            // Now for the tedious part:
            //
            //   0 = d(H)/d(alpha1) = Sum(2*(p[i] - B(u[i])) * d(B(u[i]))/d(alpha1))
            //     where d(B(u[i]))/d(alpha1) = tan1 * B1(u[i])
            //   thus: Sum(p[i] * A1[i]) = Sum(B(u[i]) * A1[i]) where A1[i] = tan1 * B1(u[i])
            //   which expands to: 
            //     alpha1 * Sum(A1[i]^2) + alpha2 * Sum(A1[i]*A2[i]) =
            //       Sum((p[i] - ...) * A1[i])                             (1)
            // 
            //   0 = d(H)/d(alpha2) = Sum(2*(p[i] - B(u[i])) * d(B(u[i]))/d(alpha2))
            //     where d(B(u[i]))/d(alpha2) = tan2 * B2(u[i])
            //   thus: Sum(p[i] * A2[i]) = Sum(B(u[i]) * A2[i]) where A2[i] = tan2 * B2(u[i])
            //   where expands to:
            //     alpha1 * Sum(A1[i]*A2[i]) + apha2 * Sum(A2[i]^2) = 
            //       Sum((p[i] - ...) * A2[i])                             (2)
            //
            // The above equations (1) and (2) can be rewritten as:
            //
            //   alpha1 * c11 + alpha2 * c12 = X1
            //   alpha1 * c21 + alpha2 * c22 = X2
            //
            // or
            //
            //   C * alpha = X
            //
            // where:
            // 
            //   C is the matrix [ C1 C2 ] = [ C11 C12 C21 C22 ] and
            //   X is the matrix [ X1 X2 ]
            // 
            // Then using Cramer's rule:
            //
            //   alpha1 = det(X C2) / det(C1 C2)
            //   alpha2 = det(C1 X) / det(C1 C2)
            //

            if (u == null)
            {
                u = ChordLengths(s);
            }

            var A = new Vector[u.Count, 2];

            for (int i = 0; i < u.Count; i++)
            {
                A[i, 0] = startTangent * B1(u[i]);
                A[i, 1] = endTangent * B2(u[i]);
            }

            // C 

            var C = new double[2, 2] { { 0, 0 }, { 0, 0 } };

            for (int i = 0; i < u.Count; i++)
            {
                C[0, 0] += A[i, 0] * A[i, 0];
                C[0, 1] += A[i, 0] * A[i, 1];
                C[1, 0] = C[0, 1];
                C[1, 1] += A[i, 1] * A[i, 1];
            }

            // X 

            var X = new double[2] { 0, 0 };

            for (int i = 0; i < s.Count; i++)
            {
                var k = ((Vector)s[i] -
                    (((Vector)s.First() * B0(u[i])) +
                        ((Vector)s.First() * B1(u[i])) +
                            ((Vector)s.Last() * B2(u[i])) +
                                ((Vector)s.Last() * B3(u[i]))));

                X[0] += A[i, 0] * k;
                X[1] += A[i, 1] * k;
            }

            double det_C0_C1 =
                C[0, 0] * C[1, 1] -
                C[1, 0] * C[0, 1];

            double det_C0_X =
                C[0, 0] * X[1] -
                C[1, 0] * X[0];

            double det_X_C1 =
                X[0] * C[1, 1] -
                X[1] * C[0, 1];

            double alpha1 = (det_C0_C1 == 0) ? 0.0 : det_X_C1 / det_C0_C1;
            double alpha2 = (det_C0_C1 == 0) ? 0.0 : det_C0_X / det_C0_C1;

            /* If alpha negative, use the Wu/Barsky heuristic (see text) */
            /* (if alpha is 0, you get coincident control points that lead to
             * divide by zero in any subsequent NewtonRaphsonRootFind() call. */

            double segLength = (s.First() - s.Last()).Length;
            double epsilon = 1.0e-6 * segLength;
            if (alpha1 < epsilon || alpha2 < epsilon)
            {
                alpha1 = alpha2 = segLength / 3.0;
            }

            return new Point[] {
                s.First(),
                s.First() + alpha1 * startTangent,
                s.Last() + alpha2 * endTangent,
                s.Last(),
            };
        }

        #endregion

        #region Bernstein Basis Polynomials

        public static double B0(double t)
        {
            return (t - 1) * (t - 1) * (t - 1);
        }

        public static double B1(double t)
        {
            return 3 * t * (1 - t) * (1 - t);
        }

        public static double B2(double t)
        {
            return 3 * t * t * (1 - t);
        }

        public static double B3(double t)
        {
            return t * t * t;
        }

        #endregion

        #region Control Points

        /// <summary>
        /// Compute the control point P0 required for a bezier with control
        /// points P1, P2, and P3 to evaluate to b at offset t.
        /// </summary>
        public static Point P0(Point b, Point p1, Point p2, Point p3, double t)
        {
            return new Point(
                C0(b.X, p1.X, p2.X, p3.X, t),
                C0(b.Y, p1.Y, p2.Y, p3.Y, t)
            );
        }

        public static double C0(double y, double c1, double c2, double c3, double x)
        {
            // Cubic bezier is given by:
            //
            //   y = c0*(1-x)^3 + 3*c1*x*(1-x)^2 + 3*c2*(1-x)*x^2 + c3*x^3
            //
            // Solving for c0:
            //
            //   c0 = (y - 3*c1*x*(1-x)^2 - 3*c2*(1-x)*x^2 - c3*x^3) / (1-x)^3
            //

            double x2 = x * x;
            double x3 = x * x2;
            double xp = (1 - x);
            double xp2 = xp * xp;
            double xp3 = xp * xp2;

            return (y - 3 * c1 * x * xp2 - 3 * c2 * xp * x2 - c3 * x3) / xp3;
        }

        /// <summary>
        /// Compute the control point P1 required for a bezier with control
        /// points P0,P2, and P3 to evaluate to b at offset t.
        /// </summary>
        public static Point P1(Point b, Point p0, Point p2, Point p3, double t)
        {
            return new Point(
                C1(b.X, p0.X, p2.X, p3.X, t),
                C1(b.Y, p0.Y, p2.Y, p3.Y, t)
            );
        }

        public static double C1(double y, double c0, double c2, double c3, double x)
        {
            // Cubic bezier is given by:
            //
            //   y = c0*(1-x)^3 + 3*c1*x*(1-x)^2 + 3*c2*(1-x)*x^2 + c3*x^3
            //
            // Solving for c1:
            //
            //   c1 = (y - c0*(1-x)^3 - 3*c2*(1-x)*x^2 - c3*x^3) / 3*x*(1-x)^2
            //

            double x2 = x * x;
            double x3 = x * x2;
            double xp = (1 - x);
            double xp2 = xp * xp;
            double xp3 = xp * xp2;

            return (y - c0 * xp3 - 3 * c2 * xp * x2 - c3 * x3) / (3 * x * xp2);
        }

        /// <summary>
        /// Compute the control point P2 required for a bezier with control
        /// points P0, P1, and P3 to evaluate to b at offset t.
        /// </summary>
        public static Point P2(Point b, Point p0, Point p1, Point p3, double t)
        {
            return new Point(
                C2(b.X, p0.X, p1.X, p3.X, t),
                C2(b.Y, p0.Y, p1.Y, p3.Y, t)
            );
        }

        public static double C2(double y, double c0, double c1, double c3, double x)
        {
            // Cubic bezier is given by:
            //
            //   y = c0*(1-x)^3 + 3*c1*x*(1-x)^2 + 3*c2*(1-x)*x^2 + c3*x^3
            //
            // Solving for c2:
            //
            //   c2 = (y - c0*(1-x)^3 - 3*c1*x*(1-x)^2 - c3*x^3) / 3*(1-x)*x^2
            //

            double x2 = x * x;
            double x3 = x * x2;
            double xp = (1 - x);
            double xp2 = xp * xp;
            double xp3 = xp * xp2;

            return (y - c0 * xp3 - 3 * c1 * x * xp2 - c3 * x3) / (3 * xp * x2);
        }

        /// <summary>
        /// Compute the control point P3 required for a bezier with control
        /// points P0, P1, and P2 to evaluate to b at offset t.
        /// </summary>
        public static Point P3(Point b, Point p0, Point p1, Point p2, double t)
        {
            return new Point(
                C3(b.X, p0.X, p1.X, p2.X, t),
                C3(b.Y, p0.Y, p1.Y, p2.Y, t)
            );
        }

        public static double C3(double y, double c0, double c1, double c2, double x)
        {
            // Cubic bezier is given by:
            //
            //   y = c0*(1-x)^3 + 3*c1*x*(1-x)^2 + 3*c2*(1-x)*x^2 + c3*x^3
            //
            // Solving for c3:
            //
            //   c3 = (y - c0*(1-x)^3 - 3*c1*x*(1-x)^2 - 3*c2*(1-x)*x^2) / x^3
            //

            double x2 = x * x;
            double x3 = x * x2;
            double xp = (1 - x);
            double xp2 = xp * xp;
            double xp3 = xp * xp2;

            return (y - c0 * xp3 - 3 * c1 * x * xp2 - 3 * c2 * xp * x2) / x3;
        }

        #endregion

        #region Parameterization

        /// <summary>
        /// Find the chord length parameterization of the given set of sample points
        /// </summary>
        public static double[] ChordLengths(IList<Point> points)
        {
            // Given a polyline defined by the set of points:
            // 
            //     P[i] = { x[i],y[i] }
            // 
            // Compute the distance along that line from point P[0] to point P[n]:
            // 
            //     d[0] = 0;
            //     d[i] = Sum(|P[j] - P[j-1]|, j=1 to i)
            //

            var d = new double[points.Count];

            d[0] = 0.0;
            for (int j = 1; j < points.Count; j++)
            {
                d[j] = d[j - 1] + (points[j - 1] - points[j]).Length;
            }

            // Normalize the results to the range (0, 1)

            var t = new double[d.Length];

            for (int j = 1; j < d.Length; j++)
            {
                t[j] = d[j] / d.Last();
            }

            return t;
        }


        #endregion

        #region Error Functions

        /// <summary>
        /// Find the maximum squared error between a bezier curve and a set
        /// of sample points
        /// </summary>
        /// <param name="p0">1st control point</param>
        /// <param name="p1">2nd control point</param>
        /// <param name="p2">3rd control point</param>
        /// <param name="p3">4th control point</param>
        /// <param name="samples">Sample points</param>
        /// <param name="u">Sample point parameterization</param>
        /// <returns>Maximum squared error</returns>
        public static double MaxError(Point p0, Point p1, Point p2, Point p3, IList<Point> samples, IList<double> u = null)
        {
            if (u == null)
            {
                u = ChordLengths(samples);
            }

            if (u.Count != samples.Count)
            {
                throw new ArgumentException("u");
            }

            double result = 0.0;

            for (int i = 1; i < u.Count; i++)
            {
                var computed = Evaluate(p0, p1, p2, p3, u[i]);
                var actual = samples[i];
                var error = (computed - actual).LengthSquared;

                result = Math.Max(error, result);
            }

            return result;
        }

        #endregion

        #endregion

    }
}
