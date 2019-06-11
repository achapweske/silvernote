/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SilverNote.Views
{
    public class WidgetManager
    {
        class WidgetContainer
        {
            public FrameworkElement Outer { get; set; }
            public StackPanel Inner { get; set; }
        }

        #region Fields

        private List<WidgetContainer> _WidgetContainers = new List<WidgetContainer>();
        private Canvas _WidgetCanavs;
        private Point? _StartPosition = null;
        private bool _IsDragging;
        private Rectangle _InsertionVisual;

        #endregion

        #region Constructors

        public WidgetManager()
        {
            _InsertionVisual = new Rectangle
            {
                Fill = Brushes.Gray
            };
        }

        #endregion

        #region Properties

        public Canvas WidgetCanvas
        {
            get
            {
                return _WidgetCanavs;
            }
            set
            {
                if (value != _WidgetCanavs)
                {
                    var oldValue = _WidgetCanavs;
                    _WidgetCanavs = value;
                    OnWidgetCanvasChanged(oldValue, value);
                }
            }
        }

        #endregion

        #region Operations

        public void AddContainer(FrameworkElement outer, StackPanel inner)
        {
            _WidgetContainers.Add(new WidgetContainer
            {
                Outer = outer,
                Inner = inner
            });

            inner.MouseLeftButtonDown += new MouseButtonEventHandler(Panel_MouseLeftButtonDown);
            inner.MouseMove += new MouseEventHandler(Panel_MouseMove);
            inner.MouseLeftButtonUp += new MouseButtonEventHandler(Panel_MouseLeftButtonUp);
        }

        #endregion

        #region Implementation

        private void OnDragStarted(object sender, MouseEventArgs e)
        {
            var widget = (FrameworkElement)LayoutHelper.ChildFromDescendant((DependencyObject)sender, (DependencyObject)e.Source);

            if (_WidgetContainers.Any(w => w.Inner == sender))
            {
                Point position = e.GetPosition(WidgetCanvas);
                Canvas.SetLeft(widget, position.X - _StartPosition.Value.X);
                Canvas.SetTop(widget, position.Y - _StartPosition.Value.Y);
                widget.Width = widget.ActualWidth;
                ((Panel)sender).Children.Remove(widget);
                WidgetCanvas.Children.Add(widget);
            }

            widget.Opacity = 0.9;
            
            Mouse.Capture(widget);
        }

        private void OnDragDelta(object sender, MouseEventArgs e)
        {
            var widget = (FrameworkElement)LayoutHelper.ChildFromDescendant((DependencyObject)sender, (DependencyObject)e.Source);

            // Update the widget's position on the canvas

            Point position = e.GetPosition(WidgetCanvas);
            Canvas.SetLeft(widget, position.X - _StartPosition.Value.X);
            Canvas.SetTop(widget, position.Y - _StartPosition.Value.Y);

            // Update insertion visual

            foreach (var container in _WidgetContainers)
            {
                position = e.GetPosition(container.Outer);
                if (container.Outer.InputHitTest(position) == null)
                {
                    continue;
                }

                if (!WidgetCanvas.Children.Contains(_InsertionVisual))
                {
                    WidgetCanvas.Children.Insert(0, _InsertionVisual);
                }

                UIElement target = GetInsertionTarget(container.Inner, e.GetPosition(container.Inner));
                Rect rect = GetInsertionRect(container.Inner, target);
                rect = container.Inner.TransformToVisual(WidgetCanvas).TransformBounds(rect);

                Canvas.SetLeft(_InsertionVisual, rect.X);
                Canvas.SetTop(_InsertionVisual, rect.Y);
                _InsertionVisual.Width = rect.Width;
                _InsertionVisual.Height = rect.Height;

                return;
            }

            WidgetCanvas.Children.Remove(_InsertionVisual);
        }

        private void OnDragCompleted(object sender, MouseEventArgs e)
        {
            var widget = (FrameworkElement)LayoutHelper.ChildFromDescendant((DependencyObject)sender, (DependencyObject)e.Source);
            widget.Opacity = 1.0;
            widget.ReleaseMouseCapture();

            foreach (var container in _WidgetContainers)
            {
                Point position = e.GetPosition(container.Outer);
                if (container.Outer.InputHitTest(position) != null)
                {
                    var target = GetInsertionTarget(container.Inner, e.GetPosition(container.Inner));

                    WidgetCanvas.Children.Remove(widget);
                    widget.Width = Double.NaN;

                    if (target != null)
                    {
                        int index = container.Inner.Children.IndexOf(target);
                        container.Inner.Children.Insert(index, widget);
                    }
                    else
                    {
                        container.Inner.Children.Add(widget);
                    }
                }
            }
            
            WidgetCanvas.Children.Remove(_InsertionVisual);
        }

        private static UIElement GetInsertionTarget(Panel panel, Point point)
        {
            foreach (UIElement child in panel.Children)
            {
                Rect rect = new Rect(child.RenderSize);
                rect = child.TransformToAncestor(panel).TransformBounds(rect);
                if (point.Y < (rect.Top + rect.Bottom) / 2.0)
                {
                    return child;
                }
            }
            return null;
        }

        private static Rect GetInsertionRect(Panel panel, UIElement child)
        {
            Rect rect = new Rect(0.0, 0.0, panel.ActualWidth, 2.0);
            if (child != null)
            {
                rect = child.TransformToAncestor(panel).TransformBounds(rect);
                rect.Y -= 6.0;
            }
            else
            {
                if (panel.Children.Count > 0)
                {
                    child = panel.Children[panel.Children.Count - 1];
                    rect = child.TransformToAncestor(panel).TransformBounds(rect);
                    rect.Y += child.RenderSize.Height + 4.0;
                }
            }
            return rect;
        }

        private void OnWidgetCanvasChanged(Canvas oldValue, Canvas newValue)
        {
            if (newValue != null)
            {
                newValue.MouseLeftButtonDown += new MouseButtonEventHandler(Panel_MouseLeftButtonDown);
                newValue.MouseMove += new MouseEventHandler(Panel_MouseMove);
                newValue.MouseLeftButtonUp += new MouseButtonEventHandler(Panel_MouseLeftButtonUp);
            }
        }

        private void Panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only process mouse events on a widget's header
            var content = LayoutHelper.GetSelfOrAncestor<ContentPresenter>((DependencyObject)e.OriginalSource);
            if (content != null && content.ContentSource != "Header")
            {
                return;
            }

            var widget = LayoutHelper.ChildFromDescendant((DependencyObject)sender, (DependencyObject)e.Source);
            if (widget == null)
            {
                return;
            }

            _StartPosition = e.GetPosition((IInputElement)widget);
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_StartPosition.HasValue)
            {
                return;
            }

            var widget = LayoutHelper.ChildFromDescendant((DependencyObject)sender, (DependencyObject)e.Source) as FrameworkElement;
            if (widget == null)
            {
                return;
            }

            if (_IsDragging)
            {
                OnDragDelta(sender, e);
                return;
            }

            Point position = e.GetPosition(widget);
            Vector vector = position - _StartPosition.Value;
            if (Math.Abs(vector.X) >= SystemParameters.MinimumHorizontalDragDistance || System.Math.Abs(vector.Y) >= SystemParameters.MinimumVerticalDragDistance)
            {
                OnDragStarted(sender, e);
                _IsDragging = true;
            }
        }

        private void Panel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_IsDragging)
            {
                OnDragCompleted(sender, e);
                _IsDragging = false;
            }
            _StartPosition = null;
        }

        #endregion
    }
}
