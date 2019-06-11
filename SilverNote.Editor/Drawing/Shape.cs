/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml;
using System.Diagnostics;
using DOM;
using DOM.Helpers;
using DOM.SVG;
using DOM.CSS;
using DOM.Events;
using Jurassic.Library;

namespace SilverNote.Editor
{
    public class Shape : DrawingVisual, IVisualElement, ISelectable, IFormattable, INodeSource, IStyleable, ICloneable
    {
        public const Brush DefaultStrokeBrush = null;
        public const double DefaultStrokeWidth = 1.0;
        public const DoubleCollection DefaultStrokeDashArray = null;
        public const PenLineJoin DefaultStrokeLineJoin = PenLineJoin.Miter;
        public static readonly Brush DefaultFillBrush = Brushes.Black;

        public static Shape Create(string name)
        {
            string[] classes = name.Split();

            // class names
            if (classes.Contains("routedLine"))
                return new RoutedLine();
            if (classes.Contains("quadraticCurve"))
                return new QuadraticCurve();
            if (classes.Contains("quadraticBezier"))
                return new QuadraticBezier();
            if (classes.Contains("arc"))
                return new Arc();
            if (classes.Contains("semiEllipse"))
                return new SemiEllipse();
            if (classes.Contains("textBox"))
                return new NTextBox();
            if (classes.Contains("snap"))
                return new SnapPoint();

            // tag names

            switch (name)
            {
                case "line":
                    return new Line();
                case "rect":
                    return new Rectangle();
                case "ellipse":
                    return new Ellipse();
                case "polygon":
                    return new Polygon();
                case "polyline":
                    return new PolyLine();
                case "path":
                    return new NPath();
                case "text":
                    return new NTextBox();
                case "g":
                    return new ShapeGroup();
                case "marker":
                    return new Marker();
                default:
                    return null;
            }
        }

        public static Shape FromResource(string path)
        {
            var canvas = NCanvas.FromResource(path);
            if (canvas != null)
            {
                return canvas.Drawings.LastOrDefault();
            }
            else
            {
                return null;
            }
        }

        #region Fields

        NCanvas _Canvas;
        Shape _Parent;
        Brush _StrokeBrush;
        double _StrokeWidth;
        DoubleCollection _StrokeDashArray;
        PenLineCap _StrokeLineCap;
        PenLineJoin _StrokeLineJoin;
        Brush _FillBrush;
        bool _IsMouseOver;
        bool _IsMouseDirectlyOver;
        bool _IsMoving;
        bool _IsVisible;
        bool _IsAdornerVisible;
        string _Script;

        #endregion

        #region Constructors

        public Shape()
        {
            Initialize();   
        }

        public Shape(Shape copy)
        {
            Initialize();

            IsThumb = copy.IsThumb;
            Label = copy.Label;
            SelectedHandle = copy.SelectedHandle;
            StrokeBrush = copy.StrokeBrush;
            StrokeWidth = copy.StrokeWidth;
            StrokeLineCap = copy.StrokeLineCap;
            StrokeLineJoin = copy.StrokeLineJoin;
            IsStrokeLocked = copy.IsStrokeLocked;

            if (copy.FillBrush == null || copy.FillBrush.IsFrozen)
            {
                FillBrush = copy.FillBrush;
            }
            else
            {
                FillBrush = (Brush)copy.FillBrush.CloneCurrentValue();
            }
            IsFillLocked = copy.IsFillLocked;
            Filter = copy.Filter;

            if (copy.GeometryTransform != null)
            {
                GeometryTransform = (Transform)copy.GeometryTransform.Clone();
            }

            if (copy.StrokeDashArray != null)
            {
                StrokeDashArray = (DoubleCollection)copy.StrokeDashArray.Clone();
            }

            IsVisible = copy.IsVisible;
            LayoutScript = copy.LayoutScript;
            DragScript = copy.DragScript;
            Cursor = copy.Cursor;
            _ClassName = copy._ClassName;
            _Script = copy._Script;
        }

