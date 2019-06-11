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
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Diagnostics;

namespace SilverNote.Editor
{
    public class Arc : Shape
    {
        #region Constructors

        public Arc()
        {
            Initialize();
        }

        public Arc(Point startPoint, Point endPoint, Size size, double tilt, bool isLargArc, SweepDirection sweepDirection)
        {
            Initialize();

            StartPoint = startPoint;
            EndPoint = endPoint;
            RX = size.Width / 2;
            RY = size.Height / 2;
            Tilt = tilt;
            IsLargeArc = isLargArc;
            SweepDirection = sweepDirection;
        }

        public Arc(Arc copy)
        {
            Initialize();

            StartPoint = copy.StartPoint;
            EndPoint = copy.EndPoint;
            RX = copy.RX;
            RY = copy.RY;
            Tilt = copy.Tilt;
            SweepDirection = copy.SweepDirection;
            IsLargeArc = copy.IsLargeArc;
        }

        public void Initialize()
        {
            StartPoint = new Point();
            EndPoint = new Point();
            RX = 0;
            RY = 0;
            Tilt = 0;
            SweepDirection = SweepDirection.Clockwise;
            IsLargeArc = false;
        }

        #endregion

        #region Attributes

        public Point StartPoint { get; set; }

        public Point EndPoint { get; set; }

        public double RX { get; set; }

        public double RY { get; set; }

        public double Tilt { get; set; }

        public SweepDirection SweepDirection { get; set; }

        public bool IsLargeArc { get; set; }

        public override Geometry Geometry
        {
            get
            {
                ArcSegment segment = new ArcSegment();
                segment.Point = EndPoint;
                segment.Size = new Size(RX, RY);
                segment.SweepDirection = SweepDirection;
                segment.RotationAngle = Tilt;
                segment.IsLargeArc = IsLargeArc;
                segment.IsStroked = true;

                PathFigure figure = new PathFigure();
                figure.StartPoint = StartPoint;
                figure.Segments.Add(segment);

                PathGeometry geometry = new PathGeometry();
                geometry.Figures.Add(figure);

                return geometry;
            }
        }

        #endregion

        #region Algorithms

        public double TiltInRadians
        {
            get { return Tilt * Math.PI / 180.0; }
            set { Tilt = value * 180.0 / Math.PI; }
        }

        public Point Center
        {
            get
            {
                return EllipseCenterFromPoints(StartPoint, EndPoint, RX, RY, TiltInRadians, IsLargeArc, SweepDirection == SweepDirection.Clockwise);
            }
        }

        public Point Left
        {
            get
            {
                Point center = Center;
                Point left = new Point(center.X - RX, center.Y);
                return RotatePoint(left, TiltInRadians, center);
            }
        }

        public Point Top
        {
            get
            {
                Point center = Center;
                Point top = new Point(center.X, center.Y - RY);
                return RotatePoint(top, TiltInRadians, center);
            }
        }

        public Point Right
        {
            get
            {
                Point center = Center;
                Point right = new Point(center.X + RX, center.Y);
                return RotatePoint(right, TiltInRadians, center);
            }
        }

        public Point Bottom
        {
            get
            {
                Point center = Center;
                Point bottom = new Point(center.X, center.Y + RY);
                return RotatePoint(bottom, TiltInRadians, center);
            }
        }

