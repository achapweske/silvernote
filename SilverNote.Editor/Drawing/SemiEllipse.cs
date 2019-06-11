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
using DOM;
using DOM.SVG;
using DOM.Helpers;

namespace SilverNote.Editor
{
    public class SemiEllipse : Shape
    {
        #region Fields

        double _CX;
        double _CY;
        double _RX;
        double _RY;
        Point _StartPoint;
        Point _EndPoint;
        SweepDirection _SweepDirection;

        #endregion

        #region Constructors

        public SemiEllipse()
        {
            CX = 0;
            CY = 0;
            RX = 0;
            RY = 0;
            StartPoint = new Point(0, 0);
            EndPoint = new Point(0, 0);
            SweepDirection = SweepDirection.Clockwise;
        }

        public SemiEllipse(SemiEllipse copy)
            : base(copy)
        {
            CX = copy.CX;
            CY = copy.CY;
            RX = copy.RX;
            RY = copy.RY;
            StartPoint = copy.StartPoint;
            EndPoint = copy.EndPoint;
            SweepDirection = copy.SweepDirection;
        }

        #endregion

        #region Properties

        public double CX
        {
            get
            {
                return _CX;
            }
            set
            {
                if (value != _CX)
                {
                    _CX = value;
                    InvalidateRender();
                }
            }
        }

