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
using DOM.Helpers;

namespace SilverNote.Editor
{
    public abstract class LineBase : Shape
    {
        #region Fields

        Marker _MarkerStart;
        Marker _MarkerEnd;

        #endregion

        #region Constructors

        public LineBase()
        {
            IsConnector = false;
        }

        public LineBase(LineBase copy)
            : base(copy)
        {
            IsConnector = copy.IsConnector;
        }

        #endregion

        #region Properties

        public virtual LineBase LineThumb
        {
            get
            {
                var result = (LineBase)Clone();
                result.RenderedBounds = new Rect(0, 0, 100, 12);
                result.IsThumb = true;
                return result;
            }
        }

        #endregion

        #region Attributes

        public bool IsConnector { get; set; }

        public virtual Point StartPoint { get; set; }

        public virtual Point EndPoint { get; set; }

        public Marker MarkerStart
        {
            get
            {
                return _MarkerStart;
            }
            set
            {
                if (value != _MarkerStart)
                {
                    _MarkerStart = value;
                    _MarkerStart.Canvas = Canvas;
                    InvalidateRender();
                }
            }
        }

        public Marker MarkerEnd
        {
            get
            {
                return _MarkerEnd;
            }
            set
            {
                if (value != _MarkerEnd)
                {
                    _MarkerEnd = value;
                    _MarkerEnd.Canvas = Canvas;
                    InvalidateRender();
                }
            }
        }

        public override Brush FillBrush
        {
            get { return null; }
            set { ; }
        }

        #endregion

        #region Drawing

        protected override void OnRenderDecorations(DrawingContext dc)
        {
            if (IsThumb && IsConnector)
            {
                var startPoint = GeometryTransform.Transform(StartPoint);
                OnRenderHandle(dc, startPoint);
                var endPoint = GeometryTransform.Transform(EndPoint);
                OnRenderHandle(dc, endPoint);
            }
        }

        protected override void OnRenderHandle(DrawingContext dc, Point position)
        {
            if (!IsConnector)
            {
                base.OnRenderHandle(dc, position);
                return;
            }

            double zoom = NoteEditor.GetZoom(this);

            position = LayoutHelper.Align(position, 1.0, zoom);

            double width = (HandleSize.Width + 1) / zoom;
            double height = (HandleSize.Height + 1) / zoom;
            double x = position.X - width / 2;
            double y = position.Y - height / 2;
            Rect rect = new Rect(x, y, width, height);
            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

            double thickness = 1.0 / zoom;
            Pen stroke = new Pen(Brushes.Black, thickness);
            stroke.Freeze();
            Brush fill = Brushes.White;

            dc.DrawEllipse(fill, stroke, center, rect.Width / 2, rect.Height / 2);

            rect.Inflate(2.0 / zoom, 2.0 / zoom);
            dc.DrawRectangle(Brushes.Transparent, null, rect);
        }

        #endregion

        #region INodeSource

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    string classNames = base.GetNodeAttribute(context, name);
                    if (IsConnector)
                        classNames = DOMHelper.PrependClass(classNames ?? "", "connector");
                    return classNames;
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        public override void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    IsConnector = DOMHelper.HasClass(value, "connector");
                    value = DOMHelper.RemoveClass(value, "connector");
                    base.SetNodeAttribute(context, name, value);
                    break;
                default:
                    base.SetNodeAttribute(context, name, value);
                    break;
            }
        }

        #endregion

        #region Implementation

        protected override void OnCanvasChanged(NCanvas oldCanvas)
        {
            base.OnCanvasChanged(oldCanvas);

            if (MarkerStart != null)
            {
                MarkerStart.Canvas = Canvas;
            }

            if (MarkerEnd != null)
            {
                MarkerEnd.Canvas = Canvas;
            }
        }

        #endregion

    }
}