        public static Point EllipseCenterFromPoints(Point point1, Point point2, double radiusX, double radiusY, double tiltInRadians, bool isLargeArc, bool isClockwise)
        {
            // Here we find the center of an ellipse given two points on that ellipse, its radii, and its tilt

            // See http://www.w3.org/TR/SVG/implnote.html#ArcConversionEndpointToCenter

            double a = (point1.X - point2.X) / 2;
            double b = (point1.Y - point2.Y) / 2;
            double x1p = a * Math.Cos(tiltInRadians) + b * Math.Sin(tiltInRadians);
            double y1p = b * Math.Cos(tiltInRadians) - a * Math.Sin(tiltInRadians);

            double c = radiusX * radiusX * radiusY * radiusY - radiusX * radiusX * y1p * y1p - radiusY * radiusY * x1p * x1p;
            c /= radiusX * radiusX * y1p * y1p + radiusY * radiusY * x1p * x1p;
            c = Math.Sqrt(c) * ((isLargeArc == isClockwise) ? -1 : 1);
            double cxp = c * radiusX * y1p / radiusY;
            double cyp = -c * radiusY * x1p / radiusX;

            double cx = Math.Cos(tiltInRadians) * cxp - Math.Sin(tiltInRadians) * cyp + (point1.X + point2.X) / 2;
            double cy = Math.Sin(tiltInRadians) * cxp + Math.Cos(tiltInRadians) * cyp + (point1.Y + point2.Y) / 2;

            return new Point(cx, cy);

            /*
             * The following formula doesn't work because it assumes the arc is >= 180 degrees:
             * 
            // Recall:
            //   For an ellipse with radii Rx and Ry, the point at angle theta is given by
            //     x = Rx * cos(theta)
            //     y = Ry * sin(theta)
            //   Rotating a point (x,y) by angle "tilt" about the origin and translating it to Cx,Cy gives point x',y'
            //     x' = x * cos(tilt) - y * sin(tilt) + Cx
            //     y' = x * sin(tilt) + y * cos(tilt) + Cy
            //   So for an ellipse with radii Rx and Ry with and tilt "tilt", the point at angle theta is given by:
            //     x' = (Rx * cos(theta)) * cos(tilt) - (Ry * sin(theta)) * sin(tilt) + Cx
            //        = (Rx * cos(tilt)) * cos(theta) - (Ry * sin(tilt)) * sin(theta) + Cx
            //        = a * cos(theta) + b * sin(theta) + Cx     where a := Rx * cos(tilt) and b:= -Ry * sin(tilt)
            //     y' = (Rx * cos(theta)) * sin(tilt) + (Ry * sin(theta)) * cos(tilt) + Cy
            //        = (Rx * sin(tilt)) * cos(theta) + (Ry * cos(tilt)) * sin(theta) + Cy
            //        = c * cos(theta) + d * sin(theta) + Cy     where c := Rx * sin(tilt) and d := Ry * cos(tilt)

            double cosTilt = Math.Cos(tiltInRadians);
            double sinTilt = Math.Sin(tiltInRadians);

            double a = radiusX * cosTilt;
            double b = -radiusY * sinTilt;
            double c = radiusX * sinTilt;
            double d = radiusY * cosTilt;

            //   Now lets say we are given two points x'1,y'1 and x'2,y'2 and we want to find theta1 and theta2:
            //     x'1 = a * cos(theta1) + b * sin(theta1) + Cx
            //     y'1 = c * cos(theta1) + d * sin(theta1) + Cy
            //     x'2 = a * cos(theta2) + b * sin(theta2) + Cx
            //     y'2 = c * cos(theta2) + d * sin(theta2) + Cy
            //   First we subtract the equations
            //     x'1 - x'2 = a * [cos(theta1) - cos(theta2)] + b * [sin(theta1) - sin(theta2)]
            //     y'1 - y'2 = c * [cos(theta1) - cos(theta2)] + d * [sin(theta1) - sin(theta2)]
            //   Then we cancel-out the cos terms:
            //     { x'1 - x'2 - b * [sin(theta1) - sin(theta2)] } / a = cos(theta1) - cos(theta2)
            //     = A + B * [sin(theta1) - sin(theta2)]    where A := (x'1 - x'2) / a and B := -b / a
            //     { y'1 - y'2 - d * [sin(theta1) - sin(theta2)] } / c = cos(theta1) - cos(theta2)
            //     = C + D * [sin(theta1) - sin(theta2)]    where C := (y'1 - y'2) / c and D := -d / c
            //     A + B * [sin(theta1) - sin(theta2)] = C + D * [sin(theta1) - sin(theta2)]
            //     (A - C) / (D - B) = sin(theta1) - sin(theta2)
            //   And we cancel-out the sin terms:
            //     { x'1 - x'2 - a * [cos(theta1) - cos(theta2)] } / b = sin(theta1) - sin(theta2)
            //     = E + F * [cos(theta1) - cos(theta2)]    where E := (x'1 - x'2) / b and F := -a / b
            //     { y'1 - y'2 - c * [cos(theta1) - cos(theta2)] } / d = sin(theta1) - sin(theta2)
            //     = G + H * [cos(theta1) - cos(theta2)]    where G := (y'1 - y'2) / d and H := -c / d
            //     E + F * [cos(theta1) - cos(theta2)] = G + H * [cos(theta1) - cos(theta2)]
            //     (E - G) / (H - F) = cos(theta1) - cos(theta2)
            //  We now have:
            //     Q1 = sin(theta1) - sin(theta2)   where Q1 = (A - C) / (D - B) = [c * (x'1 - x'2) - a * (y'1 - y'2)] / [b*c - a*d]
            //     Q2 = cos(theta1) - cos(theta2)   where Q2 = (E - G) / (H - F) = [d * (x'1 - x'2) - b * (y'1 - y'2)] / [a*d - c*b]

            double Q1 = (c * (point1.X - point2.X) - a * (point1.Y - point2.Y)) / (b * c - a * d);
            double Q2 = (d * (point1.X - point2.X) - b * (point1.Y - point2.Y)) / (a * d - c * b);

            //  Using trigonometric identities for differences of sines and cosines:
            //     Q1 =  2 * cos((theta1 + theta2) / 2) * sin((theta1 - theta2) / 2)
            //     Q2 = -2 * sin((theta1 + theta2) / 2) * sin((theta1 - theta2) / 2)
            //  Simplifying:
            //     Q1 / 2 = cos(theta1') * sin(theta2')     theta1' := (theta1 + theta2) / 2
            //    -Q2 / 2 = sin(theta1') * sin(theta2')     theta2' := (theta1 - theta2) / 2
            //  Dividing the second equation by the first:
            //    -Q2 / Q1 = sin(theta1') / cos(theta1') = tan(theta1')
            //  Solving for theta1':
            //    theta1' = arctan(-Q2/Q1) = arctan( [d * (x'1 - x'2) - b * (y'1 - y'2)] / [c * (x'1 - x'2) - a * (y'1 - y'2)] )

            double theta1Prime = Math.Atan2 ( 
                (d * (point1.X - point2.X) - b * (point1.Y - point2.Y)), 
                (c * (point1.X - point2.X) - a * (point1.Y - point2.Y)) 
            );

            //  Then we apply this to an above equation to solve for theta2':
            //    Q1 / 2 = cos(arctan(-Q2/Q1)) * sin(theta2')
            //  arctan(-Q2/Q1) gives us the angle of a triangle whose sides measure -Q2 and Q1, and
            //  cos(...) gives us the ratio of opposite : hypotenuse for that triangle. i.e.:
            //    cos(arctan(-Q2/Q1)) = Q1 / sqrt(Q1^2 + Q2^2)
            //  Then:
            //    Q1 / 2 = Q1 / sqrt(Q1^2 + Q2^2) * sin(theta2')
            //    sin(theta2') = sqrt(Q1^2 + Q2^2) / 2
            //    theta2' = arcsin(sqrt(Q1^2 + Q2^2) / 2)

            double theta2Prime = Math.Asin(Math.Sqrt(Q1*Q1 + Q2*Q2) / 2.0);

            // For some reason, Math.Asin doesn't accept 1.0 as an input
            if (Double.IsNaN(theta2Prime))
            {
                theta2Prime = Math.PI / 2.0;
            }

            //  Solve for theta1 and theta2 by adding/subtracting theta1' and theta2'
            //    theta1 = theta1' + theta2'
            //    theta2 = theta1' - theta2'

            double theta1 = theta1Prime + theta2Prime;
            double theta2 = theta1Prime - theta2Prime;

            // Finally, we can solve any of our original x' equations for center points Cx and Cy:
            //    Cx = x'1 - a * cos(theta1) - b * sin(theta1)
            //    Cy = y'1 - c * cos(theta1) - d * sin(theta1)

            double centerX = point1.X + a * Math.Cos(theta1) + b * Math.Sin(theta1);
            double centerY = point1.Y + c * Math.Cos(theta1) + d * Math.Sin(theta1);

            return new Point(centerX, centerY);
           */
        }