        void Initialize()
        {
            Canvas = null;
            Label = String.Empty;
            SelectedHandle = -1;
            StrokeBrush = DefaultStrokeBrush;
            StrokeWidth = DefaultStrokeWidth;
            StrokeDashArray = DefaultStrokeDashArray;
            StrokeLineJoin = DefaultStrokeLineJoin;
            IsStrokeLocked = false;
            FillBrush = DefaultFillBrush;
            IsFillLocked = false;
            GeometryTransform = null;
            Filter = null;
            IsVisible = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The canvas to which this drawing belongs (null if none)
        /// </summary>
        public NCanvas Canvas
        {
            get
            {
                return _Canvas;
            }
            set
            {
                if (value != _Canvas)
                {
                    var oldValue = _Canvas;
                    _Canvas = value;
                    OnCanvasChangedInternal(oldValue);
                }
            }
        }

        /// <summary>
        /// This drawing's parent drawing (null if none)
        /// </summary>
        public Shape ParentDrawing
        {
            get
            {
                return _Parent;
            }
            set
            {
                if (value != _Parent)
                {
                    var oldValue = _Parent;
                    _Parent = value;
                    OnParentChangedInternal(oldValue);
                }
            }
        }

        public virtual Brush StrokeBrush
        {
            get { return _StrokeBrush; }
            set { SetStrokeBrush(value); }
        }

        public virtual double StrokeWidth
        {
            get { return _StrokeWidth; }
            set { SetStrokeWidth(value); }
        }

        public virtual DoubleCollection StrokeDashArray
        {
            get { return _StrokeDashArray; }
            set { SetStrokeDashArray(value); }
        }

        public virtual PenLineCap StrokeLineCap
        {
            get { return _StrokeLineCap; }
            set { SetStrokeLineCap(value);  }
        }

        public virtual PenLineJoin StrokeLineJoin
        {
            get { return _StrokeLineJoin; }
            set { SetStrokeLineJoin(value); }
        }

        public virtual Brush FillBrush
        {
            get { return _FillBrush; }
            set { SetFillBrush(value); }
        }

        public virtual double FillOpacity
        {
            get { return FillBrush != null ? FillBrush.Opacity : 1.0; }
            set { SetFillOpacity(value); }
        }

        public Pen StrokePen
        {
            get
            {
                Pen result = new Pen(StrokeBrush, StrokeWidth);
                result.DashStyle = new DashStyle(StrokeDashArray, 0);
                result.DashCap = StrokeLineCap;
                result.LineJoin = StrokeLineJoin;
                result.Freeze();
                return result;
            }
        }

        public virtual Effect Filter
        {
            get { return Effect; }
            set { Effect = value; }
        }

        /// <summary>
        /// True to prevent the user from changing this drawing's stroke properties
        /// </summary>
        public virtual bool IsStrokeLocked { get; set; }

        /// <summary>
        /// True to prevent the user from changint this drawing's fill properties
        /// </summary>
        public virtual bool IsFillLocked { get; set; }

        public virtual void LimitStrokeWidth(double maxValue)
        {
            StrokeWidth = Math.Min(StrokeWidth, maxValue);
        }

        public virtual Cursor Cursor { get; set; }

        /// <summary>
        /// Get any text content of this drawing
        /// </summary>
        public virtual string Text
        {
            get { return String.Empty; }
        }

        /// <summary>
        /// True if the mouse cursor is over this drawing
        /// </summary>
        public bool IsMouseOver
        {
            get { return _IsMouseOver; }
        }

        /// <summary>
        /// True if this drawing will be selected if the user clicks the mouse
        /// </summary>
        public bool IsMouseDirectlyOver
        {
            get { return _IsMouseDirectlyOver; }
        }

        public bool IsMoving
        {
            get { return _IsMoving; }
        }

        public bool IsVisible
        {
            get 
            { 
                return _IsVisible; 
            }
            set
            {
                if (value != _IsVisible)
                {
                    _IsVisible = value;
                    RaiseVisibilityChanged();
                }
            }
        }

        /// <summary>
        /// True if this drawing's selection adorner is being rendered
        /// </summary>
        public bool IsAdornerVisible
        {
            get { return _IsAdornerVisible; }
        }

        public virtual string Label { get; set; }

        #endregion

        #region Events

        public event EventHandler<EventArgs> VisibilityChanged;

        protected void RaiseVisibilityChanged()
        {
            if (VisibilityChanged != null)
            {
                VisibilityChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Thumbnails

        /// <summary>
        /// Get a 50x50 preview of this drawing
        /// </summary>
        public virtual Shape Preview
        {
            get
            {
                Shape result = (Shape)Clone();
                result.RenderedBounds = new Rect(0, 0, 50, 50);
                return result;
            }
        }

        /// <summary>
        /// Get a 30x30 large thumbnail of this drawing
        /// </summary>
        public virtual Shape ThumbLarge
        {
            get
            {
                Shape result = (Shape)Clone();
                result.LimitStrokeWidth(2);
                result.RenderedBounds = new Rect(0, 0, 30, 30);
                result.IsThumb = true;
                return result;
            }
        }

        /// <summary>
        /// Get a 15x15 small thumbnail of this drawing
        /// </summary>
        public virtual Shape ThumbSmall
        {
            get
            {
                Shape result = (Shape)Clone();
                result.StrokeBrush = Brushes.Black;
                result.StrokeWidth = 1;
                result.Filter = null;
                result.RenderedBounds = new Rect(1, 1, 15, 15);
                result.IsThumb = true;
                return result;
            }
        }

        /// <summary>
        /// True if this is a thumbnail
        /// </summary>
        public bool IsThumb { get; set; }

        /// <summary>
        /// Simplify this drawing for display on a small scale
        /// 
        /// Generally, this means getting rid of any decorations and minute details.
        /// </summary>
        public virtual void Simplify(bool verySmall = false)
        {
            if (verySmall)
            {
                Filter = null;
            }
        }

        #endregion

        #region Operations

        public virtual void Place(Point position)
        {

        }

        public virtual bool CompletePlacing()
        {
            return true;
        }

        public virtual void Draw(Point point)
        {

        }

        public virtual bool CompleteDrawing()
        {
            return true;
        }

        public virtual bool CancelDrawing()
        {
            return true;
        }

        public virtual Shape[] Erase(Point point)
        {
            return new Shape[] { };
        }

        public void AddScript(string script, string type = null)
        {
            if (String.IsNullOrWhiteSpace(type) ||
                type.ToLower() == "application/ecmascript" ||
                type.ToLower() == "text/javascript")
            {
                _Script = script;

                if (Canvas != null)
                {
                    Canvas.EvaluateJavascript(script);
                }
            }
        }

        public void RemoveScript(string script, string type = null)
        {
            _Script = null;
        }

        #endregion

        #region Handles

        public virtual int HandleCount
        {
            get { return 0; }
        }

        protected virtual Point GetHandleInternal(int index)
        {
            return new Point(0, 0);
        }

        protected virtual void SetHandleInternal(int index, Point value)
        {

        }

        public virtual Point GetHandle(int index)
        {
            Point point = GetHandleInternal(index);

            return GeometryTransform.Transform(point);
        }

        public virtual void SetHandle(int index, Point value)
        {
            value = GeometryTransform.Inverse.Transform(value);

            SetHandleInternal(index, value);
        }

        public virtual void MoveHandle(int index, Vector delta)
        {
            if (delta.Length != 0)
            {
                SetHandle(index, GetHandle(index) + delta);
            }
        }

        public int HitHandle(Point hit)
        {
            double zoom = NoteEditor.GetZoom(this);
            double width = HandleSize.Width / zoom;
            double height = HandleSize.Height / zoom;
            double padding = 2 / zoom;

            for (int i = 0; i < HandleCount; i++)
            {
                Point position = GetHandle(i);
                Rect rect = new Rect(
                    position.X - width / 2,
                    position.Y - height / 2,
                    width,
                    height
                );
                rect.Inflate(padding, padding);

                if (rect.Contains(hit))
                {
                    return i;
                }
            }

            return -1;
        }

        protected readonly Size HandleSize = new Size(4, 4);

        public int SelectedHandle { get; set; }

        #endregion

        #region Snaps

        public virtual int SnapCount
        {
            get { return HandleCount; }
        }

        public virtual Point GetSnap(int snapIndex)
        {
            return GetHandle(snapIndex);
        }

        public int SnapFromHandle(int handleIndex)
        {
            Point handlePoint = GetHandle(handleIndex);

            int snapCount = this.SnapCount;
            for (int snapIndex = 0; snapIndex < snapCount; snapIndex++)
            {
                Point snapPoint = GetSnap(snapIndex);
                if (snapPoint == handlePoint)
                {
                    return snapIndex;
                }
            }

            return -1;
        }

        private HashSet<int> _DisabledSnaps = new HashSet<int>();

        public void EnableSnap(int index, bool enable)
        {
            if (enable)
            {
                _DisabledSnaps.Remove(index);
            }
            else
            {
                _DisabledSnaps.Add(index);
            }
        }

        public bool IsSnapEnabled(int index)
        {
            return !_DisabledSnaps.Contains(index);
        }

        public virtual int HitSnap(Point hit)
        {
            for (int i = 0; i < SnapCount; i++)
            {
                if (!IsSnapEnabled(i))
                {
                    continue;
                }

                Point position = GetSnap(i);
                Rect rect = new Rect(
                    position.X - SnapSize.Width / 2,
                    position.Y - SnapSize.Height / 2,
                    SnapSize.Width,
                    SnapSize.Height
                );

                if (rect.Contains(hit))
                {
                    return i;
                }
            }

            return -1;
        }

        private readonly Size SnapSize = new Size(4, 4);

        #endregion

        #region Geometry

        public virtual Geometry Geometry
        {
            get { return this.ToStreamGeometry(); }
        }

        public virtual StreamGeometry ToStreamGeometry()
        {
            return null;
        }

        public virtual PathGeometry ToPathGeometry()
        {
            return null;
        }

        #endregion

        #region GeometryTransform

        private Transform _GeometryTransform = null;

        public virtual Transform GeometryTransform 
        {
            get
            {
                if (_GeometryTransform == null)
                {
                    _GeometryTransform = Transform.Identity;
                }

                return _GeometryTransform;
            }
            set 
            {
                if (_GeometryTransform != value)
                {
                    _GeometryTransform = value;
                }
            } 
        }

        public virtual void Translate(Vector delta)
        {
            Matrix matrix = GeometryTransform.Value;

            matrix.Translate(delta.X, delta.Y);

            GeometryTransform = new MatrixTransform(matrix);

            Invalidate();
        }

        public virtual void Scale(double scaleX, double scaleY)
        {
            Matrix matrix = GeometryTransform.Value;

            matrix.Scale(scaleX, scaleY);

            GeometryTransform = new MatrixTransform(matrix);

            Invalidate();
        }

        public virtual void ScaleAt(double scaleX, double scaleY, double centerX, double centerY)
        {
            Matrix matrix = GeometryTransform.Value;

            matrix.ScaleAt(scaleX, scaleY, centerX, centerY);

            GeometryTransform = new MatrixTransform(matrix);

            Invalidate();
        }

        public virtual void Rotate(double angleInDegrees)
        {
            Matrix matrix = GeometryTransform.Value;

            matrix.Rotate(angleInDegrees);

            GeometryTransform = new MatrixTransform(matrix);

            Invalidate();
        }

        public virtual void RotateAt(double angleInDegrees, double centerX, double centerY)
        {
            Matrix matrix = GeometryTransform.Value;

            matrix.RotateAt(angleInDegrees, centerX, centerY);

            GeometryTransform = new MatrixTransform(matrix);

            Invalidate();
        }

        /// <summary>
        /// Apply GeometryTransform and reset the transform
        /// </summary>
        public virtual void Normalize()
        {
            var handles = new List<Point>();

            for (int i = HandleCount - 1; i >= 0; i--)
            {
                handles.Add(GetHandle(i));
            }

            for (int i = 0; i < handles.Count; i++)
            {
                SetHandleInternal(i, handles[i]);
            }

            GeometryTransform = null;
        }

        protected virtual Rect Bounds
        {
            get { return Geometry.Bounds; }
        }

        public Rect RenderedBounds
        {
            get
            {
                return GeometryTransform.TransformBounds(Bounds);
            }
            set
            {
                Matrix matrix = GeometryTransform.Value;
                if (matrix.M12 != 0 || matrix.M21 != 0 || matrix.M11 < 0 || matrix.M22 < 0)
                {
                    Normalize();
                }
                matrix = Matrix.Identity;

                Rect actualBounds = Bounds;

                // Translate to the origin
                matrix.OffsetX = -actualBounds.Location.X;
                matrix.OffsetY = -actualBounds.Location.Y;

                // Set x-scale factor to: (desired width) / (actual width)
                if (actualBounds.Width != 0)
                {
                    matrix.Scale(value.Width / actualBounds.Width, 1);
                }
                else
                {
                    matrix.OffsetX += value.Width / 2;
                }

                // Set y-scale factor to: (desired height) / (actual height)
                if (actualBounds.Height != 0)
                {
                    matrix.Scale(1, value.Height / actualBounds.Height);
                }
                else
                {
                    matrix.OffsetY += value.Height / 2;
                }

                // Translate to desired location
                matrix.Translate(value.Location.X, value.Location.Y);

                GeometryTransform = new MatrixTransform(matrix);

                Invalidate();
            }
        }

        public static Rect GetBounds(Visual reference, IEnumerable<Shape> drawings)
        {
            var result = Rect.Empty;

            foreach (var drawing in drawings)
            {
                var bounds = drawing.TransformToAncestor(reference).TransformBounds(drawing.RenderedBounds);

                if (result.IsEmpty)
                {
                    result = bounds;
                }
                else
                {
                    result.Union(bounds);
                }
            }

            return result;
        }

        #endregion

        #region Drawing

        bool _IsRenderValid;
        bool _IsRedrawPending;

        public bool IsRenderValid
        {
            get { return _IsRenderValid; }
        }

        public void Invalidate()
        {
            InvalidateLayout();
            InvalidateRender();
        }

        /// <summary>
        /// Mark this drawing to be redrawn
        /// </summary>
        public void InvalidateRender()
        {
            _IsRenderValid = false;

            if (!_IsRedrawPending)
            {
                _IsRedrawPending = true;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!_IsRenderValid)
                    {
                        Redraw();
                    }

                    _IsRedrawPending = false;
                }));
            }
        }

