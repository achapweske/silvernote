/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SilverNote.Editor
{
    public class InteractivePanel : EditingPanel, ISnappable
    {
        #region Constructors

        public InteractivePanel()
        {
            Initialize();
        }

        public InteractivePanel(InteractivePanel copy)
            : base(copy)
        {
            Initialize();
        }

        void Initialize()
        {
            Cursor = Cursors.IBeam;

            MinorGridCellSize = new Size(20, 20);
            MajorGridCellSize = new Size(100, 100);
            MinorGridLineThickness = 1;
            MajorGridLineThickness = 2;
            var gridBrush = new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xFF));
            gridBrush.Freeze();
            MinorGridLineBrush = gridBrush;
            MajorGridLineBrush = gridBrush;

            var guideBrush = new SolidColorBrush(Color.FromRgb(0xC8, 0xC8, 0xFF));
            guideBrush.Freeze();
            GuidelineBrush = guideBrush;
            GuidelineThickness = 2;

            DropHandlers.Add(new FileDropHandler(this));
            DropHandlers.Add(new DefaultDropHandler(this));

            LoadCommandBindings();
        }

        #endregion

        #region Selection

        private bool _IsSelectingGeometry = false;

        public virtual bool IsSelectingGeometry 
        {
            get 
            { 
                return _IsSelectingGeometry; 
            }
            set
            {
                if (value != _IsSelectingGeometry)
                {
                    _IsSelectingGeometry = value;
                    OnIsSelectingGeometryChanged();
                }
            }
        }

        protected void OnIsSelectingGeometryChanged()
        {
            if (IsSelectingGeometry)
            {
                AddAdorner(GeometrySelection);
            }
            else
            {
                RemoveAdorner(GeometrySelection);
            }
        }

        private GeometrySelection _GeometrySelection;

        protected GeometrySelection GeometrySelection
        {
            get
            {
                if (_GeometrySelection == null)
                {
                    _GeometrySelection = new GeometrySelection(this);
                    _GeometrySelection.Filter = GeometrySelection_Filter;
                    _GeometrySelection.Started = GeometrySelection_Started;
                    _GeometrySelection.Completed = GeometrySelection_Completed;
                    _GeometrySelection.Selected = GeometrySelection_Selected;
                    _GeometrySelection.Unselected = GeometrySelection_Unselected;
                }

                return _GeometrySelection;
            }
        }

        protected static bool GeometrySelection_Filter(DependencyObject dep)
        {
            return GetPositioning(dep) != Positioning.Static;
        }

        protected void GeometrySelection_Started(Point startPoint)
        {
            Selection.UnselectAll();
        }

        public virtual void GeometrySelection_Completed(IEnumerable<UIElement> selection, Geometry geometry)
        {
            Selection.SelectRangeOnly(selection);

            IsSelectingGeometry = false;
        }

        protected void GeometrySelection_Selected(UIElement item, Geometry geometry)
        {
            var canvas = item as NCanvas;
            if (canvas != null)
            {
                geometry = LayoutHelper.TransformToDescendant(this, item, geometry);

                canvas.Preselect(geometry);
            }
            else
            {
                Selection.Select(item);
            }
        }

        protected void GeometrySelection_Unselected(UIElement item)
        {
            var canvas = item as NCanvas;
            if (canvas != null)
            {
                canvas.UnpreselectAll();
            }
            else
            {
                Selection.Unselect(item);
            }
        }

        #endregion

        #region Erasing

        public virtual bool IsErasing { get; set; }

        protected void Erase(Point point)
        {
            var drawings = NCanvas.DrawingsFromPoint(point, this);

            foreach (var drawing in drawings)
            {
                // Remove the drawing from the canvas

                var canvas = drawing.Canvas;
                Point relativePoint = TransformToDescendant(canvas).Transform(point);
                canvas.Erase(drawing, relativePoint);

                // Remove the canvas if empty
                if (!(canvas is PrimaryCanvas))
                {
                    if (canvas.Drawings.Count == 0)
                    {
                        Remove(canvas);
                    }
                }
            }
        }

        #endregion

        #region Filling

        public virtual Brush FillBrush { get; set; }

        public virtual bool IsFilling { get; set; }

        /// <summary>
        /// Apply the current stroke settings to all selected drawings
        /// </summary>
        /// <returns>true on success, or false if no drawings selected</returns>
        public bool Fill()
        {
            return Fill(FillBrush);
        }

        public bool Fill(Brush brush)
        {
            if (brush == Brushes.Transparent)
            {
                brush = null;   // We don't want clickable backgrounds
            }

            if (!Selection.OfType<NCanvas>().Any(canvas => canvas.Selection.Any()))
            {
                return false;   // No drawings selected
            }

            using (var undo = new UndoScope(UndoStack, "Fill"))
            {
                SetProperty(Shape.FillProperty, brush);
            }

            return true;
        }

        protected void Fill(Point point)
        {
            var drawings = NCanvas.DrawingsFromPoint(point, this);

            Fill(drawings);
        }

        public void Fill(IEnumerable<Shape> drawings)
        {
            using (UndoScope undo = new UndoScope(UndoStack, "Stroke"))
            {
                foreach (var drawing in drawings)
                {
                    Fill(drawing);
                }
            }
        }

        public void Fill(Shape drawing)
        {
            Fill(drawing, FillBrush);
        }

        public void Fill(Shape drawing, Brush brush)
        {
            if (brush == Brushes.Transparent)
            {
                brush = null;   // We don't want clickable backgrounds
            }

            drawing.Canvas.SetProperty(drawing, Shape.FillProperty, brush);
        }

        #endregion

        #region Stroking

        public virtual bool IsStroking { get; set; }

        public virtual Brush StrokeBrush { get; set; }

        public virtual double StrokeWidth { get; set; }

        public virtual DoubleCollection StrokeDashArray { get; set; }

        public virtual PenLineCap StrokeDashCap { get; set; }

        /// <summary>
        /// Apply the current stroke settings to all selected drawings
        /// </summary>
        /// <returns>true on success, or false if no drawings selected</returns>
        public bool Stroke()
        {
            if (!Selection.OfType<NCanvas>().Any(canvas => canvas.Selection.Any()))
            {
                return false;   // No drawings selected
            }

            using (UndoScope undo = new UndoScope(UndoStack, "Stroke"))
            {
                Stroke(StrokeBrush);
                Stroke(StrokeWidth);
                Stroke(StrokeDashArray);
                Stroke(StrokeDashCap);
            }

            return true;
        }

        protected void Stroke(Point point)
        {
            var drawings = NCanvas.DrawingsFromPoint(point, this);

            Stroke(drawings);
        }

        public void Stroke(IEnumerable<Shape> drawings)
        {
            using (var undo = new UndoScope(UndoStack, "Stroke"))
            {
                foreach (var drawing in drawings)
                {
                    Stroke(drawing, StrokeBrush);
                    Stroke(drawing, StrokeWidth);
                    Stroke(drawing, StrokeDashArray);
                    Stroke(drawing, StrokeDashCap);
                }
            }
        }

        public void Stroke(Brush brush)
        {
            SetProperty(Shape.StrokeProperty, brush);
        }

        public void Stroke(Shape drawing, Brush brush)
        {
            drawing.Canvas.SetProperty(drawing, Shape.StrokeProperty, brush);
        }

        public void Stroke(double width)
        {
            SetProperty(Shape.StrokeWidthProperty, width);
        }

        public void Stroke(Shape drawing, double width)
        {
            drawing.Canvas.SetProperty(drawing, Shape.StrokeWidthProperty, width);
        }

        public void Stroke(DoubleCollection dashArray)
        {
            SetProperty(Shape.StrokeDashProperty, dashArray);
        }

        public void Stroke(Shape drawing, DoubleCollection dashArray)
        {
            drawing.Canvas.SetProperty(drawing, Shape.StrokeDashProperty, dashArray);
        }

        public void Stroke(PenLineCap lineCap)
        {
            SetProperty(Shape.StrokeLineCapProperty, lineCap);
        }

        public void Stroke(Shape drawing, PenLineCap lineCap)
        {
            drawing.Canvas.SetProperty(drawing, Shape.StrokeLineCapProperty, lineCap);
        }

        #endregion

        #region Moving

        public bool IsMovingChildren { get; private set; }

        public Rect MovingRegion { get; private set; }

        protected override void BeginMoveSelection()
        {
            using (new UndoScope(UndoStack, "Begin Moving"))
            {
                Selection.UnselectWhere(IsStaticParagraph);

                IsMovingChildren = true;

                MovingRegion = VisualElement.GetVisualBounds(Selection.Where(IsMovable), this);

                base.BeginMoveSelection();
            }
        }

        protected override void MoveSelection(Vector delta)
        {
            SnapSelection(ref delta);

            base.MoveSelection(delta);

            if (!MovingRegion.IsEmpty)
            {
                MovingRegion = Rect.Offset(MovingRegion, delta);
            }
        }

        protected override void EndMoveSelection()
        {
            using (new UndoScope(UndoStack, "End Moving"))
            {
                HideGuidelines();
                HideGuidepoints();
                HideSnapPoints();

                IsMovingChildren = false;

                MovingRegion = Rect.Empty;

                base.EndMoveSelection();
            }
        }

        private static bool IsMovable(UIElement element)
        {
            return element is IMovable && GetPositioning(element) != Positioning.Static;
        }

        private static bool IsStaticParagraph(UIElement element)
        {
            return element is TextParagraph && GetPositioning(element) == Positioning.Static;
        }

        #endregion

        #region ISnappable

        public bool Snap(Point[] points, ref Vector delta)
        {
            if (SnapToGuidepoints(points, ref delta))
            {
                return true;
            }

            if (!IsGridVisible)
            {
                return SnapToGuidelines(points, ref delta);
            }

            if (SnapToGridlines(points, ref delta))
            {
                SnapToGuidepoints(points, ref delta);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void SnapSelection(ref Vector delta)
        {
            if (Selection.Count == 1)
            {
                var canvas = Selection.Last() as NCanvas;
                if (canvas != null && canvas.Selection.Count == 1)
                {
                    var points = LayoutHelper.TransformToAncestor(canvas, this, canvas.SelectedSnaps);
                    Snap(points, ref delta);
                    return;
                }
            }

            this.Snap(MovingRegion, ref delta);
        }

        #endregion

        #region Guidelines

        public virtual bool IsGuidelinesEnabled { get; set; }

        protected bool SnapToGuidelines(Point[] points, ref Vector delta)
        {
            if (!IsGuidelinesEnabled)
            {
                return false;
            }

            var bounds = Rect.Empty;
            foreach (var point in points)
            {
                if (bounds.IsEmpty)
                    bounds = new Rect(point + delta, new Size(0, 0));
                else
                    bounds.Union(point + delta);
            }

            double leftGuideline = 20;
            double rightGuideline = RenderSize.Width - 20;
            //double centerGuideline = RenderSize.Width / 2;

            Vector snappedDelta = new Vector(Double.PositiveInfinity, Double.PositiveInfinity);
            double snappedGuideline = 0;

            double snapLeft = leftGuideline - bounds.Left;
            if (Math.Abs(snapLeft) < Math.Abs(snappedDelta.X))
            {
                snappedDelta.X = snapLeft;
                snappedGuideline = leftGuideline;
            }
            double snapRight = rightGuideline - bounds.Right;
            if (Math.Abs(snapRight) < Math.Abs(snappedDelta.X))
            {
                snappedDelta.X = snapRight;
                snappedGuideline = rightGuideline;
            }
            /*
            double snapCenter = centerGuideline - (bounds.Left + bounds.Right) / 2;
            if (Math.Abs(snapCenter) < Math.Abs(snappedDelta.X))
            {
                snappedDelta.X = snapCenter;
                snappedGuideline = centerGuideline;
            }
            */

            if (Math.Abs(snappedDelta.X) >= 5)
            {
                HideGuidelines();
                return false;
            }

            delta.X += snappedDelta.X;

            DrawGuideline(new Point(snappedGuideline, 0), new Point(snappedGuideline, RenderSize.Height));

            return true;
        }

        #region GuidelineVisual

        private DrawingVisual GuidelineVisual { get; set; }

        public Brush GuidelineBrush { get; set; }

        public double GuidelineThickness { get; set; }

        protected void DrawGuideline(Point startPoint, Point endPoint)
        {
            if (GuidelineVisual == null)
            {
                GuidelineVisual = new DrawingVisual();
                BackgroundVisuals.Add(GuidelineVisual);
            }

            var dc = GuidelineVisual.RenderOpen();
            {
                Pen stroke = new Pen(GuidelineBrush, GuidelineThickness);
                Point point0 = new Point(Math.Round(startPoint.X) + 0.5, Math.Round(startPoint.Y) + 0.5);
                Point point1 = new Point(Math.Round(endPoint.X) + 0.5, Math.Round(endPoint.Y) + 0.5);
                dc.DrawLine(stroke, point0, point1);
            }
            dc.Close();
        }

        protected void HideGuidelines()
        {
            if (GuidelineVisual != null)
            {
                BackgroundVisuals.Remove(GuidelineVisual);
                GuidelineVisual = null;
            }
        }

        #endregion

        #endregion

        #region Guidepoints

        protected bool SnapToGuidepoints(Point[] points, ref Vector delta)
        {
            var drawings = ShapeHitTest.HitAll ( 
                this, 
                points,  
                ShapeHitTestFlags.HitDrawings | ShapeHitTestFlags.HitSnaps | ShapeHitTestFlags.ExcludeSelected
            );

            var snapPoints = new HashSet<Point>();

            foreach (var drawing in drawings)
            {
                int snapCount = drawing.SnapCount;

                for (int i = 0; i < snapCount; i++)
                {
                    if (drawing.IsSnapEnabled(i))
                    {
                        Point snapPoint = drawing.GetSnap(i);

                        snapPoint = drawing.TransformToAncestor(this).Transform(snapPoint);

                        snapPoints.Add(snapPoint);
                    }
                }
            }

            DrawSnapPoints(snapPoints);

            foreach (var point in points)
            {
                var testPoint = point + delta;

                foreach (var snap in snapPoints)
                {
                    if (testPoint.X >= snap.X - 2 &&
                        testPoint.X <= snap.X + 2 &&
                        testPoint.Y >= snap.Y - 2 &&
                        testPoint.Y <= snap.Y + 2)
                    {
                        delta = snap - point;
                        HideGuidelines();
                        DrawGuidepoint(snap);
                        return true;
                    }
                }
            }

            HideGuidepoints();
            return false;
        }

        #region SnapPointsVisual

        private DrawingVisual SnapPointsVisual { get; set; }

        protected void DrawSnapPoints(IEnumerable<Point> points)
        {
            if (SnapPointsVisual == null)
            {
                SnapPointsVisual = new DecoratorDrawingVisual();
                ForegroundVisuals.Add(SnapPointsVisual);
            }

            var pen = new Pen(Brushes.Gray, 1.0);
            pen.Freeze();

            var dc = SnapPointsVisual.RenderOpen();
            try
            {
                foreach (var point in points)
                {
                    var point0 = new Point(point.X - 4, point.Y);
                    var point1 = new Point(point.X + 4, point.Y);

                    dc.DrawLine(pen, point0, point1);

                    point0 = new Point(point.X, point.Y - 4);
                    point1 = new Point(point.X, point.Y + 4);

                    dc.DrawLine(pen, point0, point1);
                }
            }
            finally
            {
                dc.Close();
            }
        }

        protected void HideSnapPoints()
        {
            if (SnapPointsVisual != null)
            {
                ForegroundVisuals.Remove(SnapPointsVisual);
                SnapPointsVisual = null;
            }
        }

        #endregion

        #region GuidepointVisual

        private DrawingVisual GuidepointVisual { get; set; }

        protected void DrawGuidepoint(Point guidepoint)
        {
            if (GuidepointVisual == null)
            {
                GuidepointVisual = new DecoratorDrawingVisual();
                ForegroundVisuals.Add(GuidepointVisual);
            }

            var stroke = new Pen(Brushes.Black, 2.0);
            stroke.Freeze();
            var fill = Brushes.White;

            var dc = GuidepointVisual.RenderOpen();
            try
            {
                var rect = new Rect(guidepoint.X - 2, guidepoint.Y - 2, 4, 4);

                dc.DrawRectangle(fill, stroke, rect);
            }
            finally
            {
                dc.Close();
            }
        }

        protected void HideGuidepoints()
        {
            if (GuidepointVisual != null)
            {
                ForegroundVisuals.Remove(GuidepointVisual);
                GuidepointVisual = null;
            }
        }

        #endregion

        #endregion

        #region Gridlines

        protected bool SnapToGridlines(Point[] points, ref Vector delta)
        {
            var snapPoint = new Point(Double.NaN, Double.NaN);

            var snapVector = new Vector(Double.PositiveInfinity, Double.PositiveInfinity);

            var snappingGridCellSize = SnappingGridCellSize;

            foreach (var point in points)
            {
                var target = point + delta;

                double pointX = Math.Round(target.X / snappingGridCellSize.Width) * snappingGridCellSize.Width;
                double vectorX = pointX - target.X;

                if (Math.Abs(vectorX) < Math.Abs(snapVector.X))
                {
                    snapPoint.X = pointX;
                    snapVector.X = vectorX;
                }

                double pointY = Math.Round(target.Y / snappingGridCellSize.Height) * snappingGridCellSize.Height;
                double vectorY = pointY - target.Y;

                if (Math.Abs(vectorY) < Math.Abs(snapVector.Y))
                {
                    snapPoint.Y = pointY;
                    snapVector.Y = vectorY;
                }
            }

            if (!Double.IsNaN(snapPoint.X))
            {
                delta.X += snapVector.X;
            }

            if (!Double.IsNaN(snapPoint.Y))
            {
                delta.Y += snapVector.Y;
            }

            return !Double.IsNaN(snapPoint.X) || !Double.IsNaN(snapPoint.Y);
        }

        #region GridVisual

        private bool _IsGridVisible;

        public virtual bool IsGridVisible
        {
            get { return _IsGridVisible; }
            set
            {
                _IsGridVisible = value;
                OnIsGridVisibleChanged();
            }
        }

        private void OnIsGridVisibleChanged()
        {
            UpdateBackground();
        }

        private void UpdateBackground()
        {
            if (IsGridVisible)
            {
                Background = GridDrawingBrush;
            }
            else
            {
                Background = Brushes.Transparent;
            }
        }

        private Brush _GridDrawingBrush;

        private Brush GridDrawingBrush
        {
            set { _GridDrawingBrush = value; }
            get
            {
                if (_GridDrawingBrush == null)
                {
                    _GridDrawingBrush = CreateGridBrush();
                }

                return _GridDrawingBrush;
            }
        }

        private Brush CreateGridBrush()
        {
            double zoom = this.Zoom;
            double scaleFactor = RoundToNearestPowerOf(zoom, 2);

            double minorGridCellWidth = MinorGridCellSize.Width / scaleFactor;
            double minorGridCellHeight = MinorGridCellSize.Height / scaleFactor;
            double minorGridLineThickness = MinorGridLineThickness / zoom;

            double majorGridCellWidth = MajorGridCellSize.Width / scaleFactor;
            double majorGridCellHeight = MajorGridCellSize.Height / scaleFactor;
            double majorGridLineThickness = MajorGridLineThickness / zoom;

            if (scaleFactor > 1)
            {
                double log = Math.Log(scaleFactor, 2);
                majorGridCellWidth = minorGridCellWidth * Math.Pow(2, 1 + log % 2);
                majorGridCellHeight = minorGridCellHeight * Math.Pow(2, 1 + log % 2);
            }

            double width = majorGridCellWidth * 2;
            double height = majorGridCellHeight * 2;

            // Minor grid lines
            var minorGridGeometry = new StreamGeometry();
            var gc = minorGridGeometry.Open();
            // horizontal
            for (double y = 0; y <= height; y += minorGridCellHeight)
            {
                var startPoint = new Point(0, y);
                startPoint = LayoutHelper.Align(startPoint, minorGridLineThickness, zoom);
                gc.BeginFigure(startPoint, false, false);
                var endPoint = new Point(width, startPoint.Y);
                gc.LineTo(endPoint, true, false);
            }
            // vertical
            for (double x = 0; x <= width; x += minorGridCellWidth)
            {
                var startPoint = new Point(x, 0);
                startPoint = LayoutHelper.Align(startPoint, minorGridLineThickness, zoom);
                gc.BeginFigure(startPoint, false, false);
                var endPoint = new Point(startPoint.X, height);
                gc.LineTo(endPoint, true, false);
            }
            gc.Close();

            // Major grid lines
            var majorGridGeometry = new StreamGeometry();
            gc = majorGridGeometry.Open();
            // horizontal
            for (double y = 0; y <= height; y += majorGridCellHeight)
            {
                var startPoint = new Point(0, y);
                startPoint = LayoutHelper.Align(startPoint, majorGridLineThickness, zoom);
                gc.BeginFigure(startPoint, false, false);
                var endPoint = new Point(width, startPoint.Y);
                gc.LineTo(endPoint, true, false);
            }
            // vertical
            for (double x = 0; x <= width; x += majorGridCellWidth)
            {
                var startPoint = new Point(x, 0);
                startPoint = LayoutHelper.Align(startPoint, majorGridLineThickness, zoom);
                gc.BeginFigure(startPoint, false, false);
                var endPoint = new Point(startPoint.X, height);
                gc.LineTo(endPoint, true, false);
            }
            gc.Close();

            var drawing = new DrawingGroup();
            Pen minorGridStroke = new Pen(MinorGridLineBrush, minorGridLineThickness);
            drawing.Children.Add(new GeometryDrawing(null, minorGridStroke, minorGridGeometry));
            Pen majorGridStroke = new Pen(MajorGridLineBrush, majorGridLineThickness);
            drawing.Children.Add(new GeometryDrawing(null, majorGridStroke, majorGridGeometry));

            var brush = new DrawingBrush();
            brush.Drawing = drawing;
            brush.Stretch = Stretch.None;
            brush.ViewportUnits = BrushMappingMode.Absolute;
            brush.Viewport = new Rect(width / 4, height / 4, width / 2, height / 2);
            brush.TileMode = TileMode.Tile;

            return brush;
        }

        private static double RoundToNearestPowerOf(double value, double exp)
        {
            value = Math.Log(value, exp);
            value = Math.Round(value);
            value = Math.Pow(exp, value);

            return value;
        }

        public Size MinorGridCellSize { get; set; }

        public Brush MinorGridLineBrush { get; set; }

        public double MinorGridLineThickness { get; set; }

        public Size MajorGridCellSize { get; set; }

        public Brush MajorGridLineBrush { get; set; }

        public double MajorGridLineThickness { get; set; }

        public Size SnappingGridCellSize
        {
            get
            {
                double scaleFactor = RoundToNearestPowerOf(Zoom, 2);

                Size result = new Size ( 
                    MinorGridCellSize.Width / scaleFactor, 
                    MinorGridCellSize.Height / scaleFactor
                );
                while (result.Width >= 18)
                {
                    result.Width /= 2;
                }
                while (result.Height >= 18)
                {
                    result.Height /= 2;
                }
                return result;
            }
        }

        protected override void  OnZoomChanged(double oldValue, double newValue)
        {
            base.OnZoomChanged(oldValue, newValue);

            GridDrawingBrush = null;

            UpdateBackground();
        }

        #endregion

        #endregion

        #region Panning

        private bool _IsPanning;

        public bool IsPanning
        {
            get { return _IsPanning; }
        }

        private Point _PanStartPoint;
        private double _PanStartScrollX;
        private double _PanStartScrollY;

        protected void BeginPanning(Point startPoint)
        {
            _PanStartPoint = startPoint;

            var scrollViewer = LayoutHelper.GetAncestor<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                _PanStartScrollX = scrollViewer.HorizontalOffset;
                _PanStartScrollY = scrollViewer.VerticalOffset;
            }

            Cursor = Cursors.ScrollAll;
            ForceCursor = true;

            _IsPanning = true;
        }

        protected void EndPanning()
        {
            Cursor = null;
            ForceCursor = false;

            _IsPanning = false;
        }

        protected void PanTo(Point point)
        {
            if (!IsPanning)
            {
                return;
            }

            var scrollViewer = LayoutHelper.GetAncestor<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                Vector delta = (point - _PanStartPoint) * Zoom;
                double offsetX = scrollViewer.HorizontalOffset - delta.X;
                double offsetY = scrollViewer.VerticalOffset - delta.Y;
                scrollViewer.ScrollToHorizontalOffset(offsetX);
                scrollViewer.ScrollToVerticalOffset(offsetY);
            }
        }

        #endregion

        #region Drag and Drop

        private List<IDropHandler> _DropHandlers = new List<IDropHandler>();
        private IDropHandler _CurrentDropHandler;

        public IList<IDropHandler> DropHandlers
        {
            get { return _DropHandlers; }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            foreach (var dropHandler in DropHandlers)
            {
                if (dropHandler.DragEnter(e))
                {
                    _CurrentDropHandler = dropHandler;
                    return;
                }
            }

            base.OnDragEnter(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            if (_CurrentDropHandler != null)
            {
                _CurrentDropHandler.DragOver(e);
            }
            else
            {
                base.OnDragOver(e);
            }
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            if (_CurrentDropHandler != null)
            {
                _CurrentDropHandler.DragLeave(e);
            }
            else
            {
                base.OnDragLeave(e);
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (_CurrentDropHandler != null)
            {
                _CurrentDropHandler.Drop(e);
            }
            else
            {
                base.OnDrop(e);
            }
        }

        #endregion

        #region Commands

        void LoadCommandBindings()
        {
            // SilverNote Commands

            CommandBindings.AddRange(new[] {

                // Text Commands

                new CommandBinding(NTextCommands.InsertParagraphBefore, InsertParagraphBeforeCommand_Executed),
                new CommandBinding(NTextCommands.InsertParagraphAfter, InsertParagraphAfterCommand_Executed),
                new CommandBinding(NTextCommands.MoveParagraphUp, MoveParagraphUpCommand_Executed),
                new CommandBinding(NTextCommands.MoveParagraphDown, MoveParagraphDownCommand_Executed),
                new CommandBinding(NTextCommands.SelectParagraph, SelectParagraphCommand_Executed),
                new CommandBinding(NTextCommands.DuplicateParagraph, DuplicateParagraphCommand_Executed),
                new CommandBinding(NTextCommands.DeleteParagraph, DeleteParagraphCommand_Executed),

            });
        }

        void InsertParagraphBeforeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertParagraphBefore();
        }

        void InsertParagraphAfterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertParagraphAfter();
        }

        void MoveParagraphUpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveParagraphUp();
        }

        void MoveParagraphDownCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveParagraphDown();
        }

        void SelectParagraphCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectParagraph();
        }

        void DuplicateParagraphCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DuplicateParagraph();
        }

        void DeleteParagraphCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteParagraph();
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new InteractivePanel(this);
        }

        #endregion

        #region Implementation

        #region Background

        public static readonly new DependencyProperty BackgroundProperty = DocumentPanel.BackgroundProperty.AddOwner(
            typeof(InteractivePanel),
            new FrameworkPropertyMetadata(Brushes.Transparent)
        );

        #endregion

        #region Mouse Input

        bool _IsMouseLeftButtonDownWithin;
        bool _IsMouseRightButtonDownWithin;

        /// <summary>
        /// True iff the left mouse button was pressed WITHIN this panel
        /// </summary>
        bool IsMouseLeftButtonDownWithin
        {
            get
            {
                return _IsMouseLeftButtonDownWithin;
            }
            set
            {
                if (value != _IsMouseLeftButtonDownWithin)
                {
                    _IsMouseLeftButtonDownWithin = value;
                    OnIsMouseLeftButtonDownWithinChanged();
                }
            }
        }

        /// <summary>
        /// True iff the right mouse button was pressed WITHIN this panel
        /// </summary>
        bool IsMouseRightButtonDownWithin
        {
            get 
            { 
                return _IsMouseRightButtonDownWithin; 
            }
            set 
            {
                if (value != _IsMouseRightButtonDownWithin)
                {
                    _IsMouseRightButtonDownWithin = value;
                    OnIsMouseRightButtonDownWithinChanged();
                }
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (e.ChangedButton == MouseButton.Middle)
            {
                OnPreviewMouseMiddleButtonDown(e);
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            if (e.ChangedButton == MouseButton.Middle)
            {
                OnPreviewMouseMiddleButtonUp(e);
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Ignore mouse actions originating from menus.
            //
            // This does not apply to context menus, which we'll never see mouse input for anyway.

            if (LayoutHelper.GetSelfOrAncestor<MenuItem>((DependencyObject)e.OriginalSource) != null)
            {
                return;
            }

            IsMouseLeftButtonDownWithin = true;

            Point position = e.GetPosition(this);

            // Erasing

            if (IsErasing)
            {
                Erase(position);                
                e.Handled = true;
                return;
            }

            // Filling

            if (IsFilling)
            {
                Fill(position);
                e.Handled = true;
                return;
            }

            // Stroking

            if (IsStroking)
            {
                Stroke(position);
                e.Handled = true;
                return;
            }

            RequestBringIntoView += SuppressBringIntoView;
            try
            {
                var child = LayoutHelper.ChildFromDescendant(this, (DependencyObject)e.Source) as UIElement;
                if (child != null)
                {
                    if (GetPositioning(child) == Positioning.Static)
                    {
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && !TextParagraph.GetIsHighlighting(this))
                        {
                            SelectTo(child);
                        }
                        else
                        {
                            Selection.SelectOnly(child);
                        }
                    }
                    else
                    {
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) || Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                        {
                            Selection.Select(child);
                        }
                        else if (!Selection.Contains(child))
                        {
                            Selection.SelectOnly(child);
                        }
                        else
                        {
                            BeginClickSelection(e);
                        }
                    }
                }
            }
            finally
            {
                RequestBringIntoView -= SuppressBringIntoView;
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Called when mouse button is pressed and cursor is NOT over any element
            //
            // We handle this by finding some nearby element to select

            Point position = e.GetPosition(this);

            var target = GetSelectableElement(position);
            if (target != null)
            {
                bool selectInline = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && !TextParagraph.GetIsHighlighting(this);
                if (selectInline)
                {
                    SelectTo(target);
                }
                else
                {
                    Selection.SelectOnly(target);
                }

                EmulateNavigation(target, position, selectInline);
            }

            e.Handled = true;
        }

        private void EmulateNavigation(UIElement target, Point point, bool selecting)
        {
            var navigable = target as INavigable;
            if (navigable != null)
            {
                navigable.NavigationOffset = TransformToDescendant(target).Transform(point);

                double distanceToLeftEdge = Math.Abs(navigable.NavigationOffset.X);
                double distanceToRightEdge = Math.Abs(navigable.NavigationOffset.X - target.RenderSize.Width);
                double distanceToTopEdge = Math.Abs(navigable.NavigationOffset.Y);
                double distanceToBottomEdge = Math.Abs(navigable.NavigationOffset.Y - target.RenderSize.Height);
                double smallest = Math.Min(Math.Min(Math.Min(distanceToLeftEdge, distanceToRightEdge), distanceToTopEdge), distanceToBottomEdge);

                if (smallest == distanceToLeftEdge)
                {
                    if (selecting) navigable.SelectToLeft(); else navigable.MoveToLeft();
                }
                else if (smallest == distanceToRightEdge)
                {
                    if (selecting) navigable.SelectToRight(); else navigable.MoveToRight();
                }
                else if (smallest == distanceToTopEdge)
                {
                    if (selecting) navigable.SelectToTop(); else navigable.MoveToTop();
                }
                else if (smallest == distanceToBottomEdge)
                {
                    if (selecting) navigable.SelectToBottom(); else navigable.MoveToBottom();
                }
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            // Ignore mouse actions originating from menus.
            //
            // This does not apply to context menus, which we'll never see mouse input for anyway.

            if (LayoutHelper.GetSelfOrAncestor<MenuItem>((DependencyObject)e.OriginalSource) != null)
            {
                return;
            }

            IsMouseRightButtonDownWithin = true;

            Point position = e.GetPosition(this);

            // Erasing

            if (IsErasing)
            {
                e.Handled = true;
                return;
            }

            RequestBringIntoView += SuppressBringIntoView;
            try
            {
                var child = LayoutHelper.ChildFromDescendant(this, (DependencyObject)e.Source) as UIElement;
                if (child != null)
                {
                    if (Selection.Contains(child))
                    {
                        return;
                    }

                    if (GetPositioning(child) == Positioning.Static)
                    {
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && !TextParagraph.GetIsHighlighting(this))
                        {
                            SelectTo(child);
                        }
                        else
                        {
                            Selection.SelectOnly(child);
                        }
                    }
                    else
                    {
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                        {
                            Selection.Select(child);
                        }
                        else if (!Selection.Contains(child))
                        {
                            Selection.SelectOnly(child);
                        }
                        else
                        {
                            BeginClickSelection(e);
                        }
                    }
                }
            }
            finally
            {
                RequestBringIntoView -= SuppressBringIntoView;
            }

            base.OnPreviewMouseRightButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            IsMouseLeftButtonDownWithin = false;

            EndClickSelection(e);
        }

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            IsMouseRightButtonDownWithin = false;

            EndClickSelection(e);
        }

        protected virtual void OnPreviewMouseMiddleButtonDown(MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);

            BeginPanning(position);

            e.Handled = true;
        }

        protected virtual void OnPreviewMouseMiddleButtonUp(MouseButtonEventArgs e)
        {
            EndPanning();

            e.Handled = true;
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (!e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                IsMouseLeftButtonDownWithin = false;
            }

            if (IsMovingChildren)
            {
                return;
            }

            Point point = e.GetPosition(this);

            // Panning

            if (IsPanning)
            {
                if (e.MiddleButton.HasFlag(MouseButtonState.Pressed))
                {
                    PanTo(point);
                    e.Handled = true;
                    return;
                }
                else
                {
                    EndPanning();
                }
            }

            // Erasing

            if (IsErasing)
            {
                if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
                {
                    Erase(point);
                    e.Handled = true;
                    return;
                }
            }

            // Filling

            if (IsFilling)
            {
                if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
                {
                    Fill(point);
                    e.Handled = true;
                    return;
                }
            }

            // Stroking

            if (IsStroking)
            {
                if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
                {
                    Stroke(point);
                    e.Handled = true;
                    return;
                }
            }

            // Inline selection

            if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                var source = (DependencyObject)e.Source;
                var child = LayoutHelper.ChildFromDescendant(this, source) as UIElement;
                if (child != null && GetPositioning(child) == Positioning.Static)
                {
                    SelectTo(child);
                }
            }

            base.OnPreviewMouseMove(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Called when the mouse is moved while the cursor is NOT over any element.

            if (!e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                IsMouseLeftButtonDownWithin = false;
            }

            if (!IsMouseLeftButtonDownWithin)
            {
                return;
            }

            if (IsMovingChildren)
            {
                return;
            }

            // Text drag-selection

            var position = e.GetPosition(this);

            var target = GetSelectableElement(position);
            if (target != null)
            {
                SelectTo(target);
                EmulateNavigation(target, position, true);
            }

            base.OnMouseMove(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Delta > 0)
                {
                    ZoomIn();
                }
                else
                {
                    ZoomOut();
                }

                e.Handled = true;
            }
            else
            {
                base.OnPreviewMouseWheel(e);
            }
        }

        protected virtual void OnIsMouseLeftButtonDownWithinChanged()
        {
            if (UndoStack != null)
            {
                if (IsMouseLeftButtonDownWithin)
                {
                    UndoStack.BeginScope("Drag");
                }
                else if (UndoStack.IsWithinScope)   // UndoStack.Clear() may have been called
                {
                    UndoStack.EndScope("Drag");
                }
            }
        }

        protected virtual void OnIsMouseRightButtonDownWithinChanged()
        {

        }

        #region Click Selection

        int? _ClickSelectionTimestamp;

        void BeginClickSelection(MouseButtonEventArgs e)
        {
            _ClickSelectionTimestamp = e.Timestamp;
        }

        void EndClickSelection(MouseButtonEventArgs e)
        {
            if (_ClickSelectionTimestamp.HasValue)
            {
                var delta = e.Timestamp - _ClickSelectionTimestamp;
                if (delta < 250)    // ms
                {
                    OnClickSelection(e);
                }
            }
        }

        protected virtual void OnClickSelection(MouseButtonEventArgs e)
        {
            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var position = e.GetPosition(this);

                var child = ChildFromPoint(position);
                if (child != null)
                {
                    Selection.SelectOnly(child);
                }
            }
        }

        #endregion

        #endregion

        #region Keyboard Input

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Selection.Count > 1 && InputHelper.IsEditingStroke(e))
            {
                Delete();

                var source = e.Source as DependencyObject;
                var child = LayoutHelper.ChildFromDescendant(this, source) as UIElement;
                if (child != null)
                {
                    Selection.SelectOnly(child);
                }

                if (e.Key == Key.Back || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    OnEnterPressed(e);
                    break;
                case Key.Escape:
                    OnEscapePressed(e);
                    break;
            }

            base.OnKeyDown(e);
        }

        protected void OnDeletePressed(KeyEventArgs e)
        {
            if (Selection.Count > 1)
            {
                Delete();
                e.Handled = true;
            }
            else
            {
                ITextElement target = e.Source as ITextElement;
                if (target != null)
                {
                    if (OnDeleteForward(target))
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        protected void OnBackPressed(KeyEventArgs e)
        {
            if (Selection.Count > 1)
            {
                Delete();
                e.Handled = true;
            }
            else
            {
                ITextElement target = e.Source as ITextElement;
                if (target != null)
                {
                    if (OnDeleteBack(target))
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        protected void OnEnterPressed(KeyEventArgs e)
        {
            if (Selection.Count > 1)
            {
                Delete();
                e.Handled = true;
            }
            else
            {
                ITextElement target = e.Source as ITextElement;
                if (target != null)
                {
                    if (OnEnterParagraphBreak(target))
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        protected void OnEscapePressed(KeyEventArgs e)
        {
            OnEscape();
        }

        protected virtual void OnEscape()
        {

        }

        #endregion

        #region Helpers

        /// <summary>
        /// This is called when the mouse is clicked or dragged at a point
        /// that is not over an element. This gets the element that should
        /// be the target of that click.
        /// </summary>
        private UIElement GetSelectableElement(Point position)
        {
            // If the point is NOT below the last visible child, return the closest visible child

            var lastChild = FindLast<UIElement>(e => DocumentPanel.GetPositioning(e) == Positioning.Static && e.Visibility == Visibility.Visible);

            if (lastChild != null)
            {
                if (position.Y < VisualTreeHelper.GetOffset(lastChild).Y + lastChild.RenderSize.Height)
                {
                    var candidates = InternalChildren.OfType<UIElement>().Where(e => DocumentPanel.GetPositioning(e) == Positioning.Static && e.Visibility == Visibility.Visible);
                    var closestChild = ClosestChildFromPoint(candidates, position);
                    if (closestChild != null)
                    {
                        return (UIElement)closestChild;
                    }
                }

                // If the last visible child is a collapsed section, return the section's heading

                var paragraph = lastChild as TextParagraph;
                if (paragraph != null && paragraph.IsHeading && paragraph.IsCollapsed)
                {
                    paragraph.NavigationOffset = TransformToDescendant(paragraph).Transform(position);
                    paragraph.MoveToBottom();
                    return paragraph;
                }
            }

            // Otherwise append new paragraphs until one can be clicked

            return FillTo(position, new TextParagraph());
        }

        private UIElement FillTo(Point point, ICloneable obj)
        {
            var lastChild = FindLast<UIElement>(PositioningEquals(Positioning.Static));

            while (lastChild == null ||
                VisualTreeHelper.GetOffset(lastChild).Y + lastChild.RenderSize.Height < point.Y)
            {
                lastChild = (UIElement)obj.Clone();
                Append(lastChild);
                UpdateLayout();

                // This occurs if, e.g., we're appending paragraphs to a collapsed section
                if (lastChild.RenderSize.Height == 0)
                {
                    throw new InvalidOperationException();
                }
            }

            return lastChild;
        }

        #endregion

        #endregion

    }
}
