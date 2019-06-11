/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SilverNote.Common;

namespace SilverNote.ViewModels.CategoryTree
{
    public class CategoryNodeCollection : IList<ITreeNode>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Fields

        CategoryTreeViewModel _Root;
        CategoryNode _Parent;
        IList<CategoryViewModel> _Categories;
        int _Depth;
        Dictionary<CategoryViewModel, CategoryNode> _Nodes;
        int? _Count;

        #endregion

        #region Constructors

        public CategoryNodeCollection(CategoryTreeViewModel root, CategoryNode parent, IList<CategoryViewModel> categories, int depth)
        {
            _Root = root;
            _Parent = parent;
            _Categories = categories;
            _Depth = depth;
            _Nodes = new Dictionary<CategoryViewModel, CategoryNode>();
            _Count = categories.Count;

            var notifiable = categories as INotifyCollectionChanged;
            if (notifiable != null)
            {
                notifiable.CollectionChanged += Categories_CollectionChanged;
            }
        }

        #endregion

        #region Properties

        public CategoryTreeViewModel Root
        {
            get { return _Root; }
        }

        public CategoryNode Parent
        {
            get { return _Parent; }
        }

        public int Depth
        {
            get { return _Depth; }
        }

        #endregion

        #region IList

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return _Categories.IsReadOnly; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public Object SyncRoot
        {
            get { return null; }
        }

        public int Count
        {
            get 
            {
                if (_Count == null)
                {
                    _Count = ComputeCount();
                }
                return _Count.Value;
            }
        }

        public ITreeNode this[int index]
        {
            get { return NodeAt(index); }
            set { throw new NotSupportedException(); }
        }

        Object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (CategoryNode)value; }
        }

        public void Add(ITreeNode item)
        {
            throw new NotSupportedException();
        }

        int IList.Add(Object item)
        {
            int result = Count;
            Add((ITreeNode)item);
            return result;
        }

        public void Clear()
        {
            _Categories.Clear();
        }

        public bool Contains(ITreeNode item)
        {
            return IndexOf(item) != -1;
        }

        bool IList.Contains(Object item)
        {
            return this.Contains((ITreeNode)item);
        }

        public void CopyTo(ITreeNode[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex++] = this[i];
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array.SetValue(this[i], arrayIndex++);
            }
        }

        public IEnumerator<ITreeNode> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(ITreeNode value)
        {
            return FindNode(value);
        }

        int IList.IndexOf(Object value)
        {
            return IndexOf((ITreeNode)value);
        }

        public void Insert(int index, ITreeNode item)
        {
            throw new NotSupportedException();
        }

        void IList.Insert(int index, Object value)
        {
            Insert(index, (ITreeNode)value);
        }

        public bool Remove(ITreeNode item)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(Object value)
        {
            Remove((ITreeNode)value);
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

        protected void RaiseCollectionReset()
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            RaiseCollectionChanged(e);
        }

        protected void RaiseItemsAdded(IList items, int startIndex)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)items, startIndex);

            RaiseCollectionChanged(e);
        }

        protected void RaiseItemsRemoved(IList items, int startIndex)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)items, startIndex);

            RaiseCollectionChanged(e);
        }

        protected void RaiseItemsMoved(IList items, int index, int oldIndex)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, (IList)items, index, oldIndex);

            RaiseCollectionChanged(e);
        }

        protected void RaiseItemsReplaced(IList newItems, IList oldItems, int index)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList)oldItems, (IList)newItems, index);

            RaiseCollectionChanged(e);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Implementation

        private CategoryNode GetNode(CategoryViewModel category)
        {
            CategoryNode result;
            if (!_Nodes.TryGetValue(category, out result))
            {
                result = CreateNode(category);
                _Nodes.Add(category, result);
            }
            return result;
        }

        private CategoryNode CreateNode(CategoryViewModel category)
        {
            var result = new CategoryNode(Root, Parent, category, Depth);
            result.CollectionChanged += Node_CollectionChanged;
            return result;
        }

        private void DestroyNode(CategoryNode node)
        {
            node.CollectionChanged -= Node_CollectionChanged;
        }

        private void RemoveNode(CategoryViewModel category)
        {
            CategoryNode result;
            if (_Nodes.TryGetValue(category, out result))
            {
                DestroyNode(result);
                _Nodes.Remove(category);
            }
        }

        private void RemoveNode(CategoryNode node)
        {
            DestroyNode(node);

            var result = _Nodes.FirstOrDefault(item => item.Value == node);
            if (result.Key != null)
            {
                _Nodes.Remove(result.Key);
            }
        }

        private bool IsValidNode(CategoryNode node)
        {
            CategoryNode testNode;
            return _Nodes.TryGetValue(node.Category, out testNode) && testNode == node;
        }

        private int ComputeCount()
        {
            int count = 0;

            foreach (var category in _Categories)
            {
                count++;

                CategoryNode node;
                if (_Nodes.TryGetValue(category, out node) && node.IsExpanded)
                {
                    count += node.Count;
                }
            }

            return count;
        }

        private ITreeNode NodeAt(int index)
        {
            foreach (var category in _Categories)
            {
                if (index-- == 0)
                {
                    return GetNode(category);
                }

                CategoryNode node;
                if (_Nodes.TryGetValue(category, out node) && node.IsExpanded)
                {
                    if (index < node.Count)
                    {
                        return node[index];
                    }
                    index -= node.Count;
                }
            }

            throw new IndexOutOfRangeException();
        }

        private int FindNode(ITreeNode node)
        {
            int offset = 0;

            foreach (var category in _Categories)
            {
                CategoryNode currentNode;
                if (!_Nodes.TryGetValue(category, out currentNode))
                {
                    offset++;
                    continue;
                }

                if (currentNode == node)
                {
                    return offset;
                }
                offset++;

                if (currentNode.IsExpanded)
                {
                    int index = currentNode.IndexOf(node);
                    if (index != -1)
                    {
                        return offset + index;
                    }
                    offset += currentNode.Count;
                }
            }

            return -1;
        }

        void Categories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Note: this does not synchronize _Nodes with the underlying category collection.

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    break;
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    e.OldItems.OfType<CategoryViewModel>().ForEach(RemoveNode);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    e.OldItems.OfType<CategoryViewModel>().Except(e.NewItems.OfType<CategoryViewModel>()).ForEach(RemoveNode);
                    break;
            }

            _Count = null;
            RaiseCollectionReset();
        }

        void Node_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CategoryNode category = (CategoryNode)sender;

            // Remove unused nodes in a lazy fashion
            if (!IsValidNode(category))
            {
                RemoveNode(category);
                return;
            }

            int newStartingIndex = (e.NewStartingIndex != -1) ? (IndexOf(category) + 1 + e.NewStartingIndex) : -1;
            int oldStartingIndex = (e.OldStartingIndex != -1) ? (IndexOf(category) + 1 + e.OldStartingIndex) : -1;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    _Count = null;
                    RaiseCollectionReset();
                    break;
                case NotifyCollectionChangedAction.Add:
                    if (_Count != null) _Count += e.NewItems.Count;
                    RaiseItemsAdded(e.NewItems, newStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (_Count != null) _Count -= e.OldItems.Count;
                    RaiseItemsRemoved(e.OldItems, oldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    RaiseItemsMoved(e.NewItems, newStartingIndex, oldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RaiseItemsReplaced(e.NewItems, e.OldItems, oldStartingIndex);
                    break;
            }
        }

        #endregion
    }

}