        /// <summary>
        /// Redraw this drawing (now)
        /// </summary>
        public virtual void Redraw()
        {
            UpdateLayout();
            UpdateGuidelines();
            RedrawVisual();
            RedrawAdorner();
            _IsRenderValid = true;
        }

        private double _GuidelineOffset = Double.NaN;
        private Rect _GuidelineBounds;

        protected void UpdateGuidelines()
        {
            var zoom = NoteEditor.GetZoom(this);

            double strokeWidth = StrokeWidth * zoom;
            if (Double.IsNaN(strokeWidth))
            {
                return;
            }

            double guidelineOffset;

            if (Math.Round(strokeWidth % 2) == 0)
            {
                guidelineOffset = 0.0;
            }
            else
            {
                guidelineOffset = 0.5;
            }

            var guidelineBounds = this.RenderedBounds;

            if (guidelineOffset == _GuidelineOffset && _GuidelineBounds.Contains(guidelineBounds))
            {
                return;
            }

            var guidelinesX = new DoubleCollection();
            double x = Math.Floor(guidelineBounds.Left) - guidelineOffset; 
            while (x <= Math.Ceiling(guidelineBounds.Right))
            {
                guidelinesX.Add(x);
                x += 1.0;
            }

            var guidelinesY = new DoubleCollection();
            double y = Math.Floor(guidelineBounds.Top) - guidelineOffset;
            while (y <= Math.Ceiling(guidelineBounds.Bottom))
            {
                guidelinesY.Add(y);
                y += 1.0;
            }

            XSnappingGuidelines = guidelinesX;
            YSnappingGuidelines = guidelinesY;

            _GuidelineOffset = guidelineOffset;
            _GuidelineBounds = guidelineBounds;
        }

        protected virtual void RedrawVisual()
        {
            DrawingContext dc = RenderOpen();
            try
            {
                OnRenderVisual(dc);
                OnRenderDecorations(dc);
            }
            finally
            {
                dc.Close();
            }
        }

        protected virtual void OnRenderVisual(DrawingContext dc)
        {
            var geometry = this.ToStreamGeometry();
            if (geometry != null)
            {
                geometry.Transform = GeometryTransform;
                geometry.Freeze();
                dc.DrawGeometry(FillBrush, StrokePen, geometry);

                double zoom = NoteEditor.GetZoom(this);
                if (StrokeWidth * zoom < 5.0)
                {
                    var transparent = new Pen(Brushes.Transparent, 5.0 / zoom);
                    transparent.Freeze();
                    dc.DrawGeometry(null, transparent, geometry);
                }
            }
        }

        protected virtual void OnRenderDecorations(DrawingContext dc)
        {

        }

        private DrawingVisual Adorner = new DrawingVisual();

        protected virtual void RedrawAdorner()
        {
            DrawingContext dc = Adorner.RenderOpen();
            try
            {
                OnRenderAdorner(dc);
            }
            finally
            {
                dc.Close();
            }
        }
 
        protected virtual void OnRenderAdorner(DrawingContext dc)
        {
            for (int i = 0; i < HandleCount; i++)
            {
                Point position = GetHandle(i);

                OnRenderHandle(dc, position);

                if (i == SelectedHandle)
                {
                    OnRenderCursor(dc, position);
                }
            }
        }

