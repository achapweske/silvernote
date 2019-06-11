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
using DOM;
using DOM.SVG;
using DOM.Helpers;

namespace SilverNote.Editor
{
    public class QuadraticBezier : LineBase
    {
        #region Fields

        double _X1;
        double _Y1;
        double _X2;
        double _Y2;
        double _X3;
        double _Y3;

        #endregion

        #region Constructors

        /// <summary>
        /// Create an empty quadratic bezier
        /// </summary>
        public QuadraticBezier()
        {
            Initialize();
        }

        /// <summary>
        /// Create a quadratic bezier with the given control points
        /// </summary>
        public QuadraticBezier(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            Initialize();

            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            X3 = x3;
            Y3 = y3;

            StrokeBrush = Brushes.Black;
            StrokeWidth = 2;
        }

        /// <summary>
        /// Create a quadratic bezier with the given control points
        /// </summary>
        public QuadraticBezier(Point point1, Point point2, Point point3)
        {
            Initialize();

            X1 = point1.X;
            Y1 = point1.Y;
            X2 = point2.X;
            Y2 = point2.Y;
            X3 = point3.X;
            Y3 = point3.Y;

            StrokeBrush = Brushes.Black;
            StrokeWidth = 2;
        }

        /// <summary>
        /// Create a copy of another quadratic bezier
        /// </summary>
        public QuadraticBezier(QuadraticBezier copy)
            : base(copy)
        {
            X1 = copy.X1;
            Y1 = copy.Y1;
            X2 = copy.X2;
            Y2 = copy.Y2;
            X3 = copy.X3;
            Y3 = copy.Y3;

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

            MarkerStart = null;
            MarkerEnd = null;
        }

        #endregion

        #region Properties

        public double X1
        {
            get
            {
                return _X1;
            }
            set
            {
                if (value != _X1)
                {
                    _X1 = value;
                    InvalidateRender();
                }
            }
        }

        public double Y1
        {
            get
            {
                return _Y1;
            }
            set
            {
                if (value != _Y1)
                {
                    _Y1 = value;
                    InvalidateRender();
                }
            }
        }

        public double X2
        {
            get
            {
                return _X2;
            }
            set
            {
                if (value != _X2)
                {
                    _X2 = value;
                    InvalidateRender();
                }
            }
        }

        public double Y2
        {
            get
            {
                return _Y2;
            }
            set
            {
                if (value != _Y2)
                {
                    _Y2 = value;
                    InvalidateRender();
                }
            }
        }

        public double X3
        {
            get
            {
                return _X3;
            }
            set
            {
                if (value != _X3)
                {
                    _X3 = value;
                    InvalidateRender();
                }
            }
        }

        public double Y3
        {
            get
            {
                return _Y3;
            }
            set
            {
                if (value != _Y3)
                {
                    _Y3 = value;
                    InvalidateRender();
                }
            }
        }

        public double RenderedX1
        {
            get
            {
                Point point1 = new Point(X1, Y1);
                point1 = GeometryTransform.Transform(point1);
                return point1.X;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    Point point1 = new Point(value, RenderedY1);
                    point1 = inverse.Transform(point1);
                    X1 = point1.X;
                }
            }
        }

