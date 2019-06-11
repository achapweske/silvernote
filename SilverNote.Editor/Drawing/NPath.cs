/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.SVG;
using SilverNote.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SilverNote.Editor
{
    public class NPath : Shape
    {
        #region Constructors

        /// <summary>
        /// Create an empty path
        /// </summary>
        public NPath()
        {
            Initialize();
        }

        /// <summary>
        /// Create a copy of the given path
        /// </summary>
        public NPath(NPath copy)
            : base(copy)
        {
            Initialize();

            Smoothness = copy.Smoothness;

            if (copy.Path != null)
            {
                Path = (PathGeometry)copy.Path.Clone();
            }
        }

        void Initialize()
        {
            Smoothness = 256;
            Path = new PathGeometry();
        }

        #endregion

        #region Properties

        public double Smoothness { get; set; }

        /// <summary>
        /// Get/set the PathGeometry describing this path
        /// </summary>
        public PathGeometry Path { get; set; }

        /// <summary>
        /// Get the PathFigure describing this path
        /// </summary>
        public PathFigure Figure
        {
            get 
            {
                if (Path.Figures.Count == 0)
                {
                    Path.Figures.Add(new PathFigure { IsClosed = false });
                }

                return Path.Figures.First(); 
            }
        }

        /// <summary>
        /// Get this path's start point
        /// </summary>
        public Point StartPoint
        {
            get { return Figure.StartPoint; }
            set { Figure.StartPoint = value; }
        }

        /// <summary>
        /// Get this path's end point
        /// </summary>
        public Point EndPoint
        {
            get
            {
                if (Figure.Segments.Count > 0)
                {
                    return GetEndPoint(Figure.Segments.Last());
                }
                else
                {
                    return StartPoint;
                }
            }
        }

        /// <summary>
        /// Get/set this path as a string.
        /// 
        /// Formatted as an SVG "d" attribute.
        /// </summary>
        public string Data
        {
            get
            {
                return FormatGeometry(Path);
            }
            set
            {
                PathGeometry geometry;
                if (TryParseGeometry(value, out geometry))
                {
                    Path = geometry;
                }
                else
                {
                    Path = new PathGeometry();
                }
            }
        }

        public PathGeometry RenderedPath
        {
            get
            {
                if (Path == null)
                {
                    return null;
                }

                if (GeometryTransform.Value.IsIdentity)
                {
                    return Path;
                }

                PathGeometry path = (PathGeometry)Path.Clone();
                path.Transform = null;
                TransformGeometry(path, GeometryTransform);

                return path;
            }
            set
            {
                if (value == null)
                {
                    Path = null;
                    return;
                }

                if (GeometryTransform.Value.IsIdentity)
                {
                    Path = value;
                    return;
                }

                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    PathGeometry path = (PathGeometry)value.Clone();
                    path.Transform = null;
                    TransformGeometry(path, inverse);
                    Path = path;
                }
            }
        }

        public string RenderedData
        {
            get
            {
                return FormatGeometry(RenderedPath);
            }
            set
            {
                PathGeometry geometry;
                if (TryParseGeometry(value, out geometry))
                {
                    RenderedPath = geometry;
                }
                else
                {
                    RenderedPath = new PathGeometry();
                }
            }
        }

        #endregion

        #region NDrawing

        /// <summary>
        /// Get a large thumbnail representation of this path
        /// </summary>
        public override Shape ThumbLarge
        {
            get
            {
                NPath result = (NPath)base.ThumbLarge;
                Rect bounds = result.RenderedBounds;

                if (bounds.Width >= bounds.Height)
                {
                    double aspectRatio = Math.Max(bounds.Height / bounds.Width, 0.5);
                    double width = 20;
                    double height = width * aspectRatio;
                    double offset = (20 - height) / 2;
                    result.RenderedBounds = new Rect(0, offset, width, height);
                }
                else
                {
                    double aspectRatio = Math.Max(bounds.Width / bounds.Height, 0.5);
                    double height = 20;
                    double width = height * aspectRatio;
                    double offset = (20 - width) / 2;
                    result.RenderedBounds = new Rect(offset, 0, width, height);
                }

                return result;
            }
        }

        /// <summary>
        /// Get a small thumbnail representation of this path
        /// </summary>
        public override Shape ThumbSmall
        {
            get
            {
                NPath result = (NPath)base.ThumbSmall;
                result.GeometryTransform = null;
                Rect bounds = result.RenderedBounds;

                if (bounds.Width >= bounds.Height)
                {
                    double aspectRatio = Math.Max(bounds.Height / bounds.Width, 0.5);
                    double width = 15;
                    double height = width * aspectRatio;
                    double offset = (15 - height) / 2;
                    result.RenderedBounds = new Rect(0.5, offset, width, height);
                }
                else
                {
                    double aspectRatio = Math.Max(bounds.Width / bounds.Height, 0.5);
                    double height = 15;
                    double width = height * aspectRatio;
                    double offset = (15 - width) / 2;
                    result.RenderedBounds = new Rect(offset, 0.5, width, height);
                }

                return result;
            }
        }

        SmartPencil _SmartPencil;

        /// <summary>
        /// Begin drawing this path at the given position
        /// </summary>
        public override void Place(Point position)
        {
            StartPoint = position;
        }

        /// <summary>
        /// Continue drawing this path at the given position
        /// </summary>
        public override void Draw(Point point)
        {
            // If first point being drawn, initialize _Points and compute _StartTangent

            if (_SmartPencil == null)
            {
                _SmartPencil = new SmartPencil(this);
                _SmartPencil.Smoothness = Smoothness;
                _SmartPencil.AddPoint(StartPoint);
            }

            _SmartPencil.AddPoint(point);            
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        protected override Rect Bounds
        {
            get { return Path.Bounds; }
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override void Normalize()
        {
            if (GeometryTransform != Transform.Identity)
            {
                Path.Transform = null;

                TransformGeometry(Path, GeometryTransform);

                GeometryTransform = Transform.Identity;
            }
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override int HandleCount
        {
            get { return GetHandleCount(Path); }
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        protected override Point GetHandleInternal(int index)
        {
            return GetHandle(Path, index);
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        protected override void SetHandleInternal(int index, Point value)
        {
            SetHandle(Path, index, value);
            Invalidate();
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override int SnapCount
        {
            get
            {
                // For an open path, snap to corners of bounding rectangle;
                // for closed path, also snap to start and end points

                if (Figure.IsClosed)
                {
                    return 4;
                }
                else
                {
                    return 6;
                }
            }
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override Point GetSnap(int index)
        {
            Point point = InternalGetSnap(index);

            return GeometryTransform.Transform(point);
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        protected virtual Point InternalGetSnap(int index)
        {
            switch (index)
            {
                case 0:
                    return Bounds.TopLeft;
                case 1:
                    return Bounds.TopRight;
                case 2:
                    return Bounds.BottomLeft;
                case 3:
                    return Bounds.BottomRight;
                case 4:
                    return StartPoint;
                case 5:
                    return EndPoint;
                default:
                    return new Point();
            }
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override PathGeometry ToPathGeometry()
        {
            return (PathGeometry)Path.Clone();
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override StreamGeometry ToStreamGeometry()
        {
            return base.ToStreamGeometry();
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        protected override void OnRenderVisual(DrawingContext dc)
        {
            if (Path != null)
            {
                double zoom = NoteEditor.GetZoom(this);

                // Transform and pixel-align this path

                var geometry = (PathGeometry)Path.Clone();
                geometry.Transform = GeometryTransform;
                AlignGeometry(geometry, StrokeWidth, zoom);
                geometry.Freeze();

                // Now actually draw the path

                dc.DrawGeometry(FillBrush, StrokePen, geometry);

                // Trace over the path with a transparent brush thick enough
                // to make the path easy to click on

                if (StrokeWidth * zoom < 5)
                {
                    var transparent = new Pen(Brushes.Transparent, 5 / zoom);

                    dc.DrawGeometry(null, transparent, geometry);
                }
            }
        }

        #endregion

        #region Join/Split

        /// <summary>
        /// Create a path resulting from joining the given set of drawings end-to-end.
        /// </summary>
        public static NPath Create(IEnumerable<Shape> drawings, double threshold = Double.NaN)
        {
            var first = drawings.FirstOrDefault();
            if (first == null)
            {
                return null;
            }

            if (Double.IsNaN(threshold))
            {
                threshold = first.StrokeWidth;
            }

            var result = new NPath
            {
                StrokeBrush = first.StrokeBrush,
                StrokeWidth = first.StrokeWidth,
                FillBrush = first.FillBrush,
                StrokeLineJoin = PenLineJoin.Round
            };

            if (result.Join(drawings, threshold))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Add a collection of drawings to this path by joining them end-to-end to this path
        /// </summary>
        public bool Join(IEnumerable<Shape> drawings, double threshold = 1.0)
        {
            foreach (var drawing in drawings)
            {
                if (!Join(drawing, threshold))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Add a drawing to this path by joining it end-to-end to this path
        /// </summary>
        public bool Join(Shape drawing, double threshold)
        {
            drawing.Normalize();

            var newPath = drawing.ToPathGeometry();
            if (newPath == null)
            {
                return false;
            }

            // (Each drawing's stroke must match our stroke)

            if (Shape.CompareBrushes(this.StrokeBrush, drawing.StrokeBrush) &&
                this.StrokeWidth == drawing.StrokeWidth)
            {
                Join(this.Path.Figures, newPath.Figures, threshold);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Insert all segments from newFigures into the given collection of figures
        /// by joining each figure end-to-end with this figure
        /// </summary>
        public static void Join(PathFigureCollection figures, IEnumerable<PathFigure> newFigures, double threshold)
        {
            foreach (var newFigure in newFigures)
            {
                Join(figures, newFigure, threshold);
            }
        }

        /// <summary>
        /// Insert all segments from newFigure into the given collection of figures
        /// by joining each figure end-to-end with this figure
        /// </summary>
        public static void Join(PathFigureCollection figures, PathFigure newFigure, double threshold)
        {
            // Find a figure whose end point touches the start point of newFigure

            foreach (var figure in figures)
            {
                if (figure != newFigure)
                {
                    double distance = (newFigure.StartPoint - GetEndPoint(figure)).Length;
                    if (distance <= threshold)
                    {
                        // Success - append new segments to figure
                        figure.Segments.AddRange(newFigure.Segments);
                        figures.Remove(newFigure);
                        Join(figures, figure, threshold);
                        return;
                    }
                }
            }

            // Find a figure whose start point touches the end point of newFigure

            foreach (var figure in figures)
            {
                if (figure != newFigure)
                {
                    double distance = (figure.StartPoint - GetEndPoint(newFigure)).Length;
                    if (distance <= threshold)
                    {
                        // Success - prepend new segments to figure
                        figure.StartPoint = newFigure.StartPoint;
                        figure.Segments.InsertRange(0, newFigure.Segments);
                        figures.Remove(newFigure);
                        Join(figures, figure, threshold);
                        return;
                    }
                }
            }

            // Find a figure whose end point touches the end point of newFigure

            foreach (var figure in figures)
            {
                if (figure != newFigure)
                {
                    double distance = (GetEndPoint(figure) - GetEndPoint(newFigure)).Length;
                    if (distance <= threshold)
                    {
                        // Success - append new segments (reversed) to figure
                        newFigure = Reverse(newFigure);
                        figure.Segments.AddRange(newFigure.Segments);
                        figures.Remove(newFigure);
                        Join(figures, figure, threshold);
                        return;
                    }
                }
            }

            // Find a figure whose start point touches new figure's start point

            foreach (var figure in figures)
            {
                if (figure != newFigure)
                {
                    double distance = (figure.StartPoint - newFigure.StartPoint).Length;
                    if (distance <= threshold)
                    {
                        // Success - prepend new segments (reversed) to figure
                        newFigure = Reverse(newFigure);
                        figure.StartPoint = newFigure.StartPoint;
                        figure.Segments.InsertRange(0, newFigure.Segments);
                        figures.Remove(newFigure);
                        Join(figures, figure, threshold);
                        return;
                    }
                }
            }

            // Determine if this is a closed figure

            if ((newFigure.StartPoint - GetEndPoint(newFigure)).Length <= threshold)
            {
                newFigure.IsClosed = true;
            }

            if (!figures.Contains(newFigure))
            {
                figures.Add(newFigure);
            }
        }

        /// <summary>
        /// Split this path into its constituent segments
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Shape> Split()
        {
            var drawings = GetDrawings(Path);

            foreach (var drawing in drawings)
            {
                drawing.StrokeWidth = this.StrokeWidth;
                drawing.StrokeBrush = this.StrokeBrush;
                drawing.StrokeLineCap = this.StrokeLineCap;
                drawing.StrokeDashArray = this.StrokeDashArray;
                drawing.GeometryTransform = this.GeometryTransform;
            }

            Path.Figures.Clear();

            return drawings;
        }

        #endregion

        #region Segment Operations

        #region GetHandleCount

        private static int GetHandleCount(PathGeometry geometry)
        {
            int result = 0;

            foreach (var figure in geometry.Figures)
            {
                result += GetHandleCount(figure);
            }

            return result;
        }

        private static int GetHandleCount(PathFigure figure)
        {
            int result = 0;

            if (!figure.IsClosed || figure.StartPoint != GetEndPoint(figure))
            {
                result += 1;
            }

            foreach (var segment in figure.Segments)
            {
                result += GetHandleCount(segment);
            }

            return result;
        }

        private static int GetHandleCount(PathSegment segment)
        {
            if (segment is ArcSegment)
            {
                return 1;
            }
            else if (segment is BezierSegment)
            {
                return 3;
            }
            else if (segment is LineSegment)
            {
                return 1;
            }
            else if (segment is PolyBezierSegment)
            {
                return ((PolyBezierSegment)segment).Points.Count;
            }
            else if (segment is PolyLineSegment)
            {
                return ((PolyLineSegment)segment).Points.Count;
            }
            else if (segment is PolyQuadraticBezierSegment)
            {
                return ((PolyQuadraticBezierSegment)segment).Points.Count;
            }
            else if (segment is QuadraticBezierSegment)
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region GetHandle

        static Point GetHandle(PathGeometry geometry, int index)
        {
            foreach (var figure in geometry.Figures)
            {
                int figureHandleCount = GetHandleCount(figure);

                if (index < figureHandleCount)
                {
                    return GetHandle(figure, index);
                }

                index -= figureHandleCount;
            }

            return new Point();
        }

        static Point GetHandle(PathFigure figure, int index)
        {
            bool isClosed = figure.IsClosed && figure.StartPoint == GetEndPoint(figure);
            if (!isClosed)
            {
                if (index == 0)
                {
                    return figure.StartPoint;
                }

                index -= 1;
            }

            var segmentStartPoint = figure.StartPoint;

            foreach (var segment in figure.Segments)
            {
                int segmentHandleCount = GetHandleCount(segment);

                if (index < segmentHandleCount)
                {
                    return GetHandle(segmentStartPoint, segment, index);
                }

                segmentStartPoint = GetEndPoint(segment);
                index -= segmentHandleCount;
            }

            return new Point();
        }

        static Point GetHandle(Point startPoint, PathSegment segment, int index)
        {
            if (segment is ArcSegment)
            {
                return GetArcHandle(startPoint, (ArcSegment)segment, index);
            }
            else if (segment is BezierSegment)
            {
                return GetBezierHandle(startPoint, (BezierSegment)segment, index);
            }
            else if (segment is LineSegment)
            {
                return GetLineHandle(startPoint, (LineSegment)segment, index);
            }
            else if (segment is PolyBezierSegment)
            {
                return GetPolyBezierHandle(startPoint, (PolyBezierSegment)segment, index);
            }
            else if (segment is PolyLineSegment)
            {
                return GetPolyLineHandle(startPoint, (PolyLineSegment)segment, index);
            }
            else if (segment is PolyQuadraticBezierSegment)
            {
                return GetPolyQuadraticBezierHandle(startPoint, (PolyQuadraticBezierSegment)segment, index);
            }
            else if (segment is QuadraticBezierSegment)
            {
                return GetQuadraticBezierHandle(startPoint, (QuadraticBezierSegment)segment, index);
            }
            else
            {
                return new Point();
            }
        }

        static Point GetArcHandle(Point startPoint, ArcSegment segment, int index)
        {
            return segment.Point;
        }

        static Point GetBezierHandle(Point startPoint, BezierSegment segment, int index)
        {
            if (index == 0)
            {
                return CubicBezier.Evaluate(startPoint, segment.Point1, segment.Point2, segment.Point3, 1.0 / 3.0);
            }
            else if (index == 1)
            {
                return CubicBezier.Evaluate(startPoint, segment.Point1, segment.Point2, segment.Point3, 2.0 / 3.0);
            }
            else
            {
                return segment.Point3;
            }
        }

        static Point GetLineHandle(Point startPoint, LineSegment segment, int index)
        {
            return segment.Point;
        }

        static Point GetPolyBezierHandle(Point startPoint, PolyBezierSegment segment, int index)
        {
            if (index % 3 == 0)
            {
                var p0 = (index > 0) ? segment.Points[index - 1] : startPoint;
                var p1 = segment.Points[index];
                var p2 = segment.Points[index + 1];
                var p3 = segment.Points[index + 2];

                return CubicBezier.Evaluate(p0, p1, p2, p3, 1.0 / 3.0);

            }
            else if (index % 3 == 1)
            {
                var p0 = (index > 1) ? segment.Points[index - 2] : startPoint;
                var p1 = segment.Points[index - 1];
                var p2 = segment.Points[index];
                var p3 = segment.Points[index + 1];

                return CubicBezier.Evaluate(p0, p1, p2, p3, 2.0 / 3.0);
            }
            else
            {
                return segment.Points[index];
            }
        }

        static Point GetPolyLineHandle(Point startPoint, PolyLineSegment segment, int index)
        {
            return segment.Points[index];
        }

        static Point GetPolyQuadraticBezierHandle(Point startPoint, PolyQuadraticBezierSegment segment, int index)
        {
            if (index % 2 == 0)
            {
                var p0 = (index > 0) ? segment.Points[index - 1] : startPoint;
                var p1 = segment.Points[index];
                var p2 = segment.Points[index + 1];

                return QuadraticBezier.Evaluate(p0, p1, p2, 0.5);
            }
            else
            {
                return segment.Points[index];
            }
        }

        static Point GetQuadraticBezierHandle(Point startPoint, QuadraticBezierSegment segment, int index)
        {
            if (index == 0)
            {
                return QuadraticBezier.Evaluate(startPoint, segment.Point1, segment.Point2, 0.5);
            }
            else
            {
                return segment.Point2;
            }
        }

        #endregion

        #region SetHandle

        static void SetHandle(PathGeometry geometry, int index, Point newValue)
        {
            foreach (var figure in geometry.Figures)
            {
                int figureHandleCount = GetHandleCount(figure);

                if (index < figureHandleCount)
                {
                    SetHandle(figure, index, newValue);
                    return;
                }

                index -= figureHandleCount;
            }
        }

        static void SetHandle(PathFigure figure, int index, Point newValue)
        {
            bool isClosed = figure.IsClosed && figure.StartPoint == GetEndPoint(figure);

            if (!isClosed)
            {
                if (index == 0)
                {
                    figure.StartPoint = newValue;
                    return;
                }

                index -= 1;
            }

            var segmentStartPoint = figure.StartPoint;

            for (int i = 0; i < figure.Segments.Count; i++)
            {
                var segment = figure.Segments[i];

                int segmentHandleCount = GetHandleCount(segment);

                if (index < segmentHandleCount)
                {
                    SetHandle(segmentStartPoint, segment, index, newValue);

                    if (isClosed && i == figure.Segments.Count - 1 && index == segmentHandleCount - 1)
                    {
                        figure.StartPoint = newValue;
                    }
                    break;
                }

                segmentStartPoint = GetEndPoint(segment);

                index -= segmentHandleCount;
            }
        }

        static void SetHandle(Point startPoint, PathSegment segment, int index, Point newValue)
        {
            if (segment is ArcSegment)
            {
                SetArcHandle(startPoint, (ArcSegment)segment, index, newValue);
            }
            else if (segment is BezierSegment)
            {
                SetBezierHandle(startPoint, (BezierSegment)segment, index, newValue);
            }
            else if (segment is LineSegment)
            {
                SetLineHandle(startPoint, (LineSegment)segment, index, newValue);
            }
            else if (segment is PolyBezierSegment)
            {
                SetPolyBezierHandle(startPoint, (PolyBezierSegment)segment, index, newValue);
            }
            else if (segment is PolyLineSegment)
            {
                SetPolyLineHandle(startPoint, (PolyLineSegment)segment, index, newValue);
            }
            else if (segment is PolyQuadraticBezierSegment)
            {
                SetPolyQuadraticBezierHandle(startPoint, (PolyQuadraticBezierSegment)segment, index, newValue);
            }
            else if (segment is QuadraticBezierSegment)
            {
                SetQuadraticBezierHandle(startPoint, (QuadraticBezierSegment)segment, index, newValue);
            }
            else
            {

            }
        }

        static void SetArcHandle(Point startPoint, ArcSegment segment, int index, Point newValue)
        {
            segment.Point = newValue;
        }

        static void SetBezierHandle(Point startPoint, BezierSegment segment, int index, Point newValue)
        {
            if (index == 0)
            {
                segment.Point1 = CubicBezier.P1(newValue, startPoint, segment.Point2, segment.Point3, 1.0 / 3.0);
            }
            else if (index == 1)
            {
                segment.Point2 = CubicBezier.P2(newValue, startPoint, segment.Point1, segment.Point3, 2.0 / 3.0);
            }
            else
            {
                segment.Point3 = newValue;
            }
        }

        static void SetLineHandle(Point startPoint, LineSegment segment, int index, Point newValue)
        {
            segment.Point = newValue;
        }

        static void SetPolyBezierHandle(Point startPoint, PolyBezierSegment segment, int index, Point newValue)
        {
            if (index % 3 == 0)
            {
                var p0 = (index > 0) ? segment.Points[index - 1] : startPoint;
                var p2 = segment.Points[index + 1];
                var p3 = segment.Points[index + 2];

                segment.Points[index] = CubicBezier.P1(newValue, p0, p2, p3, 1.0 / 3.0);

            }
            else if (index % 3 == 1)
            {
                var p0 = (index > 1) ? segment.Points[index - 2] : startPoint;
                var p1 = segment.Points[index - 1];
                var p3 = segment.Points[index + 1];

                segment.Points[index] = CubicBezier.P2(newValue, p0, p1, p3, 2.0 / 3.0);
            }
            else
            {
                segment.Points[index] = newValue;
            }

        }

        static void SetPolyLineHandle(Point startPoint, PolyLineSegment segment, int index, Point newValue)
        {
            segment.Points[index] = newValue;
        }

        static void SetPolyQuadraticBezierHandle(Point startPoint, PolyQuadraticBezierSegment segment, int index, Point newValue)
        {
            if (index % 2 == 0)
            {
                var p0 = (index > 0) ? segment.Points[index - 1] : startPoint;
                var p1 = segment.Points[index];
                var p2 = segment.Points[index + 1];

                segment.Points[index] = QuadraticBezier.P1(newValue, p0, p2, 0.5);
            }
            else
            {
                segment.Points[index] = newValue;
            }
        }

        static void SetQuadraticBezierHandle(Point startPoint, QuadraticBezierSegment segment, int index, Point newValue)
        {
            if (index == 0)
            {
                segment.Point1 = QuadraticBezier.P1(newValue, startPoint, segment.Point2, 0.5);
            }
            else
            {
                segment.Point2 = newValue;
            }
        }

        #endregion

        #region GetEndPoint

        /// <summary>
        /// Get a PathFigure's end point
        /// </summary>
        public static Point GetEndPoint(PathFigure figure)
        {
            if (figure.Segments.Count > 0)
            {
                return NPath.GetEndPoint(figure.Segments.Last());
            }
            else
            {
                return new Point();
            }
        }

        /// <summary>
        /// Get a PathSegment's end point
        /// </summary>
        public static Point GetEndPoint(PathSegment segment)
        {
            if (segment is ArcSegment)
            {
                return ((ArcSegment)segment).Point;
            }
            else if (segment is BezierSegment)
            {
                return ((BezierSegment)segment).Point3;
            }
            else if (segment is LineSegment)
            {
                return ((LineSegment)segment).Point;
            }
            else if (segment is PolyBezierSegment)
            {
                return ((PolyBezierSegment)segment).Points.LastOrDefault();
            }
            else if (segment is PolyLineSegment)
            {
                return ((PolyLineSegment)segment).Points.LastOrDefault();
            }
            else if (segment is PolyQuadraticBezierSegment)
            {
                return ((PolyQuadraticBezierSegment)segment).Points.LastOrDefault();
            }
            else if (segment is QuadraticBezierSegment)
            {
                return ((QuadraticBezierSegment)segment).Point2;
            }
            else
            {
                return new Point();
            }
        }

        #endregion

        #region Reverse

        /// <summary>
        /// Reverse all segments in a PathFigure such that the original 
        /// StartPoint becomes the final EndPoint and vice versa
        /// </summary>
        public static PathFigure Reverse(PathFigure figure)
        {
            var newFigure = new PathFigure();

            Point startPoint = figure.StartPoint;

            foreach (var segment in figure.Segments)
            {
                var endPoint = GetEndPoint(segment);

                var newSegment = Reverse(startPoint, segment);

                newFigure.Segments.Insert(0, newSegment);

                startPoint = endPoint;
            }

            newFigure.StartPoint = startPoint;

            return newFigure;
        }

        /// <summary>
        /// Reverse the given PathSegment such that its original start point 
        /// becomes its final end point and vice versa
        /// </summary>
        public static PathSegment Reverse(Point startPoint, PathSegment segment)
        {
            if (segment is ArcSegment)
            {
                return ReverseArc(startPoint, (ArcSegment)segment);
            }
            else if (segment is BezierSegment)
            {
                return ReverseBezier(startPoint, (BezierSegment)segment);
            }
            else if (segment is LineSegment)
            {
                return ReverseLine(startPoint, (LineSegment)segment);
            }
            else if (segment is PolyBezierSegment)
            {
                return ReversePolyBezier(startPoint, (PolyBezierSegment)segment);
            }
            else if (segment is PolyLineSegment)
            {
                return ReversePolyLine(startPoint, (PolyLineSegment)segment);
            }
            else if (segment is PolyQuadraticBezierSegment)
            {
                return ReversePolyQuadraticBezier(startPoint, (PolyQuadraticBezierSegment)segment);
            }
            else if (segment is QuadraticBezierSegment)
            {
                return ReverseQuadraticBezier(startPoint, (QuadraticBezierSegment)segment);
            }
            else
            {
                return null;
            }
        }

        public static PathSegment ReverseArc(Point startPoint, ArcSegment segment)
        {
            var result = (ArcSegment)segment.Clone();

            result.Point = startPoint;
            
            if (segment.SweepDirection == SweepDirection.Clockwise)
            {
                result.SweepDirection = SweepDirection.Counterclockwise;
            }
            else
            {
                result.SweepDirection = SweepDirection.Clockwise;
            }

            return result;
        }

        public static PathSegment ReverseBezier(Point startPoint, BezierSegment segment)
        {
            var result = (BezierSegment)segment.Clone();

            result.Point1 = segment.Point2;
            result.Point2 = segment.Point1;
            result.Point3 = startPoint;

            return result;
        }

        public static PathSegment ReverseLine(Point startPoint, LineSegment segment)
        {
            var result = (LineSegment)segment.Clone();

            result.Point = startPoint;

            return result;
        }

        public static PathSegment ReversePolyBezier(Point startPoint, PolyBezierSegment segment)
        {
            var result = (PolyBezierSegment)segment.Clone();

            result.Points.Clear();

            for (int i = segment.Points.Count - 2; i >= 0; i--)
            {
                result.Points.Add(segment.Points[i]);
            }

            result.Points.Add(startPoint);

            return result;
        }

        public static PathSegment ReversePolyLine(Point startPoint, PolyLineSegment segment)
        {
            var result = (PolyLineSegment)segment.Clone();

            result.Points.Clear();

            for (int i = segment.Points.Count - 2; i >= 0; i--)
            {
                result.Points.Add(segment.Points[i]);
            }

            result.Points.Add(startPoint);

            return result;

        }

        public static PathSegment ReversePolyQuadraticBezier(Point startPoint, PolyQuadraticBezierSegment segment)
        {
            var result = (PolyQuadraticBezierSegment)segment.Clone();

            result.Points.Clear();

            for (int i = segment.Points.Count - 2; i >= 0; i--)
            {
                result.Points.Add(segment.Points[i]);
            }

            result.Points.Add(startPoint);

            return result;

        }

        public static PathSegment ReverseQuadraticBezier(Point startPoint, QuadraticBezierSegment segment)
        {
            var result = (QuadraticBezierSegment)segment.Clone();

            result.Point2 = startPoint;

            return result;
        }

        #endregion

        #region Transform

        /// <summary>
        /// Apply the given transform to a PathGeometry
        /// </summary>
        public static void TransformGeometry(PathGeometry geometry, GeneralTransform transform)
        {
            foreach (var figure in geometry.Figures)
            {
                TransformFigure(figure, transform);
            }
        }

        /// <summary>
        /// Apply the given transform to a PathFigure
        /// </summary>
        private static void TransformFigure(PathFigure figure, GeneralTransform transform)
        {
            var startPoint = figure.StartPoint;

            figure.StartPoint = transform.Transform(figure.StartPoint);

            foreach (var segment in figure.Segments)
            {
                var endPoint = GetEndPoint(segment);

                TransformSegment(startPoint, segment, transform);

                startPoint = endPoint;
            }
        }

        /// <summary>
        /// Apply the given transform to a PathSegment
        /// </summary>
        private static void TransformSegment(Point startPoint, PathSegment segment, GeneralTransform transform)
        {
            if (segment is ArcSegment)
            {
                Arc.TransformSegment(startPoint, (ArcSegment)segment, transform);
            }
            else if (segment is BezierSegment)
            {
                TransformBezier(startPoint, (BezierSegment)segment, transform);
            }
            else if (segment is LineSegment)
            {
                TransformLine(startPoint, (LineSegment)segment, transform);
            }
            else if (segment is PolyBezierSegment)
            {
                TransformPolyBezier(startPoint, (PolyBezierSegment)segment, transform);
            }
            else if (segment is PolyLineSegment)
            {
                TransformPolyLine(startPoint, (PolyLineSegment)segment, transform);
            }
            else if (segment is PolyQuadraticBezierSegment)
            {
                TransformPolyQuadraticBezier(startPoint, (PolyQuadraticBezierSegment)segment, transform);
            }
            else if (segment is QuadraticBezierSegment)
            {
                TransformQuadraticBezier(startPoint, (QuadraticBezierSegment)segment, transform);
            }
            else
            {

            }
        }

        /// <summary>
        /// Apply the given transform to a BezierSegment
        /// </summary>
        static void TransformBezier(Point startPoint, BezierSegment segment, GeneralTransform transform)
        {
            segment.Point1 = transform.Transform(segment.Point1);
            segment.Point2 = transform.Transform(segment.Point2);
            segment.Point3 = transform.Transform(segment.Point3);
        }

        /// <summary>
        /// Apply the given transform to a LineSegment
        /// </summary>
        static void TransformLine(Point startPoint, LineSegment segment, GeneralTransform transform)
        {
            segment.Point = transform.Transform(segment.Point);
        }

        /// <summary>
        /// Apply the given transform to a PolyBezierSegment
        /// </summary>
        static void TransformPolyBezier(Point startPoint, PolyBezierSegment segment, GeneralTransform transform)
        {
            for (int i = 0; i < segment.Points.Count; i++)
            {
                segment.Points[i] = transform.Transform(segment.Points[i]);
            }
        }

        /// <summary>
        /// Apply the given transform to a PolyLineSegment
        /// </summary>
        static void TransformPolyLine(Point startPoint, PolyLineSegment segment, GeneralTransform transform)
        {
            for (int i = 0; i < segment.Points.Count; i++)
            {
                segment.Points[i] = transform.Transform(segment.Points[i]);
            }
        }

        /// <summary>
        /// Apply the given transform to a PolyQuadraticBezierSegment
        /// </summary>
        static void TransformPolyQuadraticBezier(Point startPoint, PolyQuadraticBezierSegment segment, GeneralTransform transform)
        {
            for (int i = 0; i < segment.Points.Count; i++)
            {
                segment.Points[i] = transform.Transform(segment.Points[i]);
            }
        }

        /// <summary>
        /// Apply the given transform to a QuadraticBezierSegment
        /// </summary>
        static void TransformQuadraticBezier(Point startPoint, QuadraticBezierSegment segment, GeneralTransform transform)
        {
            segment.Point1 = transform.Transform(segment.Point1);
            segment.Point2 = transform.Transform(segment.Point2);
        }

        #endregion

        #region Align

        /// <summary>
        /// Align each point within a PathGeometry to pixel boundaries
        /// </summary>
        public static void AlignGeometry(PathGeometry geometry, double thickness, double scale)
        {
            foreach (var figure in geometry.Figures)
            {
                AlignFigure(figure, thickness, scale);
            }
        }

        /// <summary>
        /// Align each point within a PathFigure to pixel boundaries
        /// </summary>
        public static void AlignFigure(PathFigure figure, double thickness, double scale)
        {
            var startPoint = figure.StartPoint;

            //startPoint = LayoutHelper.AlignPoint(startPoint, thickness, scale);

            var delta = startPoint - figure.StartPoint;

            figure.StartPoint = startPoint;

            foreach (var segment in figure.Segments)
            {
                var endPoint = GetEndPoint(segment);

                AlignSegment(startPoint, segment, thickness, scale, delta);

                startPoint = GetEndPoint(segment);

                delta = startPoint - endPoint;
            }
        }

        /// <summary>
        /// Align each point within a PathSegment to pixel boundaries
        /// </summary>
        public static void AlignSegment(Point startPoint, PathSegment segment, double thickness, double scale, Vector delta)
        {
            if (segment is ArcSegment)
            {
                AlignArc(startPoint, (ArcSegment)segment, thickness, scale, delta);
            }
            else if (segment is BezierSegment)
            {
                AlignBezier(startPoint, (BezierSegment)segment, thickness, scale);
            }
            else if (segment is LineSegment)
            {
                AlignLine(startPoint, (LineSegment)segment, thickness, scale, delta);
            }
            else if (segment is PolyBezierSegment)
            {
                AlignPolyBezier(startPoint, (PolyBezierSegment)segment, thickness, scale);
            }
            else if (segment is PolyLineSegment)
            {
                AlignPolyLine(startPoint, (PolyLineSegment)segment, thickness, scale);
            }
            else if (segment is PolyQuadraticBezierSegment)
            {
                AlignPolyQuadraticBezier(startPoint, (PolyQuadraticBezierSegment)segment, thickness, scale);
            }
            else if (segment is QuadraticBezierSegment)
            {
                AlignQuadraticBezier(startPoint, (QuadraticBezierSegment)segment, thickness, scale);
            }
            else
            {

            }
        }

        static void AlignArc(Point startPoint, ArcSegment segment, double thickness, double scale, Vector delta)
        {
            //segment.Point = segment.Point + delta;
            segment.IsSmoothJoin = true;
        }

        static void AlignBezier(Point startPoint, BezierSegment segment, double thickness, double scale)
        {

        }

        static void AlignLine(Point startPoint, LineSegment segment, double thickness, double scale, Vector delta)
        {
            //segment.Point = LayoutHelper.AlignPoint(segment.Point, thickness, scale);
        }

        static void AlignPolyBezier(Point startPoint, PolyBezierSegment segment, double thickness, double scale)
        {

        }

        static void AlignPolyLine(Point startPoint, PolyLineSegment segment, double thickness, double scale)
        {

        }

        static void AlignPolyQuadraticBezier(Point startPoint, PolyQuadraticBezierSegment segment, double thickness, double scale)
        {

        }

        static void AlignQuadraticBezier(Point startPoint, QuadraticBezierSegment segment, double thickness, double scale)
        {

        }


        #endregion

        #region GetDrawings

        /// <summary>
        /// Get a set of drawings representing the constituent segments of the given PathGeometry
        /// </summary>
        static IEnumerable<Shape> GetDrawings(PathGeometry geometry)
        {
            var results = new List<Shape>();

            foreach (var figure in geometry.Figures)
            {
                var drawings = GetDrawings(figure);

                results.AddRange(drawings);
            }

            return results;
        }

        /// <summary>
        /// Get a set of drawings representing the constituent segments of the given PathFigure
        /// </summary>
        static IEnumerable<Shape> GetDrawings(PathFigure figure)
        {
            var results = new List<Shape>();

            var startPoint = figure.StartPoint;

            foreach (var segment in figure.Segments)
            {
                var drawings = GetDrawings(startPoint, segment);

                results.AddRange(drawings);

                startPoint = GetEndPoint(segment);
            }

            return results;
        }

        /// <summary>
        /// Get a set of drawings representing the given PathSegment
        /// </summary>
        static IEnumerable<Shape> GetDrawings(Point startPoint, PathSegment segment)
        {
            if (segment is ArcSegment)
            {
                return GetArc(startPoint, (ArcSegment)segment);
            }
            else if (segment is BezierSegment)
            {
                return GetBezier(startPoint, (BezierSegment)segment);
            }
            else if (segment is LineSegment)
            {
                return GetLine(startPoint, (LineSegment)segment);
            }
            else if (segment is PolyBezierSegment)
            {
                return GetPolyBezier(startPoint, (PolyBezierSegment)segment);
            }
            else if (segment is PolyLineSegment)
            {
                return GetPolyLine(startPoint, (PolyLineSegment)segment);
            }
            else if (segment is PolyQuadraticBezierSegment)
            {
                return GetPolyQuadraticBezier(startPoint, (PolyQuadraticBezierSegment)segment);
            }
            else if (segment is QuadraticBezierSegment)
            {
                return GetQuadraticBezier(startPoint, (QuadraticBezierSegment)segment);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get an NDrawing representing the given ArcSegment
        /// </summary>
        static IEnumerable<Shape> GetArc(Point startPoint, ArcSegment segment)
        {
            yield return new Arc(startPoint, segment.Point, segment.Size, segment.RotationAngle, segment.IsLargeArc, segment.SweepDirection);
        }

        /// <summary>
        /// Get an NDrawing representing the given BezierSegment
        /// </summary>
        static IEnumerable<Shape> GetBezier(Point startPoint, BezierSegment segment)
        {
            yield return new CubicBezier(startPoint, segment.Point1, segment.Point2, segment.Point3);
        }

        /// <summary>
        /// Get an NDrawing representing the given LineSegment
        /// </summary>
        static IEnumerable<Shape> GetLine(Point startPoint, LineSegment segment)
        {
            yield return new Line(startPoint, segment.Point);
        }

        /// <summary>
        /// Get an NDrawing representing the given PolyBezierSegment
        /// </summary>
        static IEnumerable<Shape> GetPolyBezier(Point startPoint, PolyBezierSegment segment)
        {
            for (int i = 0; i + 2 < segment.Points.Count; i += 3)
            {
                var point1 = startPoint;
                var point2 = segment.Points[i];
                var point3 = segment.Points[i + 1];
                var point4 = segment.Points[i + 2];

                yield return new CubicBezier(point1, point2, point3, point4);

                startPoint = point4;
            }
        }

        /// <summary>
        /// Get an NDrawing representing the given PolyLineSegment
        /// </summary>
        static IEnumerable<Shape> GetPolyLine(Point startPoint, PolyLineSegment segment)
        {
            foreach (var point in segment.Points)
            {
                yield return new Line(startPoint, point);

                startPoint = point;
            }
        }

        /// <summary>
        /// Get an NDrawing representing the given PolyQuadraticBezierSegment
        /// </summary>
        static IEnumerable<Shape> GetPolyQuadraticBezier(Point startPoint, PolyQuadraticBezierSegment segment)
        {
            for (int i = 0; i + 1 < segment.Points.Count; i += 2)
            {
                var point1 = startPoint;
                var point2 = segment.Points[i];
                var point3 = segment.Points[i + 1];

                yield return new QuadraticCurve(point1, point2, point3);

                startPoint = point3;
            }
        }

        /// <summary>
        /// Get an NDrawing representing the given QuadraticBezierSegment
        /// </summary>
        static IEnumerable<Shape> GetQuadraticBezier(Point startPoint, QuadraticBezierSegment segment)
        {
            yield return new QuadraticCurve(startPoint, segment.Point1, segment.Point2);
        }

        #endregion

        #region Parse

        /// <summary>
        /// Parse a SVG representation of a PathGeometry.
        /// 
        /// Throws an exception on error
        /// </summary>
        public static PathGeometry ParseGeometry(string str)
        {
            using (var reader = new StringReader(str))
            {
                return ParseGeometry(reader);
            }
        }

        /// <summary>
        /// Parse a SVG representation of a PathGeometry.
        /// </summary>
        public static bool TryParseGeometry(string str, out PathGeometry geometry)
        {
            try
            {
                geometry = ParseGeometry(str);
                return true;
            }
            catch
            {
                geometry = null;
                return false;
            }
        }

        /// <summary>
        /// Parse a SVG representation of a PathGeometry.
        /// </summary>
        public static PathGeometry ParseGeometry(TextReader reader)
        {
            var geometry = new PathGeometry();

            Point currentPoint = new Point(0, 0);

            while (reader.Peek() == 'M' || reader.Peek() == 'm')
            {
                var figure = ParseFigure(ref currentPoint, reader);
                geometry.Figures.Add(figure);
            }

            return geometry;
        }

        /// <summary>
        /// Parse a SVG representation of a PathFigure.
        /// </summary>
        public static PathFigure ParseFigure(ref Point currentPoint, TextReader reader)
        {
            var figure = new PathFigure();
            
            int c = reader.Read();
            if (c != 'M' && c != 'm')
            {
                throw new FormatException();
            }
            ParseWhitespace(reader);

            bool isRelative = Char.IsLower((char)c);

            // Start point

            figure.StartPoint = currentPoint = ParsePoint(isRelative, currentPoint, reader);

            ParseWhitespace(reader);

            // Lines

            if (reader.Peek() != -1 && !Char.IsLetter((char)reader.Peek()))
            {
                var points = new PointCollection();

                do
                {
                    currentPoint = ParsePoint(isRelative, currentPoint, reader);
                    points.Add(currentPoint);
                    ParseWhitespace(reader);

                } while (reader.Peek() != -1 && !Char.IsLetter((char)reader.Peek()));

                figure.Segments.Add(new PolyLineSegment(points, true));
            }

            // Segments

            while (reader.Peek() != -1)
            {
                PathSegment segment = ParseSegment(ref currentPoint, reader);

                if (segment != null)
                {
                    figure.Segments.Add(segment);

                    ParseWhitespace(reader);
                }
                else
                {
                    break;
                }
            }

            if (reader.Peek() == 'Z' || reader.Peek() == 'z')
            {
                reader.Read();
                figure.IsClosed = true;
                currentPoint = figure.StartPoint;
            }

            return figure;
        }

        /// <summary>
        /// Parse a SVG representation of a PathSegment.
        /// </summary>
        private static PathSegment ParseSegment(ref Point currentPoint, TextReader reader)
        {
            int c = reader.Peek();
            if (c == -1)
            {
                return null;
            }

            switch (Char.ToUpper((char)c))
            {
                case 'L':
                    return ParseLine(ref currentPoint, reader);
                case 'H':
                    return ParseHLine(ref currentPoint, reader);
                case 'V':
                    return ParseVLine(ref currentPoint, reader);
                case 'C':
                    return ParseBezier(ref currentPoint, reader);
                case 'Q':
                    return ParseQuadraticBezier(ref currentPoint, reader);
                case 'S':
                    return ParseSmoothBezier(ref currentPoint, reader);
                case 'A':
                    return ParseArc(ref currentPoint, reader);
                default:
                    return null;
            }
        }

        private static PathSegment ParseLine(ref Point currentPoint, TextReader reader)
        {
            int c = reader.Read();
            if (c != (int)'L' && c != (int)'l')
            {
                throw new FormatException();
            }
            ParseWhitespace(reader);

            bool isRelative = (c == 'l');

            var points = new PointCollection();

            do
            {
                currentPoint = ParsePoint(isRelative, currentPoint, reader);
                points.Add(currentPoint);
                ParseWhitespace(reader);

            } while (reader.Peek() != -1 && !Char.IsLetter((char)reader.Peek()));

            if (points.Count > 1)
            {
                return new PolyLineSegment(points, true);
            }
            else
            {
                return new LineSegment(points[0], true);
            }
        }

        private static PathSegment ParseHLine(ref Point currentPoint, TextReader reader)
        {
            // horizontal-lineto:
            //   ( "H" | "h" ) wsp* horizontal-lineto-argument-sequence
            // horizontal-lineto-argument-sequence:
            //   coordinate
            //     | coordinate comma-wsp? horizontal-lineto-argument-sequence

            // ( "H" | "h" )
            int c = reader.Read();
            if (c != 'H' && c != 'h')
            {
                throw new FormatException();
            }
            // wsp*
            ParseWhitespace(reader);

            bool isRelative = Char.IsLower((char)c);

            var points = new PointCollection();

            // horizontal-lineto-argument-sequence:
            do
            {
                // coordinate
                currentPoint = ParseXCoordinate(isRelative, currentPoint, reader);
                points.Add(currentPoint);
                // comma-wsp?
                ParseCommaWsp(reader, false);

            } while (reader.Peek() != -1 && !Char.IsLetter((char)reader.Peek()));

            if (points.Count > 1)
            {
                return new PolyLineSegment(points, true);
            }
            else
            {
                return new LineSegment(points[0], true);
            }
        }

        private static PathSegment ParseVLine(ref Point currentPoint, TextReader reader)
        {
            // vertical-lineto:
            //   ( "V" | "v" ) wsp* vertical-lineto-argument-sequence
            // vertical-lineto-argument-sequence:
            //   coordinate
            //   | coordinate comma-wsp? vertical-lineto-argument-sequence

            // ( "V" | "v" )
            int c = reader.Read();
            if (c != 'V' && c != 'v')
            {
                throw new FormatException();
            }
            // wsp*
            ParseWhitespace(reader);

            bool isRelative = Char.IsLower((char)c);

            var points = new PointCollection();

            // vertical-lineto-argument-sequence:
            do
            {
                // coordinate
                currentPoint = ParseYCoordinate(isRelative, currentPoint, reader);
                points.Add(currentPoint);
                // comma-wsp?
                ParseCommaWsp(reader, false);

            } while (reader.Peek() != -1 && !Char.IsLetter((char)reader.Peek()));

            if (points.Count > 1)
            {
                return new PolyLineSegment(points, true);
            }
            else
            {
                return new LineSegment(points[0], true);
            }
        }

        private static PathSegment ParseBezier(ref Point currentPoint, TextReader reader)
        {
            // curveto:
            //  ( "C" | "c" ) wsp* curveto-argument-sequence
            // curveto-argument-sequence:
            //  curveto-argument
            //  | curveto-argument comma-wsp? curveto-argument-sequence
            // curveto-argument:
            //  coordinate-pair comma-wsp? coordinate-pair comma-wsp? coordinate-pair

            //  ( "C" | "c" )
            int c = reader.Read();
            if (c != 'C' && c != 'c')
            {
                throw new FormatException();
            }
            // wsp*
            ParseWhitespace(reader);

            bool isRelative = Char.IsLower((char)c);

            // curveto-argument-sequence
            var points = new PointCollection();
            do
            {
                // curveto-argument

                // coordinate-pair
                var point1 = ParsePoint(isRelative, currentPoint, reader);
                points.Add(point1);

                // comma-wsp?
                ParseCommaWsp(reader, false);

                // coordinate-pair
                var point2 = ParsePoint(isRelative, currentPoint, reader);
                points.Add(point2);

                // comma-wsp?
                ParseCommaWsp(reader, false);

                // coordinate-pair
                currentPoint = ParsePoint(isRelative, currentPoint, reader);
                points.Add(currentPoint);

                // comma-wsp?
                ParseCommaWsp(reader, false);

            } while (reader.Peek() != -1 && !Char.IsLetter((char)reader.Peek()));

            if (points.Count > 3)
            {
                return new PolyBezierSegment(points, true);
            }
            else
            {
                return new BezierSegment(points[0], points[1], points[2], true);
            }
        }

        private static PathSegment ParseQuadraticBezier(ref Point currentPoint, TextReader reader)
        {
            int c = reader.Read();
            if (c != 'Q' && c != 'q')
            {
                throw new FormatException();
            }
            ParseWhitespace(reader);

            bool isRelative = (c == 'q');

            var points = new PointCollection();

            do
            {
                var point1 = ParsePoint(isRelative, currentPoint, reader);
                points.Add(point1);
                ParseWhitespace(reader);

                currentPoint = ParsePoint(isRelative, currentPoint, reader);
                points.Add(currentPoint);
                ParseWhitespace(reader);

            } while (reader.Peek() != -1 && !Char.IsLetter((char)reader.Peek()));

            if (points.Count > 3)
            {
                return new PolyQuadraticBezierSegment(points, true);
            }
            else
            {
                return new QuadraticBezierSegment(points[0], points[1], true);
            }
        }

        private static PathSegment ParseSmoothBezier(ref Point currentPoint, TextReader reader)
        {
            // smooth-curveto:
            //  ( "S" | "s" ) wsp* smooth-curveto-argument-sequence
            // smooth-curveto-argument-sequence:
            //  smooth-curveto-argument
            //  | smooth-curveto-argument comma-wsp? smooth-curveto-argument-sequence
            // smooth-curveto-argument:
            //  coordinate-pair comma-wsp? coordinate-pair

            //  ( "S" | "s" )
            int c = reader.Read();
            if (c != 'S' && c != 's')
            {
                throw new FormatException();
            }
            // wsp*
            ParseWhitespace(reader);

            bool isRelative = Char.IsLower((char)c);

            // smooth-curveto-argument-sequence
            var points = new PointCollection();
            do
            {
                // smooth-curveto-argument

                // coordinate-pair
                var point1 = ParsePoint(isRelative, currentPoint, reader);
                points.Add(point1);

                // comma-wsp?
                ParseCommaWsp(reader, false);

                // coordinate-pair
                currentPoint = ParsePoint(isRelative, currentPoint, reader);
                points.Add(currentPoint);

                // comma-wsp?
                ParseCommaWsp(reader, false);

            } while (reader.Peek() != -1 && !Char.IsLetter((char)reader.Peek()));

            if (points.Count > 2)
            {
                return new PolyBezierSegment(points, true);
            }
            else
            {
                return new BezierSegment(points[0], points[0], points[1], true);
            }
        }

        private static PathSegment ParseArc(ref Point currentPoint, TextReader reader)
        {
            // elliptical-arc:
            //   ( "A" | "a" ) wsp* elliptical-arc-argument-sequence
            // elliptical-arc-argument-sequence:
            //   elliptical-arc-argument
            //     | elliptical-arc-argument comma-wsp? elliptical-arc-argument-sequence
            // elliptical-arc-argument:
            //   nonnegative-number comma-wsp? nonnegative-number comma-wsp? 
            //   number comma-wsp flag comma-wsp? flag comma-wsp? coordinate-pair

            int c = reader.Read();
            if (c != 'A' && c != 'a')
            {
                throw new FormatException();
            }
            ParseWhitespace(reader);

            bool isRelative = (c == 'a');

            double rx = ParseFloat(reader);
            ParseCommaWsp(reader, false);
            double ry = ParseFloat(reader);
            ParseCommaWsp(reader, false);
            double xAxisRotation = ParseFloat(reader);
            ParseCommaWsp(reader, true);
            int largeArcFlag = ParseInt(reader);
            ParseCommaWsp(reader, false);
            int sweepFlag = ParseInt(reader);
            ParseCommaWsp(reader, false);
            currentPoint = ParsePoint(isRelative, currentPoint, reader);

            var size = new Size(rx, ry);
            bool isLargeArc = (largeArcFlag != 0);
            var sweepDirection = (sweepFlag != 0) ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;

            return new ArcSegment(currentPoint, size, xAxisRotation, isLargeArc, sweepDirection, true);
        }

        private static Point ParsePoint(bool isRelative, Point currentPoint, TextReader reader)
        {
            var point = ParseCoordinatePair(reader);

            if (isRelative)
            {
                return currentPoint + new Vector(point.X, point.Y);
            }
            else
            {
                return point;
            }
        }

        private static Point ParseCoordinatePair(TextReader reader)
        {
            // coordinate-pair:
            //   coordinate comma-wsp? coordinate

            double x = ParseCoordinate(reader);
            ParseCommaWsp(reader, false);
            double y = ParseCoordinate(reader);

            return new Point(x, y);
        }

        private static double ParseCoordinate(TextReader reader)
        {
            // coordinate:
            //  number

            return ParseNumber(reader);
        }

        private static Point ParseXCoordinate(bool isRelative, Point currentPoint, TextReader reader)
        {
            double x = ParseNumber(reader);
            if (isRelative)
            {
                x += currentPoint.X;
            }
            return new Point(x, currentPoint.Y);
        }

        private static Point ParseYCoordinate(bool isRelative, Point currentPoint, TextReader reader)
        {
            double y = ParseNumber(reader);
            if (isRelative)
            {
                y += currentPoint.Y;
            }
            return new Point(currentPoint.X, y);
        }

        private static double ParseNumber(TextReader reader)
        {
            // number:
            //  sign? integer-constant
            //  | sign? floating-point-constant
            // integer-constant:
            //  digit-sequence
            // floating-point-constant:
            //  fractional-constant exponent?
            //  | digit-sequence exponent
            // fractional-constant:
            //  digit-sequence? "." digit-sequence
            //  | digit-sequence "."
            // exponent:
            //  ( "e" | "E" ) sign? digit-sequence
            // sign:
            //  "+" | "-"
            // digit-sequence:
            //  digit
            //  | digit digit-sequence
            // digit:
            //  "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"

            var buffer = new StringBuilder();

            while (reader.Peek() != -1)
            {
                char c = (char)reader.Peek();

                if (Char.IsDigit(c) ||
                    ((c == '+' || c == '-') && (buffer.Length == 0 || buffer.ToString().Last() == 'e' || buffer.ToString().Last() == 'E')) ||
                    ((c == '.') && (!buffer.ToString().Contains('.') && !buffer.ToString().Contains('e') && !buffer.ToString().Contains('E'))) ||
                    ((c == 'e' || c == 'E')) && (!buffer.ToString().Contains('e') && !buffer.ToString().Contains('E')))
                {
                    reader.Read();
                    buffer.Append(c);
                }
                else
                {
                    break;
                }
            }

            string str = buffer.ToString();

            if (str.Contains('.') || str.Contains('e') || str.Contains('E'))
            {
                return Double.Parse(str, CultureInfo.InvariantCulture);
            }
            else
            {
                return Int32.Parse(str, CultureInfo.InvariantCulture);
            }
        }

        private static int ParseInt(TextReader reader)
        {
            var buffer = new StringBuilder();

            while (reader.Peek() != -1)
            {
                char c = (char)reader.Read();
                if (Char.IsDigit(c) || c == '+' || c == '-')
                {
                    buffer.Append(c);
                }
                else
                {
                    break;
                }
            }

            return Int32.Parse(buffer.ToString(), CultureInfo.InvariantCulture);
        }

        private static double ParseFloat(TextReader reader)
        {
            var buffer = new StringBuilder();

            while (reader.Peek() != -1)
            {
                char c = (char)reader.Peek();
                if (Char.IsDigit(c) || 
                    ((c == '+' || c == '-') && (buffer.Length == 0 || buffer.ToString().Last() == 'e' || buffer.ToString().Last() == 'E')) || 
                    ((c == '.') && (!buffer.ToString().Contains('.'))) || 
                    ((c == 'e' || c == 'E')) && (!buffer.ToString().Contains('e') && !buffer.ToString().Contains('E')))
                {
                    reader.Read();
                    buffer.Append(c);
                }
                else
                {
                    break;
                }
            }

            return Double.Parse(buffer.ToString(), CultureInfo.InvariantCulture);
        }

        private static void ParseCommaWsp(TextReader reader, bool required)
        {
            // comma-wsp:
            //   (wsp+ comma? wsp*) | (comma wsp*)

            int c = reader.Peek();
            if (c == -1)
            {
                if (required)
                {
                    throw new FormatException();
                }
            }
            else if (Char.IsWhiteSpace((char)c))
            {
                ParseWhitespace(reader);

                if (reader.Peek() == (int)',')
                {
                    reader.Read();
                    ParseWhitespace(reader);
                }
            }
            else if ((char)c == ',')
            {
                reader.Read();

                ParseWhitespace(reader);
            }
            else if (required)
            {
                throw new FormatException();
            }
        }

        private static void ParseWhitespace(TextReader reader)
        {
            while (reader.Peek() != -1 && Char.IsWhiteSpace((char)reader.Peek()))
            {
                reader.Read();
            }
        }

        #endregion

        #region Format

        /// <summary>
        /// Format an SVG representation of the given PathGeometry
        /// </summary>
        public static string FormatGeometry(PathGeometry geometry)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatGeometry(geometry, writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Format an SVG representation of the given PathGeometry
        /// </summary>
        public static void FormatGeometry(PathGeometry geometry, TextWriter writer)
        {
            if (geometry != null)
            {
                foreach (var figure in geometry.Figures)
                {
                    FormatFigure(figure, writer);
                }
            }
        }

        /// <summary>
        /// Format an SVG representation of the given PathFigure
        /// </summary>
        public static void FormatFigure(PathFigure figure, TextWriter writer)
        {
            if (figure != null)
            {
                writer.Write("M{0},{1}", figure.StartPoint.X, figure.StartPoint.Y);

                PathSegment previous = null;

                foreach (var segment in figure.Segments)
                {
                    writer.Write(' ');

                    FormatSegment(segment, previous, writer);

                    previous = segment;
                }

                if (figure.IsClosed)
                {
                    writer.Write('z');
                }
            }
        }

        /// <summary>
        /// Format an SVG representation of the given PathSegment
        /// </summary>
        public static void FormatSegment(PathSegment segment, PathSegment previous, TextWriter writer)
        {
            if (segment is ArcSegment)
            {
                FormatArc((ArcSegment)segment, previous, writer);
            }
            else if (segment is BezierSegment)
            {
                FormatBezier((BezierSegment)segment, previous, writer);
            }
            else if (segment is LineSegment)
            {
                FormatLine((LineSegment)segment, previous, writer);
            }
            else if (segment is PolyBezierSegment)
            {
                FormatPolyBezier((PolyBezierSegment)segment, previous, writer);
            }
            else if (segment is PolyLineSegment)
            {
                FormatPolyLine((PolyLineSegment)segment, previous, writer);
            }
            else if (segment is PolyQuadraticBezierSegment)
            {
                FormatPolyQuadraticBezier((PolyQuadraticBezierSegment)segment, previous, writer);
            }
            else if (segment is QuadraticBezierSegment)
            {
                FormatQuadraticBezier((QuadraticBezierSegment)segment, previous, writer);
            }
            else
            {

            }
        }

        /// <summary>
        /// Format an SVG representation of the given ArcSegment
        /// </summary>
        static void FormatArc(ArcSegment segment, PathSegment previous, TextWriter writer)
        {
            double rx = segment.Size.Width;
            double ry = segment.Size.Height;
            double xAxisRotation = segment.RotationAngle;
            int largeArcFlag = segment.IsLargeArc ? 1 : 0;
            int sweepFlag = (segment.SweepDirection == SweepDirection.Clockwise) ? 1 : 0;
            double x = segment.Point.X;
            double y = segment.Point.Y;

            writer.Write("A{0},{1} {2} {3},{4} {5},{6}", rx, ry, xAxisRotation, largeArcFlag, sweepFlag, x, y);
        }

        /// <summary>
        /// Format an SVG representation of the given BezierSegment
        /// </summary>
        static void FormatBezier(BezierSegment segment, PathSegment previous, TextWriter writer)
        {
            writer.Write("C{0},{1} {2},{3} {4},{5}", segment.Point1.X, segment.Point1.Y, segment.Point2.X, segment.Point2.Y, segment.Point3.X, segment.Point3.Y);
        }

        /// <summary>
        /// Format an SVG representation of the given LineSegment
        /// </summary>
        static void FormatLine(LineSegment segment, PathSegment previous, TextWriter writer)
        {
            writer.Write("L{0},{1}", segment.Point.X, segment.Point.Y);
        }

        /// <summary>
        /// Format an SVG representation of the given PolyBezierSegment
        /// </summary>
        static void FormatPolyBezier(PolyBezierSegment segment, PathSegment previous, TextWriter writer)
        {
            writer.Write('C');

            for (int i = 0; i < segment.Points.Count; i++)
            {
                writer.Write("{0},{1}", segment.Points[i].X, segment.Points[i].Y);

                if (i != segment.Points.Count - 1)
                {
                    writer.Write(' ');
                }
            }
        }

        /// <summary>
        /// Format an SVG representation of the given PolyLineSegment
        /// </summary>
        static void FormatPolyLine(PolyLineSegment segment, PathSegment previous, TextWriter writer)
        {
            writer.Write('L');

            for (int i = 0; i < segment.Points.Count; i++)
            {
                writer.Write("{0},{1}", segment.Points[i].X, segment.Points[i].Y);

                if (i != segment.Points.Count - 1)
                {
                    writer.Write(' ');
                }
            }
        }

        /// <summary>
        /// Format an SVG representation of the given PolyQuadraticBezierSegment
        /// </summary>
        static void FormatPolyQuadraticBezier(PolyQuadraticBezierSegment segment, PathSegment previous, TextWriter writer)
        {
            writer.Write('Q');

            for (int i = 0; i < segment.Points.Count; i++)
            {
                writer.Write("{0},{1}", segment.Points[i].X, segment.Points[i].Y);

                if (i != segment.Points.Count - 1)
                {
                    writer.Write(' ');
                }
            }
        }

        /// <summary>
        /// Format an SVG representation of the given QuadraticBezierSegment
        /// </summary>
        static void FormatQuadraticBezier(QuadraticBezierSegment segment, PathSegment previous, TextWriter writer)
        {
            writer.Write("Q{0},{1} {2},{3}", segment.Point1.X, segment.Point1.Y, segment.Point2.X, segment.Point2.Y);
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
                case SVGAttributes.D:
                    return RenderedData;
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        public override void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.D:
                    RenderedData = value;
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
                    RenderedData = "";
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
            return new NPath(this);
        }

        #endregion
    }
}