        public static Point RotatePoint(Point point, double angleInRadians, Point center)
        {
            double pointX = point.X - center.X;
            double pointY = point.Y - center.Y;
            double resultX = pointX * Math.Cos(angleInRadians) - pointY * Math.Sin(angleInRadians);
            double resultY = pointX * Math.Sin(angleInRadians) + pointY * Math.Cos(angleInRadians);
            resultX += center.X;
            resultY += center.Y;
            return new Point(resultX, resultY);
        }

        #endregion

        #region NDrawing

        Point placePoint;

        public override void Place(Point position)
        {
            placePoint = StartPoint = EndPoint = position;
            RX = RY = 0;
        }

        public override void Draw(Point point)
        {
            EndPoint = point;

            Vector delta = EndPoint - StartPoint;

            RX = RY = delta.Length / 2;

            TiltInRadians = Math.Atan2(delta.Y, delta.X) - Math.PI / 2.0;

            SweepDirection = SweepDirection.Clockwise;

            IsLargeArc = true;
        }

        protected override Rect Bounds
        {
            get { return Geometry.Bounds; }
        }

        public override void Normalize()
        {
            Point startPoint = StartPoint;
            Point endPoint = EndPoint;
            double radiusX = RX;
            double radiusY = RY;
            double tiltInDegrees = Tilt;
            SweepDirection sweepDirection = SweepDirection;
            bool isLargeArc = IsLargeArc;

            TransformArc(GeometryTransform, ref startPoint, ref endPoint, ref radiusX, ref radiusY, ref tiltInDegrees, ref sweepDirection, ref isLargeArc);

            StartPoint = startPoint;
            EndPoint = endPoint;
            RX = radiusX;
            RY = radiusY;
            Tilt = tiltInDegrees;
            SweepDirection = sweepDirection;
            IsLargeArc = isLargeArc;

            GeometryTransform = null;
        }

