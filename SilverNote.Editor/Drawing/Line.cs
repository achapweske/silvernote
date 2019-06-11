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
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using DOM;
using DOM.SVG;

namespace SilverNote.Editor
{
    public class Line : LineBase
    {
        #region Fields

        double _X1;
        double _Y1;
        double _X2;
        double _Y2;

        #endregion

        #region Constructors

        public Line()
        {
            Initialize();
        }

        public Line(double x1, double y1, double x2, double y2)
        {
            Initialize();

            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;

            StrokeBrush = Brushes.Black;
            StrokeWidth = 2;
        }

        public Line(Point point1, Point point2)
        {
            Initialize();

            X1 = point1.X;
            Y1 = point1.Y;
            X2 = point2.X;
            Y2 = point2.Y;

            StrokeBrush = Brushes.Black;
            StrokeWidth = 2;
        }

        public Line(Line copy)
            : base(copy)
        {
            X1 = copy.X1;
            Y1 = copy.Y1;
            X2 = copy.X2;
            Y2 = copy.Y2;

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

        #endregion

        #region NLineBase

        public override Point StartPoint
        {
            get 
            { 
                return new Point(X1, Y1); 
            }
            set 
            { 
                X1 = value.X; 
                Y1 = value.Y; 
            }
        }

        public override Point EndPoint
        {
            get 
            { 
                return new Point(X2, Y2); 
            }
            set
            {
                X2 = value.X;
                Y2 = value.Y;
            }
        }

        public override LineBase LineThumb
        {
            get
            {
                var result = (Line)base.LineThumb;
                result.X1 = 0;
                result.Y1 = 6;
                result.X2 = 100;
                result.Y2 = 6;
                result.GeometryTransform = null;
                return result;
            }
        }

        #endregion

        #region NDrawing

        public override Shape ThumbSmall
        {
            get
            {
                Line result = (Line)base.ThumbSmall;
                result.X1 = 0.5;
                result.Y1 = 15.5;
                result.X2 = 15.5;
                result.Y2 = 0.5;

                if (result.MarkerStart != null)
                {
                    result.MarkerStart = result.MarkerStart.ThumbSmallMarker;
                }

                if (result.MarkerEnd != null)
                {
                    result.MarkerEnd = result.MarkerEnd.ThumbSmallMarker;
                }

                result.GeometryTransform = null;
                return result;
            }
        }

        protected override Rect Bounds
        {
            get { return new Rect(new Point(X1, Y1), new Point(X2, Y2)); }
        }

        public override void Place(Point position)
        {
            X1 = X2 = position.X;
            Y1 = Y2 = position.Y;
        }

        public override void Draw(Point point)
        {
            X2 = point.X;
            Y2 = point.Y;
        }

        public override int HandleCount
        {
            get { return 2; }
        }

        protected override Point GetHandleInternal(int index)
        {
            switch (index)
            {
                case 0:
                    return new Point(X1, Y1);
                case 1:
                    return new Point(X2, Y2);
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
                default:
                    break;
            }
        }

        public override PathGeometry ToPathGeometry()
        {
            var segment = new LineSegment();
            segment.Point = new Point(X2, Y2);
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
            gc.LineTo(new Point(X2, Y2), StrokeBrush != null, false);
            gc.Close();

            return geometry;
        }

        protected override void OnRenderVisual(DrawingContext dc)
        {
            double zoom = NoteEditor.GetZoom(this);

            Point point1 = GeometryTransform.Transform(new Point(X1, Y1));
            Point point2 = GeometryTransform.Transform(new Point(X2, Y2));

            if (point1.X == point2.X)
            {
                point1.X = point2.X = Math.Floor(point2.X * zoom) / zoom;
            }

            if (point1.Y == point2.Y)
            {
                point1.Y = point2.Y = Math.Floor(point2.Y * zoom) / zoom;
            }

            Point render1 = point1;
            Point render2 = point2;

            if (MarkerStart != null)
            {
                double length = (point2 - point1).Length;
                render1 = Evaluate(point1, point2, StrokeWidth / length);
            }

            if (MarkerEnd != null)
            {
                double length = (point2 - point1).Length;
                render2 = Evaluate(point2, point1, StrokeWidth / length);
            }

            dc.DrawLine(StrokePen, render1, render2);

            if (StrokeWidth * zoom < 5)
            {
                Pen transparent = new Pen(Brushes.Transparent, 5 / zoom);

                dc.DrawLine(transparent, point1, point1);
            }

            if (X1 == X2 && Y1 == Y2)
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

                if (!Children.Contains(MarkerStart))
                {
                    Children.Add(MarkerStart);
                }
            }

            if (MarkerEnd != null)
            {
                MarkerEnd.Offset = new Vector(point2.X, point2.Y);
                MarkerEnd.Orient = Math.Atan2(point2.Y - point1.Y, point2.X - point1.X) * 180.0 / Math.PI;
                MarkerEnd.Brush = this.StrokeBrush;

                if (!Children.Contains(MarkerEnd))
                {
                    Children.Add(MarkerEnd);
                }
            }
        }

