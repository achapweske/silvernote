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
using DOM.Helpers;

namespace SilverNote.Editor
{
    public class QuadraticCurve : QuadraticBezier
    {
        #region Constructors

        public QuadraticCurve()
        {

        }

        public QuadraticCurve(double x1, double y1, double x2, double y2, double x3, double y3)
            : base(x1, y1, x2, y2, x3, y3)
        {
            StrokeBrush = Brushes.Black;
            StrokeWidth = 2;
        }

        public QuadraticCurve(Point point1, Point point2, Point point3)
            : base(point1, point2, point3)
        {
            StrokeBrush = Brushes.Black;
            StrokeWidth = 2;
        }

        public QuadraticCurve(QuadraticCurve copy)
            : base(copy)
        {
         
        }

        #endregion

        #region Attributes

        public double CX2
        {
            get { return 0.25 * X1 + 0.5 * X2 + 0.25 * X3; }
            set { X2 = -0.5 * X1 + 2 * value - 0.5 * X3; }
        }

        public double CY2
        {
            get { return 0.25 * Y1 + 0.5 * Y2 + 0.25 * Y3; }
            set { Y2 = -0.5 * Y1 + 2 * value - 0.5 * Y3; }
        }

        #endregion

        #region NDrawing

        protected override Point GetHandleInternal(int index)
        {
            if (index == 1)
                return new Point(CX2, CY2);
            else
                return base.GetHandleInternal(index);
        }

        protected override void SetHandleInternal(int index, Point value)
        {
            if (index == 1)
            {
                CX2 = value.X;
                CY2 = value.Y;
            }
            else
            {
                base.SetHandleInternal(index, value);
            }
        }

        #endregion

        #region INodeSource

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    string classNames = base.GetNodeAttribute(context, name) ?? "";
                    return DOMHelper.PrependClass(classNames, "quadraticCurve");
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new QuadraticCurve(this);
        }

        #endregion
    }
}
