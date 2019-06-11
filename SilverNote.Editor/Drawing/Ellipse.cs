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
using System.Windows.Media;
using System.Xml;
using DOM;
using DOM.SVG;

namespace SilverNote.Editor
{
    public class Ellipse : Shape
    {
        #region Fields

        double _CX;
        double _CY;
        double _RX;
        double _RY;

        #endregion

        #region Constructors

        public Ellipse()
            : this(0, 0, 0, 0)
        {

        }

        public Ellipse(double cx, double cy, double rx, double ry)
        {
            CX = cx;
            CY = cy;
            RX = rx;
            RY = ry;
        }

        public Ellipse(Ellipse copy)
            : base(copy)
        {
            CX = copy.CX;
            CY = copy.CY;
            RX = copy.RX;
            RY = copy.RY;
        }

        #endregion

        #region Properties

        /// <summary>
        /// X-coordinate of the ellipse's center point
        /// </summary>
        public double CX
        {
            get
            {
                return _CX;
            }
            set
            {
                if (_CX != value)
                {
                    _CX = value;
                    InvalidateRender();
                }
            }
        }

        /// <summary>
        /// Y-coordinate of the ellipse's center point
        /// </summary>
        public double CY
        {
            get
            {
                return _CY;
            }
            set
            {
                if (_CY != value)
                {
                    _CY = value;
                    InvalidateRender();
                }
            }
        }

        /// <summary>
        /// X-radius of the ellipse
        /// </summary>
        public double RX
        {
            get
            {
                return _RX;
            }
            set
            {
                if (_RX != value)
                {
                    _RX = value;
                    InvalidateRender();
                }
            }
        }

        /// <summary>
        /// Y-radius of the ellipse
        /// </summary>
        public double RY
        {
            get
            {
                return _RY;
            }
            set
            {
                if (_RY != value)
                {
                    _RY = value;
                    InvalidateRender();
                }
            }
        }

        #endregion

        #region NDrawing

        protected override Rect Bounds
        {
            get { return new Rect(CX - RX, CY - RY, RX * 2, RY * 2); }
        }

        public override void Normalize()
        {
            if (GeometryTransform != Transform.Identity)
            {
                double cx, cy, rx, ry;
                TransformEllipse(CX, CY, RX, RY, out cx, out cy, out rx, out ry, GeometryTransform);
                CX = cx; CY = cy; RX = rx; RY = ry;
                GeometryTransform = Transform.Identity;
            }
        }

        Point _StartPoint;

        public override void Place(Point position)
        {
            _StartPoint = position;

            CX = position.X;
            CY = position.Y;
            RX = RY = 0;
        }

        public override void Draw(Point point)
        {
            Rect bounds = new Rect(_StartPoint, point);

            CX = (bounds.Left + bounds.Right) / 2;
            CY = (bounds.Top + bounds.Bottom) / 2;
            RX = bounds.Width / 2;
            RY = bounds.Height / 2;
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
                    return new Point(CX - RX, CY - RY);
                case 1:
                    return new Point(CX + RX, CY - RY);
                case 2:
                    return new Point(CX + RX, CY + RY);
                case 3:
                    return new Point(CX - RX, CY + RY);
                default:
                    return new Point();
            }
        }

        protected override void SetHandleInternal(int index, Point value)
        {
            double left = CX - RX;
            double top = CY - RY;
            double right = CX + RX;
            double bottom = CY + RY;

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

            CX = (left + right) / 2;
            CY = (top + bottom) / 2;
            RX = (right - left) / 2;
            RY = (bottom - top) / 2;
        }

        public override PathGeometry ToPathGeometry()
        {
            return new PathGeometry(
                figures: new [] {
                    new PathFigure(
                        start: new Point(CX - RX, CY),
                        segments: new [] {
                            new ArcSegment(
                                point: new Point(CX - RX, CY),
                                size: new Size(RX * 2, RY * 2),
                                rotationAngle: 0,
                                isLargeArc: true,
                                sweepDirection: SweepDirection.Clockwise,
                                isStroked: true)

                        }, 
                        closed: false)});
        }

        public override StreamGeometry ToStreamGeometry()
        {
            var geometry = new StreamGeometry();

            var gc = geometry.Open();
            try
            {
                gc.BeginFigure(
                    startPoint: new Point(CX - RX, CY), 
                    isFilled: FillBrush != null, 
                    isClosed: false);
                gc.ArcTo(
                    point: new Point(CX - RX, CY), 
                    size: new Size(RX * 2, RY * 2), 
                    rotationAngle: 0, 
                    isLargeArc: true, 
                    sweepDirection: SweepDirection.Clockwise, 
                    isStroked: StrokeBrush != null, 
                    isSmoothJoin: false);
            }
            finally
            {
                gc.Close();
            }

            return geometry;
        }

        protected override void OnRenderVisual(DrawingContext dc)
        {
            double cx, cy, rx, ry;
            TransformEllipse(CX, CY, RX, RY, out cx, out cy, out rx, out ry, GeometryTransform);
            Point center = new Point(cx, cy);

            dc.DrawEllipse(FillBrush, StrokePen, center, rx, ry);

            if (StrokeWidth < 4)
            {
                Pen stroke = new Pen(Brushes.Transparent, 4);
                dc.DrawEllipse(null, stroke, center, rx, ry);
            }

            base.OnRenderVisual(dc);
        }

        #endregion

        #region INodeSource

        private readonly string _NodeName = SVGElements.NAMESPACE + ' ' + SVGElements.ELLIPSE;

        public override string GetNodeName(NodeContext context)
        {
            return _NodeName;
        }

        private readonly string[] _NodeAttributes = new string[] { 
            SVGAttributes.CX,
            SVGAttributes.CY,
            SVGAttributes.RX,
            SVGAttributes.RY
        };

        public override IList<string> GetNodeAttributes(ElementContext context)
        {
            return base.GetNodeAttributes(context).Concat(_NodeAttributes).ToArray();
        }

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            Rect bounds = GeometryTransform.TransformBounds(Bounds);

            switch (name)
            {
                case SVGAttributes.CX:
                    return SafeConvert.ToString((bounds.Left + bounds.Right) / 2);
                case SVGAttributes.CY:
                    return SafeConvert.ToString((bounds.Top + bounds.Bottom) / 2);
                case SVGAttributes.RX:
                    return SafeConvert.ToString(bounds.Width / 2);
                case SVGAttributes.RY:
                    return SafeConvert.ToString(bounds.Height / 2);
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        public override void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.CX:
                    CX = SafeConvert.ToDouble(value);
                    break;
                case SVGAttributes.CY:
                    CY = SafeConvert.ToDouble(value);
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
                case SVGAttributes.CX:
                    CX = 0;
                    break;
                case SVGAttributes.CY:
                    CY = 0;
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
            return new Ellipse(this);
        }

        #endregion

        #region Algorithms

        public static void TransformEllipse(double inCX, double inCY, double inRX, double inRY, out double outCX, out double outCY, out double outRX, out double outRY, Transform transform)
        {
            if (transform == Transform.Identity)
            {
                outCX = inCX;
                outCY = inCY;
                outRX = inRX;
                outRY = inRY;
            }
            else
            {
                Rect bounds = new Rect(inCX - inRX, inCY - inRY, inRX * 2, inRY * 2);
                bounds = transform.TransformBounds(bounds);
                outCX = (bounds.Left + bounds.Right) / 2;
                outCY = (bounds.Top + bounds.Bottom) / 2;
                outRX = bounds.Width / 2;
                outRY = bounds.Height / 2;
            }
        }

        #endregion
    }
}
