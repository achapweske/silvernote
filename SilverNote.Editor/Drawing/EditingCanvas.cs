/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SilverNote.Editor
{
    public class EditingCanvas : DocumentCanvas, ISelectableText, IFormattable, ISearchable
    {
        #region Fields

        Selection<Shape> _Selection;
        List<Shape> _Preselection;

        #endregion

        #region Constructors

        public EditingCanvas()
        {
            Initialize();
        }

        public EditingCanvas(EditingCanvas copy)
            : base(copy)
        {
            Initialize();

            // Copy selection

            _Selection.SelectionChanged -= Selection_Changed;

            foreach (Shape selected in copy.Selection)
            {
                int index = copy.Drawings.IndexOf(selected);
                Selection.Select(Drawings[index]);
            }

            _Selection.SelectionChanged += Selection_Changed;

        }

        private void Initialize()
        {
            _Selection = new Selection<Shape>();
            _Selection.SelectionChanged += Selection_Changed;
            _Preselection = new List<Shape>();
        }

        #endregion

        #region Properties

        public Selection<Shape> Selection
        {
            get
            {
                _Selection.UndoStack = this.UndoStack;
                return _Selection;
            }
        }

        public Point[] SelectedSnaps
        {
            get
            {
                var points = new PointCollection();

                foreach (var drawing in Selection)
                {
                    for (int i = 0; i < drawing.SnapCount; i++)
                    {
                        points.Add(drawing.GetSnap(i) + drawing.Offset);
                    }
                }

                return points.ToArray();
            }
        }

        #endregion

        #region Methods

        public void Rotate(double angleInDegrees)
        {
            if (UndoStack != null)
            {
                UndoStack.Push("Rotate Selection", () => Rotate(-angleInDegrees));
            }

            Rect bounds = VisualBounds;

            double centerX = (bounds.Left + bounds.Right) / 2;
            double centerY = (bounds.Top + bounds.Bottom) / 2;

            foreach (var selectedDrawing in Selection)
            {
                selectedDrawing.RotateAt(angleInDegrees, centerX, centerY);
            }
        }

        public void FlipHorizontal()
        {
            if (UndoStack != null)
            {
                UndoStack.Push("Flip Horizontal", () => FlipHorizontal());
            }

            Rect bounds = VisualBounds;

            double centerX = (bounds.Left + bounds.Right) / 2;
            double centerY = (bounds.Top + bounds.Bottom) / 2;

            foreach (var selectedDrawing in Selection)
            {
                selectedDrawing.ScaleAt(-1, 1, centerX, centerY);
            }
        }

        public void FlipVertical()
        {
            if (UndoStack != null)
            {
                UndoStack.Push("Flip Vertical", () => FlipVertical());
            }

            Rect bounds = VisualBounds;

            double centerX = (bounds.Left + bounds.Right) / 2;
            double centerY = (bounds.Top + bounds.Bottom) / 2;

            foreach (var selectedDrawing in Selection)
            {
                selectedDrawing.ScaleAt(1, -1, centerX, centerY);
            }
        }

        /// <summary>
        /// Group the selected drawings.
        /// 
        /// This operation is undo-able.
        /// </summary>
        public void Group()
        {
            if (Selection.Count == 0)
            {
                return;
            }

            if (Join())
            {
                return;
            }

            if (Selection.Count == 1 && Selection.Last() is ShapeGroup)
            {
                return;
            }

            using (new UndoScope(UndoStack, "Group"))
            {
                var group = new ShapeGroup();

                foreach (var drawing in Selection.ToArray())
                {
                    Drawings.Remove(drawing);
                    AddToGroup(drawing, group);
                }

                Drawings.Add(group);
                Selection.Select(group);
            }
        }

        /// <summary>
        /// Add a drawing to the given group.
        /// 
        /// This operation is undo-able
        /// </summary>
        /// <param name="drawing">The drawing to be added</param>
        /// <param name="group">Target group</param>
        public void AddToGroup(Shape drawing, ShapeGroup group)
        {
            if (UndoStack != null)
            {
                UndoStack.Push("Add to group", () => RemoveFromGroup(drawing, group));
            }

            group.Drawings.Add(drawing);
        }

        /// <summary>
        /// Ungroup the selected drawings.
        /// 
        /// This operation is undo-able.
        /// </summary>
        public void Ungroup()
        {
            Split();

            using (var undo = new UndoScope(UndoStack, "Ungroup"))
            {
                foreach (var selected in Selection.ToArray())
                {
                    var group = selected as ShapeGroup;

                    if (group != null && !group.IsLocked)
                    {
                        Drawings.Remove(group);

                        foreach (var child in group.Drawings.ToArray())
                        {
                            RemoveFromGroup(child, group);
                            Drawings.Add(child);
                            Selection.Select(child);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove a drawing from the given group.
        /// 
        /// This operation is undo-able.
        /// </summary>
        /// <param name="drawing">The drawing to be removed</param>
        /// <param name="group">The target group</param>
        public void RemoveFromGroup(Shape drawing, ShapeGroup group)
        {
            if (UndoStack != null)
            {
                UndoStack.Push("Remove from group", () => AddToGroup(drawing, group));
            }

            group.Drawings.Remove(drawing);
        }

        /// <summary>
        /// Join the selected drawings.
        /// 
        /// This operation is undo-able.
        /// </summary>
        /// <returns></returns>
        public bool Join()
        {
            if (Selection.Count <= 1)
            {
                return false;
            }

            NPath newPath = NPath.Create(Selection);
            if (newPath == null || newPath.Path.Figures.Count != 1)
            {
                return false;
            }

            using (var undo = new UndoScope(UndoStack, "Join"))
            {
                foreach (var drawing in Selection.ToArray())
                {
                    Drawings.Remove(drawing);
                }

                Drawings.Add(newPath);
                Selection.Select(newPath);
            }

            return true;
        }

        private void Join(NPath path, IEnumerable<Shape> drawings)
        {
            if (UndoStack != null)
            {
                UndoStack.Push("Join item", () => Split(path));
            }

            path.Join(drawings);
        }

        /// <summary>
        /// Split the selected drawings.
        /// 
        /// This operation is undo-able.
        /// </summary>
        public void Split()
        {
            using (var undo = new UndoScope(UndoStack, "Split"))
            {
                foreach (var selected in Selection.ToArray())
                {
                    var path = selected as NPath;
                    if (path != null)
                    {
                        Drawings.Remove(path);

                        var children = Split(path);
                        foreach (var child in children)
                        {
                            Drawings.Add(child);
                            Selection.Select(child);
                        }
                    }
                }
            }
        }

        private IEnumerable<Shape> Split(NPath path)
        {
            var results = path.Split().ToArray();

            if (UndoStack != null)
            {
                UndoStack.Push("Split item", () => Join(path, results));
            }

            return results;
        }

        public void MoveDrawing(Shape drawing, Vector delta)
        {
            Point savedPosition = drawing.RenderedBounds.Location;

            drawing.Translate(delta);

            if (UndoStack != null)
            {
                UndoStack.Push(delegate()
                {
                    Vector undoDelta = savedPosition - drawing.RenderedBounds.Location;
                    MoveDrawing(drawing, undoDelta);
                });
            }
        }

        public void MoveHandle(Shape drawing, int handle, Vector delta)
        {
            Point savedPosition = drawing.GetHandle(handle);

            drawing.MoveHandle(handle, delta);

            if (UndoStack != null)
            {
                UndoStack.Push(delegate()
                {
                    Vector undoDelta = savedPosition - drawing.GetHandle(handle);
                    MoveHandle(drawing, handle, undoDelta);
                });
            }
        }

        #endregion

        #region Events

        public event DrawingEventHandler DrawingSelected;

        protected void RaiseDrawingSelected(Shape drawing)
        {
            if (DrawingSelected != null)
            {
                DrawingSelected(this, new DrawingEventArgs(drawing));
            }
        }

        public event DrawingEventHandler DrawingUnselected;

        protected void RaiseDrawingUnselected(Shape drawing)
        {
            if (DrawingUnselected != null)
            {
                DrawingUnselected(this, new DrawingEventArgs(drawing));
            }
        }
        #endregion

        #region IEditable

        public override IList<object> Cut()
        {
            using (new UndoScope(UndoStack, "Cut"))
            {
                var result = Copy();
                Delete();
                return result;
            }
        }

        public override IList<object> Copy()
        {
            foreach (var drawing in Drawings.OfType<IEditable>())
            {
                var result = drawing.Copy();

                if (result.FirstOrDefault() != drawing)
                {
                    return result;
                }
            }

            return new object[] { Clone() };
        }

        public override IList<object> Paste(IList<object> items)
        {
            using (new UndoScope(UndoStack, "Paste"))
            {
                var target = Selection.FirstOrDefault() as IEditable;
                if (target != null)
                {
                    items = target.Paste(items);
                }

                var results = new List<object>();

                foreach (object item in items)
                {
                    if (item is Shape)
                    {
                        Drawings.Add((Shape)item);
                    }
                    else if (item is NCanvas)
                    {
                        PasteCanvas((NCanvas)item);
                    }
                    else
                    {
                        results.Add(item);
                    }
                }

                return results;
            }
        }

        private void PasteCanvas(DocumentCanvas canvas)
        {
            // Place pasted items near currently-selected items, or in upper-left corner of
            // visible region if no items selected

            Vector offset = new Point(20, 20) - canvas.VisualBounds.Location;
            if (Selection.Count > 0)
            {
                Rect selectionBounds = VisualBounds;
                offset.X += selectionBounds.X;
                offset.Y += selectionBounds.Y;
            }
            else
            {
                ScrollViewer scroller = LayoutHelper.GetAncestor<ScrollViewer>(this);
                if (scroller != null)
                {
                    Point scrollPosition = scroller.TransformToDescendant(this).Transform(new Point(0, 0));
                    offset.X += scrollPosition.X;
                    offset.Y += scrollPosition.Y;
                }
            }
            var drawings = canvas.Drawings.ToArray();
            foreach (Shape drawing in drawings)
            {
                canvas.Drawings.Remove(drawing);
                this.Drawings.Add(drawing);
                MoveDrawing(drawing, offset);
            }
            Selection.SelectRangeOnly(drawings);
        }

        public override bool Delete()
        {
            using (UndoScope undo = new UndoScope(UndoStack, "Delete"))
            {
                if (!Selection.OfType<IEditable>().Any(drawing => drawing.Delete()))
                {
                    foreach (var drawing in Selection.ToArray())
                    {
                        Drawings.Remove(drawing);
                    }
                }
            }

            return true;
        }

        #endregion

        #region ISelectable

        public void Preselect(IEnumerable<Shape> drawings)
        {
            var removedDrawings = _Preselection.Except(drawings).ToArray();
            var addedDrawings = drawings.Except(_Preselection).ToArray();

            foreach (var drawing in removedDrawings)
            {
                drawing.MouseLeave();
                _Preselection.Remove(drawing);
            }

            foreach (var drawing in addedDrawings)
            {
                drawing.MouseEnter(true);
                _Preselection.Add(drawing);
            }
        }

        public void PreselectAll()
        {
            _Preselection.Clear();
            _Preselection.AddRange(Drawings);
        }

        public void UnpreselectAll()
        {
            foreach (var drawing in _Preselection)
            {
                drawing.MouseLeave();
            }

            _Preselection.Clear();
        }

        public override void Select()
        {
            base.Select();

            // Select all drawings in Preselection while maintaining their z-order

            var selection = Drawings.Where(_Preselection.Contains);

            Selection.SelectRange(selection);
        }

        public override void Unselect()
        {
            UnpreselectAll();

            Selection.UnselectAll();

            base.Unselect();
        }

        #endregion

        #region ISelectableText

        public bool IsTextSelected
        {
            get
            {
                return Selection.OfType<ISelectableText>().Any(s => s.IsTextSelected);
            }
        }

        public string SelectedText
        {
            get
            {
                return String.Concat(Selection.OfType<ISelectableText>().Select(s => s.SelectedText));
            }
        }

        #endregion

        #region IFormattable

        public override bool HasProperty(string name)
        {
            return Selection.Any(s => s.HasProperty(name));
        }

        public override void SetProperty(string name, object value)
        {
            using (new UndoScope(UndoStack, "Set Property"))
            {
                foreach (var formattable in Selection)
                {
                    SetProperty(formattable, name, value);
                }
            }
        }

        public void SetProperty(IFormattable formattable, string name, object value)
        {
            object oldValue = formattable.GetProperty(name);

            formattable.SetProperty(name, value);

            if (UndoStack != null)
            {
                UndoStack.Push(() => formattable.SetProperty(name, oldValue));
            }
        }

        public override object GetProperty(string name)
        {
            var target = Selection.LastOrDefault() as IFormattable;

            if (target != null)
            {
                return target.GetProperty(name);
            }
            else
            {
                return null;
            }
        }

        public override void ResetProperties()
        {
            using (new UndoScope(UndoStack, "Reset Text Properties"))
            {
                foreach (var text in Selection)
                {
                    text.ResetProperties();
                }
            }
        }

        public override int ChangeProperty(string name, object oldValue, object newValue)
        {
            int result = 0;

            using (new UndoScope(UndoStack, "Change Property"))
            {
                foreach (var formattable in Selection)
                {
                    result += formattable.ChangeProperty(name, oldValue, newValue);
                }
            }

            return result;
        }

        #endregion

        #region ISearchable

        public int Find(string findText)
        {
            return Searchable.Find(Drawings, findText);
        }

        public bool FindFirst()
        {
            var result = Searchable.FindFirst(Drawings);
            if (result != null)
            {
                Selection.SelectOnly((Shape)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindLast()
        {
            var result = Searchable.FindLast(Drawings);
            if (result != null)
            {
                Selection.SelectOnly((Shape)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindNext()
        {
            var result = Searchable.FindNext(Drawings, Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((Shape)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindPrevious()
        {
            var result = Searchable.FindPrevious(Drawings, Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((Shape)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool FindFirst(string pattern, RegexOptions options)
        {
            var result = Searchable.FindFirst(Drawings, pattern, options);
            if (result != null)
            {
                Selection.SelectOnly((Shape)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool FindLast(string pattern, RegexOptions options)
        {
            var result = Searchable.FindLast(Drawings, pattern, options);
            if (result != null)
            {
                Selection.SelectOnly((Shape)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool FindNext(string pattern, RegexOptions options)
        {
            var result = Searchable.FindNext(Drawings, pattern, options, Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((Shape)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool FindPrevious(string pattern, RegexOptions options)
        {
            var result = Searchable.FindPrevious(Drawings, pattern, options, Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((Shape)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Implementation

        #region Selection

        private void Selection_Changed(object sender, SelectionChangedEventArgs<Shape> e)
        {
            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    OnDrawingUnselected(item);
                }
            }

            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    OnDrawingSelected(item);
                }
            }
        }

        protected virtual void OnDrawingSelected(Shape drawing)
        {
            RaiseDrawingSelected(drawing);
        }

        protected virtual void OnDrawingUnselected(Shape drawing)
        {
            RaiseDrawingUnselected(drawing);
        }

        protected override void OnDrawingRemoved(int index, Shape drawing)
        {
            Selection.Unselect(drawing);

            base.OnDrawingRemoved(index, drawing);
        }

        #endregion

        #endregion
    }
}
