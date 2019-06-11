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
    public class SearchResultNodeCollection : IList<ITreeNode>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Fields

        CategoryTreeViewModel _Root;
        CategoryNode _Parent;
        IList<SearchResultViewModel> _SearchResults;
        int _Depth;
        Dictionary<SearchResultViewModel, SearchResultNode> _Nodes;

        #endregion

        #region Constructors

        public SearchResultNodeCollection(CategoryTreeViewModel root, CategoryNode parent, IList<SearchResultViewModel> searchResults, int depth)
        {
            _Root = root;
            _Parent = parent;
            _SearchResults = searchResults;
            _Depth = depth;
            _Nodes = new Dictionary<SearchResultViewModel, SearchResultNode>();

            var notifiable = searchResults as INotifyCollectionChanged;
            if (notifiable != null)
            {
                notifiable.CollectionChanged += SearchResults_CollectionChanged;
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
            get { return _SearchResults.IsReadOnly; }
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
            get { return _SearchResults.Count;  }
        }

        public ITreeNode this[int index]
        {
            get 
            { 
                return GetNode(_SearchResults[index]); 
            }
            set 
            {
                var node = value as SearchResultNode;
                if (node != null)
                {
                    _SearchResults[index] = node.SearchResult;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        Object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (ITreeNode)value; }
        }

        public void Add(ITreeNode item)
        {
            var node = item as SearchResultNode;
            if (node != null)
            {
                _SearchResults.Add(node.SearchResult);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        int IList.Add(Object item)
        {
            int result = Count;
            Add((ITreeNode)item);
            return result;
        }

        public void Clear()
        {
            _SearchResults.Clear();
        }

        public bool Contains(ITreeNode item)
        {
            var node = item as SearchResultNode;
            if (node != null)
            {
                return _SearchResults.Contains(node.SearchResult);
            }
            else
            {
                return false;
            }
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
            var node = value as SearchResultNode;
            if (node != null)
            {
                return _SearchResults.IndexOf(node.SearchResult);
            }
            else
            {
                return -1;
            }
        }

        int IList.IndexOf(Object value)
        {
            return IndexOf((SearchResultNode)value);
        }

        public void Insert(int index, ITreeNode item)
        {
            var node = item as SearchResultNode;
            if (node != null)
            {
                _SearchResults.Insert(index, node.SearchResult);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        void IList.Insert(int index, Object value)
        {
            Insert(index, (ITreeNode)value);
        }

        public bool Remove(ITreeNode item)
        {
            var node = item as SearchResultNode;
            if (node != null)
            {
                return _SearchResults.Remove(node.SearchResult);
            }
            else
            {
                return false;
            }
        }

        void IList.Remove(Object value)
        {
            Remove((ITreeNode)value);
        }

        public void RemoveAt(int index)
        {
            _SearchResults.RemoveAt(index);
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

        private SearchResultNode GetNode(SearchResultViewModel note)
        {
            SearchResultNode result;
            if (!_Nodes.TryGetValue(note, out result))
            {
                result = new SearchResultNode(Root, Parent, note, Depth);
                _Nodes.Add(note, result);
            }
            return result;
        }

        void SearchResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IList newItems = null;
            if (e.NewItems != null)
            {
                newItems = e.NewItems.OfType<SearchResultViewModel>().Select(GetNode).ToList();
            }

            IList oldItems = null;
            if (e.OldItems != null)
            {
                oldItems = e.OldItems.OfType<SearchResultViewModel>().Select(GetNode).ToList();
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    _Nodes.Clear();
                    RaiseCollectionReset();
                    break;
                case NotifyCollectionChangedAction.Add:
                    RaiseItemsAdded(newItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RaiseItemsRemoved(oldItems, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    RaiseItemsMoved(newItems, e.NewStartingIndex, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RaiseItemsReplaced(newItems, oldItems, e.OldStartingIndex);
                    break;
            }
        }

        #endregion
    }

}
