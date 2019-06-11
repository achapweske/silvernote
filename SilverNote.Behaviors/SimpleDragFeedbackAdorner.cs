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
using System.Windows.Documents;
using System.Windows.Media;

namespace SilverNote.Behaviors
{
    public class SimpleDragFeedbackAdorner : Adorner
    {
        #region Fields

        VisualCollection _Children;

        #endregion

        #region Constructors

        public SimpleDragFeedbackAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            _Children = new VisualCollection(this);
            IsHitTestVisible = false;
        }

        #endregion

        #region Properties

        public Point Position
        {
            get
            {
                return GetPosition();
            }
            set
            {
                SetPosition(value);
            }
        }

        public Visual Content
        {
            get
            {
                return GetContent();
            }
            set
            {
                SetContent(value);
            }
        }

        #endregion

        #region Implementation

        private Point GetPosition()
        {
            var transform = RenderTransform as TranslateTransform;
            if (transform != null)
            {
                return new Point(transform.X, transform.Y);
            }
            else
            {
                return default(Point);
            }
        }

        private void SetPosition(Point newValue)
        {
            var transform = RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                RenderTransform = transform;
            }
            transform.X = newValue.X;
            transform.Y = newValue.Y;
        }

        private Visual GetContent()
        {
            if (_Children.Count > 0)
            {
                return _Children[0];
            }
            else
            {
                return null;
            }
        }

        private void SetContent(Visual newValue)
        {
            if (!_Children.Contains(newValue))
            {
                _Children.Clear();
                _Children.Add(newValue);
            }
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return _Children.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _Children[index];
        }

        #endregion
    }
}