        #endregion

        #region INodeSource

        private readonly string _NodeName = SVGElements.NAMESPACE + ' ' + SVGElements.LINE;

        public override string GetNodeName(NodeContext context)
        {
            return _NodeName;
        }

        private readonly string[] _NodeAttributes = new string[] { 
            SVGAttributes.X1,
            SVGAttributes.Y1,
            SVGAttributes.X2,
            SVGAttributes.Y2,
            SVGAttributes.MARKER_START,
            SVGAttributes.MARKER_END
        };

        public override IList<string> GetNodeAttributes(ElementContext context)
        {
            return base.GetNodeAttributes(context).Concat(_NodeAttributes).ToArray();
        }

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.X1:
                    double x1 = Round(RenderedX1, -3);
                    return SVGFormatter.FormatLength(x1);
                case SVGAttributes.Y1:
                    double y1 = Round(RenderedY1, -3);
                    return SVGFormatter.FormatLength(y1);
                case SVGAttributes.X2:
                    double x2 = Round(RenderedX2, -3);
                    return SVGFormatter.FormatLength(x2);
                case SVGAttributes.Y2:
                    double y2 = Round(RenderedY2, -3);
                    return SVGFormatter.FormatLength(y2);
                case SVGAttributes.MARKER_START:
                    if (MarkerStart != null)
                        return Canvas.GetDefinitionURL(MarkerStart);
                    else
                        return null;
                case SVGAttributes.MARKER_END:
                    if (MarkerEnd != null)
                        return Canvas.GetDefinitionURL(MarkerEnd);
                    else
                        return null;
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        public override void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.X1:
                    var x1 = SVGParser.ParseLength(value);
                    if (x1 != null)
                        RenderedX1 = x1.Value;
                    break;
                case SVGAttributes.Y1:
                    var y1 = SVGParser.ParseLength(value);
                    if (y1 != null)
                        RenderedY1 = y1.Value;
                    break;
                case SVGAttributes.X2:
                    var x2 = SVGParser.ParseLength(value);
                    if (x2 != null)
                        RenderedX2 = x2.Value;
                    break;
                case SVGAttributes.Y2:
                    var y2 = SVGParser.ParseLength(value);
                    if (y2 != null)
                        RenderedY2 = y2.Value;
                    break;
                case SVGAttributes.MARKER_START:
                    MarkerStart = (Marker)Canvas.GetDefinition(value);
                    if (MarkerStart != null)
                        MarkerStart.InvalidateRender();
                    break;
                case SVGAttributes.MARKER_END:
                    MarkerEnd = (Marker)Canvas.GetDefinition(value);
                    if (MarkerEnd != null)
                        MarkerEnd.InvalidateRender();
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
                case SVGAttributes.X1:
                    X1 = 0;
                    break;
                case SVGAttributes.Y1:
                    Y1 = 0;
                    break;
                case SVGAttributes.X2:
                    X2 = 0;
                    break;
                case SVGAttributes.Y2:
                    Y2 = 0;
                    break;
                case SVGAttributes.MARKER_START:
                    MarkerStart = null;
                    break;
                case SVGAttributes.MARKER_END:
                    MarkerEnd = null;
                    break;
                default:
                    base.ResetNodeAttribute(context, name);
                    break;
            }
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new Line(this);
        }

        #endregion

        #region Algorithms

        /// <summary>
        /// Find the maximum squared error between a straight line and a set
        /// of sample points
        /// </summary>
        /// <param name="startPoint">start point</param>
        /// <param name="endPoint">end point</param>
        /// <param name="samples">Sample points</param>
        /// <returns>Maximum squared error</returns>
        public static double MaxError(Point startPoint, Point endPoint, IList<Point> samples)
        {
            double result = 0.0;

            for (int i = 1; i < samples.Count; i++)
            {
                var error = Error(startPoint, endPoint, samples[i]);
                result = Math.Max(error, result);
            }

            return result;
        }

        /// <summary>
        /// Compute the square distance between the given line segment and test point
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="testPoint"></param>
        /// <returns></returns>
        public static double Error(Point startPoint, Point endPoint, Point testPoint)
        {
            var delta = endPoint - startPoint;
            if (delta.Length == 0)
            {
                return (testPoint - startPoint).LengthSquared;
            }

            double t = ((testPoint.X - startPoint.X) * delta.X + (testPoint.Y - startPoint.Y) * delta.Y) / delta.LengthSquared;

            if (t < 0)
            {
                return (testPoint - startPoint).LengthSquared;
            }
            else if (t > 1)
            {
                return (testPoint - endPoint).LengthSquared;
            }
            else
            {
                var closestPoint = new Point(startPoint.X + t * delta.X, startPoint.Y + t * delta.Y);
                return (testPoint - closestPoint).LengthSquared;
            }
        }

        /// <summary>
        /// Evaluate a parametric line at offset t (0 <= t <= 1)
        /// </summary>
        public static Point Evaluate(Point p0, Point p1, double t)
        {
            return new Point(
                Evaluate(p0.X, p1.X, t),
                Evaluate(p0.Y, p1.Y, t)
            );
        }

        public static double Evaluate(double c0, double c1, double x)
        {
            // y = c0 + (c1 - c0)*x;

            return c0 + (c1 - c0) * x;
        }

        public static Tuple<Point, Point> AlignLine(Point point1, Point point2, double thickness, double scale = 1.0)
        {
            if (point1.X == point2.X)
            {
                point1 = LayoutHelper.Align(point1, thickness, scale);
                point2 = LayoutHelper.Align(point2, thickness, scale);

                if (point1.Y < point2.Y)
                {
                    point1.Y = Math.Floor(point1.Y * scale) / scale;
                    point2.Y = Math.Ceiling(point2.Y * scale) / scale;
                }
                else if (point1.Y > point2.Y)
                {
                    point1.Y = Math.Ceiling(point1.Y * scale) / scale;
                    point2.Y = Math.Floor(point2.Y * scale) / scale;
                }
            }

            if (point1.Y == point2.Y)
            {
                point1 = LayoutHelper.Align(point1, thickness, scale);
                point2 = LayoutHelper.Align(point2, thickness, scale);

                if (point1.X < point2.X)
                {
                    point1.X = Math.Floor(point1.X * scale) / scale;
                    point2.X = Math.Ceiling(point2.X * scale) / scale;
                }
                else if (point1.X > point2.X)
                {
                    point1.X = Math.Ceiling(point1.X * scale) / scale;
                    point2.X = Math.Floor(point2.X * scale) / scale;
                }
            }

            return new Tuple<Point, Point>(point1, point2);
        }

        #endregion
    }
}
