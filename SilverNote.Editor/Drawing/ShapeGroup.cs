/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml;
using DOM;
using DOM.SVG;
using DOM.CSS;
using DOM.Helpers;

namespace SilverNote.Editor
{
    public class ShapeGroup : Shape, ISelectableText, IEditable
    {
        #region Fields

        ShapeCollection _Drawings = new ShapeCollection();
        Rect _Bounds = Rect.Empty;

        #endregion

        #region Constructors

        public ShapeGroup()
        {
            Initialize();
        }

        public ShapeGroup(ShapeGroup copy)
            : base(copy)
        {
            Initialize();

            IsLocked = copy.IsLocked;
            DragToPlace = copy.DragToPlace;

            foreach (Shape copyDrawing in copy.Drawings)
            {
                Shape drawing = (Shape)copyDrawing.Clone();
                Drawings.Add(drawing);  
            }
        }

        void Initialize()
        {
            IsLocked = false;
            DragToPlace = false;
            OnDrawingsChanged(null, _Drawings);
        }

        #endregion

        #region Properties

        public override Brush StrokeBrush
        {
            get
            {
                return null;
            }
            set
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.StrokeBrush = value;
                }
            }
        }

        public override double StrokeWidth
        {
            get
            {
                return Double.NaN;
            }
            set
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.StrokeWidth = value;
                }
            }
        }

        public override void LimitStrokeWidth(double maxValue)
        {
            foreach (Shape drawing in Drawings)
            {
                drawing.LimitStrokeWidth(maxValue);
            }
        }

        public override DoubleCollection StrokeDashArray
        {
            get
            {
                return base.StrokeDashArray;
            }
            set
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.StrokeDashArray = value;
                }
            }
        }

        public override PenLineCap StrokeLineCap
        {
            get
            {
                return base.StrokeLineCap;
            }
            set
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.StrokeLineCap = value;
                }
            }
        }

        public override Brush FillBrush
        {
            get
            {
                return null;
            }
            set
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.FillBrush = value;
                }
            }
        }

        public override double FillOpacity
        {
            get
            {
                return base.FillOpacity;
            }
            set
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.FillOpacity = value;
                }
            }
        }

        public override Effect Filter
        {
            get
            {
                return null;
            }
            set
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.Filter = value;
                }
            }
        }

        public override string Text
        {
            get { return String.Concat(Drawings.Select(d => d.Text)); }
        }

        public override Shape Preview
        {
            get
            {
                var result = CreateThumb();
                Rect bounds = result.RenderedBounds;

                // Scale to fit in a 50x50 view, maintaining aspect ratio

                if (bounds.Width >= bounds.Height)
                {
                    double width = 50;
                    double height = width * (bounds.Height / bounds.Width);
                    double offset = (50 - height) / 2;
                    result.RenderedBounds = new Rect(0, offset, width, height);
                }
                else
                {
                    double height = 50;
                    double width = height * (bounds.Width / bounds.Height);
                    double offset = (50 - width) / 2;
                    result.RenderedBounds = new Rect(offset, 0, width, height);
                }

                return result;
            }
        }

        public override Shape ThumbLarge
        {
            get
            {
                var result = CreateThumb();
                result.LimitStrokeWidth(2);
                result.Simplify(false);

                // Scale to fit in a 30x30 view, maintaining aspect ratio

                Rect bounds = result.RenderedBounds;

                if (bounds.Width >= bounds.Height)
                {
                    double aspectRatio = bounds.Height / bounds.Width;
                    double width = Math.Min(30, bounds.Width);
                    double height = width * aspectRatio;
                    double x = (30 - width) / 2;
                    double y = (30 - height) / 2;
                    result.RenderedBounds = new Rect(x, y, width, height);
                }
                else
                {
                    double aspectRatio = bounds.Width / bounds.Height;
                    double height = Math.Min(30, bounds.Height);
                    double width = height * aspectRatio;
                    double x = (30 - width) / 2;
                    double y = (30 - height) / 2;
                    result.RenderedBounds = new Rect(x, y, width, height);
                }

                return result;
            }
        }

        public override Shape ThumbSmall
        {
            get
            {
                var result = CreateThumb();
                result.LimitStrokeWidth(2);
                result.Simplify(true);

                // Scale to fit in a 15x15 view, maintaining aspect ratio

                Rect bounds = result.RenderedBounds;

                if (bounds.Width >= bounds.Height)
                {
                    double aspectRatio = bounds.Height / bounds.Width;
                    double width = 15;
                    double height = width * aspectRatio;
                    double offset = (15 - height) / 2;
                    result.RenderedBounds = new Rect(0.5, offset, width, height);
                }
                else
                {
                    double aspectRatio = bounds.Width / bounds.Height;
                    double height = 15;
                    double width = height * aspectRatio;
                    double offset = (15 - width) / 2;
                    result.RenderedBounds = new Rect(offset, 0.5, width, height);
                }

                return result;
            }
        }

        Shape CreateThumb()
        {
            var result = Drawings.FirstOrDefault(d => d.IsThumb) ?? this;
            result = (Shape)result.Clone();
            result.IsVisible = true;    // embedded thumbnails are invisible by default
            result.IsThumb = true;
            return result;
        }

        public override void Simplify(bool verySmall = false)
        {
            foreach (var drawing in Drawings.ToArray())
            {
                if (drawing is NTextBox && ((NTextBox)drawing).Paragraph.Length == 0)
                {
                    Drawings.Remove(drawing);
                }
                else
                {
                    drawing.Simplify(verySmall);
                }
            }
        }

        public override Transform GeometryTransform
        {
            get
            {
                return base.GeometryTransform;
            }
            set
            {
                base.GeometryTransform = value;

                foreach (var drawing in Drawings)
                {
                    drawing.GeometryTransform = GeometryTransform;
                }
            }
        }

        /// <summary>
        /// Collection of drawings belonging to this group
        /// </summary>
        public ShapeCollection Drawings
        {
            get
            {
                return _Drawings;
            }
            set
            {
                var oldDrawings = _Drawings;
                var newDrawings = value;

                if (newDrawings != oldDrawings)
                {
                    _Drawings = newDrawings;
                    OnDrawingsChanged(oldDrawings, newDrawings);
                }
            }
        }

        /// <summary>
        /// A locked NDrawingGroup cannot be ungrouped
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Click-drag to place this group
        /// </summary>
        public bool DragToPlace { get; set; }

        #endregion

        #region Operations

        public override void Normalize()
        {
            foreach (var drawing in Drawings)
            {
                drawing.Normalize();
            }

            _Bounds = GeometryTransform.TransformBounds(_Bounds);

            base.GeometryTransform = Transform.Identity;
        }

        #region Placing

        Point _StartPoint;

        public override void Place(Point position)
        {
            _StartPoint = position;

            Rect bounds = RenderedBounds;

            if (DragToPlace)
            {
                bounds.Width = 0;
                bounds.Height = 0;
            }

            RenderedBounds = new Rect(
                position.X - bounds.Width / 2,
                position.Y - bounds.Height / 2,
                bounds.Width,
                bounds.Height);
        }

        public override void Draw(Point point)
        {
            if (DragToPlace)
            {
                RenderedBounds = new Rect(
                    Math.Min(_StartPoint.X, point.X),
                    Math.Min(_StartPoint.Y, point.Y),
                    Math.Abs(point.X - _StartPoint.X),
                    Math.Abs(point.Y - _StartPoint.Y)
                );
            }
        }

        public override bool CompleteDrawing()
        {
            Autofocus();
            return true;
        }

        public void Autofocus()
        {
            Autofocus(this);
        }


        #endregion

        #region Handles

        public override int HandleCount
        {
            get { return 4; }
        }

        public override Point GetHandle(int index)
        {
            switch (index)
            {
                case 0:
                    return RenderedBounds.TopLeft;
                case 1:
                    return RenderedBounds.TopRight;
                case 2:
                    return RenderedBounds.BottomRight;
                case 3:
                    return RenderedBounds.BottomLeft;
                default:
                    return new Point();
            }
        }

        public override void SetHandle(int index, Point value)
        {
            Rect bounds = RenderedBounds;

            double left = bounds.Left;
            double top = bounds.Top;
            double right = bounds.Right;
            double bottom = bounds.Bottom;

            switch (index)
            {
                case 0:
                    left = Math.Min(value.X, right);
                    top = Math.Min(value.Y, bottom);
                    break;
                case 1:
                    right = Math.Max(value.X, left);
                    top = Math.Min(value.Y, bottom);
                    break;
                case 2:
                    right = Math.Max(value.X, left);
                    bottom = Math.Max(value.Y, top);
                    break;
                case 3:
                    left = Math.Min(value.X, right);
                    bottom = Math.Max(value.Y, top);
                    break;
            }

            RenderedBounds = new Rect(left, top, right - left, bottom - top);
        }

        #endregion

        #region Snaps

        private int _SnapCount;

        public override int SnapCount
        {
            get { return _SnapCount; }
        }

        protected void UpdateSnapCount()
        {
            _SnapCount = Drawings.OfType<SnapPoint>().Select(snap => snap.GetSnap(0)).Count();

            if (_SnapCount == 0)
            {
                foreach (var drawing in Drawings)
                {
                    if (!(drawing is ShapeGroup))
                    {
                        _SnapCount += drawing.SnapCount;
                    }
                }
            }
        }

        public override Point GetSnap(int index)
        {
            if (Drawings.OfType<SnapPoint>().Any())
            {
                return Drawings.OfType<SnapPoint>().ElementAt(index).GetSnap(0);
            }

            foreach (var drawing in Drawings)
            {
                if (!(drawing is ShapeGroup))
                {
                    if (index < drawing.SnapCount)
                    {
                        return drawing.GetSnap(index);
                    }
                    index -= drawing.SnapCount;
                }
            }

            return new Point();
        }

        #endregion

        #region Rendering

        public override void Layout()
        {
            base.Layout();

            foreach (var drawing in Drawings)
            {
                drawing.Layout();
            }
        }

        public override void Redraw()
        {
            base.Redraw();

            foreach (var drawing in Drawings)
            {
                drawing.Redraw();
            }
        }

        #endregion

        #endregion

        #region INodeSource

        private readonly string _NodeName = SVGElements.NAMESPACE + ' ' + SVGElements.G;

        public override string GetNodeName(NodeContext context)
        {
            return _NodeName;
        }

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    string classNames = base.GetNodeAttribute(context, name) ?? "";
                    if (IsLocked)
                        classNames = DOMHelper.PrependClass(classNames, "locked");
                    if (DragToPlace)
                        classNames = DOMHelper.PrependClass(classNames, "dragdraw");
                    return !String.IsNullOrWhiteSpace(classNames) ? classNames : null;
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        public override void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    IsLocked = DOMHelper.HasClass(value, "locked");
                    DragToPlace = DOMHelper.HasClass(value, "dragdraw");
                    base.SetNodeAttribute(context, name, value);
                    break;
                default:
                    base.SetNodeAttribute(context, name, value);
                    break;
            }
        }

        public override IEnumerable<object> GetChildNodes(NodeContext context)
        {
            var children = base.GetChildNodes(context);
            if (children != null)
            {
                return children.Concat(Drawings);
            }
            else
            {
                return Drawings;
            }
        }

        public override object CreateNode(NodeContext context)
        {
            var element = context as Element;
            if (element == null)
            {
                return null;
            }

            Shape drawing = null;

            string className = element.GetAttribute(SVGAttributes.CLASS);
            if (!String.IsNullOrEmpty(className))
            {
                drawing = Shape.Create(className);
            }

            if (drawing == null)
            {
                drawing = Shape.Create(element.TagName);
            }

            if (drawing != null)
            {
                drawing.Canvas = Canvas;
            }

            return drawing ?? base.CreateNode(context);
        }

        public override void AppendNode(NodeContext context, object newChild)
        {
            var newDrawing = newChild as Shape;
            if (newDrawing != null)
            {
                Drawings.Add(newDrawing);
            }
            else
            {
                base.AppendNode(context, newChild);
            }
        }

        public override void InsertNode(NodeContext context, object newChild, object refChild)
        {
            var newDrawing = newChild as Shape;
            if (newDrawing == null)
            {
                base.InsertNode(context, newChild, refChild);
                return;
            }

            int index = Drawings.IndexOf((Shape)refChild);
            if (index != -1)
            {
                Drawings.Insert(index, newDrawing);
            }
            else
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }
        }

        public override void RemoveNode(NodeContext context, object oldChild)
        {
            var oldDrawing = oldChild as Shape;
            if (oldDrawing == null)
            {
                base.RemoveNode(context, oldChild);
                return;
            }

            if (!Drawings.Remove((Shape)oldChild))
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }
        }

        #endregion

        #region ISelectableText

        public bool IsTextSelected
        {
            get
            {
                return Drawings.OfType<ISelectableText>().Any(d => d.IsTextSelected);
            }
        }

        public string SelectedText
        {
            get
            {
                return String.Concat(Drawings.OfType<ISelectableText>().Select(d => d.SelectedText));
            }
        }

        #endregion

        #region IEditable

        public IList<object> Cut()
        {
            foreach (var drawing in Drawings.OfType<IEditable>())
            {
                var result = drawing.Cut();

                if (result.FirstOrDefault() != drawing)
                {
                    return result;
                }
            }

            return new object[] { this };
        }

        public IList<object> Copy()
        {
            foreach (var drawing in Drawings.OfType<IEditable>())
            {
                var result = drawing.Copy();

                if (result.FirstOrDefault() != drawing)
                {
                    return result;
                }
            }

            return new object[] { this };
        }

        public IList<object> Paste(IList<object> objects)
        {
            foreach (var drawing in Drawings.OfType<IEditable>())
            {
                var result = drawing.Paste(objects);

                if (result != objects)
                {
                    return result;
                }
            }

            return objects;
        }

        public bool Delete()
        {
            return Drawings.OfType<IEditable>().Any(drawing => drawing.Delete());
        }

        #endregion

        #region IFormattable

        public override bool HasProperty(string name)
        {
            return base.HasProperty(name) || Drawings.OfType<IFormattable>().Any(drawing => drawing.HasProperty(name));
        }

        public override void SetProperty(string name, object value)
        {
            foreach (var drawing in Drawings)
            {
                drawing.SetProperty(name, value);
            }

            base.SetProperty(name, value);
        }

        public override object GetProperty(string name)
        {
            return base.GetProperty(name) ?? Drawings.Select(drawing => drawing.GetProperty(name)).FirstOrDefault(result => result != null);
        }

        public override int ChangeProperty(string name, object oldValue, object newValue)
        {
            foreach (var drawing in Drawings)
            {
                drawing.ChangeProperty(name, oldValue, newValue);
            }

            return base.ChangeProperty(name, oldValue, newValue);
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new ShapeGroup(this);
        }

        #endregion

        #region Implementation

        protected override Rect Bounds
        {
            get { return _Bounds; }
        }

        /// <summary>
        /// Set focus to the first descendant NTextBox with HasInitialFocus = true
        /// </summary>
        /// <param name="drawing"></param>
        /// <returns></returns>
        static bool Autofocus(Shape drawing)
        {
            var textBox = drawing as NTextBox;
            if (textBox != null && textBox.HasInitialFocus)
            {
                textBox.Paragraph.Focus();
                textBox.Paragraph.SelectAll();
                return true;
            }

            var group = drawing as ShapeGroup;
            if (group != null)
            {
                return group.Drawings.Any(Autofocus);
            }

            return false;
        }

        /// <summary>
        /// Called when the underlying Drawings collection changes
        /// </summary>
        /// <param name="oldDrawings">Old collection</param>
        /// <param name="newDrawings">New collection</param>
        void OnDrawingsChanged(ShapeCollection oldDrawings, ShapeCollection newDrawings)
        {
            if (oldDrawings != null)
            {
                Drawings_CollectionChanged(oldDrawings, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldDrawings));
                oldDrawings.CollectionChanged -= Drawings_CollectionChanged;
            }

            if (newDrawings != null)
            {
                newDrawings.CollectionChanged += Drawings_CollectionChanged;
                Drawings_CollectionChanged(newDrawings, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newDrawings));
            }
        }

        /// <summary>
        /// Called when the contents of Drawings has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Drawings_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Shape removedDrawing in e.OldItems)
                {
                    OnDrawingRemovedInternal(removedDrawing);
                }
            }

            if (e.NewItems != null)
            {
                foreach (Shape addedDrawing in e.NewItems)
                {
                    OnDrawingAddedInternal(addedDrawing);
                }
            }

            UpdateBounds();
        }

        /// <summary>
        /// Called when a drawing has been added to this group
        /// </summary>
        void OnDrawingAddedInternal(Shape drawing)
        {
            Normalize();

            if (drawing.IsVisible)
            {
                Children.Add(drawing);
            }

            drawing.ParentDrawing = this;

            drawing.VisibilityChanged += Drawing_VisibilityChanged;

            UpdateSnapCount();

            OnDrawingAdded(drawing);
        }

        /// <summary>
        /// Called when a drawing has been removed from this group
        /// </summary>
        void OnDrawingRemovedInternal(Shape drawing)
        {
            Children.Remove(drawing);

            drawing.ParentDrawing = null;

            drawing.VisibilityChanged -= Drawing_VisibilityChanged;

            UpdateSnapCount();

            OnDrawingRemoved(drawing);
        }

        void Drawing_VisibilityChanged(object sender, EventArgs e)
        {
            var drawing = (Shape)sender;

            Children.Remove(drawing);

            if (drawing.IsVisible)
            {
                Children.Add(drawing);
            }
        }

        /// <summary>
        /// Recompute this group's bounding rectangle based on its children's rectangles
        /// </summary>
        private void UpdateBounds()
        {
            _Bounds = Rect.Empty;

            foreach (Shape drawing in Drawings)
            {
                // Do not include text boxes in this calculation
                if (drawing is NTextBox && Drawings.Count > 1)
                {
                    continue;
                }

                var bounds = GeometryTransform.Inverse.TransformBounds(drawing.RenderedBounds);

                if (_Bounds.IsEmpty)
                {
                    _Bounds = bounds;
                }
                else
                {
                    _Bounds = Rect.Union(_Bounds, bounds);
                }
            }
        }

        /// <summary>
        /// Called when a drawing has been added to this group
        /// </summary>
        protected virtual void OnDrawingAdded(Shape drawing)
        {

        }

        /// <summary>
        /// Called when a drawing has been removed from this group
        /// </summary>
        protected virtual void OnDrawingRemoved(Shape drawing)
        {

        }

        protected override void OnCanvasChanged(NCanvas oldCanvas)
        {
            if (Drawings != null)
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.Canvas = Canvas;
                }
            }
        }

        protected override void OnSelected()
        {
            if (Drawings != null)
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.Select();
                }
            }
        }

        protected override void OnUnselected()
        {
            if (Drawings != null)
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.Unselect();
                }
            }
        }

        protected override void OnMouseEnter(bool directlyOver)
        {
            if (Drawings != null)
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.MouseEnter(directlyOver);
                }
            }
        }

        protected override void OnMouseLeave()
        {
            if (Drawings != null)
            {
                foreach (Shape drawing in Drawings)
                {
                    drawing.MouseLeave();
                }
            }
        }

        #endregion
    }
}
