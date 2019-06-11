/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SilverNote.Editor
{
    public class Selection<T> : IEnumerable<T> where T : class
    {
        #region Static Methods

        public static IEnumerable<T> GetRange(IList items, object firstItem, object lastItem)
        {
            int firstIndex = items.IndexOf(firstItem);
            if (firstIndex == -1)
            {
                throw new ArgumentException("firstItem");
            }

            int lastIndex = items.IndexOf(lastItem);
            if (lastIndex == -1)
            {
                throw new ArgumentException("lastItem");
            }

            return GetRange(items, firstIndex, lastIndex);
        }

        public static IEnumerable<T> GetRange(IList items, int startIndex, int endIndex)
        {
            int tick = (endIndex > startIndex) ? 1 : -1;

            for (int i = startIndex; i != endIndex + tick; i += tick)
            {
                var item = items[i] as T;
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region Fields

        private List<T> _Items = new List<T>();

        #endregion

        #region Constructors

        public Selection()
        {  
        
        }

        #endregion

        #region Properties

        public UndoStack UndoStack { get; set; }

        public int Count
        {
            get { return Items.Count; }
        }

        public T this[int index]
        {
            get { return Items[index]; }
            set { Items[index] = value; }
        }

        #endregion

        #region Methods

        public bool Contains(T item)
        {
            return Items.Contains(item);
        }

        public void Select(T item, int index = Int32.MaxValue)
        {
            if (Items.Contains(item))
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Select Object", isEditing: false))
            {
                if (UndoStack != null)
                {
                    UndoStack.Push(() => Unselect(item));
                }

                if (index != Int32.MaxValue)
                {
                    Items.Insert(index, item);
                }
                else
                {
                    Items.Add(item);
                }

                OnSelectionChanged(null, new T[] { item });
            }
        }

        public void Unselect(T item)
        {
            int index = Items.IndexOf(item);
            if (index == -1)
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Unselect Object", isEditing: false))
            {
                if (UndoStack != null)
                {
                    UndoStack.Push(() => Select(item, index));
                }

                Items.RemoveAt(index);

                OnSelectionChanged(new T[] { item }, null);
            }
        }

        public void SelectOnly(T item)
        {
            T[] newSelection = { item };

            Set(newSelection);
        }

        public void SelectRange(IList items, object firstItem, object lastItem, Func<T, bool> filter = null)
        {
            int firstIndex = items.IndexOf(firstItem);
            if (firstIndex == -1)
            {
                throw new ArgumentException("firstItem");
            }

            int lastIndex = items.IndexOf(lastItem);
            if (lastIndex == -1)
            {
                throw new ArgumentException("lastItem");
            }

            SelectRange(items, firstIndex, lastIndex, filter);
        }

        public void SelectRange(IList items, int startIndex, int endIndex, Func<T, bool> filter = null)
        {
            var selection = GetRange(items, startIndex, endIndex);

            if (filter != null)
            {
                selection = selection.Where(filter);
            }

            SelectRange(selection.ToArray());
        }

        public void SelectRange(IEnumerable items)
        {
            SelectRange(items.OfType<T>());
        }

        public void SelectRange(IEnumerable<T> items)
        {
            var newSelection = Items.Union(items);

            Set(newSelection);
        }

        public void SelectRangeOnly(IList items, object firstItem, object lastItem, Func<T, bool> filter = null)
        {
            int firstIndex = items.IndexOf(firstItem);
            if (firstIndex == -1)
            {
                throw new ArgumentException("firstItem");
            }

            int lastIndex = items.IndexOf(lastItem);
            if (lastIndex == -1)
            {
                throw new ArgumentException("lastItem");
            }

            SelectRangeOnly(items, firstIndex, lastIndex, filter);
        }

        public void SelectRangeOnly(IList items, int startIndex, int endIndex, Func<T, bool> filter = null)
        {
            var selection = GetRange(items, startIndex, endIndex);

            if (filter != null)
            {
                selection = selection.Where(filter);
            }

            SelectRangeOnly(selection.ToArray());
        }

        public void SelectRangeOnly(IEnumerable items)
        {
            SelectRangeOnly(items.OfType<T>());
        }

        public void SelectRangeOnly(IEnumerable<T> items)
        {
            Set(items);
        }

        public void SelectAll(IEnumerable items)
        {
            SelectAll(items.OfType<T>());
        }

        public void SelectAll(IEnumerable<T> items)
        {
            Set(items);
        }

        public void UnselectRange(IEnumerable<T> items)
        {
            var newSelection = Items.Except(items);

            Set(newSelection);
        }

        public void UnselectWhere(Func<T, bool> predicate)
        {
            var newSelection = Items.Where(item => !predicate(item));

            Set(newSelection);
        }

        public void UnselectAll()
        {
            var newSelection = new T[0];

            Set(newSelection);
        }

        #endregion

        #region Events

        public event SelectionChangedEventHandler<T> SelectionChanged;

        protected void RaiseSelectionChanged(T[] removedItems, T[] addedItems)
        {
            var handler = SelectionChanged;
            if (handler != null)
            {
                handler(this, new SelectionChangedEventArgs<T>(removedItems, addedItems));
            }
        }

        #endregion

        #region Implementation

        private List<T> Items
        {
            get { return _Items; }
        }

        private void Set(IEnumerable<T> items)
        {
            if (Items.SequenceEqual(items))
            {
                return;
            }

            using (var undo = new UndoScope(UndoStack, "Select Object", isEditing: false))
            {
                var oldSelection = Items.ToArray();
                var newSelection = items.ToArray();

                if (UndoStack != null)
                {
                    UndoStack.Push(() => Set(oldSelection));
                }

                Items.Clear();
                Items.AddRange(newSelection);

                var removedItems = oldSelection.Except(newSelection).ToArray();
                var addedItems = newSelection.Except(oldSelection).ToArray();

                OnSelectionChanged(removedItems, addedItems);
            }
        }

        protected void OnSelectionChanged(T[] removedItems, T[] addedItems)
        {
            // Note: OnSelected() and OnUnselected() call external methods,
            // which may result in additional changes to the selection.
            //
            // Therefore, RaiseSelectionChanged() must be called first so
            // that the current selection is consistent with the arguments
            // passed to the event handler.
            //
            // In addition, we double-check the current selection prior
            // to calling OnSelected() or OnUnselected() to ensure all
            // calls to those methods are consistent with the current
            // selection.

            RaiseSelectionChanged(removedItems, addedItems);

            if (removedItems != null)
            {
                foreach (var item in removedItems)
                {
                    if (!Items.Contains(item))
                    {
                        OnUnselected(item);
                    }
                }
            }

            if (addedItems != null)
            {
                foreach (var item in addedItems)
                {
                    if (Items.Contains(item))
                    {
                        OnSelected(item);
                    }
                }
            }

        }

        protected void OnUnselected(T item)
        {
            var selectable = item as ISelectable;
            if (selectable != null)
            {
                selectable.Unselect();
            }
        }

        protected void OnSelected(T item)
        {
            var selectable = item as ISelectable;
            if (selectable != null)
            {
                selectable.Select();
            }
        }

        #endregion

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion
    }

    public delegate void SelectionChangedEventHandler<T>(object sender, SelectionChangedEventArgs<T> e);

    public class SelectionChangedEventArgs<T> : EventArgs
    {
        public SelectionChangedEventArgs(T[] removedItems, T[] addedItems)
        {
            RemovedItems = removedItems;
            AddedItems = addedItems;
        }

        public T[] RemovedItems { get; set; }

        public T[] AddedItems { get; set; }
    }
}
