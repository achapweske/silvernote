/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SilverNote.ViewModels.CategoryTree
{
    public class SearchResultNode : ITreeNode
    {
        #region Fields

        CategoryTreeViewModel _Root;
        CategoryNode _Parent;
        SearchResultViewModel _SearchResult;
        int _Depth;
        bool _IsSelected;

        #endregion

        #region Constructors

        public SearchResultNode(CategoryTreeViewModel root, CategoryNode parent, SearchResultViewModel searchResult, int depth)
        {
            _Root = root;
            _Parent = parent;
            _SearchResult = searchResult;
            _Depth = depth;
        }

        #endregion

        #region Properties

        public CategoryTreeViewModel Root
        {
            get { return _Root; }
        }

        public ITreeNode Parent
        {
            get { return _Parent; }
        }

        public SearchResultViewModel SearchResult
        {
            get { return _SearchResult; }
        }

        public Uri Uri
        {
            get { return SearchResult.Note.Model.Uri; }
        }

        public int Depth
        {
            get { return _Depth; }
        }

        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }

        public bool IsExpanded
        {
            get { return false; }
            set { }
        }

        #endregion

        #region Operations

        public void Open()
        {
            SearchResult.Open();
        }

        public bool Delete()
        {
            return SearchResult.Note.Delete();
        }

        public void Remove()
        {
            SearchResult.Note.RemoveFromCategory(SearchResult.Search.Category);
        }

        #endregion

        #region Commands

        #endregion

        #region IList

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return true; }
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
            get { return 0; }
        }

        public ITreeNode this[int index]
        {
            get { throw new IndexOutOfRangeException(); }
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
            throw new NotSupportedException();
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
            return -1;
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

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }
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
    }

}
