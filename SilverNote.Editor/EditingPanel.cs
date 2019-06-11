/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SilverNote.Editor
{
    public class EditingPanel : DocumentPanel, ITextElement, ISearchable, ICloneable
    {
        #region Constructors

        public EditingPanel()
        {
            Initialize();
        }

        public EditingPanel(EditingPanel copy)
            : base(copy)
        {
            Initialize();

            _Selection.SelectionChanged -= Selection_Changed;

            foreach (var selected in copy.Selection)
            {
                int index = copy.InternalChildren.IndexOf(selected);
                _Selection.Select(this.InternalChildren[index]);
            }

            _Selection.SelectionChanged += Selection_Changed;

            this.Zoom = copy.Zoom;
        }

        void Initialize()
        {
            LoadCommandBindings();

            Movable.AddRequestingBeginMoveHandler(this, Child_RequestingBeginMove);
            Movable.AddRequestingMoveDeltaHandler(this, Child_RequestingMoveDelta);
            Movable.AddRequestingEndMoveHandler(this, Child_RequestingEndMove);

            _Selection = new Selection<UIElement>();
            _Selection.SelectionChanged += Selection_Changed;
        }

        #endregion

        #region Commands

        public static readonly RoutedUICommand DeleteCommand = new RoutedUICommand("Delete", "Delete", typeof(EditingPanel));

        void LoadCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(DeleteCommand, DeleteCommand_Executed));
        }

        void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is UIElement)
            {
                var element = (UIElement)e.Parameter;
                if (Remove(element) != -1)
                {
                    e.Handled = true;
                }
            }
            else if (e.Parameter is IEnumerable)
            {
                var elements = (IEnumerable)e.Parameter;
                if (RemoveRange(elements.OfType<UIElement>()) > 0)
                {
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region Properties

        #region UndoStack

        public static readonly DependencyProperty UndoStackProperty = DependencyProperty.RegisterAttached(
            "UndoStack",
            typeof(UndoStack),
            typeof(EditingPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits)
        );

        public static void SetUndoStack(DependencyObject target, UndoStack value)
        {
            target.SetValue(UndoStackProperty, value);
        }

        public static UndoStack GetUndoStack(DependencyObject target)
        {
            return (UndoStack)target.GetValue(UndoStackProperty);
        }

        public virtual UndoStack UndoStack
        {
            get { return GetUndoStack(this); }
            set { SetUndoStack(this, value); }
        }

        #endregion

        #region IsDeletable

        public static readonly DependencyProperty IsDeletableProperty = DependencyProperty.RegisterAttached(
            "IsDeletable",
            typeof(bool),
            typeof(EditingPanel),
            new FrameworkPropertyMetadata(true)
        );

        public static void SetIsDeletable(DependencyObject target, bool value)
        {
            target.SetValue(IsDeletableProperty, value);
        }

        public static bool GetIsDeletable(DependencyObject target)
        {
            return (bool)target.GetValue(IsDeletableProperty);
        }

        public bool IsDeletable 
        {
            get { return GetIsDeletable(this); }
            set { SetIsDeletable(this, value); }
        }

        #endregion

        #endregion

        #region Children

        public virtual int InsertIndex
        {
            get
            {
                int result = -1;

                foreach (var child in Selection)
                {
                    int index = InternalChildren.IndexOf(child);
                    result = Math.Max(result, index);
                }

                if (result == -1)
                {
                    return InternalChildren.Count;
                }

                var paragraph = InternalChildren[result] as TextField;
                if (paragraph != null && paragraph.SelectionEnd > 0)
                {
                    result++;
                }

                return result;
            }
        }

        public int Insert(UIElement element)
        {
            int index = InsertIndex;

            Insert(index, element);

            return index;
        }

        public int InsertRange(IEnumerable<UIElement> elements)
        {
            int index = InsertIndex;

            InsertRange(index, elements);

            return index;
        }

        public override void Insert(int index, UIElement element)
        {
            if (UndoStack != null)
            {
                UndoStack.Push(() => Remove(element));
            }

            base.Insert(index, element);
        }
        
        public override int Remove(UIElement element)
        {
            int index = InternalChildren.IndexOf(element);
            if (index == -1)
            {
                return -1;
            }

            Selection.Unselect(element);

            if (UndoStack != null)
            {
                UndoStack.Push(() => Insert(index, element));
            }

            return base.Remove(element);
        }

        protected override void OnChildAdded(UIElement element)
        {
            Debug.Assert(!(element is TextFragment));

            base.OnChildAdded(element);
        }

        #endregion

        #region Selection

        private Selection<UIElement> _Selection;

        public Selection<UIElement> Selection
        {
            get 
            {
                _Selection.UndoStack = this.UndoStack;

                return _Selection; 
            }
        }

        public void SelectAll()
        {
            RequestBringIntoView += SuppressBringIntoView;
            try
            {
                foreach (var canvas in InternalChildren.OfType<NCanvas>())
                {
                    canvas.PreselectAll();
                }
                Selection.SelectAll(InternalChildren);
                Navigable.SelectAll(InternalChildren);
            }
            finally
            {
                RequestBringIntoView -= SuppressBringIntoView;
            }
        }

        protected void SuppressBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        public virtual void SelectTo(UIElement element)
        {
            using (var undo = new UndoScope(UndoStack, "Select"))
            {
                var first = Selection.FirstOrDefault();
                if (first != null)
                {
                    var items = Selection<UIElement>.GetRange(InternalChildren, first, element);
                    items = InlineSelectionFilter(items);
                    Selection.SelectRangeOnly(items);
                    Navigable.SelectRange(InternalChildren, first, element, Selection.Contains);
                }
                else
                {
                    Selection.Select(element);
                }
            }
        }

        /// <summary>
        /// Returns all items in the given collection that are either static or
        /// are entirely contained within the vertical region bounded by the 
        /// static items within the collection.
        /// 
        /// This is used by SelectTo() to ensure that non-static items are selected
        /// only when they are entirely contained within the region of selected
        /// static items.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private IEnumerable<UIElement> InlineSelectionFilter(IEnumerable<UIElement> items)
        {
            // Get the region bounded by the static items within the collection
            var staticItems = items.Where(IsPositioningStatic);
            Rect staticBounds = VisualElement.GetVisualBounds(staticItems, this);

            items = items.Where(item =>
                {
                    if (IsPositioningStatic(item))
                    {
                        return true;    // item is static
                    }
                    Rect itemBounds = VisualElement.GetVisualBounds(item, this);
                    if (itemBounds.Top < staticBounds.Top || itemBounds.Bottom > staticBounds.Bottom)
                    {
                        return false;   // item is NOT contained within vertical region of staticBounds
                    }

                    var canvas = item as NCanvas;
                    if (canvas != null)
                    {
                        canvas.PreselectAll();
                    }

                    return true;
                });
            return items;
        }

        public void SelectDefault(Positioning positioningHint, Point positionHint)
        {
            int indexHint = ClosestIndexFromPoint(positionHint);
            
            SelectDefault(positioningHint, indexHint);
        }

        public void SelectDefault(Positioning positioningHint, int indexHint = 0)
        {
            if (InternalChildren.Count == 0)
            {
                return;
            }

            if (positioningHint == Positioning.Absolute)
            {
                positioningHint = Positioning.Overlapped;
            }

            indexHint = Math.Max(Math.Min(indexHint, InternalChildren.Count - 1), 0);

            int index = FindIndex<ISelectable>(indexHint, PositioningEquals(positioningHint));
            if (index == -1)
            {
                index = FindLastIndex<ISelectable>(indexHint, PositioningEquals(positioningHint));
            }

            index = (index != -1) ? index : indexHint;

            UIElement result = InternalChildren[index];

            Selection.Select(result);
        }

        private void Selection_Changed(object sender, SelectionChangedEventArgs<UIElement> e)
        {
            OnSelectionChanged(e.RemovedItems, e.AddedItems);
        }

        protected virtual void OnSelectionChanged(UIElement[] removedItems, UIElement[] addedItems)
        {
            var selectedItem = Selection.LastOrDefault();
            if (selectedItem == null)
            {
                return;
            }

            var paragraph = selectedItem as TextParagraph;
            if (paragraph != null)
            {
                paragraph.RevealSectionItem();
                paragraph.RevealListItem();
            }

            selectedItem.Focus();
        }

        #endregion 

        #region Operations

        public virtual int InsertBefore(UIElement newElement, UIElement insertBefore = null)
        {
            if (insertBefore == null)
            {
                insertBefore = Selection.OrderBy(InternalChildren.IndexOf).FirstOrDefault();

                if (insertBefore == null)
                {
                    return -1;
                }
            }

            using (var undo = new UndoScope(UndoStack, "Insert before"))
            {
                int index = Children.IndexOf(insertBefore);
                if (index != -1)
                {
                    Insert(index, newElement);
                    Selection.SelectOnly(newElement);
                    Navigable.MoveToStart(newElement);
                    Navigable.SelectToEnd(newElement);
                }
                return index;
            }
        }

        public virtual int InsertAfter(UIElement newElement, UIElement insertAfter = null)
        {
            if (insertAfter == null)
            {
                insertAfter = Selection.OrderBy(InternalChildren.IndexOf).LastOrDefault();

                if (insertAfter == null)
                {
                    return -1;
                }
            }

            using (var undo = new UndoScope(UndoStack, "Insert after"))
            {
                int index = Children.IndexOf(insertAfter);
                if (index != -1)
                {
                    Insert(index + 1, newElement);
                    Selection.SelectOnly(newElement);
                    Navigable.MoveToStart(newElement);
                    Navigable.SelectToEnd(newElement);
                }
                return index;
            }
        }

        public virtual void InsertParagraphBefore(UIElement insertBefore = null)
        {
            TextField refParagraph = insertBefore as TextField;
            if (refParagraph == null)
            {
                refParagraph = Selection.OfType<TextField>().OrderBy(InternalChildren.IndexOf).FirstOrDefault();
            }

            TextField newParagraph = null;
            if (refParagraph != null)
            {
                newParagraph = (TextField)refParagraph.Clone();
                newParagraph.Text = "";
            }
            else
            {
                newParagraph = new TextParagraph();
            }

            InsertBefore(newParagraph, insertBefore);
        }

        public virtual void InsertParagraphAfter(UIElement insertAfter = null)
        {
            TextField refParagraph = insertAfter as TextField;
            if (refParagraph == null)
            {
                refParagraph = Selection.OfType<TextField>().OrderBy(InternalChildren.IndexOf).LastOrDefault();
            }

            TextField newParagraph = null;
            if (refParagraph != null)
            {
                newParagraph = (TextField)refParagraph.Clone();
                newParagraph.Text = "";
            }
            else
            {
                newParagraph = new TextParagraph();
            }

            if (insertAfter == null)
            {
                insertAfter = Selection.OrderBy(InternalChildren.IndexOf).LastOrDefault();

                if (insertAfter == null)
                {
                    return;
                }
            }

            if (insertAfter is TextParagraph)
            {
                insertAfter = ((TextParagraph)insertAfter).List.SelfAndDescendants.Last();
            }

            InsertAfter(newParagraph, insertAfter);
        }

        public virtual void MoveParagraphUp()
        {
            var paragraphs = Selection.OfType<TextParagraph>();

            paragraphs = GetListDescendantsAndSelf(paragraphs);

            MoveParagraphUp(paragraphs);
        }

        public virtual void MoveParagraphUp(IEnumerable<TextParagraph> _paragraphs)
        {
            var paragraphs = _paragraphs
                .OrderBy(InternalChildren.IndexOf)
                .ToArray();

            if (paragraphs.Length == 0)
            {
                return;
            }

            // Get the destination index

            int index = Get_MoveParagraphUp_DestinationIndex(paragraphs);
            if (index == -1)
            {
                return;
            }

            // Now move paragraphs to the computed index

            using (var undo = new UndoScope(UndoStack, "Move paragraph(s) up"))
            {
                var selection = Selection.ToArray();

                MoveRange(paragraphs, index);

                Selection.SelectRangeOnly(selection);
            }
        }

        int Get_MoveParagraphUp_DestinationIndex(TextParagraph[] paragraphs)
        {
            if (paragraphs.Length == 0)
            {
                return -1;
            }

            TextParagraph topParagraph = paragraphs[0];

            int topParagraphLevel = topParagraph.ListLevel;

            int index = InternalChildren.IndexOf(topParagraph);

            while (--index >= 0)
            {
                index = FindLastIndex(index, DocumentPanel.PositioningEquals(Positioning.Static));
                if (index == -1)
                {
                    // No more static elements found
                    return -1;
                }

                TextParagraph target = InternalChildren[index] as TextParagraph;
                if (target == null)
                {
                    // We found a non-paragraph static element
                    return index;
                }

                int targetListLevel = target.ListLevel;
                if (targetListLevel <= topParagraphLevel)
                {
                    // We found a paragraph with the right list level
                    return index;
                }
            }

            return -1;
        }

        public virtual void MoveParagraphDown()
        {
            var paragraphs = Selection.OfType<TextParagraph>();

            paragraphs = GetListDescendantsAndSelf(paragraphs);

            MoveParagraphDown(paragraphs);
        }

        public virtual void MoveParagraphDown(IEnumerable<TextParagraph> _paragraphs)
        {
            var paragraphs = _paragraphs
                .OrderBy(InternalChildren.IndexOf)
                .ToArray();

            if (paragraphs.Length == 0)
            {
                return;
            }

            // Get the destination index

            int index = Get_MoveParagraphDown_DestinationIndex(paragraphs);
            if (index == -1)
            {
                Append(new TextParagraph());
                index = InternalChildren.Count;
            }
              
            // Now move paragraphs to the computed index

            using (var undo = new UndoScope(UndoStack, "Move paragraph(s) down"))
            {
                var selection = Selection.ToArray();

                MoveRange(paragraphs, index);

                Selection.SelectRangeOnly(selection);
            }

            if (Selection.OfType<FrameworkElement>().Any())
            {
                Selection.OfType<FrameworkElement>().OrderBy(InternalChildren.IndexOf).Last().BringIntoView();
            }
        }

        int Get_MoveParagraphDown_DestinationIndex(TextParagraph[] paragraphs)
        {
            if (paragraphs.Length == 0)
            {
                return -1;
            }

            TextParagraph topParagraph = paragraphs[0];
            TextParagraph bottomParagraph = paragraphs[paragraphs.Length - 1];

            int topParagraphLevel = topParagraph.ListLevel;

            int index = InternalChildren.IndexOf(bottomParagraph);

            while (++index < InternalChildren.Count)
            {
                index = FindIndex(index, DocumentPanel.PositioningEquals(Positioning.Static));
                if (index == -1)
                {
                    // No more static elements found
                    return -1;
                }

                TextParagraph target = InternalChildren[index] as TextParagraph;
                if (target == null)
                {
                    // We found a non-paragraph static element
                    return index + 1;
                }

                if (target.ListLevel < topParagraphLevel)
                {
                    // We found a paragraph with a smaller list level
                    return index + 1;
                }

                if (target.ListLevel == topParagraphLevel)
                {
                    // We found a paragraph with the same list level -
                    // skip past its descendants
                    var lastDescendant = target.List.SelfAndDescendants.Last();
                    index = InternalChildren.IndexOf(lastDescendant);
                    return index + 1;
                }
            }

            return -1;
        }

        public virtual void SelectParagraph()
        {
            var paragraphs = Selection.OfType<TextParagraph>();

            SelectParagraph(paragraphs);
        }

        public virtual void SelectParagraph(IEnumerable<TextParagraph> paragraphs)
        {
            if (!paragraphs.Any())
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Select paragraph(s)"))
            {
                Selection.SelectRangeOnly(paragraphs);

                foreach (var p in paragraphs)
                {
                    if (p.SelectionBegin < p.SelectionEnd)
                    {
                        p.SelectionBegin = 0;
                        p.SelectionEnd = p.Length;
                    }
                    else
                    {
                        p.SelectionBegin = p.Length;
                        p.SelectionEnd = 0;
                    }
                }
            }
        }

        public virtual void DuplicateParagraph()
        {
            var paragraphs = Selection.OfType<TextParagraph>();

            paragraphs = GetListDescendantsAndSelf(paragraphs);

            DuplicateParagraph(paragraphs);
        }

        public virtual void DuplicateParagraph(IEnumerable<TextParagraph> paragraphs)
        {
            if (!paragraphs.Any())
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Duplicate paragraph(s)"))
            {
                paragraphs = paragraphs.OrderBy(InternalChildren.IndexOf);

                var duplicated = paragraphs.Select(p => (TextParagraph)p.Clone());

                int insertIndex = InternalChildren.IndexOf(paragraphs.Last()) + 1;

                InsertRange(insertIndex, duplicated);
            }
        }

        public virtual void DeleteParagraph()
        {
            var paragraphs = Selection.OfType<TextParagraph>();

            paragraphs = GetListDescendantsAndSelf(paragraphs);

            DeleteParagraph(paragraphs);
        }

        public virtual void DeleteParagraph(IEnumerable<TextParagraph> paragraphs)
        {
            if (!paragraphs.Any())
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Delete paragraph"))
            {
                int index = InternalChildren.IndexOf(paragraphs.First());

                RemoveRange(paragraphs.ToArray());

                if (Selection.Count == 0)
                {
                    SelectDefault(Positioning.Static, index);
                }
            }
        }

        public static IEnumerable<TextParagraph> GetListDescendantsAndSelf(IEnumerable<TextParagraph> paragraphs)
        {
            var result = new List<TextParagraph>();

            foreach (var paragraph in paragraphs)
            {
                foreach (TextParagraph selfOrDescendant in paragraph.List.SelfAndDescendants)
                {
                    if (!result.Contains(selfOrDescendant))
                    {
                        result.Add(selfOrDescendant);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Zoom

        public void ZoomIn()
        {
            decimal zoom = (decimal)this.Zoom;

            if (zoom >= 1.5m)
            {
                zoom += 0.5m;
            }
            else if (zoom >= 1.0m)
            {
                zoom += 0.1m;
            }
            else
            {
                zoom = Math.Min(zoom + 0.1m, 1.0m);
            }

            Zoom = (double)zoom;
        }

        public void ZoomOut()
        {
            decimal zoom = (decimal)this.Zoom;

            if (zoom > 1.5m)
            {
                zoom -= 0.5m;
            }
            else if (zoom > 1.0m)
            {
                zoom = Math.Max(zoom - 0.1m, 1.0m);
            }
            else
            {
                zoom = Math.Max(zoom - 0.1m, 0.1m);
            }

            this.Zoom = (double)zoom;
        }

        private double _Zoom = 1;

        public virtual double Zoom
        {
            get { return _Zoom; }
            set
            {
                double oldValue = _Zoom;
                double newValue = value;

                if (newValue != oldValue)
                {
                    _Zoom = value;
                    ZoomAnimated(newValue, oldValue, () =>
                    {
                        ZoomTransform.ScaleX = newValue;
                        ZoomTransform.ScaleY = newValue;
                        OnZoomChanged(oldValue, newValue);
                    });
                }
            }
        }

        int _ZoomAnimationCount = 0;

        private void ZoomAnimated(double newValue, double oldValue, Action completed = null)
        {
            if (!AnimationSettings.IsAnimationEnabled)
            {
                if (completed != null)
                {
                    completed();
                }
                return;
            }

            var transform = RenderTransform as ScaleTransform;
            if (transform == null)
            {
                transform = new ScaleTransform();
                RenderTransform = transform;
            }

            var storyboard = new Storyboard();
            var duration = new Duration(TimeSpan.FromMilliseconds(250));

            var animationX = new DoubleAnimation(newValue / ZoomTransform.ScaleX, duration);
            Storyboard.SetTarget(animationX, this);
            Storyboard.SetTargetProperty(animationX, new PropertyPath("RenderTransform.ScaleX"));
            storyboard.Children.Add(animationX);

            var animationY = new DoubleAnimation(newValue / ZoomTransform.ScaleY, duration);
            Storyboard.SetTarget(animationY, this);
            Storyboard.SetTargetProperty(animationY, new PropertyPath("RenderTransform.ScaleY"));
            storyboard.Children.Add(animationY);

            if (completed != null)
            {
                storyboard.Completed += (i, j) =>
                {
                    if (--_ZoomAnimationCount == 0)
                    {
                        RenderTransform = null;
                        completed();
                    }
                };
            }

            ++_ZoomAnimationCount;
            BeginStoryboard(storyboard);
        }

        private ScaleTransform _ZoomTransform;

        private ScaleTransform ZoomTransform
        {
            get
            {
                if (_ZoomTransform == null)
                {
                    _ZoomTransform = new ScaleTransform();
                    LayoutTransform = _ZoomTransform;
                }
                return _ZoomTransform;
            }
        }

        protected virtual void OnZoomChanged(double oldValue, double newValue)
        {
            foreach (var canvas in InternalChildren.OfType<NCanvas>())
            {
                foreach (var drawing in canvas.Drawings)
                {
                    drawing.Redraw();
                }
            }

            UpdateLayout();

            var scrollViewer = LayoutHelper.GetAncestor<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                if (scrollViewer.HorizontalOffset != 0)
                {
                    double centerX = scrollViewer.HorizontalOffset + scrollViewer.ViewportWidth / 2;
                    centerX *= _ZoomTransform.ScaleX / oldValue;
                    scrollViewer.ScrollToHorizontalOffset(centerX - scrollViewer.ViewportWidth / 2);
                }

                if (scrollViewer.VerticalOffset != 0)
                {
                    double centerY = scrollViewer.VerticalOffset + scrollViewer.ViewportHeight / 2;
                    centerY *= _ZoomTransform.ScaleY / oldValue;
                    scrollViewer.ScrollToVerticalOffset(centerY - scrollViewer.ViewportHeight / 2);
                }
            }
        }

        #endregion

        #region IMovable

        private Vector _MoveDelta;

        protected virtual void BeginMoveSelection()
        {
            using (new UndoScope(UndoStack))
            {
                if (UndoStack != null)
                {
                    UndoStack.Push(() => EndMoveSelection());
                }

                var selection = Selection.OfType<IMovable>();

                foreach (var item in selection)
                {
                    item.MoveStarted();
                }

                _MoveDelta = new Vector();
            }
        }

        protected virtual void MoveSelection(Vector delta)
        {
            var selection = Selection.OfType<IMovable>();

            foreach (var item in selection)
            {
                item.MoveDelta(delta);
            }

            _MoveDelta += delta;
        }

        protected virtual void EndMoveSelection()
        {
            using (var undo = new UndoScope(UndoStack, "Move Selection", isEditing: false))
            {
                if (UndoStack != null)
                {
                    var delta = -_MoveDelta;
                    UndoStack.Push(() => MoveSelection(delta));
                    UndoStack.Push(() => BeginMoveSelection());
                }

                var selection = Selection.OfType<IMovable>();

                foreach (var item in selection)
                {
                    item.MoveCompleted();
                }
            }
        }

        private void Child_RequestingBeginMove(object sender, RoutedEventArgs e)
        {
            BeginMoveSelection();
            e.Handled = true;
        }

        private void Child_RequestingMoveDelta(object sender, MoveDeltaEventArgs e)
        {
            MoveSelection(e.Delta);
            e.Handled = true;
        }

        private void Child_RequestingEndMove(object sender, RoutedEventArgs e)
        {
            EndMoveSelection();
            e.Handled = true;
        }

        #endregion

        #region IEditable

        public override IList<object> Cut()
        {
            using (var undo = new UndoScope(UndoStack, "Cut"))
            {
                var results = Copy();
                Delete(true);
                return results;
            }
        }

        public override IList<object> Copy()
        {
            var results = new List<object>();

            using (var undo = new UndoScope(UndoStack, "Copy"))
            {
                var selection = Selection.OrderBy(InternalChildren.IndexOf).ToArray();

                foreach (var element in selection)
                {
                    var items = Editable.Copy(element);
                    results.AddRange(items);
                }
            }

            return results;
        }

        public override IList<object> Paste(IList<object> items)
        {
            if (Selection.Count == 0)
            {
                return new object[0];
            }

            using (var undo = new UndoScope(UndoStack, "Paste"))
            {
                if (ContainsStaticItems(items))
                {
                    Delete();
                }

                int index = InternalChildren.Count;

                if (Selection.Count > 0)
                {
                    UIElement target = Selection.Last();

                    index = InternalChildren.IndexOf(target) + 1;

                    items = Editable.Paste(target, items);
                }

                foreach (var item in items.OfType<UIElement>())
                {
                    Insert(index, item);

                    index = InternalChildren.IndexOf(item) + 1;
                }

                var select = InternalChildren[index - 1];

                Selection.SelectOnly(select);
            }

            return new object[0];
        }

        private static bool ContainsStaticItems(IEnumerable<object> items)
        {
            return items.ToList().Find(PositioningEquals(Positioning.Static)) != null;
        }

        public override bool Delete()
        {
            return Delete(false);
        }

        private bool Delete(bool isCutOperation)
        {
            // Delete all selected items.
            //
            // This is called whenever a non-navigation key is pressed

            using (var undo = new UndoScope(UndoStack, "Delete"))
            {
                var selection = Selection.OrderBy(InternalChildren.IndexOf).ToArray();

                foreach (var element in selection)
                {
                    // Elements marked as non-removable will not be removed 
                    // (though their contents will be deleted)

                    Delete(element, IsRemovable(element, selection, isCutOperation));
                }

                if (Selection.Count > 0)
                {
                    // If multiple items are selected for a delete operation,
                    // try to merge the first and last items.

                    MergeSelection();
                }

                // Following a delete operation, don't let an empty canvas be 
                // selected since there's nothing useful a user can do with it.

                var canvases = Selection.OfType<NCanvas>().ToArray();

                foreach (NCanvas canvas in canvases)
                {
                    if (canvas.Drawings.Count == 0)
                    {
                        Selection.Unselect(canvas);
                    }
                }

                // After a delete, the user expects a single item to be selected:

                if (Selection.Count > 0)
                {
                    Selection.SelectOnly(Selection.Last());
                }
                else
                {
                    // if nothing is selectable, at least set focus to editor so it will receive input commands
                    Focus();
                }
            }

            return true;
        }

        private void Delete(UIElement element, bool isRemovable)
        {
            if (!Editable.Delete(element) && isRemovable)
            {
                int index = Remove(element);

                if (Selection.Count == 0)
                {
                    SelectDefault(GetPositioning(element), index);
                }
            }
        }

        private bool IsRemovable(UIElement element, UIElement[] selection, bool isCutOperation)
        {
            bool isRemovable = EditingPanel.GetIsDeletable(element);

            // We generally don't want to delete the top-most paragraph in a selection
            // (so that the user doesn't lose bulleting/indentation when they
            // delete the text). One exception is if the user is performing a cut
            // operation, as they most likely want to move the entire thing from
            // one location to another... Note that no matter what we do here,
            // the *content* of the paragraph is always cleared by NParagraph's
            // Delete() handler.

            isRemovable &= !(!isCutOperation && (element is TextField) && element == selection.First());

            return isRemovable;
        }

        private void MergeSelection()
        {
            var selection = Selection.OrderBy(InternalChildren.IndexOf).ToArray();

            var first = selection.FirstOrDefault() as ITextElement;
            var last = selection.LastOrDefault() as ITextElement;

            if (first != null && last != null && first != last)
            {
                if (first.Merge(last))
                {
                    Remove((UIElement)last);
                }
            }
        }

        #endregion

        #region INavigable

        private Point _NavigationOffset;

        public virtual Point NavigationOffset
        {
            get
            {
                return _NavigationOffset;
            }
            set
            {
                _NavigationOffset = value;
            }
        }

        public virtual bool MoveUp()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)target);

            if (target.MoveUp())
            {
                return true;
            }

            if (OnMoveUp(target))
            {
                return true;
            }

            _NavigationOffset = target.NavigationOffset;
            return false;
        }

        public virtual bool MoveDown()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)target);

            if (target.MoveDown())
            {
                return true;
            }

            if (OnMoveDown(target))
            {
                return true;
            }

            _NavigationOffset = target.NavigationOffset;
            return false;
        }

        public virtual bool MoveLeft()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)target);

            if (target.MoveLeft())
            {
                return true;
            }

            if (OnMoveLeft(target))
            {
                return true;
            }

            _NavigationOffset = target.NavigationOffset;
            return false;
        }

        public virtual bool MoveRight()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)target);

            if (target.MoveRight())
            {
                return true;
            }

            if (OnMoveRight(target))
            {
                return true;
            }

            _NavigationOffset = target.NavigationOffset;
            return false;
        }

        public virtual void MoveToStart()
        {
            var first = Find<INavigable>(PositioningEquals(Positioning.Static));
            if (first != null)
            {
                Selection.SelectOnly((UIElement)first);

                first.MoveToStart();
            }

            var scroller = Parent as ScrollViewer;
            if (scroller != null)
            {
                scroller.ScrollToTop();
            }
        }

        public virtual void MoveToEnd()
        {
            var last = FindLast<INavigable>(PositioningEquals(Positioning.Static));
            if (last != null)
            {
                Selection.SelectOnly((UIElement)last);

                last.MoveToEnd();
            }

            ScrollViewer scroller = Parent as ScrollViewer;
            if (scroller != null)
            {
                scroller.ScrollToBottom();
            }
        }

        public virtual void MoveToTop()
        {
            var first = Find<INavigable>(PositioningEquals(Positioning.Static));
            if (first != null)
            {
                Selection.SelectOnly((UIElement)first);
                first.NavigationOffset = NavigationOffset;
                first.MoveToTop();
            }
        }

        public virtual void MoveToBottom()
        {
            var last = FindLast<INavigable>(PositioningEquals(Positioning.Static));
            if (last != null)
            {
                Selection.SelectOnly((UIElement)last);
                last.NavigationOffset = NavigationOffset;
                last.MoveToBottom();
            }
        }

        public virtual void MoveToLeft()
        {

        }

        public virtual void MoveToRight()
        {

        }

        public virtual bool MoveUpByPage()
        {
            var scroller = LayoutHelper.GetAncestor<ScrollViewer>(this) as ScrollViewer;
            if (scroller == null)
            {
                return false;
            }

            var scrollTo = new Point(0, scroller.VerticalOffset - scroller.ViewportHeight);
            var child = ChildFromPoint(scrollTo);
            if (child == null)
            {
                child = (UIElement)ClosestTextElement(scrollTo);
                if (child == null)
                {
                    return false;
                }
            }

            Selection.SelectOnly(child);
            var navigable = child as INavigable;
            if (navigable != null)
            {
                navigable.NavigationOffset = scrollTo - VisualTreeHelper.GetOffset(child);
                navigable.MoveToLeft();
                navigable.MoveDown();
            }

            scroller.ScrollToVerticalOffset(scrollTo.Y);

            return true;
        }

        public virtual bool MoveDownByPage()
        {
            var scroller = LayoutHelper.GetAncestor<ScrollViewer>(this) as ScrollViewer;
            if (scroller == null)
            {
                return false;
            }

            var scrollTo = new Point(0, scroller.VerticalOffset + scroller.ViewportHeight);
            var child = ChildFromPoint(scrollTo);
            if (child == null)
            {
                child = (UIElement)ClosestTextElement(scrollTo);
                if (child == null)
                {
                    return false;
                }
            }

            Selection.SelectOnly(child);
            var navigable = child as INavigable;
            if (navigable != null)
            {
                navigable.NavigationOffset = scrollTo - VisualTreeHelper.GetOffset(child);
                navigable.MoveToLeft();
                navigable.MoveDown();
            }

            scroller.ScrollToVerticalOffset(scrollTo.Y);

            return true;
        }

        public virtual bool SelectUp()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectUp())
            {
                return true;
            }

            if (OnSelectUp(target))
            {
                return true;
            }

            _NavigationOffset = target.NavigationOffset;
            return false;
        }

        public virtual bool SelectDown()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target== null)
            {
                return false;
            }

            if (target.SelectDown())
            {
                return true;
            }

            if (OnSelectDown(target))
            {
                return true;
            }

            _NavigationOffset = target.NavigationOffset;
            return false;
        }

        public virtual bool SelectLeft()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectLeft())
            {
                return true;
            }

            if (OnSelectLeft(target))
            {
                return true;
            }

            _NavigationOffset = target.NavigationOffset;
            return false;
        }

        public virtual bool SelectRight()
        {
            var target = Selection.OfType<INavigable>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectRight())
            {
                return true;
            }

            if (OnSelectRight(target))
            {
                return true;
            }

            _NavigationOffset = target.NavigationOffset;
            return false;
        }

        public virtual void SelectToStart()
        {
            var first = Find<INavigable>(PositioningEquals(Positioning.Static));
            if (first != null)
            {
                SelectTo((UIElement)first);

                first.SelectToStart();
            }

            var scroller = Parent as ScrollViewer;
            if (scroller != null)
            {
                scroller.ScrollToTop();
            }
        }

        public virtual void SelectToEnd()
        {
            var last = FindLast<INavigable>(PositioningEquals(Positioning.Static));
            if (last != null)
            {
                SelectTo((UIElement)last);

                last.SelectToEnd();
            }

            var scroller = Parent as ScrollViewer;
            if (scroller != null)
            {
                scroller.ScrollToBottom();
            }
        }

        public virtual void SelectToTop()
        {
            var first = Find<INavigable>(PositioningEquals(Positioning.Static));
            if (first != null)
            {
                SelectTo((UIElement)first);
                first.NavigationOffset = NavigationOffset;
                first.SelectToTop();
            }
        }

        public virtual void SelectToBottom()
        {
            var last = FindLast<INavigable>(PositioningEquals(Positioning.Static));
            if (last != null)
            {
                SelectTo((UIElement)last);
                last.NavigationOffset = NavigationOffset;
                last.SelectToBottom();
            }
        }

        public virtual void SelectToLeft()
        {

        }

        public virtual void SelectToRight()
        {

        }

        public virtual bool SelectUpByPage()
        {
            var scroller = LayoutHelper.GetAncestor<ScrollViewer>(this) as ScrollViewer;
            if (scroller == null)
            {
                return false;
            }

            var scrollTo = new Point(0, scroller.VerticalOffset - scroller.ViewportHeight);
            var child = ChildFromPoint(scrollTo);
            if (child == null)
            {
                child = (UIElement)ClosestTextElement(scrollTo);
                if (child == null)
                {
                    return false;
                }
            }

            SelectTo(child);
            var navigable = child as INavigable;
            if (navigable != null)
            {
                navigable.NavigationOffset = scrollTo - VisualTreeHelper.GetOffset(child);
                navigable.SelectToLeft();
                navigable.SelectDown();
            }

            scroller.ScrollToVerticalOffset(scrollTo.Y);

            return true;
        }

        public virtual bool SelectDownByPage()
        {
            var scroller = LayoutHelper.GetAncestor<ScrollViewer>(this) as ScrollViewer;
            if (scroller == null)
            {
                return false;
            }

            var scrollTo = new Point(0, scroller.VerticalOffset + scroller.ViewportHeight);
            var navigable = ChildFromPoint(scrollTo);
            if (navigable == null)
            {
                navigable = (UIElement)ClosestTextElement(scrollTo);
                if (navigable == null)
                {
                    return false;
                }
            }

            SelectTo(navigable);
            var text = navigable as INavigable;
            if (text != null)
            {
                text.NavigationOffset = scrollTo - VisualTreeHelper.GetOffset(navigable);
                text.SelectToLeft();
                text.SelectDown();
            }

            scroller.ScrollToVerticalOffset(scrollTo.Y);

            return true;
        }

        public virtual bool TabForward()
        {
            if (Selection.Count == 0)
            {
                return false;
            }

            var target = Selection.Last() as INavigable;
            if (target != null && target.TabForward())
            {
                return true;
            }

            return OnTabForward(Selection.Last());
        }

        public virtual bool TabBackward()
        {
            if (Selection.Count == 0)
            {
                return false;
            }

            var target = Selection.Last() as INavigable;
            if (target != null && target.TabBackward())
            {
                return true;
            }

            return OnTabBackward(Selection.Last());
        }

        #endregion

        #region IFormattable

        public override bool HasProperty(string name)
        {
            foreach (var formattable in Selection.OfType<IFormattable>())
            {
                if (formattable.HasProperty(name))
                {
                    return true;
                }
            }

            return false;
        }

        public override void SetProperty(string name, object value)
        {
            // Special panel-level handling needed for ListStyle property
            if (name == TextParagraph.ListStylePropertyName && value is IListStyle)
            {
                SetListStyleProperty(name, (IListStyle)value);
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Set Property"))
            {
                foreach (var formattable in Selection.OfType<IFormattable>())
                {
                    formattable.SetProperty(name, value);
                }

                // Special panel-level handling needed for FontClass = "code"
                if (name == TextProperties.FontClassProperty && Object.Equals(value, FontClass.SourceCode.ID))
                {
                    Merge(Selection);
                }
            }
        }

        private void SetListStyleProperty(string name, IListStyle listStyle)
        {
            var paragraphs = Selection.OfType<TextParagraph>();
            if (paragraphs.Any())
            {
                int baseLevel = paragraphs.Select(p => p.ListLevel).Min();
                foreach (var paragraph in paragraphs)
                {
                    int listLevel = paragraph.ListLevel;

                    var itemStyle = listStyle;
                    for (int i = listLevel; i < baseLevel; i++)
                    {
                        itemStyle = itemStyle.PreviousStyle;
                    }
                    for (int i = listLevel; i > baseLevel; i--)
                    {
                        itemStyle = itemStyle.NextStyle;
                    }

                    paragraph.SetProperty(name, itemStyle);
                }
            }

            using (var undo = new UndoScope(UndoStack, "Set Property"))
            {
                foreach (var formattable in Selection.OfType<IFormattable>().Except(paragraphs))
                {
                    formattable.SetProperty(name, listStyle);
                }
            }
        }

        private void Merge(IEnumerable<UIElement> elements)
        {
            var ordered = elements.OrderBy(InternalChildren.IndexOf).ToArray();

            for (int i = ordered.Length - 1; i > 0; i--)
            {
                var element1 = ordered[i - 1] as ITextElement;
                var element2 = ordered[i] as ITextElement;

                if (element1 != null && element2 != null)
                {
                    element1.MoveToEnd();
                    element1.EnterLineBreak();

                    if (element1.Merge(element2))
                    {
                        Remove((UIElement)element2);
                    }
                    element1.SelectAll();
                }
            }
        }

        public override object GetProperty(string name)
        {
            var target = Selection.OfType<IFormattable>().LastOrDefault();

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
            using (var undo = new UndoScope(UndoStack, "Reset Properties"))
            {
                foreach (var formattable in Selection.OfType<IFormattable>())
                {
                    formattable.ResetProperties();
                }
            }
        }

        public override int ChangeProperty(string name, object oldValue, object newValue)
        {
            int result = 0;

            using (var undo = new UndoScope(UndoStack, "Change Property"))
            {
                foreach (var formattable in Selection.OfType<IFormattable>())
                {
                    result += formattable.ChangeProperty(name, oldValue, newValue);
                }
            }

            return result;
        }

        #endregion

        #region ISelectableText

        public virtual bool IsTextSelected
        {
            get
            {
                var target = Selection.OfType<ISelectableText>().LastOrDefault();
                if (target != null)
                {
                    return target.IsTextSelected;
                }
                else
                {
                    return false;
                }
            }
        }

        public virtual string SelectedText
        {
            get
            {
                var buffer = new StringBuilder();

                foreach (var textElement in Selection.OfType<ISelectableText>())
                {
                    buffer.Append(textElement.SelectedText);
                }

                return buffer.ToString();
            }
        }

        #endregion

        #region ITextElement

        public virtual void Insert(string value)
        {
            using (var undo = new UndoScope(UndoStack))
            {
                var target = Selection.OfType<ITextElement>().LastOrDefault();

                if (target != null)
                {
                    target.Insert(value);
                }
            }
        }

        public virtual void Replace(string value)
        {
            using (var undo = new UndoScope(UndoStack))
            {
                if (Selection.Count == 1 && Selection.First() is ITextElement)
                {
                    ((ITextElement)Selection.First()).Replace(value);
                }
                else
                {
                    Delete();
                    Insert(value);
                }
            }
        }

        public virtual int Replace(string oldValue, string newValue)
        {
            using (var undo = new UndoScope(UndoStack))
            {
                int count = 0;

                foreach (var text in Selection.OfType<ITextElement>())
                {
                    count += text.Replace(oldValue, newValue);
                }

                return count;
            }
        }

        public virtual bool MoveLeftByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)target);
            if (target.MoveLeftByWord())
            {
                return true;
            }

            return OnMoveLeftByWord(target);
        }

        public virtual bool MoveRightByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)target);
            if (target.MoveRightByWord())
            {
                return true;
            }

            return OnMoveRightByWord(target);
        }

        public virtual bool MoveToLineStart()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)target);
            if (target.MoveToLineStart())
            {
                return true;
            }

            return OnMoveToLineStart(target);
        }

        public virtual bool MoveToLineEnd()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)target);
                
            if (target.MoveToLineEnd())
            {
                return true;
            }

            return OnMoveToLineEnd(target);
        }

        public virtual bool MoveToParagraphStart()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)target);

            if (target.MoveToParagraphStart())
            {
                return true;
            }

            return OnMoveToParagraphStart(target);
        }

        public virtual bool MoveToParagraphEnd()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)target);
            if (target.MoveToParagraphEnd())
            {
                return true;
            }

            return OnMoveToParagraphEnd(target);
        }

        public virtual bool SelectLeftByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectLeftByWord())
            {
                return true;
            }

            return OnSelectLeftByWord(target);
        }

        public virtual bool SelectRightByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectRightByWord())
            {
                return true;
            }

            return OnSelectRightByWord(target);
        }

        public virtual bool SelectToLineStart()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectToLineStart())
            {
                return true;
            }

            return OnSelectToLineStart(target);
        }

        public virtual bool SelectToLineEnd()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectToLineEnd())
            {
                return true;
            }

            return OnSelectToLineEnd(target);
        }

        public virtual bool SelectToParagraphStart()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectToParagraphStart())
            {
                return true;
            }

            return OnSelectToParagraphStart(target);
        }

        public virtual bool SelectToParagraphEnd()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.SelectToParagraphEnd())
            {
                return true;
            }

            return OnSelectToParagraphEnd(target);
        }

        public virtual bool DeleteBack()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.DeleteBack())
            {
                return true;
            }
            
            return OnDeleteBack(target);
        }

        public virtual bool DeleteBackByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.DeleteBackByWord())
            {
                return true;
            }

            return OnDeleteBackByWord(target);
        }

        public virtual bool DeleteBackByParagraph()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.DeleteBackByParagraph())
            {
                return true;
            }

            return OnDeleteBackByParagraph(target);
        }

        public virtual bool DeleteForward()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.DeleteForward())
            {
                return true;
            }

            return OnDeleteForward(target);
        }

        public virtual bool DeleteForwardByWord()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.DeleteForwardByWord())
            {
                return true;
            }

            return OnDeleteForwardByWord(target);
        }

        public virtual bool DeleteForwardByParagraph()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.DeleteForwardByParagraph())
            {
                return true;
            }

            return OnDeleteForwardByParagraph(target);
        }

        public virtual bool EnterLineBreak()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.EnterLineBreak())
            {
                return true;
            }

            return false;
        }

        public virtual bool EnterParagraphBreak()
        {
            var target = Selection.OfType<ITextElement>().LastOrDefault();

            if (target == null)
            {
                return false;
            }

            if (target.EnterParagraphBreak())
            {
                return true;
            }

            return OnEnterParagraphBreak(target);
        }

        public ITextElement Split()
        {
            return null;
        }

        public bool Merge(ITextElement other)
        {
            return false;
        }

        #endregion

        #region ISearchable

        public int Find(string text)
        {
            return Searchable.Find(InternalChildren, text);
        }

        public bool FindFirst()
        {
            var result = Searchable.FindFirst(InternalChildren);
            if (result != null)
            {
                Selection.SelectOnly((UIElement)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindLast()
        {
            var result = Searchable.FindLast(InternalChildren);
            if (result != null)
            {
                Selection.SelectOnly((UIElement)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindNext()
        {
            var result = Searchable.FindNext(InternalChildren, Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((UIElement)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FindPrevious()
        {
            var result = Searchable.FindPrevious(InternalChildren, Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((UIElement)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool FindFirst(string pattern, RegexOptions options)
        {
            var result = Searchable.FindFirst(InternalChildren, pattern, options);
            if (result != null)
            {
                Selection.SelectOnly((UIElement)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool FindLast(string pattern, RegexOptions options)
        {
            var result = Searchable.FindLast(InternalChildren, pattern, options);
            if (result != null)
            {
                Selection.SelectOnly((UIElement)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool FindNext(string pattern, RegexOptions options)
        {
            var result = Searchable.FindNext(InternalChildren, pattern, options, Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((UIElement)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool FindPrevious(string pattern, RegexOptions options)
        {
            var result = Searchable.FindPrevious(InternalChildren, pattern, options, Selection.LastOrDefault());
            if (result != null)
            {
                Selection.SelectOnly((UIElement)result);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region ICloneable

        public virtual object Clone()
        {
            return new EditingPanel(this);
        }

        #endregion

        #region Implementation

        protected virtual bool OnMoveUp(INavigable target)
        {
            var previous = FindPrevious<INavigable>(
                (UIElement)target, 
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (previous == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)previous);

            previous.NavigationOffset = target.NavigationOffset;

            previous.MoveToBottom();

            return true;
        }

        protected virtual bool OnMoveDown(INavigable target)
        {
            var next = FindNext<INavigable> (
                (UIElement)target, 
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (next == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)next);

            next.NavigationOffset = target.NavigationOffset;

            next.MoveToTop();

            return true;
        }

        protected virtual bool OnMoveLeft(INavigable target)
        {
            var previous = FindPrevious<INavigable> (
                (UIElement)target, 
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (previous == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)previous);

            previous.MoveToEnd();

            return true;
        }

        protected virtual bool OnMoveRight(INavigable target)
        {
            var next = FindNext<INavigable> ( 
                (UIElement)target, 
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (next == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)next);

            next.MoveToStart();

            return true;
        }

        protected virtual bool OnMoveLeftByWord(ITextElement target)
        {
            var previous = FindPrevious<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (previous == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)previous);

            previous.MoveToEnd();
            previous.MoveLeftByWord();

            return true;
        }

        protected virtual bool OnMoveRightByWord(ITextElement target)
        {
            var next = FindNext<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (next == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)next);

            next.MoveToStart();
            next.MoveRightByWord();

            return true;
        }

        protected virtual bool OnMoveToParagraphStart(ITextElement target)
        {
            var previous = FindPrevious<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (previous == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)previous);

            previous.MoveToParagraphStart();

            return true;
        }

        protected virtual bool OnMoveToParagraphEnd(ITextElement target)
        {
            var next = FindNext<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (next == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)next);

            next.MoveToParagraphEnd();

            return true;
        }

        protected virtual bool OnMoveToLineStart(ITextElement target)
        {
            var previous = FindPrevious<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (previous == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)previous);

            previous.MoveToEnd();
            previous.MoveToLineStart();

            return true;
        }

        protected virtual bool OnMoveToLineEnd(ITextElement target)
        {
            var next = FindNext<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (next == null)
            {
                return false;
            }

            Selection.SelectOnly((UIElement)next);

            next.MoveToStart();
            next.MoveToLineEnd();

            return true;
        }

        protected virtual bool OnSelectUp(INavigable target)
        {
            var previous = FindPrevious<INavigable> ( 
                (UIElement)target, 
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (previous == null)
            {
                return false;
            }

            SelectTo((UIElement)previous);

            previous.NavigationOffset = target.NavigationOffset;

            previous.SelectToBottom();

            return true;
        }

        protected virtual bool OnSelectDown(INavigable target)
        {
            var next = FindNext<INavigable> ( 
                (UIElement)target, 
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (next == null)
            {
                return false;
            }

            SelectTo((UIElement)next);

            next.NavigationOffset = target.NavigationOffset;

            next.SelectToTop();

            return true;
        }

        protected virtual bool OnSelectLeft(INavigable target)
        {
            var previous = FindPrevious<INavigable> ( 
                (UIElement)target, 
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (previous == null)
            {
                return false;
            }

            if (!Selection.Contains((UIElement)previous))
            {
                previous.MoveToEnd();
            }

            SelectTo((UIElement)previous);

            previous.SelectToEnd();

            return true;
        }

        protected virtual bool OnSelectRight(INavigable target)
        {
            var next = FindNext<INavigable> ( 
                (UIElement)target, 
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (next == null)
            {
                return false;
            }

            if (!Selection.Contains((UIElement)next))
            {
                next.MoveToStart();
            }

            SelectTo((UIElement)next);

            next.SelectToStart();

            return true;
        }

        protected virtual bool OnSelectLeftByWord(ITextElement target)
        {
            var previous = FindPrevious<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (previous == null)
            {
                return false;
            }

            if (!Selection.Contains((UIElement)previous))
            {
                previous.MoveToEnd();
            }

            SelectTo((UIElement)previous);

            previous.SelectToEnd();
            previous.SelectLeftByWord();

            return true;
        }

        protected virtual bool OnSelectRightByWord(ITextElement target)
        {
            var next = FindNext<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (next == null)
            {
                return false;
            }

            if (!Selection.Contains((UIElement)next))
            {
                next.MoveToStart();
            }

            SelectTo((UIElement)next);

            next.SelectToStart();
            next.SelectRightByWord();

            return true;
        }

        protected virtual bool OnSelectToLineStart(ITextElement target)
        {
            var previous = FindPrevious<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (previous == null)
            {
                return false;
            }

            if (!Selection.Contains((UIElement)previous))
            {
                previous.MoveToEnd();
            }

            SelectTo((UIElement)previous);

            previous.SelectToLineStart();

            return true;
        }

        protected virtual bool OnSelectToLineEnd(ITextElement target)
        {
            var next = FindNext<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (next == null)
            {
                return false;
            }

            if (!Selection.Contains((UIElement)next))
            {
                next.MoveToStart();
            }

            SelectTo((UIElement)next);

            next.SelectToLineEnd();

            return true;
        }

        protected virtual bool OnSelectToParagraphStart(ITextElement target)
        {
            var previous = FindPrevious<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (previous == null)
            {
                return false;
            }

            if (!Selection.Contains((UIElement)previous))
            {
                previous.MoveToEnd();
            }

            SelectTo((UIElement)previous);

            previous.SelectToParagraphStart();

            return true;
        }

        protected virtual bool OnSelectToParagraphEnd(ITextElement target)
        {
            var next = FindNext<ITextElement>(
                (UIElement)target,
                PositioningEquals(Positioning.Static),
                Visibility.Visible
            );
            if (next == null)
            {
                return false;
            }

            if (!Selection.Contains((UIElement)next))
            {
                next.MoveToStart();
            }

            SelectTo((UIElement)next);

            next.SelectToParagraphEnd();

            return true;
        }

        /*
         * Default delete-back handler.
         * 
         * Called when backspace is pressed at the beginning of a text element.
         */
        protected virtual bool OnDeleteBack(ITextElement target)
        {
            // Merge the target element with the preceding visible static element

            using (var undo = new UndoScope(UndoStack, "Delete back"))
            {
                var previous = FindPrevious<UIElement>(
                    (UIElement)target,
                    PositioningEquals(Positioning.Static),
                    Visibility.Visible
                ) as ITextElement;

                if (previous != null)
                {
                    if (previous.Merge(target))
                    {
                        Remove((UIElement)target);
                        Selection.SelectOnly((UIElement)previous);
                    }

                    return true;
                }

                return false;
            }
        }

        protected virtual bool OnDeleteBackByWord(ITextElement target)
        {
            using (var undo = new UndoScope(UndoStack, "Delete back by word"))
            {
                // Merge with preceding element

                var previous = FindPrevious<UIElement>(
                    (UIElement)target,
                    PositioningEquals(Positioning.Static),
                    Visibility.Visible
                ) as ITextElement;

                if (previous == null)
                {
                    return false;
                }

                if (previous.Merge(target))
                {
                    Remove((UIElement)target);
                    Selection.SelectOnly((UIElement)previous);
                }

                return true;
            }
        }

        protected virtual bool OnDeleteBackByParagraph(ITextElement target)
        {
            using (var undo = new UndoScope(UndoStack, "Delete back by paragraph"))
            {
                // Delete preceding paragraph

                var previous = FindPrevious<UIElement>(
                    (UIElement)target,
                    PositioningEquals(Positioning.Static),
                    Visibility.Visible
                ) as TextField;

                if (previous == null)
                {
                    return false;
                }

                Remove((UIElement)target);
                previous.Delete(0, previous.Length);
                Selection.SelectOnly((UIElement)previous);

                return true;
            }
        }

        protected virtual bool OnDeleteForward(ITextElement target)
        {
            using (var undo = new UndoScope(UndoStack, "Delete forward"))
            {
                // Merge with following element

                var next = FindNext<UIElement>(
                    (UIElement)target,
                    PositioningEquals(Positioning.Static),
                    Visibility.Visible
                ) as ITextElement;

                if (next == null)
                {
                    return false;
                }

                if (target.Merge(next))
                {
                    Remove((UIElement)next);
                }

                return true;
            }
        }

        protected virtual bool OnDeleteForwardByWord(ITextElement target)
        {
            using (var undo = new UndoScope(UndoStack, "Delete forward by word"))
            {
                // Merge with following element

                var next = FindNext<UIElement>(
                    (UIElement)target,
                    PositioningEquals(Positioning.Static),
                    Visibility.Visible
                ) as ITextElement;

                if (next == null)
                {
                    return false;
                }

                if (target.Merge(next))
                {
                    Remove((UIElement)next);
                }

                return true;
            }
        }

        protected virtual bool OnDeleteForwardByParagraph(ITextElement target)
        {
            using (var undo = new UndoScope(UndoStack, "Delete forward by paragraph"))
            {
                // Remove the following paragraph

                var next = FindNext<UIElement>(
                    (UIElement)target,
                    PositioningEquals(Positioning.Static),
                    Visibility.Visible
                ) as TextField;

                if (next == null)
                {
                    return false;
                }

                Remove((UIElement)next);

                return true;
            }
        }

        protected virtual bool OnEnterParagraphBreak(ITextElement target)
        {
            using (new UndoScope(UndoStack, "Paragraph break"))
            {
                var targetParagraph = target as TextParagraph;

                // Get next sibling

                INavigable next = FindNext<INavigable>(
                    (UIElement)target,
                    PositioningEquals(Positioning.Static),
                    Visibility.Visible
                );

                // Split child in two

                INavigable split = target.Split() ?? new TextParagraph();

                if (!InternalChildren.Contains((UIElement)split))
                {
                    int index;
                    if (targetParagraph != null && targetParagraph.Length == 0)
                    {
                        index = InternalChildren.IndexOf(targetParagraph) + 1;
                    }
                    else if (next != null)
                    {
                        index = InternalChildren.IndexOf((UIElement)next);
                    }
                    else
                    {
                        index = InternalChildren.Count;
                    }

                    Insert(index, (UIElement)split);
                }

                // Automatically reset bulleting on target paragraph when empty

                var splitParagraph = split as TextParagraph;

                if (split != target && 
                    targetParagraph != null && targetParagraph.Length == 0 &&
                    splitParagraph != null && splitParagraph.Length == 0)
                {
                    targetParagraph.IsListItem = false;
                    targetParagraph.ListLevel = 0;
                    targetParagraph.LeftMargin = 0;
                }

                Selection.SelectOnly((UIElement)split);
                split.MoveToStart();
                return true;
            }
        }

        protected virtual bool OnTabForward(UIElement target)
        {
            var next = FindNext<INavigable>(target, PositioningEquals(Positioning.Static));
            if (next != null)
            {
                Selection.SelectOnly((UIElement)next);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual bool OnTabBackward(UIElement target)
        {
            var previous = FindPrevious<INavigable>(target, PositioningEquals(Positioning.Static));
            if (previous != null)
            {
                Selection.SelectOnly((UIElement)previous);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnPositioningChanged(UIElement element, Positioning oldPositioning, Positioning newPositioning)
        {
            // All operations that modify positioning are undoable

            var undo = UndoStack;

            if (undo == null || (!undo.IsUndoing && !undo.IsRedoing))
            {
                base.OnPositioningChanged(element, oldPositioning, newPositioning);
            }
        }

        #endregion
    }
}
