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
    public class Polygon : Shape
    {
        #region Fields

        PointCollection _Points;

        #endregion

        #region Constructors

        public Polygon()
        {
            _Points = new PointCollection();
        }

        public Polygon(IEnumerable<Point> points)
        {
            _Points = new PointCollection(points);
        }

        public Polygon(params double[] coordinates)
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
        }

        public Polygon(Polygon copy)
            : base(copy)
        {
            if (copy._Points != null)
            {
                _Points = (PointCollection)copy._Points.Clone();
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

        #region NDrawing

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
                        result = new Rect(point, new Size(0, 0));
                    else
                        result.Union(point);
                }
                return result;
            }
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
            figure.IsClosed = Points.Count > 2;

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
                gc.BeginFigure(Points.First(), FillBrush != null, true);
                gc.PolyLineTo(Points.Skip(1).ToList(), StrokeBrush != null, false);
                gc.Close();
            }

            return geometry;
        }

        protected override void OnRenderVisual(DrawingContext dc)
        {
            base.OnRenderVisual(dc);
        }

        #endregion

        #region INodeSource

        private readonly string _NodeName = SVGElements.NAMESPACE + ' ' + SVGElements.POLYGON;

        public override string GetNodeName(NodeContext context)
        {
            return _NodeName;
        }

        private readonly string[] _NodeAttributes = new string[] { 
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
            return new Polygon(this);
        }

        #endregion

    }
}
