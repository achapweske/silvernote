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
    public class Rectangle : Shape
    {
        #region Fields

        double _X;
        double _Y;
        double _Width;
        double _Height;
        double _RX;
        double _RY;

        #endregion

        #region Constructors

        public Rectangle()
            : this(0, 0, 0, 0)
        {

        }

        public Rectangle(double x, double y, double width, double height)
            : this(x, y, width, height, 0, 0)
        {
            
        }

        public Rectangle(double x, double y, double width, double height, double rx, double ry)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            RX = rx;
            RY = ry;
        }

        public Rectangle(Rectangle copy)
            : base(copy)
        {
            X = copy.X;
            Y = copy.Y;
            Width = copy.Width;
            Height = copy.Height;
            RX = copy.RX;
            RY = copy.RY;
        }

        #endregion

        #region Attributes

        public double X
        {
            get
            {
                return _X;
            }
            set
            {
                if (value != _X)
                {
                    _X = value;
                    InvalidateRender();
                }
            }
        }

        public double Y
        {
            get
            {
                return _Y;
            }
            set
            {
                if (value != _Y)
                {
                    _Y = value;
                    InvalidateRender();
                }
            }
        }

        public double Width
        {
            get
            {
                return _Width;
            }
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                    InvalidateRender();
                }
            }
        }

        public double Height
        {
            get
            {
                return _Height;
            }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    InvalidateRender();
                }
            }
        }

        public double RX
        {
            get
            {
                return _RX;
            }
            set
            {
                if (value != _RX)
                {
                    _RX = value;
                    InvalidateRender();
                }
            }
        }

        public double RY
        {
            get
            {
                return _RY;
            }
            set
            {
                if (value != _RY)
                {
                    _RY = value;
                    InvalidateRender();
                }
            }
        }

        public double RenderedX
        {
            get
            {
                Point point = new Point(X, Y);
                point = GeometryTransform.Transform(point);
                return point.X;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    Point point = new Point(value, RenderedY);
                    point = inverse.Transform(point);
                    X = point.X;
                }
            }
        }

        public double RenderedY
        {
            get
            {
                Point point = new Point(X, Y);
                point = GeometryTransform.Transform(point);
                return point.Y;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    Point point = new Point(RenderedX, value);
                    point = inverse.Transform(point);
                    Y = point.Y;
                }
            }
        }

        public double RenderedWidth
        {
            get
            {
                return RenderedBounds.Width;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    var rect = RenderedBounds;
                    rect.Width = value;
                    rect = inverse.TransformBounds(rect);
                    Width = rect.Width;
                }
            }
        }

        public double RenderedHeight
        {
            get
            {
                return RenderedBounds.Height;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    var rect = RenderedBounds;
                    rect.Height = value;
                    rect = inverse.TransformBounds(rect);
                    Height = rect.Height;
                }
            }
        }

        #endregion

        #region NDrawing

        protected override Rect Bounds
        {
            get { return new Rect(X, Y, Width, Height); }
        }

        public override void Normalize()
        {
            Rect bounds = RenderedBounds;

            X = bounds.X;
            Y = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;

            GeometryTransform = null;
        }

        Point _StartPoint;

        public override void Place(Point position)
        {
            _StartPoint = position;

            X = position.X;
            Y = position.Y;
            Width = Height = 0;
        }

        public override void Draw(Point point)
        {
            Rect rect = new Rect(_StartPoint, point);

            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
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
                    return new Point(X, Y);
                case 1:
                    return new Point(X + Width, Y);
                case 2:
                    return new Point(X + Width, Y + Height);
                case 3:
                    return new Point(X, Y + Height);
                default:
                    return new Point();
            }
        }

        protected override void SetHandleInternal(int index, Point value)
        {
            double left = X;
            double top = Y;
            double right = X + Width;
            double bottom = Y + Height;

            switch (index)
            {
                case 0:
                    left = Math.Min(value.X, right);
                    top = Math.Min(value.Y, bottom);
                    break;
                case 1:
                    right = Math.Max(value.X, left);
                    top = Math.Min(value.Y, bottom);
                    break;
                case 2:
                    right = Math.Max(value.X, left);
                    bottom = Math.Max(value.Y, top);
                    break;
                case 3:
                    left = Math.Min(value.X, right);
                    bottom = Math.Max(value.Y, top);
                    break;
            }

            X = left;
            Y = top;
            Width = right - left;
            Height = bottom - top;
        }

        public override PathGeometry ToPathGeometry()
        {
            var segment = new PolyLineSegment();
            segment.Points.Add(new Point(X + Width, Y));
            segment.Points.Add(new Point(X + Width, Y + Height));
            segment.Points.Add(new Point(X, Y + Height));

            var figure = new PathFigure();
            figure.Segments.Add(segment);
            figure.StartPoint = new Point(X, Y);
            figure.IsClosed = true;

            var path = new PathGeometry();
            path.Figures.Add(figure);

            return path;
        }

        public override StreamGeometry ToStreamGeometry()
        {
            var geometry = new StreamGeometry();

            var gc = geometry.Open();
            gc.BeginFigure(new Point(X, Y), FillBrush != null, true);
            gc.LineTo(new Point(X + Width, Y), StrokeBrush != null, false);
            gc.LineTo(new Point(X + Width, Y + Height), StrokeBrush != null, false);
            gc.LineTo(new Point(X, Y + Height), StrokeBrush != null, false);
            gc.Close();

            return geometry;
        }

        protected override void OnRenderVisual(DrawingContext dc)
        {
            double zoom = NoteEditor.GetZoom(this);

            Rect rect = RenderedBounds;

            rect.X = Math.Floor(rect.X * zoom) / zoom;
            rect.Y = Math.Floor(rect.Y * zoom) / zoom;
            rect.Width = Math.Floor(rect.Width * zoom) / zoom;
            rect.Height = Math.Floor(rect.Height * zoom) / zoom;

            if (RX == 0 && RY == 0)
            {
                dc.DrawRectangle(FillBrush, StrokePen, rect);
            }
            else
            {
                dc.DrawRoundedRectangle(FillBrush, StrokePen, rect, RX, RY);
            }

            if (StrokeWidth * zoom < 5)
            {
                Pen transparent = new Pen(Brushes.Transparent, 5 / zoom);

                if (RX == 0 && RY == 0)
                {
                    dc.DrawRectangle(null, transparent, rect);
                }
                else
                {
                    dc.DrawRoundedRectangle(null, transparent, rect, RX, RY);
                }
            }
        }

        #endregion

        #region INodeSource

        private readonly string _NodeName = SVGElements.NAMESPACE + ' ' + SVGElements.RECT;

        public override string GetNodeName(NodeContext context)
        {
            return _NodeName;
        }

        private readonly string[] _NodeAttributes = new string[] { 
            SVGAttributes.X,
            SVGAttributes.Y,
            SVGAttributes.WIDTH,
            SVGAttributes.HEIGHT,
            SVGAttributes.RX,
            SVGAttributes.RY
        };

        public override IList<string> GetNodeAttributes(ElementContext context)
        {
            return base.GetNodeAttributes(context).Concat(_NodeAttributes).ToArray();
        }

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.X:
                    return SafeConvert.ToString(RenderedX);
                case SVGAttributes.Y:
                    return SafeConvert.ToString(RenderedY);
                case SVGAttributes.WIDTH:
                    return SafeConvert.ToString(RenderedWidth);
                case SVGAttributes.HEIGHT:
                    return SafeConvert.ToString(RenderedHeight);
                case SVGAttributes.RX:
                    if (RX != 0)
                        return SafeConvert.ToString(RX);
                    else
                        return null;
                case SVGAttributes.RY:
                    if (RY != 0)
                        return SafeConvert.ToString(RY);
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
                case SVGAttributes.X:
                    RenderedX = SafeConvert.ToDouble(value);
                    break;
                case SVGAttributes.Y:
                    RenderedY = SafeConvert.ToDouble(value);
                    break;
                case SVGAttributes.WIDTH:
                    RenderedWidth = SafeConvert.ToDouble(value);
                    break;
                case SVGAttributes.HEIGHT:
                    RenderedHeight = SafeConvert.ToDouble(value);
                    break;
                case SVGAttributes.RX:
                    RX = SafeConvert.ToDouble(value);
                    break;
                case SVGAttributes.RY:
                    RY = SafeConvert.ToDouble(value);
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
                case SVGAttributes.X:
                    X = 0;
                    break;
                case SVGAttributes.Y:
                    Y = 0;
                    break;
                case SVGAttributes.WIDTH:
                    Width = 0;
                    break;
                case SVGAttributes.HEIGHT:
                    Height = 0;
                    break;
                case SVGAttributes.RX:
                    RX = 0;
                    break;
                case SVGAttributes.RY:
                    RY = 0;
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
            return new Rectangle(this);
        }

        #endregion
    }
}
