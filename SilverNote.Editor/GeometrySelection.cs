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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace SilverNote.Editor
{
    public class GeometrySelection : Adorner
    {
        public GeometrySelection(UIElement adornedElement)
            : base(adornedElement)
        {
            Children = new VisualCollection(this);
        }

        public Func<DependencyObject, bool> Filter { get; set; }

        public Action<Point> Started { get; set; }

        public Action<IEnumerable<UIElement>, Geometry> Completed { get; set; }

        public Action<UIElement, Geometry> Selected { get; set; }

        public Action<UIElement> Unselected { get; set; }

        public bool IsSelecting { get; private set; }

        public Point StartPoint { get; private set; }

        private RectangleGeometry _Geometry = new RectangleGeometry();

        public Geometry Geometry
        {
            get { return _Geometry; }
        }

        public void Begin(Point startPoint)
        {
            if (!IsSelecting)
            {
                IsSelecting = true;

                StartPoint = startPoint;

                Selection.Clear();

                ShowSelectionVisual();

                CaptureMouse();

                Started(startPoint);
            }
        }

        public void SelectTo(Point nextPoint)
        {
            if (IsSelecting)
            {
                double left = Math.Min(StartPoint.X, nextPoint.X);
                double top = Math.Min(StartPoint.Y, nextPoint.Y);
                double right = Math.Max(StartPoint.X, nextPoint.X);
                double bottom = Math.Max(StartPoint.Y, nextPoint.Y);
                double width = right - left;
                double height = bottom - top;

                _Geometry.Rect = new Rect(left, top, width, height);

                DrawSelectionVisual();

                SelectGeometry(Geometry);
            }
        }

        public void End()
        {
            if (IsSelecting)
            {
                IsSelecting = false;

                HideSelectionVisual();

                ReleaseMouseCapture();

                Completed(Selection, Geometry);
            }
        }

        public void Cancel()
        {
            Selection.Clear();

            End();
        }

        HashSet<UIElement> Selection = new HashSet<UIElement>();

        public virtual void SelectGeometry(Geometry geometry)
        {
            var newSelection = SelectionFromGeometry(geometry);
            var oldSelection = Selection.Except(newSelection).ToArray();

            foreach (var item in oldSelection)
            {
                Selection.Remove(item);
                Unselected(item);
            }

            Selection.Clear();

            foreach (var item in newSelection)
            {
                Selection.Add(item);
                Selected(item, geometry);
            }
        }

        private IEnumerable<UIElement> SelectionFromGeometry(Geometry geometry)
        {
            var result = LayoutHelper.ChildrenFromGeometry(AdornedElement, geometry, Filter);

            return result.OfType<UIElement>();
        }

        private DrawingVisual SelectionVisual = new DrawingVisual();

        private void ShowSelectionVisual()
        {
            if (!Children.Contains(SelectionVisual))
            {
                Children.Add(SelectionVisual);
            }
        }

        private void HideSelectionVisual()
        {
            Children.Remove(SelectionVisual);
        }

        private void DrawSelectionVisual()
        {
            var dc = SelectionVisual.RenderOpen();
            var guidelines = new GuidelineSet(new double[] { 0.5 }, new double[] { 0.5 });
            dc.PushGuidelineSet(guidelines);
            var stroke = new Pen(Brushes.Black, 1.0);
            stroke.DashStyle = DashStyles.Dash;
            dc.DrawGeometry(null, stroke, Geometry);
            dc.Close();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Cursor = Cursors.Cross;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);

            Begin(position);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                return;
            }

            var position = e.GetPosition(this);

            if (!IsSelecting)
            {
                Begin(position);
            }
            else
            {
                SelectTo(position);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            End();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                Cancel();
                e.Handled = true;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            base.OnRender(dc);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            base.MeasureOverride(availableSize);

            return AdornedElement.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }
        protected VisualCollection Children { get; private set; }

        protected override int VisualChildrenCount
        {
            get { return Children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Children[index];
        }
    }
}
