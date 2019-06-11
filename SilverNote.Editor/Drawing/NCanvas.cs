/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.LS;
using DOM.SVG;
using Jurassic;
using SilverNote.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace SilverNote.Editor
{
    public delegate void DrawingEventHandler(object sender, DrawingEventArgs e);

    public class DrawingEventArgs : EventArgs
    {
        public DrawingEventArgs(Shape drawing)
        {
            Drawing = drawing;
        }

        public Shape Drawing { get; set; }
    }

    public enum DrawingMode
    {
        Ready,
        Placing,
        Drawing,
        Dragging,
        Erasing
    };

    public delegate void DrawingModeChangedEventHandler(object sender, DrawingModeChangedEventArgs e);

    public class DrawingModeChangedEventArgs
    {
        public DrawingModeChangedEventArgs(DrawingMode oldMode, DrawingMode newMode)
        {
            OldMode = oldMode;
            NewMode = newMode;
        }

        public DrawingMode OldMode { get; private set; }
        public DrawingMode NewMode { get; private set; }
    }

    [ContentProperty("Drawing")]
    public partial class NCanvas : EditingCanvas
    {
        #region Constructors

        public NCanvas()
        {
            Initialize();
        }

        public NCanvas(NCanvas copy)
            : base(copy)
        {
            Initialize();
        }

        private void Initialize()
        {
            Focusable = true;
        }

        #endregion

        #region Drawings

        public void PlaceDrawing(Shape drawing, bool oneClick = false)
        {
            Point startPoint = Mouse.GetPosition(this);

            PlaceDrawing(drawing, startPoint, oneClick);
        }

        /// <summary>
        /// Add the given drawing to this canvas for interactive placement by the user.
        /// </summary>
        /// <param name="drawing">The drawing to be inserted</param>
        /// <param name="startPoint">Where to begin placing the drawing</param>
        /// <param name="simple"></param>
        public void PlaceDrawing(Shape drawing, Point startPoint, bool oneClick = false)
        {
            Drawings.Add(drawing);

            Selection.SelectOnly(drawing);

            if (oneClick)
            {
                CenterDrawing(drawing, startPoint);
                BeginDragging(startPoint, treatAsPlacing: true);
            }
            else
            {
                BeginPlacing(startPoint);
            }
        }

        private void CenterDrawing(Shape drawing, Point center)
        {
            var bounds = drawing.RenderedBounds;
            bounds.X = center.X - bounds.Width / 2;
            bounds.Y = center.Y - bounds.Height / 2;
            drawing.RenderedBounds = bounds;
        }

        protected override void OnDrawingAdded(int index, Shape drawing)
        {
            base.OnDrawingAdded(index, drawing);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (drawing.Canvas == this)
                {
                    Point point = Mouse.GetPosition(this);
                    point = TransformToDescendant(drawing).Transform(point);
                    if (drawing.HitTest(point) != null)
                    {
                        _MouseOverDrawings.Add(drawing);
                        drawing.MouseEnter(false);
                    }
                }
            }));
        }

        protected override void OnDrawingRemoved(int index, Shape drawing)
        {
            if (drawing.IsMouseOver)
            {
                drawing.MouseLeave();
            }

            _MouseOverDrawings.Remove(drawing);

            base.OnDrawingRemoved(index, drawing);
        }

        #endregion

        #region Hit Testing

        public Shape DrawingFromPoint(Point point)
        {
            return DrawingFromPoint(point, this);
        }

        public Shape[] DrawingsFromPoint(Point point)
        {
            return DrawingsFromPoint(point, this);
        }

        public Shape[] DrawingsFromGeometry(Geometry geometry)
        {
            Geometry bounds = new RectangleGeometry(VisualTreeHelper.GetDescendantBounds(this));

            if (geometry.FillContainsWithDetail(bounds) == IntersectionDetail.FullyContains)
            {
                return Drawings.Reverse().ToArray();    // optimization
            }
            else
            {
                return DrawingsFromGeometry(geometry, this);
            }
        }

        public static Shape DrawingFromPoint(Point point, Visual referenceVisual)
        {
            HitTestResult hit = VisualTreeHelper.HitTest(referenceVisual, point);
            if (hit != null)
            {
                return DrawingFromVisual(hit.VisualHit);
            }
            else
            {
                return null;
            }
        }

        public static Shape[] DrawingsFromPoint(Point point, Visual referenceVisual)
        {
            var results = new List<Shape>();

            VisualTreeHelper.HitTest(
                referenceVisual,
                null,
                new HitTestResultCallback(delegate(HitTestResult hit)
                {
                    Shape drawing = DrawingFromVisual(hit.VisualHit);
                    if (drawing != null && !results.Contains(drawing))
                        results.Add(drawing);
                    return HitTestResultBehavior.Continue;
                }),
                new PointHitTestParameters(point));

            return results.ToArray();
        }

        public static Shape[] DrawingsFromGeometry(Geometry geometry, Visual referenceVisual)
        {
            var results = new LinkedList<Shape>();

            VisualTreeHelper.HitTest(
                referenceVisual,
                (hit) =>
                {
                    var drawing = DrawingFromVisual(hit);
                    if (drawing != null)
                    {
                        return HitTestFilterBehavior.Continue;
                    }
                    return HitTestFilterBehavior.ContinueSkipSelf;
                },
                (hit) =>
                {
                    var drawing = DrawingFromVisual(hit.VisualHit);
                    if (drawing != null)
                    {
                        if (!results.Contains(drawing))
                        {
                            results.AddLast(drawing);
                        }
                    }
                    return HitTestResultBehavior.Continue;
                },
                new GeometryHitTestParameters(geometry)
            );

            return results.ToArray();
        }

        public static Shape DrawingFromVisual(DependencyObject visual)
        {
            while (visual != null)
            {
                var parent = VisualTreeHelper.GetParent(visual);
                if (parent is NCanvas)
                    break;
                visual = parent;
            }
            return visual as Shape;
        }

        #endregion

        #region Mode

        private DrawingMode _Mode = DrawingMode.Ready;

        public DrawingMode Mode
        {
            get { return _Mode; }
            protected set
            {
                var oldMode = _Mode;
                var newMode = value;

                if (newMode != oldMode)
                {
                    _Mode = newMode;
                    OnDrawingModeChanged(oldMode, newMode);
                }
            }
        }

        public event DrawingModeChangedEventHandler DrawingModeChanged;

        protected void OnDrawingModeChanged(DrawingMode oldMode, DrawingMode newMode)
        {
            switch (newMode)
            {
                case DrawingMode.Ready:
                    Cursor = null;
                    Background = null;
                    break;
                case DrawingMode.Placing:
                    Background = Brushes.Transparent;
                    InvalidateVisual();
                    break;
                case DrawingMode.Drawing:
                    Background = Brushes.Transparent;
                    break;
                case DrawingMode.Dragging:
                    Background = Brushes.Transparent;
                    break;
                case DrawingMode.Erasing:
                    Background = Brushes.Transparent;
                    break;
            }

            if (DrawingModeChanged != null)
            {
                DrawingModeChanged(this, new DrawingModeChangedEventArgs(oldMode, newMode));
            }
        }

        #endregion

        #region Drawing I/O

        Shape _MouseCapture;

        public void CaptureMouse(Shape drawing)
        {
            _MouseCapture = drawing;
        }

        protected void Drawing_OnMouseDown(Visual hitReference, MouseButtonEventArgs e)
        {
            Shape drawing = _MouseCapture;

            if (drawing == null)
            {
                Point hitPoint = e.GetPosition(this);
                if (hitReference != this)
                {
                    hitPoint = TransformToDescendant(hitReference).Transform(hitPoint);
                }

                drawing = ShapeHitTest.HitOne(hitReference, hitPoint, ShapeHitTestFlags.ReturnDirectHits);
            }

            if (drawing != null)
            {
                drawing.MouseDown(e);
            }
        }

        protected void Drawing_OnMouseUp(Visual hitReference, MouseButtonEventArgs e)
        {
            Shape drawing = _MouseCapture;

            if (drawing == null)
            {
                Point hitPoint = e.GetPosition(this);
                if (hitReference != this)
                {
                    hitPoint = TransformToDescendant(hitReference).Transform(hitPoint);
                }

                drawing = ShapeHitTest.HitOne(hitReference, hitPoint, ShapeHitTestFlags.ReturnDirectHits);
            }

            if (drawing != null)
            {
                drawing.MouseUp(e);
            }
        }

        static Shape _MouseOverDrawing = null;

        protected void Drawing_OnMouseMove(Visual hitReference, MouseEventArgs e)
        {
            Shape drawing = _MouseCapture;

            if (drawing == null)
            {
                if (hitReference != null)
                {
                    Point hitPoint = e.GetPosition(this);
                    if (hitReference != this)
                    {
                        hitPoint = TransformToDescendant(hitReference).Transform(hitPoint);
                    }

                    drawing = ShapeHitTest.HitOne(hitReference, hitPoint, ShapeHitTestFlags.ReturnDirectHits);
                }
            }

            if (drawing != _MouseOverDrawing)
            {
                /*
                if (_MouseOverDrawing != null)
                {
                    RaiseMouseEvent(MouseEventTypes.MouseOut, _MouseOverDrawing, e);
                }
                 */

                _MouseOverDrawing = drawing;

                /*
                if (_MouseOverDrawing != null)
                {
                    RaiseMouseEvent(MouseEventTypes.MouseOver, _MouseOverDrawing, e);
                }
                */
            }

            if (drawing != null)
            {
                drawing.MouseMove(e);
            }
        }

        #endregion

        #region Ready Mode

        protected static void ReadyMode_OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Get the drawing that the mouse is currently over
            //
            // Note: _MouseOverDrawings can contain drawings from ANY canvas

            Shape drawing = _MouseOverDrawings.FirstOrDefault();
            
            if (drawing == null || drawing.Canvas == null)
            {
                return;
            }

            // Select/unselect the drawing as appropriate
            //
            // Note: selecting a drawing may cause it to move to a different canvas

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ||
                Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                drawing.Canvas.Selection.Select(drawing);
            }
            else if (!drawing.Canvas.Selection.Contains(drawing))
            {
                drawing.Canvas.Selection.SelectOnly(drawing);
                // in case drawing moved to a different canvas:
                drawing.Canvas.Selection.SelectOnly(drawing);   
            }
            
            drawing.Canvas.ReadyMode_OnPreviewMouseLeftButtonDown(drawing, e);
        }

        protected void ReadyMode_OnPreviewMouseLeftButtonDown(Shape drawing, MouseButtonEventArgs e)
        {
            // Give the drawing a chance to handle the mouse event

            if (Selection.Count == 1)
            {
                Drawing_OnMouseDown(drawing, e);

                if (e.Handled)
                {
                    Mouse.Capture(this, CaptureMode.Element);
                    return;
                }
            }

            // If a NTextLine has been clicked, let its NParagraph handle this event

            Point mousePosition = e.GetPosition(drawing.Canvas);
            drawing.Redraw();
            if (ShapeHitTest.IsText(drawing, mousePosition, relativeTo: drawing.Canvas))
            {
                return;
            }

            // If a paragraph has focus and anything else is clicked, remove focus
            // from that paragraph

            Focus();

            // When the user double-clicks an NTextBox belonging to an NDrawingGroup,
            // set focus to that text box.

            if (e.ClickCount == 2)
            {
                if (Selection.Count == 1 && Selection[0] is ShapeGroup)
                {
                    var hitPoint = e.GetPosition(this);
                    hitPoint = TransformToDescendant(Selection[0]).Transform(hitPoint);
                    var textBox =
                        ShapeHitTest.HitOne(Selection[0], hitPoint, ShapeHitTestFlags.ReturnDirectHits) as
                            NTextBox;

                    if (textBox != null)
                    {
                        textBox.Paragraph.Focus();
                        textBox.Paragraph.SelectAll();
                    }
                }
                e.Handled = true;
                return;
            }

            // Otherwise, begin moving the drawing

            BeginDragging(mousePosition);

            e.Handled = true;
        }

        protected void ReadyMode_OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Drawing_OnMouseUp(this, e);

            Mouse.Capture(this, CaptureMode.None);
        }

        protected void ReadyMode_OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            Point cursor = e.GetPosition(this);

            Shape drawing = DrawingFromPoint(cursor);

            if (drawing != null)
            {
                if (!Selection.Contains(drawing))
                {
                    if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) &&
                        !Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        Selection.UnselectAll();
                    }
                }

                Selection.Select(drawing);

                if (!Selection.Contains(drawing))
                {
                    if (drawing.Canvas != this)
                    {
                        drawing.Canvas.ReadyMode_OnMouseRightButtonDown(e);
                    }
                }
            }
        }

        protected void ReadyMode_OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            Point cursor = e.GetPosition(this);

            Shape drawing = DrawingFromPoint(cursor);

            if (drawing != null)
            {
                if (!Selection.Contains(drawing))
                {
                    if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) &&
                        !Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        Selection.UnselectAll();
                    }
                }

                Selection.Select(drawing);

                e.Handled = true;
            }
        }

        static List<Shape> _MouseOverDrawings = new List<Shape>();

        void ReadyMode_OnMouseMove(MouseEventArgs e)
        {
            // We set the IsMouseOver flag on all drawings the mouse is 
            // currently over. This flag causes the drawings to become
            // translucent so you can see all overlapping drawings.

            // We set the IsMouseDirectlyOver flag on the one drawing that
            // will be selected if the user clicks it. This flag causes the
            // drawing to show its selection handles.

            // When a user is trying to select an object, they will click 
            // as soon as it becomes selectable. If the object is selectable
            // but they continue to move the mouse, they are probably trying
            // to click on some other overlapping object. However, they may
            // overshoot so it is important the more recent an object was
            // selectable, the easier it is to be selectable again.

            // ---------------------------------------------------------------

            // Get ALL drawings the mouse is currently over, 
            //
            // ***including those belonging to other canvases***

            Visual parent = VisualParent as Visual;
            if (parent == null)
            {
                return;
            }

            Point mousePosition = TransformToAncestor(parent).Transform(e.GetPosition(this));

            Shape[] drawings = DrawingsFromPoint(point: mousePosition, referenceVisual: parent);

            // Set IsMouseOver flag on drawings the mouse is currently over 

            foreach (Shape drawing in drawings)
            {
                drawing.MouseEnter(drawing.IsMouseDirectlyOver);
            }

            // Clear the flag on any drawings the mouse is no longer over

            var oldMouseOverDrawings = _MouseOverDrawings.Except(drawings).ToArray();
            var oldMouseDirectlyOverDrawing = _MouseOverDrawings.FirstOrDefault();

            foreach (Shape drawing in oldMouseOverDrawings)
            {
                drawing.MouseLeave();
                _MouseOverDrawings.Remove(drawing);
            }

            // Update mouseOverDrawings:
            //
            // add items in the order in which the mouse moved over them

            var newMouseOverDrawings = drawings.Except(_MouseOverDrawings).ToArray();

            foreach (Shape drawing in newMouseOverDrawings)
            {
                _MouseOverDrawings.Insert(0, drawing);
            }

            // Set the IsMouseDirectlyOver flag on drawing the drawing
            // the mouse most recently moved over

            Shape mouseDirectlyOverDrawing = _MouseOverDrawings.FirstOrDefault();

            // If the set of drawings the mouse is over has changed at all,
            // then make sure the mouseDirectlyOverDrawing changes too

            if (oldMouseOverDrawings.Length > 0 &&
                mouseDirectlyOverDrawing == oldMouseDirectlyOverDrawing)
            {
                // But only do so if:
                //   1) The mouse is not over a handle of that drawing AND
                //   2) Another drawing is available AND
                //   3) it does not entirely enclose the currently-chosen drawing

                if (mouseDirectlyOverDrawing.HitHandle(mousePosition) != -1 &&
                    _MouseOverDrawings.Count > 1 &&
                    !_MouseOverDrawings[1].RenderedBounds.Contains(mouseDirectlyOverDrawing.RenderedBounds))
                {
                    _MouseOverDrawings.Remove(mouseDirectlyOverDrawing);
                    _MouseOverDrawings.Insert(1, mouseDirectlyOverDrawing);
                    mouseDirectlyOverDrawing = _MouseOverDrawings[0];
                }
            }

            // If the mouse is directly over a BaseTextLine, then set 
            // mouseDirectlyOverDrawing to the owning NTextBox

            bool isMouseOverText = false;

            foreach (Shape drawing in _MouseOverDrawings)
            {
                if (drawing is NTextBox || drawing is ShapeGroup)
                {
                    // Case 129
                    if (drawing.Parent == null)
                    {
                        Debug.Assert(false);
                        continue;
                    }

                    Point hitPoint = parent.TransformToDescendant(drawing).Transform(mousePosition);

                    HitTestResult hitResult = drawing.HitTest(hitPoint);

                    if (hitResult != null && hitResult.VisualHit is TextLineVisual)
                    {
                        _MouseOverDrawings.Remove(drawing);
                        _MouseOverDrawings.Insert(0, drawing);
                        mouseDirectlyOverDrawing = drawing;
                        isMouseOverText = true;
                        break;
                    }
                }
            }

            // If the mouse is over a handle of a selected drawing, then set
            // mouseDirectlyOverDrawing to that selected drawing

            foreach (Shape drawing in Selection.Reverse())
            {
                if (drawing.HitHandle(mousePosition) != -1)
                {
                    _MouseOverDrawings.Remove(drawing);
                    _MouseOverDrawings.Insert(0, drawing);
                    mouseDirectlyOverDrawing = drawing;
                    isMouseOverText = false;
                    break;
                }
            }

            // Now that we've figured out which drawing the mouse is directly
            // over, set that drawing's IsMouseDirectlyOver flag

            if (mouseDirectlyOverDrawing != oldMouseDirectlyOverDrawing)
            {
                if (oldMouseDirectlyOverDrawing != null)
                {
                    if (oldMouseDirectlyOverDrawing.IsMouseOver)
                    {
                        oldMouseDirectlyOverDrawing.MouseEnter(false);
                    }
                    else
                    {
                        oldMouseDirectlyOverDrawing.MouseLeave();
                    }
                }

                if (mouseDirectlyOverDrawing != null)
                {
                    mouseDirectlyOverDrawing.MouseEnter(true);
                }
            }

            // Allow the drawing to process the event

            if (mouseDirectlyOverDrawing != null && mouseDirectlyOverDrawing.Canvas != null)
            {
                mouseDirectlyOverDrawing.Canvas.Drawing_OnMouseMove(mouseDirectlyOverDrawing, e);
            }
            else if (_MouseOverDrawing != null && _MouseOverDrawing.Canvas != null)
            {
                _MouseOverDrawing.Canvas.Drawing_OnMouseMove(null, e);
            }
            else
            {
                Drawing_OnMouseMove(null, e);
            }

            // Update the cursor

            if (mouseDirectlyOverDrawing != null && mouseDirectlyOverDrawing.Cursor != null)
            {
                Cursor = mouseDirectlyOverDrawing.Cursor;
            }
            else if (_MouseOverDrawing != null && _MouseOverDrawing.Cursor != null)
            {
                Cursor = _MouseOverDrawing.Cursor;
            }
            else if (drawings.Length > 0)
            {
                Cursor = isMouseOverText ? Cursors.IBeam : Cursors.Arrow;
            }
            else if (IsMouseCaptured)
            {
                Cursor = Cursors.Arrow;
            }
            else
            {
                Cursor = null;
            }

            if (e.LeftButton == MouseButtonState.Pressed && !isMouseOverText)
            {
                e.Handled = true;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (IsEnabled)
            {
                if (Mode == DrawingMode.Ready)
                {
                    if (!e.MouseDevice.LeftButton.HasFlag(MouseButtonState.Pressed))
                    {
                        ReadyMode_OnMouseMove(e);
                    }
                }
            }
        }

        public void SetVerticalAlignment(VerticalAlignment value)
        {
            using (UndoScope undo = new UndoScope(UndoStack, "Set Vertical Alignment"))
            {
                foreach (Shape drawing in Selection)
                {
                    if (drawing is NTextBox)
                    {
                        SetVerticalAlignment((NTextBox)drawing, value);
                    }
                }
            }
        }

        public void SetVerticalAlignment(NTextBox textBox, VerticalAlignment value)
        {
            if (UndoStack != null)
            {
                VerticalAlignment oldValue = textBox.RenderedVerticalAlignment;

                UndoStack.Push(delegate()
                {
                    SetVerticalAlignment(textBox, oldValue);
                });
            }

            textBox.RenderedVerticalAlignment = value;
        }


        public void SetLabel(Shape drawing, string value)
        {
            if (UndoStack != null)
            {
                string oldValue = drawing.Label;

                UndoStack.Push(delegate()
                {
                    SetLabel(drawing, oldValue);
                });
            }

            drawing.Label = value;
        }

        #endregion

        #region Placing Mode

        private void BeginPlacing(Point point)
        {
            var drawing = Selection.FirstOrDefault();
            if (drawing != null)
            {
                drawing.Place(point);
                drawing.Redraw();

                if (drawing.RenderedBounds.Width == 0 && 
                    drawing.RenderedBounds.Height == 0)
                {
                    drawing.HideAdorner();
                }

                Mode = DrawingMode.Placing;
            }
        }

        private void ContinuePlacing(Shape drawing, Point point)
        {
            Rect bounds = drawing.RenderedBounds;
            double centerX = (bounds.Left + bounds.Right) / 2;
            double centerY = (bounds.Top + bounds.Bottom) / 2;
            Point center = new Point(centerX, centerY);
            Vector delta = point - center;

            Snap(SelectedSnaps, ref delta);

            drawing.Place(center + delta);
            drawing.Redraw();
        }

        private void CompletePlacing(Shape drawing, Point point)
        {
            ContinuePlacing(drawing, point);

            drawing.CompletePlacing();
            drawing.ShowAdorner();

            Mode = DrawingMode.Drawing;
        }

        public void CancelPlacing()
        {
            if (Mode == DrawingMode.Placing)
            {
                Selection.ToArray().ForEach(drawing => Drawings.Remove(drawing));

                Mode = DrawingMode.Ready;
            }
        }

        void PlacingMode_OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var drawing = Selection.LastOrDefault();
            if (drawing != null)
            {
                var point = e.GetPosition(this);
                CompletePlacing(drawing, point);
            }
            else
            {
                // Error
                Mode = DrawingMode.Ready;
            }

            e.Handled = true;
        }

        void PlacingMode_OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            var drawing = Selection.LastOrDefault();
            if (drawing != null)
            {
                var point = e.GetPosition(this);
                CompletePlacing(drawing, point);
                drawing.CompleteDrawing();
            }

            Mode = DrawingMode.Ready;

            e.Handled = true;
        }

        void PlacingMode_OnMouseMove(MouseEventArgs e)
        {
            var drawing = Selection.LastOrDefault();
            if (drawing != null)
            {
                var point = e.GetPosition(this);
                ContinuePlacing(drawing, point);
            }
            else
            {
                // Error
                Mode = DrawingMode.Ready;
            }

            e.Handled = true;
        }

        #endregion

        #region Drawing Mode

        public void CancelDrawing()
        {
            if (Mode == DrawingMode.Drawing)
            {
                var drawing = Selection.Last();

                if (drawing.CancelDrawing())
                {
                    drawing.Redraw();
                }
                else
                {
                    Drawings.Remove(drawing);
                }

                Mode = DrawingMode.Ready;
            }
        }

        void DrawingMode_OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Shape drawing = Selection.LastOrDefault();
            if (drawing != null)
            {
                if (drawing.CompletePlacing())
                {
                    Mode = DrawingMode.Ready;
                }
            }
            else
            {
                // Error
                Mode = DrawingMode.Ready;
            }

            e.Handled = true;
        }

        void DrawingMode_OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Shape drawing = Selection.LastOrDefault();
            if (drawing == null)
            {
                // Error
                Mode = DrawingMode.Ready;
                e.Handled = true;
                return;
            }

            if (drawing.CompleteDrawing())
            {
                if (drawing is NPath)
                {
                    var nextDrawing = new NPath
                    {
                        Smoothness = ((NPath)drawing).Smoothness,
                        StrokeBrush = drawing.StrokeBrush,
                        StrokeWidth = drawing.StrokeWidth,
                        StrokeDashArray = drawing.StrokeDashArray,
                        StrokeLineCap = drawing.StrokeLineCap,
                        FillBrush = drawing.FillBrush
                    };
                    PlaceDrawing(nextDrawing, e.GetPosition(this), false);
                }
                else
                {
                    Mode = DrawingMode.Ready;
                }
            }

            e.Handled = true;
        }

        void DrawingMode_OnMouseMove(MouseEventArgs e)
        {
            var drawing = Selection.LastOrDefault();
            if (drawing == null)
            {
                // Error
                Mode = DrawingMode.Ready;
                e.Handled = true;
                return;
            }

            Point point = e.GetPosition(this);

            point = Snap(point);

            if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                drawing.Draw(point);
            }
            else
            {
                drawing.Place(point);
            }

            drawing.Redraw();

            e.Handled = true;
        }

        #endregion

        #region Dragging Mode

        Point _BeginDragPoint;
        bool _IsDragStarted;
        bool _TreatAsPlacing;

        private void BeginDragging(Point point, bool treatAsPlacing = false)
        {
            _BeginDragPoint = point;
            _IsDragStarted = false;
            _TreatAsPlacing = treatAsPlacing;

            Mode = DrawingMode.Dragging;
            Cursor = Cursors.Arrow;

            if (treatAsPlacing)
            {
                OnDragSelectionStarted(point);
                _IsDragStarted = true;
            }
        }

        private void EndDragging()
        {
            if (_IsDragStarted)
            {
                OnDragCompleted();

                // Hack to autofocus after placing an NDrawingGroup
                if (_TreatAsPlacing)
                {
                    var drawing = Selection.LastOrDefault() as ShapeGroup;
                    if (drawing != null)
                    {
                        drawing.Autofocus();
                    }
                }
            }

            Mode = DrawingMode.Ready;
        }

        void DraggingMode_OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            EndDragging();

            ReadyMode_OnPreviewMouseLeftButtonDown(e);
        }

        void DraggingMode_OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            EndDragging();

            e.Handled = true;
        }

        void DraggingMode_OnMouseMove(MouseEventArgs e)
        {
            if (!e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                EndDragging();
                return;
            }

            Point cursorPosition = e.GetPosition(this);

            if (!_IsDragStarted && cursorPosition != _BeginDragPoint)
            {
                OnDragStarted(_BeginDragPoint);
                _IsDragStarted = true;
            }

            if (_IsDragStarted)
            {
                OnDragDelta(cursorPosition);
            }

            e.Handled = true;
        }

        Vector _DragVector;

        void OnDragStarted(Point dragPoint)
        {
            if (Selection.Count > 0)
            {
                if (Selection.Count == 1 && Selection.Last().HitHandle(dragPoint) != -1)
                {
                    OnDragHandleStarted(dragPoint);
                }
                else
                {
                    OnDragSelectionStarted(dragPoint);
                }
            }
        }

        void OnDragDelta(Point dragPoint)
        {
            if (Selection.Count > 0)
            {
                if (Selection.Last().SelectedHandle != -1)
                {
                    OnDragHandleDelta(dragPoint);
                }
                else
                {
                    OnDragSelectionDelta(dragPoint);
                }
            }
        }

        void OnDragCompleted()
        {
            if (Selection.Count > 0)
            {
                if (Selection.Last().SelectedHandle != -1)
                {
                    OnDragHandleCompleted();
                }
                else
                {
                    OnDragSelectionCompleted();
                }
            }
        }

        void OnDragHandleStarted(Point dragPoint)
        {
            var selected = Selection.LastOrDefault();
            if (selected != null)
            {
                selected.SelectedHandle = selected.HitHandle(dragPoint);

                Point selectedPoint = selected.GetHandle(selected.SelectedHandle);

                _DragVector = dragPoint - selectedPoint;

                selected.Redraw();

                Cursor = Cursors.None;

            }
        }

        void OnDragHandleDelta(Point dragPoint)
        {
            var selected = Selection.LastOrDefault();
            if (selected != null)
            {
                Point selectedPoint = selected.GetHandle(selected.SelectedHandle);

                Point snappedPoint = Snap(dragPoint - _DragVector);

                Vector delta = snappedPoint - selectedPoint;

                MoveHandle(selected, selected.SelectedHandle, delta);
            }
        }

        void OnDragHandleCompleted()
        {
            var selected = Selection.LastOrDefault();
            if (selected != null)
            {
                selected.SelectedHandle = -1;

                selected.Redraw();

                Cursor = null;

            }
        }

        void OnDragSelectionStarted(Point dragPoint)
        {
            var referenceDrawing = Selection.LastOrDefault();
            if (referenceDrawing != null)
            {
                var referencePoint = referenceDrawing.RenderedBounds.TopLeft + referenceDrawing.Offset;

                _DragVector = dragPoint - referencePoint;

                RequestBeginMove();
            }
        }

        void OnDragSelectionDelta(Point dragPoint)
        {
            var referenceDrawing = Selection.LastOrDefault();
            if (referenceDrawing != null)
            {
                var referencePoint = referenceDrawing.RenderedBounds.TopLeft + referenceDrawing.Offset;

                var delta = dragPoint - (referencePoint + _DragVector);

                RequestMoveDelta(delta);
            }
        }

        void OnDragSelectionCompleted()
        {
            if (Selection.Count > 0)
            {
                RequestEndMove();
            }
        }

        #endregion 

        #region Erasing Mode

        public void Erase(Point point)
        {
            Shape drawing = DrawingFromPoint(point);
            if (drawing != null)
            {
                Erase(drawing, point);
            }
        }

        public void Erase(Shape drawing, Point point)
        {
            Shape[] results = drawing.Erase(point);

            if (!results.Contains(drawing))
            {
                Drawings.Remove(drawing);
            }
            else
            {
                drawing.Redraw();
            }

            foreach (Shape result in results)
            {
                if (result != drawing)
                {
                    Drawings.Add(result);
                }
            }
        }

        void ErasingMode_OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Selection.UnselectAll();

            e.Handled = true;
        }

        void ErasingMode_OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Mode = DrawingMode.Ready;
        }

        void ErasingMode_OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton.HasFlag(MouseButtonState.Pressed))
            {
                Erase(e.GetPosition(this));
            }

            e.Handled = true;
        }

        #endregion

        #region Snapping

        protected Point Snap(Point point)
        {
            var parent = VisualParent as ISnappable;
            if (parent != null)
            {
                return parent.Snap(point);
            }
            else
            {
                return point;
            }
        }

        public bool Snap(Point[] points, ref Vector delta)
        {
            var parent = VisualParent as ISnappable;
            if (parent != null)
            {
                return parent.Snap(points, ref delta);
            }
            else
            {
                return false;
            }
        }

        public NConnection[] GetConnections(Point[] points)
        {
            var reference = VisualParent as Visual;
            if (reference != null)
            {
                return GetConnections(reference, points);
            }
            else
            {
                return new NConnection[0];
            }
        }

        public static NConnection[] GetConnections(Visual reference, Point[] points)
        {
            var result = new HashSet<NConnection>();

            foreach (var point in points)
            {
                var connections = GetConnections(reference, point);

                foreach (var connection in connections)
                {
                    result.Add(connection);
                }
            }

            return result.ToArray();
        }

        public static NConnection[] GetConnections(Visual reference, Point point)
        {
            var results = new List<NConnection>();

            VisualTreeHelper.HitTest(
                reference,
                (DependencyObject dep) =>
                {
                    var connector = dep as LineBase;
                    if (connector == null || !connector.IsConnector)
                        return HitTestFilterBehavior.ContinueSkipSelf;
                    var filterCanvas = ((Shape)dep).Parent as NCanvas;
                    if (filterCanvas == null || filterCanvas.IsSelected)
                        return HitTestFilterBehavior.ContinueSkipSelf;
                    return HitTestFilterBehavior.Continue;
                },
                (HitTestResult hitResult) =>
                {
                    var hitDrawing = (Shape)hitResult.VisualHit;
                    var hitCanvas = hitDrawing.Parent as NCanvas;
                    var hitPoint = reference.TransformToVisual(hitCanvas).Transform(point);
                    int hitHandle = hitDrawing.HitHandle(hitPoint);
                    if (hitHandle != -1)
                    {
                        if (hitDrawing is Line)
                        {
                            results.Add(new NConnection(hitDrawing, hitHandle));
                        }
                        else if (hitDrawing is RoutedLine)
                        {
                            if (hitHandle == 0 || hitHandle == 1)
                                results.Add(new NConnection(hitDrawing, hitHandle));
                        }
                        else if (hitDrawing is PolyLine)
                        {
                            if (hitHandle == 0 || hitHandle == hitDrawing.HandleCount - 1)
                                results.Add(new NConnection(hitDrawing, hitHandle));
                        }
                        else if (hitDrawing is QuadraticCurve)
                        {
                            if (hitHandle == 0 || hitHandle == 2)
                                results.Add(new NConnection(hitDrawing, hitHandle));
                        }
                    }
                    return HitTestResultBehavior.Continue;
                },
                new GeometryHitTestParameters(
                    new RectangleGeometry(
                        new Rect(point.X - 2, point.Y - 2, 4, 4)
                    )
                )
            );

            return results.ToArray();
        }


        #endregion

        #region ISelectable

        public void Preselect(Geometry region)
        {
            Shape[] newPreselection = DrawingsFromGeometry(region);

            Preselect(newPreselection);
        }

        #endregion

        #region Implementation

        #region Mouse Input

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                switch (Mode)
                {
                    case DrawingMode.Ready:
                        ReadyMode_OnPreviewMouseLeftButtonDown(e);
                        break;
                    case DrawingMode.Placing:
                        PlacingMode_OnPreviewMouseLeftButtonDown(e);
                        break;
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                switch (Mode)
                {
                    case DrawingMode.Drawing:
                        DrawingMode_OnMouseLeftButtonDown(e);
                        break;
                    case DrawingMode.Dragging:
                        DraggingMode_OnMouseLeftButtonDown(e);
                        break;
                    case DrawingMode.Erasing:
                        ErasingMode_OnMouseLeftButtonDown(e);
                        break;
                }
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                switch (Mode)
                {
                    case DrawingMode.Placing:
                        PlacingMode_OnPreviewMouseLeftButtonUp(e);
                        break;
                    case DrawingMode.Drawing:
                        DrawingMode_OnPreviewMouseLeftButtonUp(e);
                        break;
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                switch (Mode)
                {
                    case DrawingMode.Dragging:
                        DraggingMode_OnMouseLeftButtonUp(e);
                        break;
                    case DrawingMode.Erasing:
                        ErasingMode_OnMouseLeftButtonUp(e);
                        break;
                    case DrawingMode.Ready:
                    default:
                        ReadyMode_OnMouseLeftButtonUp(e);
                        break;
                }
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                switch (Mode)
                {
                    case DrawingMode.Ready:
                        ReadyMode_OnPreviewMouseRightButtonDown(e);
                        break;
                }
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                switch (Mode)
                {
                    case DrawingMode.Ready:
                        ReadyMode_OnMouseRightButtonDown(e);
                        break;
                }
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (IsEnabled)
            {
                switch (Mode)
                {
                    case DrawingMode.Placing:
                        PlacingMode_OnMouseMove(e);
                        break;
                    case DrawingMode.Drawing:
                        DrawingMode_OnMouseMove(e);
                        break;
                    case DrawingMode.Dragging:
                        DraggingMode_OnMouseMove(e);
                        break;
                    case DrawingMode.Erasing:
                        ErasingMode_OnMouseMove(e);
                        break;
                    case DrawingMode.Ready:
                    default:
                        ReadyMode_OnMouseMove(e);
                        break;
                }
            }
        }

        #endregion

        #region Keyboard Input

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsEnabled)
            {
                if (Mode == DrawingMode.Placing)
                {
                    CancelPlacing();
                    e.Handled = true;
                    return;
                }

                if (Mode == DrawingMode.Drawing)
                {
                    CancelDrawing();
                    e.Handled = true;
                    return;
                }

                if (Keyboard.FocusedElement == this && 
                    !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) &&
                    !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt))
                {
                    switch (e.Key)
                    {
                        case Key.R:
                            Rotate(90);
                            e.Handled = true;
                            break;
                        case Key.L:
                            Rotate(-90);
                            e.Handled = true;
                            break;
                        case Key.H:
                            FlipHorizontal();
                            e.Handled = true;
                            break;
                        case Key.V:
                            FlipVertical();
                            e.Handled = true;
                            break;
                        case Key.G:
                            Group();
                            e.Handled = true;
                            break;
                        case Key.U:
                            Ungroup();
                            e.Handled = true;
                            break;
                    }
                }
            }

            base.OnKeyDown(e);
        }

        #endregion

        public void Merge(NCanvas other)
        {
            Vector offset = other.TransformToVisual(this).Transform(new Point(0, 0)) - new Point(0, 0);

            Shape[] drawings = other.Drawings.ToArray();
            foreach (Shape drawing in drawings)
            {
                other.Drawings.Remove(drawing);
                this.Drawings.Add(drawing);
                MoveDrawing(drawing, offset);
            }
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new NCanvas(this);
        }

        #endregion
    }

    public struct NConnection
    {
        public NConnection(Shape drawing, int handle)
        {
            _Drawing = drawing;
            _Handle = handle;
        }

        private Shape _Drawing;

        public Shape Drawing
        {
            get { return _Drawing; }
        }

        private int _Handle;

        public int Handle
        {
            get { return _Handle; }
        }
    };

}