        public override int HandleCount
        {
            get { return 3; }
        }

        protected override Point GetHandleInternal(int index)
        {
            switch (index)
            {
                case 0:
                    return EndPoint;
                case 1:
                    return StartPoint;
                case 2:
                    {
                        Point center = Center, point;
                        if (SweepDirection == SweepDirection.Clockwise)
                            point = new Point(center.X + RX, center.Y);
                        else
                            point = new Point(center.X - RX, center.Y);
                        return RotatePoint(point, TiltInRadians, center);
                    }
                default:
                    return new Point();
            }
        }

        protected override void SetHandleInternal(int index, Point value)
        {

            switch (index)
            {
                case 0:
                    {
                        EndPoint = value;
                        Vector delta = EndPoint - StartPoint;
                        RY = delta.Length / 2;
                        TiltInRadians = Math.Atan2(delta.Y, delta.X) - Math.PI / 2;
                    }
                    break;
                case 1:
                    {
                        StartPoint = value;
                        Vector delta = EndPoint - StartPoint;
                        RY = delta.Length / 2;
                        TiltInRadians = Math.Atan2(delta.Y, delta.X) - Math.PI / 2;
                    }
                    break;
                case 2:
                    {
                        Point center = Center;
                        RX = RotatePoint(value, -TiltInRadians, center).X - center.X;
                        if (RX < 0)
                            SweepDirection = SweepDirection.Counterclockwise;
                        else
                            SweepDirection = SweepDirection.Clockwise;
                        RX = Math.Abs(RX);
                    }
                    break;
            }

        }

        public override PathGeometry ToPathGeometry()
        {
            var segment = new ArcSegment();
            segment.Point = EndPoint;
            segment.Size = new Size(RX, RY);
            segment.SweepDirection = SweepDirection;
            segment.RotationAngle = Tilt;
            segment.IsLargeArc = IsLargeArc;
            segment.IsStroked = true;

            var figure = new PathFigure();
            figure.StartPoint = StartPoint;
            figure.Segments.Add(segment);

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            return geometry;
        }

        public override StreamGeometry ToStreamGeometry()
        {
            var geometry = new StreamGeometry();
            var gc = geometry.Open();
            gc.BeginFigure(StartPoint, FillBrush != null, false);
            gc.ArcTo(EndPoint, new Size(RX, RY), Tilt, IsLargeArc, SweepDirection, StrokeBrush != null, false);
            gc.Close();

            return geometry;
        }

        protected override void OnRenderVisual(DrawingContext dc)
        {
            base.OnRenderVisual(dc);
        }

        #endregion

        #region ISvgSerializable

        public void WriteSvgTag(XmlWriter writer)
        {
            writer.WriteStartElement("path");
        }

        public void WriteSvgClass(XmlWriter writer)
        {
            writer.WriteAttributeString("class", "arc");
        }

        public static void TransformSegment(Point startPoint, ArcSegment segment, GeneralTransform transform)
        {
            Point endPoint = segment.Point;
            double radiusX = segment.Size.Width;
            double radiusY = segment.Size.Height;
            double tiltInDegrees = segment.RotationAngle;
            var sweepDirection = segment.SweepDirection;
            bool isLargeArc = segment.IsLargeArc;

            TransformArc(transform, ref startPoint, ref endPoint, ref radiusX, ref radiusY, ref tiltInDegrees, ref sweepDirection, ref isLargeArc);

            segment.Point = endPoint;
            segment.Size = new Size(radiusX, radiusY);
            segment.RotationAngle = tiltInDegrees;
            segment.SweepDirection = sweepDirection;
            segment.IsLargeArc = isLargeArc;
        }