        /// <summary>
        /// Render a square selection handle
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="position"></param>
        protected virtual void OnRenderHandle(DrawingContext dc, Point position)
        {
            double zoom = NoteEditor.GetZoom(this);

            double width = HandleSize.Width / zoom;
            double height = HandleSize.Height / zoom;
            double thickness = 1.0 / zoom;

            Pen stroke = new Pen(Brushes.Black, thickness);
            stroke.Freeze();

            double left = position.X - width / 2;
            double top = position.Y - height / 2;
            double right = left + width;
            double bottom = top + height;

            left = LayoutHelper.Align(left, thickness, zoom);
            top = LayoutHelper.Align(top, thickness, zoom);
            bottom = LayoutHelper.Align(bottom, thickness, zoom);
            right = LayoutHelper.Align(right, thickness, zoom);

            Rect rect = new Rect(left, top, right - left, bottom - top);

            dc.DrawRectangle(Brushes.White, stroke, rect);

            rect.Inflate(2.0 / zoom, 2.0 / zoom);
            dc.DrawRectangle(Brushes.Transparent, null, rect);
        }

        /// <summary>
        /// Render crosshairs over the selected handle
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="position"></param>
        protected virtual void OnRenderCursor(DrawingContext dc, Point position)
        {
            double zoom = NoteEditor.GetZoom(this);

            double width = 10.0 / zoom;
            double height = 10.0 / zoom;
            double thickness = 1.0 / zoom;

            Pen stroke = new Pen(Brushes.Black, thickness);
            stroke.Freeze();

            // Draw crosshairs

            Point top = new Point(position.X, position.Y - height / 2.0);
            Point bottom = new Point(position.X, position.Y + height / 2.0);
            Point left = new Point(position.X - width / 2.0, position.Y);
            Point right = new Point(position.X + width / 2.0, position.Y);

            top = LayoutHelper.Align(top, thickness, zoom);
            bottom = LayoutHelper.Align(bottom, thickness, zoom);
            left = LayoutHelper.Align(left, thickness, zoom);
            right = LayoutHelper.Align(right, thickness, zoom);

            dc.DrawLine(stroke, top, bottom);
            dc.DrawLine(stroke, left, right);
        }

        #endregion

        #region Scripting

        string _LayoutScript;
        FunctionInstance _LayoutFunction;
        bool _NeedsLayout;
        bool _IsLayoutPending;
        bool _LayoutSucceeded;

        public string LayoutScript
        {
            get
            {
                return _LayoutScript;
            }
            set
            {
                if (value != _LayoutScript)
                {
                    _LayoutScript = value;
                    _LayoutFunction = null;
                    InvalidateLayout();
                }
            }
        }

