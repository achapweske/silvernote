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
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SilverNote.Common;

namespace SilverNote.Editor
{
    public class SectionHandle : FrameworkElement
    {
        #region Constructors

        public SectionHandle()
        {
            Cursor = Cursors.SizeNS;
            Width = 16;
            Height = 10;
        }

        #endregion

        #region Events

        public event EventHandler Dragging;

        protected void RaiseDragging()
        {
            if (Dragging != null)
            {
                Dragging(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Implementation

        Point? _DragStartPosition;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _DragStartPosition = e.GetPosition(this);

            CaptureMouse();

            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_DragStartPosition != null)
            {
                _DragStartPosition = null;

                ReleaseMouseCapture();

                e.Handled = true;
            }
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_DragStartPosition != null)
            {
                var delta = _DragStartPosition.Value - e.GetPosition(this);
                if (delta.Length > 2)
                {
                    _DragStartPosition = null;

                    RaiseDragging();
                }

                e.Handled = true;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            var bounds = new Rect(0, 0, RenderSize.Width, RenderSize.Height);
            dc.DrawRectangle(Brushes.Transparent, null, bounds);

            var pen1 = new Pen(Brushes.DarkGray, 1);
            pen1.Freeze();          
            var pen2 = new Pen(Brushes.White, 1);
            pen2.Freeze();

            dc.DrawLine(pen1, new Point(0.5, 0.5), new Point(15.5, 0.5));
            dc.DrawLine(pen2, new Point(0.5, 1.5), new Point(15.5, 1.5));
            dc.DrawLine(pen1, new Point(0.5, 4.5), new Point(15.5, 4.5));
            dc.DrawLine(pen2, new Point(0.5, 5.5), new Point(15.5, 5.5));
            dc.DrawLine(pen1, new Point(0.5, 8.5), new Point(15.5, 8.5));
            dc.DrawLine(pen2, new Point(0.5, 9.5), new Point(15.5, 9.5));
        }

        #endregion
    }
}