        public static void TransformArc (
            GeneralTransform transform, 
            ref Point startPoint, 
            ref Point endPoint, 
            ref double radiusX, 
            ref double radiusY, 
            ref double tiltInDegrees, 
            ref SweepDirection sweepDirection, 
            ref bool isLargeArc)
        {
            double tiltInRadians = tiltInDegrees * Math.PI / 180.0;
            Point ellipseCenter = EllipseCenterFromPoints(startPoint, endPoint, radiusX, radiusY, tiltInRadians, isLargeArc, sweepDirection == SweepDirection.Clockwise);
            Point ellipseRight = RotatePoint(new Point(ellipseCenter.X + radiusX, ellipseCenter.Y), tiltInRadians, ellipseCenter);
            Point ellipseTop = RotatePoint(new Point(ellipseCenter.X, ellipseCenter.Y - radiusY), tiltInRadians, ellipseCenter);

            startPoint = transform.Transform(startPoint);
            endPoint = transform.Transform(endPoint);
            ellipseCenter = transform.Transform(ellipseCenter);
            ellipseRight = transform.Transform(ellipseRight);
            ellipseTop = transform.Transform(ellipseTop);

            Vector delta = ellipseRight - ellipseCenter;
            tiltInRadians = Math.Atan2(delta.Y, delta.X);
            tiltInDegrees = tiltInRadians * 180.0 / Math.PI;
            radiusX = (ellipseRight - ellipseCenter).Length;
            radiusY = (ellipseTop - ellipseCenter).Length;

            Point untiltedRight = RotatePoint(ellipseRight, -tiltInRadians, ellipseCenter);
            if (untiltedRight.X < ellipseCenter.X)
            {
                if (sweepDirection == SweepDirection.Clockwise)
                    sweepDirection = SweepDirection.Counterclockwise;
                else
                    sweepDirection = SweepDirection.Clockwise;
            }
        }

        public void WriteSvgAttributes(XmlWriter writer)
        {
            //base.WriteSvgAttributes(writer);

            Point startPoint = StartPoint;
            Point endPoint = EndPoint;
            double radiusX = RX;
            double radiusY = RY;
            double tiltInDegrees = Tilt;
            SweepDirection sweepDirection = SweepDirection;
            bool isLargeArc = IsLargeArc;

            TransformArc(GeometryTransform, ref startPoint, ref endPoint, ref radiusX, ref radiusY, ref tiltInDegrees, ref sweepDirection, ref isLargeArc);

            string data = String.Format(CultureInfo.InvariantCulture, "M{0},{1} A{2},{3} {4} {5} {6} {7},{8}", startPoint.X, startPoint.Y, radiusX, radiusY, tiltInDegrees, "1", ((sweepDirection == SweepDirection.Clockwise) ? "1" : "0"), endPoint.X, endPoint.Y);

            writer.WriteAttributeString("d", data);
        }

        public bool ReadSvgTag(XmlReader reader)
        {
            return (reader.Name == "path");
        }

        public bool ReadSvgClass(XmlReader reader)
        {
            return reader.GetAttribute("class").Contains("arc");
        }

        public bool ReadSvgAttributes(XmlReader reader)
        {
            //if (!base.ReadSvgAttributes(reader))
           // {
           //     return false;
            //}

            string data = reader.GetAttribute("d");
            if (data == null)
            {
                return false;
            }

            PathGeometry geometry;
            if (!NPath.TryParseGeometry(data, out geometry))
            {
                return false;
            }

            PathFigure figure = geometry.Figures.FirstOrDefault();
            if (figure == null)
            {
                return false;
            }

            var segment = figure.Segments.FirstOrDefault() as ArcSegment;
            if (segment == null)
            {
                return false;
            }

            StartPoint = figure.StartPoint;
            EndPoint = segment.Point;
            RX = segment.Size.Width;
            RY = segment.Size.Height;
            Tilt = segment.RotationAngle;
            SweepDirection = segment.SweepDirection;
            IsLargeArc = segment.IsLargeArc;

            return true;
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new Arc(this);
        }

        #endregion
    }
}
