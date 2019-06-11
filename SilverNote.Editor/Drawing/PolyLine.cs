/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
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
    public class PolyLine : LineBase
    {
        #region Fields

        PointCollection _Points;

        #endregion

        #region Constructors

        public PolyLine()
        {
            _Points = new PointCollection();
        }

        public PolyLine(IEnumerable<Point> points)
        {
            _Points = new PointCollection(points);

            StrokeBrush = Brushes.Black;
            StrokeWidth = 2;
        }

        public PolyLine(params double[] coordinates)
        {
            if (coordinates.Length % 2 != 0)
            {
                throw new ArgumentException("An even number of coordinates is required.", "coordinates");
            }

            _Points = new PointCollection();

            for (int i = 0; i < coordinates.Length; i += 2)
            {
                double x = coordinates[i];
                double y = coordinates[i + 1];

                _Points.Add(new Point(x, y));
            }

            StrokeBrush = Brushes.Black;
            StrokeWidth = 2;
        }

        public PolyLine(PolyLine copy)
            : base(copy)
        {
            if (copy._Points != null)
            {
                _Points = (PointCollection)copy._Points.Clone();
            }

            if (copy.MarkerStart != null)
            {
                MarkerStart = (Marker)copy.MarkerStart.Clone();
            }

            if (copy.MarkerEnd != null)
            {
                MarkerEnd = (Marker)copy.MarkerEnd.Clone();
            }
        }

        #endregion

        #region Properties

        public PointCollection Points
        {
            get
            {
                return _Points;
            }
            set
            {
                if (value != _Points)
                {
                    _Points = value;
                    InvalidateRender();
                }
            }
        }

        public PointCollection RenderedPoints
        {
            get
            {
                if (GeometryTransform.Value.IsIdentity)
                {
                    return Points;
                }

                var points = new PointCollection();
                foreach (Point point in Points)
                {
                    points.Add(GeometryTransform.Transform(point));
                }
                return points;
            }
            set
            {
                if (GeometryTransform.Value.IsIdentity)
                {
                    Points = value;
                    return;
                }

                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    var points = new PointCollection();
                    foreach (Point point in value)
                    {
                        points.Add(inverse.Transform(point));
                    }
                    Points = points;
                }
            }
        }

        #endregion

        #region NLineBase

        public override Point StartPoint
        {
            get 
            {
                if (Points.Count > 0)
                {
                    return Points[0];
                }
                else
                {
                    return default(Point);
                }
            }
            set
            {
                if (Points.Count > 0)
                {
                    Points[0] = value;
                }
                else
                {
                    Points.Add(value);
                }
            }
        }

        public override Point EndPoint
        {
            get
            {
                if (Points.Count > 0)
                {
                    return Points[Points.Count - 1];
                }
                else
                {
                    return default(Point);
                }
            }
            set
            {
                if (Points.Count > 1)
                {
                    Points[Points.Count - 1] = value;
                }
                else
                {
                    Points.Add(value);
                }
            }
        }

        public override LineBase LineThumb
        {
            get
            {
                var result = (PolyLine)base.LineThumb;
                result.Points.Clear();
                result.Points.Add(new Point(0, 3));
                result.Points.Add(new Point(50, 3));
                result.Points.Add(new Point(50, 12));
                result.Points.Add(new Point(100, 12));
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
                var result = (PolyLine)base.ThumbSmall;

                if (result.MarkerStart != null)
                {
                    result.MarkerStart.MarkerWidth = 5;
                    result.MarkerStart.MarkerHeight = 5;
                }

                if (result.MarkerEnd != null)
                {
                    result.MarkerEnd.MarkerWidth = 5;
                    result.MarkerEnd.MarkerHeight = 5;
                }

                return result;
            }
        }

        bool reset = true;

        public override void Place(Point position)
        {
            if (reset)
            {
                Points.Clear();
                Points.Add(position);
                reset = false;
            }

            Points[Points.Count - 1] = position;
        }

        public override bool CompletePlacing()
        {
            if (Points.Count > 1 && Points.First() == Points.Last())
            {
                return true;
            }

            Points.Add(new Point(Points.Last().X, Points.Last().Y));
            return false;
        }

        public override void Draw(Point point)
        {
            Place(point);
        }

        public override bool CompleteDrawing()
        {
            if (Points.Count > 2 && Points.First() == Points.Last())
            {
                return true;
            }

            if (Points.Count > 1)
            {
                Vector delta = Points[Points.Count - 1] - Points[Points.Count - 2];
                if (delta.Length >= 5)
                {
                    Points.Add(new Point(Points.Last().X, Points.Last().Y));
                }
            }

            return false;
        }

        public override bool CancelDrawing()
        {
            if (Points.Count == 0)
            {
                return false;
            }

            Points.RemoveAt(Points.Count - 1);
            return true;
        }

        protected override Rect Bounds
        {
            get
            {
                Rect result = Rect.Empty;

                foreach (Point point in Points)
                {
                    if (result.IsEmpty)
                    {
                        result = new Rect(point, new Size(0, 0));
                    }
                    else
                    {
                        result.Union(point);
                    }
                }

                return result;
            }
        }

        public override void Normalize()
        {
            if (GeometryTransform != Transform.Identity)
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    Points[i] = GeometryTransform.Transform(Points[i]);
                }
            }

            GeometryTransform = Transform.Identity;
        }

        public override int HandleCount
        {
            get { return Points.Count; }
        }

        protected override Point GetHandleInternal(int index)
        {
            return Points[index];
        }

        protected override void SetHandleInternal(int index, Point value)
        {
            Points[index] = value;
            InvalidateRender();
        }

        public override int SnapCount
        {
            get { return Math.Min(Points.Count, 2); }
        }

        public override Point GetSnap(int index)
        {
            Point point = InternalGetSnap(index);

            return GeometryTransform.Transform(point);
        }

        protected virtual Point InternalGetSnap(int index)
        {
            switch (index)
            {
                case 0:
                    return Points.FirstOrDefault();
                case 1:
                    return Points.LastOrDefault();
                default:
                    return new Point();
            }
        }

        public override PathGeometry ToPathGeometry()
        {
            var segment = new PolyLineSegment();
            if (Points.Count > 1)
            {
                segment.Points = new PointCollection(Points.Skip(1));
            }

            var figure = new PathFigure();
            figure.Segments.Add(segment);
            figure.StartPoint = Points.FirstOrDefault();
            figure.IsClosed = false;

            var path = new PathGeometry();
            path.Figures.Add(figure);

            return path;
        }

        public override StreamGeometry ToStreamGeometry()
        {
            var geometry = new StreamGeometry();

            if (Points.Count > 0)
            {
                var gc = geometry.Open();
                gc.BeginFigure(Points.First(), FillBrush != null, false);
                gc.PolyLineTo(Points.Skip(1).ToList(), StrokeBrush != null, false);
                gc.Close();
            }

            return geometry;
        }

        protected override void OnRenderVisual(DrawingContext dc)
        {
            base.OnRenderVisual(dc);

            if (Points.Count < 2 || 
                (Points.Count == 2 && Points[0] == Points[1]))
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
                Point point1 = GeometryTransform.Transform(Points[0]);
                Point point2 = GeometryTransform.Transform(Points[1]);

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
                Point point1 = GeometryTransform.Transform(Points[Points.Count - 2]);
                Point point2 = GeometryTransform.Transform(Points[Points.Count - 1]);

                MarkerEnd.Offset = new Vector(point2.X, point2.Y);
                MarkerEnd.Orient = Math.Atan2(point2.Y - point1.Y, point2.X - point1.X) * 180.0 / Math.PI;
                MarkerEnd.Brush = this.StrokeBrush;
                MarkerEnd.Redraw();

                if (!Children.Contains(MarkerEnd))
                {
                    Children.Add(MarkerEnd);
                }
            }
        }

        #endregion

        #region INodeSource

        private readonly string _NodeName = SVGElements.NAMESPACE + ' ' + SVGElements.POLYLINE;

        public override string GetNodeName(NodeContext context)
        {
            return _NodeName;
        }

        private readonly string[] _NodeAttributes = new string[] { 
            SVGAttributes.MARKER_START,
            SVGAttributes.MARKER_END,
            SVGAttributes.POINTS
        };

        public override IList<string> GetNodeAttributes(ElementContext context)
        {
            return base.GetNodeAttributes(context).Concat(_NodeAttributes).ToArray();
        }

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.MARKER_START:
                    return Canvas != null ? Canvas.GetDefinitionURL(MarkerStart) : null;
                case SVGAttributes.MARKER_END:
                    return Canvas != null ? Canvas.GetDefinitionURL(MarkerEnd) : null;
                case SVGAttributes.POINTS:
                    var points = Round(RenderedPoints, -3);
                    return SafeConvert.ToString(points);
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        public override void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.MARKER_START:
                    MarkerStart = (Marker)Canvas.GetDefinition(value);
                    break;
                case SVGAttributes.MARKER_END:
                    MarkerEnd = (Marker)Canvas.GetDefinition(value);
                    break;
                case SVGAttributes.POINTS:
                    RenderedPoints = SafeConvert.ToPointCollection(value, new PointCollection());
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
                case SVGAttributes.MARKER_START:
                    MarkerStart = null;
                    break;
                case SVGAttributes.MARKER_END:
                    MarkerEnd = null;
                    break;
                case SVGAttributes.POINTS:
                    Points.Clear();
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
            return new PolyLine(this);
        }

        #endregion

    }
}