        public void InvalidateLayout()
        {
            _NeedsLayout = true;

            if (!_IsLayoutPending)
            {
                _IsLayoutPending = true;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_NeedsLayout)
                    {
                        Layout();
                    }

                    _IsLayoutPending = false;
                }));
            }
        }

        public void UpdateLayout()
        {
            if (_NeedsLayout)
            {
                Layout();
            }

            if (_LayoutSucceeded)
            {
                GeometryTransform = null;
            }
        }

        private FunctionInstance LayoutFunction
        {
            get
            {
                if (_LayoutFunction == null)
                {
                    if (!String.IsNullOrWhiteSpace(LayoutScript) && Canvas != null)
                    {
                        Canvas.EvaluateJavascript("function temp(bounds) { " + LayoutScript + " };");
                        _LayoutFunction = (FunctionInstance)Canvas.JSEngine.GetGlobalValue("temp");
                    }
                }
                return _LayoutFunction;
            }
        }

        private ObjectInstance LayoutObject
        {
            get
            {
                if (LayoutScript != null && LayoutScript.StartsWith("layout.update(this,") && Canvas != null)
                {
                    int i = LayoutScript.IndexOf(',');
                    int j = LayoutScript.IndexOfAny(",)".ToCharArray(), i + 1);
                    if (i != -1 && j != -1)
                    {
                        string json = LayoutScript.Substring(i + 1, j - i - 1).Trim();
                        try
                        {
                            return Canvas.JSEngine.Evaluate<ObjectInstance>(json);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
                return null;
            }
        }

        public virtual void Layout()
        {
            Rect bounds;

            if (ParentDrawing != null)
            {
                bounds = ParentDrawing.RenderedBounds;
            }
            else
            {
                bounds = RenderedBounds;
            }

            Layout(bounds);
        }

        protected virtual void Layout(Rect bounds)
        {
            _LayoutSucceeded = false;

            ObjectInstance layoutObject = null;
            FunctionInstance layoutFunction = null;

            if ((layoutObject = LayoutObject) == null && (layoutFunction = LayoutFunction) == null)
            {
                _NeedsLayout = false;
                return;
            }

            try
            {
                var originalTransform = GeometryTransform;

                if (!(this is ShapeGroup))
                {
                    GeometryTransform = null;
                }

                if (layoutObject != null)
                {
                    _LayoutSucceeded = Layout(layoutObject);
                }
                else
                {
                    _LayoutSucceeded = Layout(layoutFunction, bounds);
                }

                if (_LayoutSucceeded)
                {
                    InvalidateRender();
                }
                else
                {
                    GeometryTransform = originalTransform;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                _NeedsLayout = false;
            }
        }

        bool Layout(ObjectInstance obj)
        {
            var element = (ElementContext)Canvas.SVGDocument.GetNode(this);

            foreach (var property in obj.Properties)
            {
                if (property.Name == Attributes.STYLE)
                {
                    var style = (ObjectInstance)property.Value;
                    foreach (var styleProperty in style.Properties)
                    {
                        var cssValue = CSSProperties.GetPropertyCSSValue(styleProperty.Name, styleProperty.Value.ToString());
                        SetStyleProperty(element, styleProperty.Name, cssValue);
                    }
                }
                else
                {
                    SetNodeAttribute(element, property.Name, property.Value.ToString());
                }
            }

            return true;
        }

        bool Layout(FunctionInstance function, Rect bounds)
        {
            var element = (Element)Canvas.SVGDocument.GetNode(this);
            element.UpdateStyle(false);
            var thisObj = DOM.Javascript.JSFactory.CreateObject(Canvas.JSEngine, element);
            var argument = DOM.Javascript.JSFactory.CreateObject(Canvas.JSEngine, bounds);
            var result = function.Call(thisObj, argument);
            return !false.Equals(result);
        }

        string _DragScript;
        FunctionInstance _DragFunction;

        public string DragScript
        {
            get
            {
                return _DragScript;
            }
            set
            {
                if (value != _DragScript)
                {
                    _DragScript = value;
                    _DragFunction = null;
                }
            }
        }

        private FunctionInstance DragFunction
        {
            get
            {
                if (_DragFunction == null)
                {
                    if (!String.IsNullOrWhiteSpace(DragScript) && Canvas != null)
                    {
                        Canvas.EvaluateJavascript("function temp(bounds, delta) { " + DragScript + " };");
                        _DragFunction = (FunctionInstance)Canvas.JSEngine.GetGlobalValue("temp");
                    }
                }
                return _DragFunction;
            }
        }


        public void Drag(Rect bounds, Vector delta)
        {
            var function = DragFunction;
            if (function != null)
            {
                var node = Canvas.SVGDocument.GetNode(this);
                var obj = DOM.Javascript.JSFactory.CreateObject(Canvas.JSEngine, node);
                var arg1 = DOM.Javascript.JSFactory.CreateObject(Canvas.JSEngine, bounds);
                var arg2 = DOM.Javascript.JSFactory.CreateObject(Canvas.JSEngine, delta);

                function.Call(obj, arg1, arg2);

                if (ParentDrawing != null)
                {
                    ParentDrawing.InvalidateLayout();
                }
            }
        }

        #endregion

        #region IVisualElement

        public Rect VisualBounds
        {
            get { return RenderedBounds; }
        }

        #endregion

        #region ISelectable

        private bool _IsSelected = false;

        public virtual bool IsSelected
        {
            get { return _IsSelected; }
        }

        public virtual void Select()
        {
            if (!_IsSelected)
            {
                _IsSelected = true;
                OnSelectedInternal();
            }
        }

        public virtual void Unselect()
        {
            if (_IsSelected)
            {
                _IsSelected = false;
                OnUnselectedInternal();
            }
        }

        #endregion

        #region I/O

        public void MouseDown(MouseButtonEventArgs e)
        {
            RaiseMouseEvent(MouseEventTypes.MouseDown, e);

            if (!e.Handled)
            {
                DispatchEvent(
                    (target) =>
                    {
                        target.OnPreviewMouseDownInternal(this, e);
                        return e.Handled;
                    },
                    (target) =>
                    {
                        target.OnMouseDownInternal(this, e);
                        return e.Handled;
                    });
            }
        }

        public void MouseUp(MouseButtonEventArgs e)
        {
            RaiseMouseEvent(MouseEventTypes.MouseUp, e);

            if (!e.Handled)
            {
                DispatchEvent(
                    (target) =>
                    {
                        target.OnPreviewMouseUpInternal(this, e);
                        return e.Handled;
                    },
                    (target) =>
                    {
                        target.OnMouseUpInternal(this, e);
                        return e.Handled;
                    });
            }
        }

        public void MouseMove(MouseEventArgs e)
        {
            RaiseMouseEvent(MouseEventTypes.MouseMove, e);

            if (!e.Handled)
            {
                DispatchEvent(
                    (target) =>
                    {
                        target.OnPreviewMouseMoveInternal(this, e);
                        return e.Handled;
                    },
                    (target) =>
                    {
                        target.OnMouseMoveInternal(this, e);
                        return e.Handled;
                    });
            }
        }

        delegate bool DispatchEventDelegate(Shape currentTarget);

        void DispatchEvent(DispatchEventDelegate previewCallback, DispatchEventDelegate bubblingCallback)
        {
            var ancestors = new List<Shape>();

            for (var drawing = this; drawing != null; drawing = drawing.ParentDrawing)
            {
                ancestors.Add(drawing);
            }

            foreach (var ancestor in ancestors.Reverse<Shape>())
            {
                if (previewCallback(this))
                {
                    return;
                }
            }

            foreach (var ancestor in ancestors)
            {
                if (bubblingCallback(this))
                {
                    return;
                }
            }
        }

        protected void OnPreviewMouseDownInternal(Shape target, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    OnPreviewMouseLeftButtonDownInternal(target, e);
                    break;
            }
        }

        protected void OnPreviewMouseUpInternal(Shape target, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    OnPreviewMouseLeftButtonUpInternal(target, e);
                    break;
            }
        }

        protected void OnMouseDownInternal(Shape target, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    OnMouseLeftButtonDownInternal(target, e);
                    break;
            }
        }

        protected void OnMouseUpInternal(Shape target, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    OnMouseLeftButtonUpInternal(target, e);
                    break;
            }
        }

        protected void OnPreviewMouseLeftButtonDownInternal(Shape target, MouseButtonEventArgs e)
        {
            OnPreviewMouseLeftButtonDown(target, e);
        }

        protected void OnPreviewMouseLeftButtonUpInternal(Shape target, MouseButtonEventArgs e)
        {
            OnPreviewMouseLeftButtonUp(target, e);
        }

        protected void OnPreviewMouseMoveInternal(Shape target, MouseEventArgs e)
        {
            OnPreviewMouseMove(target, e);
        }

        protected void OnMouseLeftButtonDownInternal(Shape target, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(target, e);
        }

        protected void OnMouseLeftButtonUpInternal(Shape target, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonUp(target, e);
        }

        protected void OnMouseMoveInternal(Shape target, MouseEventArgs e)
        {
            OnMouseMove(target, e);
        }

        protected virtual void OnPreviewMouseLeftButtonDown(Shape target, MouseButtonEventArgs e)
        {

        }

        protected virtual void OnPreviewMouseLeftButtonUp(Shape target, MouseButtonEventArgs e)
        {

        }

        protected virtual void OnPreviewMouseMove(Shape target, MouseEventArgs e)
        {

        }

        bool _IsDragging;
        Point _LastDragPoint;

        protected virtual void OnMouseLeftButtonDown(Shape target, MouseButtonEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(DragScript) && ParentDrawing != null && target == this)
            {
                _LastDragPoint = e.GetPosition(Canvas);
                _IsDragging = true;
                Canvas.CaptureMouse(this);
                e.Handled = true;
            }
        }

        protected virtual void OnMouseLeftButtonUp(Shape target, MouseButtonEventArgs e)
        {
            if (_IsDragging)
            {
                Canvas.CaptureMouse(null);
                _IsDragging = false;
                e.Handled = true;
            }
        }

        protected virtual void OnMouseMove(Shape target, MouseEventArgs e)
        {
            if (_IsDragging && ParentDrawing != null && target == this)
            {
                var point = e.GetPosition(Canvas);
                var delta = point - _LastDragPoint;
                _LastDragPoint = point;

                Drag(ParentDrawing.RenderedBounds, delta);
                ParentDrawing.Redraw();

                e.Handled = true;
            }
        }

        #endregion

        #region IFormattable

        public const string StrokeProperty = "stroke";
        public const string StrokeWidthProperty = "stroke-width";
        public const string StrokeDashProperty = "stroke-dasharray";
        public const string StrokeLineCapProperty = "stroke-linecap";
        public const string FillProperty = "fill";
        public const string FilterProperty = "filter";

        public virtual bool HasProperty(string name)
        {
            switch (name)
            {
                case StrokeProperty:
                case StrokeWidthProperty:
                case StrokeDashProperty:
                case StrokeLineCapProperty:
                case FillProperty:
                case FilterProperty:
                    return true;
                default:
                    return false;
            }
        }

        public virtual void SetProperty(string name, object value)
        {
            switch (name)
            {
                case StrokeProperty:
                    if (value == null || value is Brush)
                        StrokeBrush = (Brush)value;
                    else
                        StrokeBrush = SafeConvert.ToBrush(value.ToString(), Brushes.Black);
                    break;
                case StrokeWidthProperty:
                    if (value is double)
                        StrokeWidth = (double)value;
                    else
                        StrokeWidth = SafeConvert.ToDouble(value);
                    break;
                case StrokeDashProperty:
                    if (value == null || value is DoubleCollection)
                        StrokeDashArray = (DoubleCollection)value;
                    else
                        StrokeDashArray = SafeConvert.ToDoubleCollection(value.ToString());
                    break;
                case StrokeLineCapProperty:
                    if (value == null || value is PenLineCap)
                        StrokeLineCap = (PenLineCap)value;
                    else
                        StrokeLineCap = PenLineCap.Flat;
                    break;
                case FillProperty:
                    if (value == null || value is Brush)
                        FillBrush = (Brush)value;
                    else
                        FillBrush = SafeConvert.ToBrush(value.ToString());
                    break;
                case FilterProperty:
                    if (value == null || value is Effect)
                        Filter = (Effect)value;
                    else
                        Filter = null;
                    break;
            }
        }

        public virtual object GetProperty(string name)
        {
            switch (name)
            {
                case StrokeProperty:
                    return StrokeBrush;
                case StrokeWidthProperty:
                    return StrokeWidth;
                case StrokeDashProperty:
                    return StrokeDashArray;
                case StrokeLineCapProperty:
                    return StrokeLineCap;
                case FillProperty:
                    return FillBrush;
                case FilterProperty:
                    return Filter;
                default:
                    return null;
            }
        }

        public virtual void ResetProperties()
        {

        }

        public virtual int ChangeProperty(string name, object oldValue, object newValue)
        {
            return 0;
        }

        #endregion

        #region INodeSource

        string _ID;
        string _ClassName;

        public virtual NodeType GetNodeType(NodeContext context)
        {
            return NodeType.ELEMENT_NODE;
        }

        public virtual string GetNodeName(NodeContext context)
        {
            throw new NotImplementedException();
        }

        private readonly string[] _NodeAttributes = new string[] { 
            SVGAttributes.ID,
            SVGAttributes.CLASS,
            SVGAttributes.STROKE_WIDTH,
            SVGAttributes.STROKE,
            SVGAttributes.STROKE_DASHARRAY,
            SVGAttributes.STROKE_LINECAP,
            SVGAttributes.STROKE_LINEJOIN,
            SVGAttributes.FILL,
            SVGAttributes.FILTER,
            SVGAttributes.TRANSFORM,
            SVGAttributesExt.DRAG,
            SVGAttributesExt.LAYOUT
        };

        public virtual IList<string> GetNodeAttributes(ElementContext context)
        {
            return _NodeAttributes;
        }

        public virtual string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.ID:
                    return _ID;
                case SVGAttributes.CLASS:
                    string className = _ClassName ?? "";
                    if (IsStrokeLocked) className = DOMHelper.PrependClass(className, "stroke-locked");
                    if (IsFillLocked) className = DOMHelper.PrependClass(className, "fill-locked");
                    if (IsThumb) className = DOMHelper.PrependClass(className, "thumb");
                    return !String.IsNullOrWhiteSpace(className) ? className : null;
                case SVGAttributes.FILL:
                    if (FillBrush == null || FillBrush is SolidColorBrush)
                    {
                        var fill = SVGConverter.ToSVGPaint(FillBrush);
                        return SVGFormatter.FormatPaint(fill);
                    }
                    return Canvas.GetDefinitionURL(FillBrush);
                case SVGAttributes.FILTER:
                    if (Effect != null)
                        return Canvas.GetDefinitionURL(Effect);
                    else
                        return null;
                case SVGAttributes.STROKE:
                    if (Double.IsNaN(StrokeWidth))
                        return null;
                    if (StrokeBrush == null || StrokeBrush is SolidColorBrush)
                    {
                        var stroke = SVGConverter.ToSVGPaint(StrokeBrush);
                        return SVGFormatter.FormatPaint(stroke);
                    }
                    return Canvas.GetDefinitionURL(StrokeBrush);
                case SVGAttributes.STROKE_DASHARRAY:
                    if (!Double.IsNaN(StrokeWidth) && StrokeDashArray != null && StrokeDashArray.Count > 0)
                        return SafeConvert.ToString(StrokeDashArray);
                    else
                        return null;
                case SVGAttributes.STROKE_LINECAP:
                    if (!Double.IsNaN(StrokeWidth) && StrokeLineCap != PenLineCap.Flat)
                        return SafeConvert.ToString(StrokeLineCap);
                    else
                        return null;
                case SVGAttributes.STROKE_LINEJOIN:
                    if (!Double.IsNaN(StrokeWidth) && StrokeLineJoin != PenLineJoin.Miter)
                        return SafeConvert.ToString(StrokeLineJoin);
                    else
                        return null;
                case SVGAttributes.STROKE_WIDTH:
                    if (!Double.IsNaN(StrokeWidth))
                        return SVGFormatter.FormatLength(StrokeWidth);
                    else
                        return null;
                case SVGAttributesExt.DRAG:
                    return DragScript;
                case SVGAttributesExt.LAYOUT:
                    return LayoutScript;
                default:
                    return null;
            }
        }

        public virtual void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.ID:
                    _ID = value;
                    break;
                case SVGAttributes.CLASS:
                    IsStrokeLocked = DOMHelper.HasClass(value, "stroke-locked");
                    IsFillLocked = DOMHelper.HasClass(value, "fill-locked");
                    IsThumb = DOMHelper.HasClass(value, "thumb");
                    _ClassName = value;
                    break;
                case SVGAttributes.FILL:
                    SVGPaint fill = SVGParser.ParsePaint(value);
                    if (fill.PaintType == SVGPaintType.SVG_PAINTTYPE_NONE)
                        SetFillBrush(null, true);
                    else if (!String.IsNullOrEmpty(fill.Uri))
                        SetFillBrush(Canvas.GetDefinition(fill.Uri) as Brush, true);
                    else if (fill.RgbColor != null)
                        SetFillBrush(CSSConverter.ToBrush(fill.RgbColor), true);
                    break;
                case SVGAttributes.FILTER:
                    if (value.StartsWith("url"))
                        Filter = Canvas.GetDefinition(value) as Effect;
                    else
                        Filter = null;
                    break;
                case SVGAttributes.STROKE:
                    SVGPaint stroke = SVGParser.ParsePaint(value);
                    if (stroke.PaintType == SVGPaintType.SVG_PAINTTYPE_NONE)
                        SetStrokeBrush(null, true);
                    else if (!String.IsNullOrEmpty(stroke.Uri))
                        SetStrokeBrush(Canvas.GetDefinition(stroke.Uri) as Brush, true);
                    else if (stroke.RgbColor != null)
                        SetStrokeBrush(CSSConverter.ToBrush(stroke.RgbColor), true);
                    break;
                case SVGAttributes.STROKE_DASHARRAY:
                    SetStrokeDashArray(SafeConvert.ToDoubleCollection(value, null), true);
                    break;
                case SVGAttributes.STROKE_LINECAP:
                    SetStrokeLineCap(SafeConvert.ToPenLineCap(value, PenLineCap.Flat), true);
                    break;
                case SVGAttributes.STROKE_LINEJOIN:
                    SetStrokeLineJoin(SafeConvert.ToPenLineJoin(value, PenLineJoin.Miter), true);
                    break;
                case SVGAttributes.STROKE_WIDTH:
                    SVGLength strokeWidth = SVGParser.ParseLength(value);
                    if (strokeWidth != null)
                        SetStrokeWidth(strokeWidth.Value, true);
                    break;
                case SVGAttributes.TRANSFORM:
                    SVGTransform transform = SVGParser.ParseTransformList(value).Consolidate();
                    if (transform != null)
                    {
                        GeometryTransform = SVGConverter.ToTransform(transform);
                    }
                    else
                    {
                        GeometryTransform = Transform.Identity;
                    }
                    break;
                case SVGAttributesExt.DRAG:
                    DragScript = value;
                    break;
                case SVGAttributesExt.LAYOUT:
                    LayoutScript = value;
                    break;
                default:
                    break;
            }
        }

        public virtual void ResetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.ID:
                    _ID = null;
                    break;
                case SVGAttributes.CLASS:
                    IsStrokeLocked = IsFillLocked = IsThumb = false;
                    _ClassName = null;
                    break;
                case SVGAttributes.FILL:
                    SetFillBrush(null, true);
                    break;
                case SVGAttributes.FILTER:
                    Filter = null;
                    break;
                case SVGAttributes.STROKE:
                    SetStrokeBrush(Brushes.Black, true);
                    break;
                case SVGAttributes.STROKE_DASHARRAY:
                    SetStrokeDashArray(null, true);
                    break;
                case SVGAttributes.STROKE_LINECAP:
                    SetStrokeLineCap(PenLineCap.Flat, true);
                    break;
                case SVGAttributes.STROKE_LINEJOIN:
                    SetStrokeLineJoin(PenLineJoin.Miter, true);
                    break;
                case SVGAttributes.STROKE_WIDTH:
                    SetStrokeWidth(1.0, true);
                    break;
                case SVGAttributesExt.DRAG:
                    DragScript = null;
                    break;
                case SVGAttributesExt.LAYOUT:
                    LayoutScript = null;
                    break;
                default:
                    break;
            }
        }

        public virtual object GetParentNode(NodeContext context)
        {
            if (Parent is Shape)
            {
                return Parent;
            }

            if (Canvas == null)
            {
                return null;
            }

            if (context.OwnerDocument == Canvas.SVGDocument)
            {
                return Canvas.SVGDocument.RootElement;
            }

            return Canvas;
        }

        public virtual IEnumerable<object> GetChildNodes(NodeContext context)
        {
            if (!String.IsNullOrWhiteSpace(_Script))
            {
                var script = (SVGScriptElement)context.OwnerDocument.CreateElementNS(SVGElements.NAMESPACE, SVGElements.SCRIPT);
                script.TextContent = _Script;
                yield return script;
            }
        }

        public virtual object CreateNode(NodeContext context)
        {
            if (context is SVGScriptElement)
            {
                return context;
            }
            else
            {
                return null;
            }
        }

        public virtual void AppendNode(NodeContext context, object newChild)
        {
            var script = newChild as SVGScriptElement;
            if (script != null)
            {
                AddScript(script.TextContent, script.Type);
            }
        }

        public virtual void InsertNode(NodeContext context, object newChild, object refChild)
        {
            var script = newChild as SVGScriptElement;
            if (script != null)
            {
                AddScript(script.TextContent, script.Type);
            }
        }

        public virtual void RemoveNode(NodeContext context, object oldChild)
        {
            var script = oldChild as SVGScriptElement;
            if (script != null)
            {
                RemoveScript(script.TextContent, script.Type);
            }
            else
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }
        }

        public event NodeEventHandler NodeEvent;

        public void RaiseNodeEvent(IEventSource e)
        {
            OnNodeEvent(e);

            if (NodeEvent != null)
            {
                NodeEvent(e);
            }
        }

        protected virtual void OnNodeEvent(IEventSource e)
        {
            
        }

        MouseEventSource _MouseEventSource = new MouseEventSource();

        void RaiseMouseEvent(string eventType, MouseEventArgs e)
        {
            Point clientPoint = e.GetPosition(Canvas);
            Point screenPoint;
            try
            {
                // This sometimes fails because the canvas may not be attached
                // to a presentation source - needs to be fixed.
                screenPoint = Canvas.PointToScreen(clientPoint);
            }
            catch
            {
                screenPoint = clientPoint;
            }

            ushort button = 0;
            ushort detail = 0;  // click count

            var mouseButtonEventArgs = e as MouseButtonEventArgs;
            if (mouseButtonEventArgs != null)
            {
                switch (mouseButtonEventArgs.ChangedButton)
                {
                    case MouseButton.Left:
                        button = MouseButtons.LeftButton;
                        break;
                    case MouseButton.Right:
                        button = MouseButtons.RightButton;
                        break;
                    case MouseButton.Middle:
                        button = MouseButtons.MiddleButton;
                        break;
                }
                detail = (ushort)mouseButtonEventArgs.ClickCount;
            }

            ushort buttons = MouseButtons.None;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                buttons |= MouseButtons.LeftButtonFlag;
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                buttons |= MouseButtons.MiddleButtonFlag;
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                buttons |= MouseButtons.RightButtonFlag;
            }

            _MouseEventSource.InitMouseEvent(
                eventType, true, true, null, detail,
                (int)screenPoint.X, (int)screenPoint.Y,
                (int)clientPoint.X, (int)clientPoint.Y,
                Keyboard.Modifiers.HasFlag(ModifierKeys.Control),
                Keyboard.Modifiers.HasFlag(ModifierKeys.Alt),
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift),
                Keyboard.Modifiers.HasFlag(ModifierKeys.Windows),
                button, buttons,
                null);

            RaiseNodeEvent(_MouseEventSource);

            e.Handled |= _MouseEventSource.IsCanceled;
        }

        #endregion

        #region IStyleable

        private static readonly string[] _SupportedStyles = new string[]
            {
                CSSProperties.Cursor,
                CSSProperties.Display
            };

        /// <summary>
        /// Get the CSS styles supported by this element
        /// </summary>
        public virtual IList<string> GetSupportedStyles(ElementContext context)
        {
            return _SupportedStyles;
        }

        /// <summary>
        /// Get a CSS style for this element
        /// </summary>
        /// <param name="name">Name of the style to retrieve</param>
        /// <returns>The CSS value, or null if not set</returns>
        public virtual CSSValue GetStyleProperty(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.Cursor:
                    return CSSConverter.ToCSSValue(Cursor);
                case CSSProperties.Display:
                    return IsVisible ? null : CSSValues.None;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Set a CSS style for this element
        /// </summary>
        public virtual void SetStyleProperty(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.Cursor:
                    Cursor = CSSConverter.ToCursor(value);
                    break;
                case CSSProperties.Display:
                    IsVisible = value != CSSValues.None;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new Shape(this);
        }

        #endregion

        #region Implementation

        protected virtual void SetStrokeBrush(Brush newValue, bool force = false)
        {
            if (!IsStrokeLocked || force)
            {
                if (newValue != _StrokeBrush)
                {
                    _StrokeBrush = newValue;
                    InvalidateRender();
                }
            }
        }

        protected virtual void SetStrokeWidth(double newValue, bool force = false)
        {
            if (!IsStrokeLocked || force)
            {
                if (newValue != _StrokeWidth)
                {
                    _StrokeWidth = newValue;
                    InvalidateRender();
                }
            }
        }

        protected virtual void SetStrokeDashArray(DoubleCollection newValue, bool force = false)
        {
            if (!IsStrokeLocked || force)
            {
                if (newValue != _StrokeDashArray)
                {
                    _StrokeDashArray = newValue;
                    InvalidateRender();
                }
            }
        }

        protected virtual void SetStrokeLineCap(PenLineCap newValue, bool force = false)
        {
            if (!IsStrokeLocked || force)
            {
                if (newValue != _StrokeLineCap)
                {
                    _StrokeLineCap = newValue;
                    InvalidateRender();
                }
            }
        }

        protected virtual void SetStrokeLineJoin(PenLineJoin newValue, bool force = false)
        {
            if (!IsStrokeLocked || force)
            {
                if (newValue != _StrokeLineJoin)
                {
                    _StrokeLineJoin = newValue;
                    InvalidateRender();
                }
            }
        }

        protected virtual void SetFillBrush(Brush newValue, bool force = false)
        {
            if (!IsFillLocked || force)
            {
                if (newValue != _FillBrush)
                {
                    _FillBrush = newValue;
                    InvalidateRender();
                }
            }
        }

        void SetFillOpacity(double newValue)
        {
            // No need to enforce IsFillLocked here because FillOpacity
            // is only used internally (TODO: change this)

            if (newValue != FillOpacity)
            {
                if (_FillBrush != null && _FillBrush != Brushes.Transparent)
                {
                    if (_FillBrush.IsFrozen)
                    {
                        _FillBrush = (Brush)_FillBrush.CloneCurrentValue();
                    }

                    _FillBrush.Opacity = newValue;

                    if (_FillBrush.IsFrozen)
                    {
                        InvalidateRender();
                    }
                }
            }
        }

        /// <summary>
        /// Round a value to the given decimal place
        /// 
        /// For example, Round(8.123456789, -3) returns 8.123
        /// </summary>
        public static double Round(double value, int precision)
        {
            double factor = Math.Pow(10, precision);

            return Math.Round(value / factor) * factor;
        }

        public static Point Round(Point point, int precision)
        {
            double x = Round(point.X, precision);
            double y = Round(point.Y, precision);

            return new Point(x, y);
        }

        public static Rect Round(Rect rect, int precision)
        {
            double x = Round(rect.X, precision);
            double y = Round(rect.Y, precision);
            double width = Round(rect.Width, precision);
            double height = Round(rect.Height, precision);

            return new Rect(x, y, width, height);
        }

        public static PointCollection Round(PointCollection points, int precision)
        {
            var result = new PointCollection();

            foreach (var point in points)
            {
                result.Add(Round(point, precision));
            }

            return result;
        }

        public static bool CompareBrushes(Brush brush1, Brush brush2)
        {
            if (brush1 == brush2)
            {
                return true;
            }
            else if (brush1 is SolidColorBrush && brush2 is SolidColorBrush)
            {
                return CompareBrushes((SolidColorBrush)brush1, (SolidColorBrush)brush2);
            }
            else
            {
                return false;
            }
        }

        public static bool CompareBrushes(SolidColorBrush brush1, SolidColorBrush brush2)
        {
            return brush1.Color == brush2.Color;
        }

        public static bool IsNullOrTransparent(Brush brush)
        {
            if (brush == null || brush.Opacity == 0)
            {
                return true;
            }
            else if (brush is SolidColorBrush)
            {
                return IsNullOrTransparent((SolidColorBrush)brush);
            }
            else
            {
                return false;
            }
        }

        public static bool IsNullOrTransparent(SolidColorBrush brush)
        {
            return (brush.Color.A == 0);
        }

        private void OnCanvasChangedInternal(NCanvas oldCanvas)
        {
            _LayoutFunction = null;
            _DragFunction = null;

            if (Canvas != null && !String.IsNullOrWhiteSpace(_Script))
            {
                Canvas.EvaluateJavascript(_Script);
            }

            InvalidateRender();

            OnCanvasChanged(oldCanvas);
        }

        protected virtual void OnCanvasChanged(NCanvas oldCanvas)
        {

        }

        private void OnParentChangedInternal(Shape oldParent)
        {
            UpdateIsAdornerVisible();
            UpdateFillOpacity();
            OnParentChanged(oldParent);
        }

        protected virtual void OnParentChanged(Shape oldParent)
        {

        }

        private void OnSelectedInternal()
        {
            UpdateIsAdornerVisible();
            OnSelected();
        }

        protected virtual void OnSelected()
        {

        }

        private void OnUnselectedInternal()
        {
            UpdateIsAdornerVisible();
            OnUnselected();
        }

        protected virtual void OnUnselected()
        {

        }

        public void MouseEnter(bool directlyOver)
        {
            if (!_IsMouseOver || _IsMouseDirectlyOver != directlyOver)
            {
                _IsMouseOver = true;
                _IsMouseDirectlyOver = directlyOver;
                OnMouseEnterInternal(directlyOver);
            }
        }

        private void OnMouseEnterInternal(bool directlyOver)
        {
            UpdateIsAdornerVisible();
            UpdateFillOpacity();
            OnMouseEnter(directlyOver);
        }

        protected virtual void OnMouseEnter(bool directlyOver)
        {

        }

        public void MouseLeave()
        {
            if (_IsMouseOver)
            {
                _IsMouseOver = false;
                _IsMouseDirectlyOver = false;
                OnMouseLeaveInternal();
            }
        }

        private void OnMouseLeaveInternal()
        {
            UpdateIsAdornerVisible();
            UpdateFillOpacity();
            OnMouseLeave();
        }

        protected virtual void OnMouseLeave()
        {

        }

        public void MoveStarted()
        {
            OnMoveStartedInternal();
        }

        private void OnMoveStartedInternal()
        {
            _IsMoving = true;
            UpdateFillOpacity();
            OnMoveStarted();
        }

        protected virtual void OnMoveStarted()
        {

        }

        public void MoveCompleted()
        {
            OnMoveCompletedInternal();
        }

        private void OnMoveCompletedInternal()
        {
            Translate(Offset);
            Offset = new Vector(0, 0);
            _IsMoving = false;
            UpdateFillOpacity();
            OnMoveCompleted();
        }

        protected virtual void OnMoveCompleted()
        {

        }

        public void ShowAdorner()
        {
            if (!_IsAdornerVisible)
            {
                _IsAdornerVisible = true;
                OnShowAdornerInternal();
            }
        }

        private void OnShowAdornerInternal()
        {
            if (ParentDrawing == null)
            {
                Children.Add(Adorner);
                RedrawAdorner();
            }
            OnShowAdorner();
        }

        protected virtual void OnShowAdorner()
        {

        }

        public void HideAdorner()
        {
            if (_IsAdornerVisible)
            {
                _IsAdornerVisible = false;
                OnHideAdornerInternal();
            }
        }

        private void OnHideAdornerInternal()
        {
            Children.Remove(Adorner);
            OnHideAdorner();
        }

        protected virtual void OnHideAdorner()
        {

        }

        protected void UpdateIsAdornerVisible()
        {
            if (IsSelected || IsMouseDirectlyOver)
            {
                ShowAdorner();
            }
            else
            {
                HideAdorner();
            }
        }

        protected void UpdateFillOpacity()
        {
            if (IsMouseOver || IsMoving)
            {
                FillOpacity = 0.5;
            }
            else
            {
                FillOpacity = 1.0;
            }
        }

        #endregion
    }
}