        public double RenderedY1
        {
            get
            {
                Point point1 = new Point(X1, Y1);
                point1 = GeometryTransform.Transform(point1);
                return point1.Y;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    Point point1 = new Point(RenderedX1, value);
                    point1 = inverse.Transform(point1);
                    Y1 = point1.Y;
                }
            }
        }

        public double RenderedX2
        {
            get
            {
                Point point2 = new Point(X2, Y2);
                point2 = GeometryTransform.Transform(point2);
                return point2.X;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    Point point2 = new Point(value, RenderedY2);
                    point2 = inverse.Transform(point2);
                    X2 = point2.X;
                }
            }
        }

        public double RenderedY2
        {
            get
            {
                Point point2 = new Point(X2, Y2);
                point2 = GeometryTransform.Transform(point2);
                return point2.Y;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    Point point2 = new Point(RenderedX2, value);
                    point2 = inverse.Transform(point2);
                    Y2 = point2.Y;
                }
            }
        }

        public double RenderedX3
        {
            get
            {
                Point point3 = new Point(X3, Y3);
                point3 = GeometryTransform.Transform(point3);
                return point3.X;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    Point point3 = new Point(value, RenderedY3);
                    point3 = inverse.Transform(point3);
                    X3 = point3.X;
                }
            }
        }

        public double RenderedY3
        {
            get
            {
                Point point3 = new Point(X3, Y3);
                point3 = GeometryTransform.Transform(point3);
                return point3.Y;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    Point point3 = new Point(RenderedX3, value);
                    point3 = inverse.Transform(point3);
                    Y3 = point3.Y;
                }
            }
        }

        #endregion

        #region NLineBase

        /// <summary>
        /// Get this curve's start point
        /// </summary>
        public override Point StartPoint
        {
            get { return new Point(X1, Y1); }
            set
            {
                X1 = value.X;
                Y1 = value.Y;
            }
        }

        /// <summary>
        /// Get this curve's end point
        /// </summary>
        public override Point EndPoint
        {
            get { return new Point(X3, Y3); }
            set
            {
                X3 = value.X;
                Y3 = value.Y;
            }
        }

        /// <summary>
        /// Get a thumbnail representation of this curve
        /// </summary>
        public override LineBase LineThumb
        {
            get
            {
                var result = (QuadraticBezier)base.LineThumb;
                result.X1 = 0;
                result.Y1 = 0;
                result.X2 = 50;
                result.Y2 = 15;
                result.X3 = 100;
                result.Y3 = 0; 
                result.GeometryTransform = null;
                return result;
            }
        }

        #endregion

        #region NDrawing

        /// <summary>
        /// Get a large thumbnail representation of this curve
        /// </summary>
        public override Shape ThumbLarge
        {
            get
            {
                var result = (QuadraticCurve)base.ThumbLarge;
                result.X1 = 0;
                result.Y1 = 30;
                result.X2 = 15;
                result.Y2 = 15;
                result.X3 = 30;
                result.Y3 = 0;
                result.GeometryTransform = null;
                return result;
            }
        }

        /// <summary>
        /// Get a small thumbnail representation of this curve
        /// </summary>
        public override Shape ThumbSmall
        {
            get
            {
                var result = (QuadraticBezier)base.ThumbSmall;
                result.StrokeWidth = 1;
                result.X1 = 0;
                result.Y1 = 16;
                result.X2 = 8;
                result.Y2 = 8;
                result.X3 = 16;
                result.Y3 = 0;
                result.GeometryTransform = null;
                return result;
            }
        }

        /// <summary>
        /// Begin drawing a quadratic bezier at the given position
        /// </summary>
        public override void Place(Point position)
        {
            X1 = X2 = X3 = position.X;
            Y1 = Y2 = Y3 = position.Y;
        }

        /// <summary>
        /// Continue drawing a quadratic bezier at the given position
        /// </summary>
        /// <param name="point"></param>
        public override void Draw(Point point)
        {
            X3 = point.X;
            Y3 = point.Y;

            X2 = (X1 + X3) / 2.0;
            Y2 = (Y1 + Y3) / 2.0;
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override int HandleCount
        {
            get { return 3; }
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
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
                default:
                    return new Point();
            }
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        protected override void SetHandleInternal(int index, Point value)
        {
            switch (index)
            {
                case 0:
                    X2 += (value.X - X1) / 2;
                    Y2 += (value.Y - Y1) / 2;
                    X1 = value.X;
                    Y1 = value.Y;
                    break;
                case 1:
                    X2 = value.X;
                    Y2 = value.Y;
                    break;
                case 2:
                    X2 += (value.X - X3) / 2;
                    Y2 += (value.Y - Y3) / 2;
                    X3 = value.X;
                    Y3 = value.Y;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        protected override Rect Bounds
        {
            get
            {
                double left = Math.Min(X1, X3);
                double top = Math.Min(Y1, Y3);
                double right = Math.Max(X1, X3);
                double bottom = Math.Max(Y1, Y3);

                if (X2 < left || X2 > right)
                {
                    double root = Root(X1, X2, X3);
                    if (!Double.IsInfinity(root))
                    {
                        double minOrMax = Evaluate(X1, X2, X3, root);
                        left = Math.Min(left, minOrMax);
                        right = Math.Max(right, minOrMax);
                    }
                }

                if (Y2 < top || Y2 > bottom)
                {
                    double root = Root(Y1, Y2, Y3);
                    if (!Double.IsInfinity(root))
                    {
                        double minOrMax = Evaluate(Y1, Y2, Y3, root);
                        top = Math.Min(top, minOrMax);
                        bottom = Math.Max(bottom, minOrMax);
                    }
                }

                return new Rect(left, top, right - left, bottom - top);
            }
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override void Normalize()
        {
            var point1 = GeometryTransform.Transform(new Point(X1, Y1));
            var point2 = GeometryTransform.Transform(new Point(X2, Y2));
            var point3 = GeometryTransform.Transform(new Point(X3, Y3));

            X1 = point1.X;
            Y1 = point1.Y;
            X2 = point2.X;
            Y2 = point2.Y;
            X3 = point3.X;
            Y3 = point3.Y;

            GeometryTransform = null;
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override PathGeometry ToPathGeometry()
        {
            var segment = new QuadraticBezierSegment();
            segment.Point1 = new Point(X2, Y2);
            segment.Point2 = new Point(X3, Y3);
            segment.IsStroked = true;

            var figure = new PathFigure();
            figure.StartPoint = new Point(X1, Y1);
            figure.Segments.Add(segment);

            var path = new PathGeometry();
            path.Figures.Add(figure);

            return path;
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override StreamGeometry ToStreamGeometry()
        {
            var geometry = new StreamGeometry();

            var gc = geometry.Open();
            gc.BeginFigure(new Point(X1, Y1), FillBrush != null, false);
            gc.QuadraticBezierTo(new Point(X2, Y2), new Point(X3, Y3), StrokeBrush != null, false);
            gc.Close();

            return geometry;
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        protected override void OnRenderVisual(DrawingContext dc)
        {
            double zoom = NoteEditor.GetZoom(this);

            // Apply GeometryTransform

            Point point1 = GeometryTransform.Transform(new Point(X1, Y1));
            Point point2 = GeometryTransform.Transform(new Point(X2, Y2));
            Point point3 = GeometryTransform.Transform(new Point(X3, Y3));

            // Pixel-align vertical lines

            if (point1.X == point2.X && point2.X == point3.X)
            {
                point1.X = point2.X = point3.X = Math.Floor(point3.X * zoom) / zoom;
            }

            // Pixel-align horizontal lines

            if (point1.Y == point2.Y && point2.Y == point3.Y)
            {
                point1.Y = point2.Y = point3.Y = Math.Floor(point3.Y * zoom) / zoom;
            }

            // Compute rendering points for line markers

            Point render1 = point1;
            Point render2 = point2;
            Point render3 = point3;

            if (MarkerStart != null)
            {
                double arcLength = ArcLength(point1, point2, point3);
                render1 = Evaluate(point1, point2, point3, StrokeWidth / arcLength);
            }

            if (MarkerEnd != null)
            {
                double arcLength = ArcLength(point1, point2, point3);
                render3 = Evaluate(point3, point2, point1, StrokeWidth / arcLength);
            }

            // Now actually draw the curve

            var geometry = new StreamGeometry();
            var gc = geometry.Open();
            try
            {
                gc.BeginFigure(render1, FillBrush != null, false);
                gc.QuadraticBezierTo(render2, render3, StrokeBrush != null, false);
            }
            finally
            {
                gc.Close();
            }

            dc.DrawGeometry(null, StrokePen, geometry);

            // Trace over the curve with a transparent brush thick enough
            // to make it easy to click on

            if (StrokeWidth * zoom < 5)
            {
                Pen transparent = new Pen(Brushes.Transparent, 5 / zoom);

                dc.DrawGeometry(null, transparent, geometry);
            }

            // Render line markers

            if (X1 == X2 && X2 == X3 && 
                Y1 == Y2 && Y2 == Y3)
            {
                if (MarkerStart != null)
                {
                    Children.Remove(MarkerStart);
                }

                if (MarkerEnd != null)
                {
                    Children.Remove(MarkerEnd);
                }

                return;
            }

            if (MarkerStart != null)
            {
                MarkerStart.Offset = new Vector(point1.X, point1.Y);
                MarkerStart.Orient = Math.Atan2(point2.Y - point1.Y, point2.X - point1.X) * 180.0 / Math.PI;
                MarkerStart.Brush = this.StrokeBrush;
                MarkerStart.Redraw();

                if (!Children.Contains(MarkerStart))
                {
                    Children.Add(MarkerStart);
                }
            }

            if (MarkerEnd != null)
            {
                MarkerEnd.Offset = new Vector(point3.X, point3.Y);
                MarkerEnd.Orient = Math.Atan2(point3.Y - point2.Y, point3.X - point2.X) * 180.0 / Math.PI;
                MarkerEnd.Brush = this.StrokeBrush;
                MarkerEnd.Redraw();

                if (!Children.Contains(MarkerEnd))
                {
                    Children.Add(MarkerEnd);
                }
            }
        }

        #endregion

        #region Algorithms

        /// <summary>
        /// Evaluate a quadratic bezier at offset t
        /// </summary>
        public static Point Evaluate(Point p0, Point p1, Point p2, double t)
        {
            return new Point(
                Evaluate(p0.X, p1.X, p2.X, t),
                Evaluate(p0.Y, p1.Y, p2.Y, t)
            );
        }

        public static double Evaluate(double a, double b, double c, double x)
        {
            // Quadratic bezier function

            return a * (1 - x) * (1 - x) + 2 * b * (1 - x) * x + c * x * x;
        }

        public static double Root(double a, double b, double c)
        {
            // x = a(1-t)^2 + 2b(1-t) + ct^2
            // dx/dt = -2a(1-t) + (2b-4bt) + 2ct
            // t = (a-b) / (a-2b+c) | t = 0

            return (a - b) / (a - 2 * b + c);
        }

        #region Curve Fitting

        /// <summary>
        /// Fit a set of points to a quadratic bezier curve
        /// </summary>
        public static Point[] FitCurve(Vector startTangent, IList<Point> s, IList<double> u = null)
        {
            // A quadratic bezier curve is defined as:
            //
            //   B(t) = P0*(1-t)^2 + 2*P1*(1-t)*t + P2*t^2
            //        = P0*B0(t) + P1*B1(t) + P2*B2(t)
            //
            // The outer control points (P0 and P2) always lie on the curve itself,
            // so given a set of samples s:
            //
            //   P0 = s[0]
            //   P2 = s[n]
            //
            // So that our curves have smooth connections, we constrain the tangent at
            // the start of the curve to be equal to the tangent at the end of the
            // previous curve. Thus:
            //
            //   P1 = P[0] + alpha * startTangent
            //
            // where alpha is the distance from s[0] to P1 along the given tangent.
            //
            // The best fit alpha is the one that minimizes the (squared) distance
            // between the curve and the sample points:
            //
            //   H = Sum((s[i] - B(u[i]))^2) 
            //   
            // where u is parameterization of s. i.e., u[i] is the estimated 
            // distance from s[0] to s[i] along the curve.
            //
            // The minimum is simply the point at which the derivative is 0:
            //
            //   d(H)/d(alpha) = 0
            //
            // So we begin by expanding d(H)/d(alpha):
            //
            //   d(H)/d(alpha) = Sum(2*(s[i] - B(u[i])) * d(B(u[i]))/d(alpha))
            //                 = Sum(2*(s[i] - B(u[i])) * A[i])
            //
            //   where A[i] := d(B(u[i]))/d(alpha) = startTangent * B1(u[i])
            //
            // Setting this equal to 0 and rearranging gives:
            //
            //   Sum(s[i] * A[i]) = Sum(B(u[i]) * A[i])
            //                    = Sum((P0*B0(u[i]) + P1*B1(u[i]) + P2*B2(u[i])) * A[i])
            //                    = Sum((P0*B0(u[i]) + (P0*B1(u[i]) + alpha*startTangent*B1(u[i])) + P2*B2(u[i])) * A[i])
            //                    = Sum((P0*B0(u[i]) + P0*B1(u[i]) + alpha*A[i] + P2*B2(u[i])) * A[i])
            //                    = Sum(P0*B0(u[i])*A[i]) + Sum(P0*B1(u[i])*A[i]) + alpha*Sum(A[i]^2) + Sum(P2*B2(u[i])*A[i])
            //
            // Finally, we solve for alpha:
            //
            //   alpha * Sum(A[i]^2) = Sum((s[i] - P0*B0(u[i]) - P0*B1(u[i]) - P2*B2(u[i])) * A[i])
            //   alpha = Sum((s[i] - P0*B0(u[i]) - P0*B1(u[i]) - P2*B2(u[i])) * A[i]) / Sum(A[i]^2)
            //

            if (u == null)
            {
                u = ChordLengths(s);
            }

            var A = new Vector[u.Count];

            for (int i = 0; i < u.Count; i++)
            {
                A[i] = startTangent * B1(u[i]);
            }

            double denominator = 0;

            for (int i = 0; i < A.Length; i++)
            {
                denominator += A[i] * A[i];
            }

            double numerator = 0;

            for (int i = 0; i < s.Count; i++)
            {
                var k = (Vector)s[i] - (Vector)s.First() * B0(u[i]) - (Vector)s.First() * B1(u[i]) - (Vector)s.Last() * B2(u[i]);

                numerator += A[i] * k;
            }

            double alpha = numerator / denominator;

            double segLength = (s.First() - s.Last()).Length;
            double epsilon = 1.0e-6 * segLength;
            if (alpha < epsilon || Double.IsNaN(alpha))
            {
                alpha = segLength / 2.0;
            }

            var p0 = s.First();
            var p1 = s.First() + alpha * startTangent;
            var p2 = s.Last();

            return new Point[] { p0, p1, p2 };
        }

        #endregion

        #region Bernstein Basis Polynomials

        /// <summary>
        /// Compute the Bernstien basis polynomials for a quadratic bezier
        /// </summary>

        public static double B0(double t)
        {
            return (t - 1) * (t - 1);
        }

        public static double B1(double t)
        {
            return 2.0 * t * (1 - t);
        }

        public static double B2(double t)
        {
            return t * t;
        }

        #endregion

        #region Control Points

        /// <summary>
        /// Compute the control point P0 required for a bezier with control
        /// points p1 and p2 to evaluate to b at offset t.
        /// </summary>
        public static Point P0(Point b, Point p1, Point p2, double t)
        {
            return new Point(
                C0(b.X, p1.X, p2.X, t),
                C0(b.Y, p1.Y, p2.Y, t)
            );
        }

        public static double C0(double y, double c1, double c2, double x)
        {
            // Quadratic bezier is given by:
            //
            //   y = c0*(1-x)^2 + 2*c1*(1-x)*x + c2*x^2
            //
            // Solving for c0:
            //
            //   (y - 2*c1*(1-x)*x - c2*x^2) / (1-x)^2
            //

            return (y - 2 * c1 * (1 - x) * x - c2 * x * x) / ((1 - x) * (1 - x));
        }

        /// <summary>
        /// Compute the control point P1 required for a bezier with control
        /// points P0 and P2 to evaluate to b at offset t.
        /// </summary>
        public static Point P1(Point b, Point p0, Point p2, double t)
        {
            return new Point(
                C1(b.X, p0.X, p2.X, t),
                C1(b.Y, p0.Y, p2.Y, t)
            );
        }

        public static double C1(double y, double c0, double c2, double x)
        {
            // Quadratic bezier is given by:
            //
            //   y = c0*(1-x)^2 + 2*c1*(1-x)*x + c2*x^2
            //
            // Solving for c1:
            //
            //   c1 = (y - c0*(1-x)^2 - c2*x^2) / 2*(1-x)*x
            //

            return (y - c0 * (1 - x) * (1 - x) - c2 * x * x) / (2 * (1 - x) * x);
        }

        /// <summary>
        /// Compute the control point P2 required for a bezier with control
        /// points P0 and P1 to evaluate to b at offset t.
        /// </summary>
        public static Point P2(Point b, Point p0, Point p1, double t)
        {
            return new Point(
                C2(b.X, p0.X, p1.X, t),
                C2(b.Y, p0.Y, p1.Y, t)
            );
        }

        public static double C2(double y, double c0, double c1, double x)
        {
            // Quadratic bezier is given by:
            //
            //   y = c0*(1-x)^2 + 2*c1*(1-x)*x + c2*x^2
            //
            // Solving for c2:
            //
            //   (y - c0*(1-x)^2 - 2*c1*(1-x)*x) / x^2
            //

            return (y - c0 * (1 - x) * (1 - x) - 2 * c1 * (1 - x) * x) / (x * x);
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

        /// <summary>
        /// Find the (approximate) arc length of a quadratic bezier
        /// </summary>
        public static double ArcLength(Point p0, Point p1, Point p2)
        {
            return (p1 - p0).Length + (p2 - p1).Length;
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
        /// <param name="samples">Sample points</param>
        /// <param name="u">Sample point parameterization</param>
        /// <returns>Maximum squared error</returns>
        public static double MaxError(Point p0, Point p1, Point p2, IList<Point> samples, IList<double> u = null)
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
                var computed = Evaluate(p0, p1, p2, u[i]);
                var actual = samples[i];
                var error = (computed - actual).LengthSquared;

                result = Math.Max(error, result);
            }

            return result;
        }

        #endregion

        #endregion

        #region INodeSource

        private readonly string _NodeName = SVGElements.NAMESPACE + ' ' + SVGElements.PATH;

        public override string GetNodeName(NodeContext context)
        {
            return _NodeName;
        }

        private readonly string[] _NodeAttributes = new string[] { 
            SVGAttributes.MARKER_START,
            SVGAttributes.MARKER_END,
            SVGAttributes.D
        };

        public override IList<string> GetNodeAttributes(ElementContext context)
        {
            return base.GetNodeAttributes(context).Concat(_NodeAttributes).ToArray();
        }

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    string classNames = base.GetNodeAttribute(context, name) ?? "";
                    return DOMHelper.PrependClass(classNames, "quadraticBezier");
                case SVGAttributes.D:
                    return TransformedPathData;
                case SVGAttributes.MARKER_START:
                    return Canvas.GetDefinitionURL(MarkerStart);
                case SVGAttributes.MARKER_END:
                    return Canvas.GetDefinitionURL(MarkerEnd);
                default:
                    return base.GetNodeAttribute(context, name);
            }


        }

        public override void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.D:
                    TransformedPathData = value;
                    break;
                case SVGAttributes.MARKER_START:
                    MarkerStart = (Marker)Canvas.GetDefinition(value);
                    break;
                case SVGAttributes.MARKER_END:
                    MarkerEnd = (Marker)Canvas.GetDefinition(value);
                    break;
                default:
                    base.SetNodeAttribute(context, name, value);
                    break;
            }
        }

        public override void ResetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.D:
                    X1 = X2 = Y1 = Y2 = X3 = Y3 = 0;
                    break;
                case SVGAttributes.MARKER_START:
                case SVGAttributes.MARKER_END:
                default:
                    base.ResetNodeAttribute(context, name);
                    break;
            }
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new QuadraticBezier(this);
        }

        #endregion

        #region Implementation

        string TransformedPathData
        {
            get
            {
                Point point1 = new Point(X1, Y1);
                Point point2 = new Point(X2, Y2);
                Point point3 = new Point(X3, Y3);

                if (GeometryTransform != Transform.Identity)
                {
                    point1 = GeometryTransform.Transform(point1);
                    point2 = GeometryTransform.Transform(point2);
                    point3 = GeometryTransform.Transform(point3);
                }

                return String.Format(CultureInfo.InvariantCulture, "M{0},{1} Q{2},{3} {4},{5}", point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y);
            }
            set
            {
                PathGeometry geometry;
                if (!NPath.TryParseGeometry(value, out geometry))
                {
                    return;
                }

                var figure = geometry.Figures.FirstOrDefault();
                if (figure == null)
                {
                    return;
                }

                var segment = figure.Segments.FirstOrDefault() as QuadraticBezierSegment;
                if (segment == null)
                {
                    return;
                }

                X1 = figure.StartPoint.X;
                Y1 = figure.StartPoint.Y;
                X2 = segment.Point1.X;
                Y2 = segment.Point1.Y;
                X3 = segment.Point2.X;
                Y3 = segment.Point2.Y;
            }
        }

        #endregion

    }
}
