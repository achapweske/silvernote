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
using System.Windows.Input;
using DOM;
using DOM.CSS;

namespace SilverNote.Editor
{
    public class PrimaryCanvas : NCanvas
    {
        #region Fields

        NConnection[] _SelectedConnections;

        #endregion

        #region Constructors

        public PrimaryCanvas()
        {
            Initialize();
        }

        public PrimaryCanvas(PrimaryCanvas copy)
            : base(copy)
        {
            Initialize();
        }

        void Initialize()
        {
            SelectionAdorner = null;
        }

        #endregion

        #region IMovable

        public override void MoveStarted()
        {
            foreach (Shape drawing in Selection)
            {
                drawing.MoveStarted();
            }

            _SelectedConnections = GetConnections(SelectedSnaps);

            if (_SelectedConnections != null)
            {
                EnableSnaps(_SelectedConnections, false);
            }
        }

        public override void MoveDelta(Vector delta)
        {
            foreach (var drawing in Selection)
            {
                drawing.Offset += delta;
            }

            if (_SelectedConnections != null)
            {
                foreach (var connection in _SelectedConnections)
                {
                    connection.Drawing.MoveHandle(connection.Handle, delta);
                }
            }
        }

        public override void MoveCompleted()
        {
            foreach (Shape drawing in Selection)
            {
                drawing.MoveCompleted();
            }

            if (_SelectedConnections != null)
            {
                EnableSnaps(_SelectedConnections, true);
            }
        }

        static void EnableSnaps(NConnection[] connections, bool enable)
        {
            foreach (var connection in connections)
            {
                int snapIndex = connection.Drawing.SnapFromHandle(connection.Handle);
                if (snapIndex != -1)
                {
                    connection.Drawing.EnableSnap(snapIndex, enable);
                }
            }
        }

        #endregion

        #region IStyleable

        public override CSSValue GetStyleProperty(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.Right:
                case CSSProperties.Bottom:
                    return CSSValues.Auto;
                default:
                    return base.GetStyleProperty(context, name);
            }
        }

        public override void SetHTMLStyle(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.Left:
                case CSSProperties.Top:
                case CSSProperties.Right:
                case CSSProperties.Bottom:
                case CSSProperties.Position:
                    // Just to be safe (we should never be anything other than absolute-positioned)
                    break;
                default:
                    base.SetHTMLStyle(context, name, value);
                    break;
            }
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new PrimaryCanvas(this);
        }

        #endregion

        #region Implementation

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (DocumentElement.GetDebugFlags(this).HasFlag(NDebugFlags.ShowPosition) &&
                RenderSize.Width > 0 && RenderSize.Height > 0)
            {
                var borderPen = new Pen(Brushes.Blue, 1.0);
                borderPen.Freeze();
                var borderRect = new Rect(0, 0, RenderSize.Width - 1, RenderSize.Height - 1);
                dc.DrawRectangle(null, borderPen, borderRect);
            }
        }

        #endregion
    }
}