        public double CY
        {
            get
            {
                return _CY;
            }
            set
            {
                if (value != _CY)
                {
                    _CY = value;
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

        public Point StartPoint
        {
            get
            {
                return _StartPoint;
            }
            set
            {
                if (value != _StartPoint)
                {
                    _StartPoint = value;
                    InvalidateRender();
                }
            }
        }

        public Point EndPoint
        {
            get
            {
                return _EndPoint;
            }
            set
            {
                if (value != _EndPoint)
                {
                    _EndPoint = value;
                    InvalidateRender();
                }
            }
        }

        public SweepDirection SweepDirection
        {
            get
            {
                return _SweepDirection;
            }
            set
            {
                if (value != _SweepDirection)
                {
                    _SweepDirection = value;
                    InvalidateRender();
                }
            }
        }

        public double RenderedRX
        {
            get
            {
                if (!GeometryTransform.Value.IsIdentity)
                {
                    return GeometryTransform.TransformBounds(Bounds).Width / 2;
                }
                else
                {
                    return RX;
                }
            }
        }

        public double RenderedRY
        {
            get
            {
                if (!GeometryTransform.Value.IsIdentity)
                {
                    return GeometryTransform.TransformBounds(Bounds).Height / 2;
                }
                else
                {
                    return RY;
                }
            }
        }

        public Point RenderedStartPoint
        {
            get
            {
                if (!GeometryTransform.Value.IsIdentity)
                {
                    return GeometryTransform.Transform(StartPoint);
                }
                else
                {
                    return StartPoint;
                }
            }
        }

        public Point RenderedEndPoint
        {
            get
            {
                if (!GeometryTransform.Value.IsIdentity)
                {
                    return GeometryTransform.Transform(EndPoint);
                }
                else
                {
                    return EndPoint;
                }
            }
        }

        public SweepDirection RenderedSweepDirection
        {
            get
            {
                if ((GeometryTransform.Value.M11 < 0) ^ (GeometryTransform.Value.M22 < 0))
                {
                    return SweepDirection == SweepDirection.Clockwise ?
                        SweepDirection.Counterclockwise :
                        SweepDirection.Clockwise;
                }
                else
                {
                    return SweepDirection;
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
            Rect bounds = RenderedBounds;
            CX = (bounds.Left + bounds.Right) / 2;
            CY = (bounds.Top + bounds.Bottom) / 2;
            RX = bounds.Width / 2;
            RY = bounds.Height / 2;
            StartPoint = GeometryTransform.Transform(StartPoint);
            EndPoint = GeometryTransform.Transform(EndPoint);

            if ((GeometryTransform.Value.M11 < 0) ^ (GeometryTransform.Value.M22 < 0))
            {
                SweepDirection =
                    SweepDirection == SweepDirection.Clockwise ?
                    SweepDirection.Counterclockwise :
                    SweepDirection.Clockwise;
            }

            GeometryTransform = null;
        }

        Point startPoint;

        public override void Place(Point position)
        {
            startPoint = position;

            CX = position.X;
            CY = position.Y;
            RX = RY = 0;
            StartPoint = position;
            EndPoint = position;
        }

        public override void Draw(Point point)
        {
            Rect bounds = new Rect(startPoint, point);

            CX = (bounds.Left + bounds.Right) / 2;
            CY = (bounds.Top + bounds.Bottom) / 2;
            RX = bounds.Width / 2;
            RY = bounds.Height / 2;
            StartPoint = new Point(CX, CY - RY);
            EndPoint = new Point(CX, CY + RY);
        }

        public override int HandleCount
        {
            get { return 6; }
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
                case 4:
                    return StartPoint;
                case 5:
                    return EndPoint;
                default:
                    return new Point();
            }
        }

        protected override void SetHandleInternal(int index, Point value)
        {
            if (index < 4)
            {
                double left = CX - RX;
                double top = CY - RY;
                double right = CX + RX;
                double bottom = CY + RY;
                double startAngle = StartAngle;
                double endAngle = EndAngle;

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
                StartAngle = startAngle;
                EndAngle = endAngle;
            }
            else if (index == 4)
            {
                double oldSweep = SweepMagnitude;
                StartPoint = value;
                StartPoint = new Point(CX + RX * Math.Cos(StartAngle), CY + RY * Math.Sin(StartAngle));
                double newSweep = SweepMagnitude;
                if (Math.Abs(newSweep - oldSweep) >= Math.PI)
                {
                    if (SweepDirection == SweepDirection.Clockwise)
                        SweepDirection = SweepDirection.Counterclockwise;
                    else
                        SweepDirection = SweepDirection.Clockwise;
                }
            }
            else if (index == 5)
            {
                double oldSweep = SweepMagnitude;
                EndPoint = value;
                EndPoint = new Point(CX + RX * Math.Cos(EndAngle), CY + RY * Math.Sin(EndAngle));
                double newSweep = SweepMagnitude;
                if (Math.Abs(newSweep - oldSweep) >= Math.PI)
                {
                    if (SweepDirection == SweepDirection.Clockwise)
                        SweepDirection = SweepDirection.Counterclockwise;
                    else
                        SweepDirection = SweepDirection.Clockwise;
                }

            }
        }

        public override PathGeometry ToPathGeometry()
        {
            var segment = new ArcSegment();
            segment.Point = EndPoint;
            segment.Size = new Size(RX, RY);
            segment.SweepDirection = SweepDirection;
            segment.RotationAngle = 0.0;
            segment.IsLargeArc = SweepMagnitude >= Math.PI;
            segment.IsStroked = true;
            segment.IsSmoothJoin = true;

            var figure = new PathFigure();
            figure.StartPoint = StartPoint;
            figure.Segments.Add(segment);

            var path = new PathGeometry();
            path.Figures.Add(figure);

            return path;
        }

        public override StreamGeometry ToStreamGeometry()
        {
            var geometry = new StreamGeometry();
            var gc = geometry.Open();
            gc.BeginFigure(StartPoint, FillBrush != null, false);
            gc.ArcTo(EndPoint, new Size(RX, RY), 0.0, SweepMagnitude >= Math.PI, SweepDirection, StrokeBrush != null, false);
            gc.Close();
            return geometry;
        }

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
                case SVGAttributes.CLASS:
                    string classNames = base.GetNodeAttribute(context, name) ?? "";
                    return DOMHelper.PrependClass(classNames, "semiEllipse");
                case SVGAttributes.D:
                    return PathData;
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        public override void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.D:
                    PathData = value;
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
                    PathData = "";
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
            return new SemiEllipse(this);
        }

        #endregion

        #region Implementation

        double StartAngle
        {
            get { return Math.Atan2(StartPoint.Y - CY, StartPoint.X - CX); }
            set { StartPoint = new Point(CX + RX * Math.Cos(value), CY + RY * Math.Sin(value)); }
        }

        double EndAngle
        {
            get { return Math.Atan2(EndPoint.Y - CY, EndPoint.X - CX); }
            set { EndPoint = new Point(CX + RX * Math.Cos(value), CY + RY * Math.Sin(value)); }
        }

        double SweepMagnitude
        {
            get
            {
                double sweep = EndAngle - StartAngle;
                if (SweepDirection == SweepDirection.Counterclockwise)
                    sweep = -sweep;
                if (sweep < 0)
                    sweep += 2.0 * Math.PI;
                return sweep;
            }
        }

        string PathData
        {
            get
            {
                // M sx sy A rx ry x-axis-rotation large-arc-flag sweep-flag x y
                //   sx,sy := start point
                //   rx,ry := horizontal and vertical radii
                //   x-axis-rotation := rotation of the ellipse
                //   large-arc-flag := '1' if arc >= 180 degrees; otherwise '0'
                //   sweep-flag := '1' to draw arc clockwise from start point to end point; otherwise '0'
                //   x,y := end point

                return String.Format(CultureInfo.InvariantCulture, "M{0},{1} A{2},{3} {4} {5} {6} {7},{8}", 
                    RenderedStartPoint.X,
                    RenderedStartPoint.Y, 
                    RenderedRX, 
                    RenderedRY, 
                    "0",
                    SweepMagnitude >= Math.PI ? "1" : "0", 
                    RenderedSweepDirection == SweepDirection.Clockwise ? "1" : "0", 
                    RenderedEndPoint.X,
                    RenderedEndPoint.Y);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    StartPoint = default(Point);
                    EndPoint = default(Point);
                    RX = RY = 0;
                    CX = CY = 0;
                    SweepDirection = SweepDirection.Clockwise;
                    return;
                }

                try
                {
                    PathGeometry geometry = NPath.ParseGeometry(value);
                    var figure = geometry.Figures.First();
                    var segment = figure.Segments.OfType<ArcSegment>().First();

                    StartPoint = figure.StartPoint;
                    EndPoint = segment.Point;
                    RX = segment.Size.Width;
                    RY = segment.Size.Height;
                    Point center = Arc.EllipseCenterFromPoints(StartPoint, EndPoint, RX, RY, 0.0, segment.IsLargeArc, segment.SweepDirection == SweepDirection.Clockwise);
                    CX = center.X;
                    CY = center.Y;
                    SweepDirection = segment.SweepDirection;
                }
                catch
                {
                    PathData = "";
                }
            }
        }

        #endregion
    }
}
