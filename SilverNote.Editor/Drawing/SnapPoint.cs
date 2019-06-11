/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using SilverNote.Common;
using DOM;
using DOM.SVG;
using DOM.Helpers;

namespace SilverNote.Editor
{
    public class SnapPoint : Line
    {
        #region Constructors

        public SnapPoint()
        {

        }

        public SnapPoint(double x, double y)
            : base(x, y, x, y)
        {

        }

        public SnapPoint(SnapPoint copy)
            : base(copy)
        {

        }

        #endregion

        #region NLine

        public override Shape ThumbSmall
        {
            get
            {
                SnapPoint result = (SnapPoint)base.ThumbSmall;
                result.X1 = result.X2 = 7.5;
                result.Y1 = result.Y2 = 7.5;
                return result;
            }
        }

        #endregion

        #region NDrawing

        public override void Place(Point position)
        {
            X1 = X2 = position.X;
            Y1 = Y2 = position.Y;
        }

        public override void Draw(Point point)
        {
            
        }

        public override int HandleCount
        {
            get { return 1; }
        }

        protected override Point GetHandleInternal(int index)
        {
            if (index == 0)
            {
                return new Point(X1, Y1);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        protected override void SetHandleInternal(int index, Point value)
        {
            if (index == 0)
            {
                X1 = X2 = value.X;
                Y1 = Y2 = value.Y;
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override int SnapCount
        {
            get { return 1; }
        }

        public override Point GetSnap(int index)
        {
            Point point = GetSnapInternal(index);

            return GeometryTransform.Transform(point);
        }

        public Point GetSnapInternal(int index)
        {
            if (index == 0)
            {
                return new Point(X1, Y1);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        protected override void OnRenderVisual(DrawingContext dc)
        {
            if (ParentDrawing != null)
            {
                return;
            }

            Point center = GeometryTransform.Transform(new Point(X1, Y1));

            Point point1 = new Point(center.X - 4, center.Y);
            Point point2 = new Point(center.X + 4, center.Y);
            Point point3 = new Point(center.X, center.Y - 4);
            Point point4 = new Point(center.X, center.Y + 4);

            var stroke = new Pen(Brushes.Gray, 2.0);
            stroke.Freeze();

            dc.DrawLine(stroke, point1, point2);
            dc.DrawLine(stroke, point3, point4);
        }

        #endregion

        #region INodeSource

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    string classNames = base.GetNodeAttribute(context, name) ?? "";
                    return DOMHelper.PrependClass(classNames, "snap");
                case SVGAttributes.STROKE:
                case SVGAttributes.FILL:
                case SVGAttributes.FILTER:
                case SVGAttributes.STROKE_WIDTH:
                    return null;
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        #endregion

        #region Object

        public override object Clone()
        {
            return new SnapPoint(this);
        }

        #endregion
    }
}
