/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Behaviors;
using SilverNote.Common;
using SilverNote.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SilverNote.ViewModels.CategoryTree
{
    public class CategoryNode : ITreeNode
    {
        #region Fields

        CategoryTreeViewModel _Root;
        CategoryNode _Parent;
        CategoryViewModel _Category;
        int _Depth;
        bool _IsSelected;
        bool _IsExpanded;
        CategoryNodeCollection _Categories;
        SearchResultNodeCollection _SearchResults;

        #endregion

        #region Constructors

        public CategoryNode(CategoryTreeViewModel root, CategoryNode parent, CategoryViewModel category, int depth = 0)
        {
            _Root = root;
            _Parent = parent;
            _Category = category;
            _Depth = depth;

            _Categories = new CategoryNodeCollection(root, this, category.Children, depth + 1);
            _Categories.CollectionChanged += Categories_CollectionChanged;

            _SearchResults = new SearchResultNodeCollection(root, this, category.Search.Results, depth + 1);
            _SearchResults.CollectionChanged += SearchResults_CollectionChanged;
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

        public CategoryViewModel Category
        {
            get { return _Category; }
        }

        public Uri Uri
        {
            get { return Category.Model.Uri; }
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
            get
            {
                return _IsExpanded;
            }
            set
            {
                if (value != _IsExpanded)
                {
                    _IsExpanded = value;
                    RaisePropertyChanged("IsExpanded");
                    RaiseCollectionReset();
                }
            }
        }

        #endregion

        #region Operations

        public void Open()
        {
            
        }

        public void Paste()
        {
            var data = NClipboard.GetDataObject();
            if (data != null)
            {
                Paste(data);
            }
        }

        public bool CanPaste()
        {
            var data = NClipboard.GetDataObject();
            if (data != null)
            {
                return CanPaste(data);
            }
            else
            {
                return false;
            }
        }

        private void Paste(IDataObject data)
        {
            var uriList = (List<Uri>)data.GetData(typeof(List<Uri>));
            if (uriList != null)
            {
                Paste(uriList);
            }
        }

        private bool CanPaste(IDataObject data)
        {
            return data.GetDataPresent(typeof(List<Uri>));
        }

        private void Paste(IEnumerable<Uri> items)
        {
            foreach (var item in items)
            {
                PasteItem(item);
            }
        }

        private void PasteItem(Uri uri)
        {
            var item = Category.Notebook.Locate(uri);

            if (item is CategoryViewModel)
            {
                PasteItem((CategoryViewModel)item);
            }
            else if (item is NoteViewModel)
            {
                PasteItem((NoteViewModel)item);
            }
        }

        private void PasteItem(CategoryViewModel category)
        {
            if (!Category.Model.IsPseudoCategory && !category.Model.IsPseudoCategory)
            {
                Category.AddChild(category);
            }
        }

        private void PasteItem(NoteViewModel note)
        {
            Category.AddNote(note);
        }

        public bool Delete()
        {
            Category.Delete();
            return true;
        }

        public void Remove()
        {

        }

        public void ToggleExpanded()
        {
            IsExpanded = !IsExpanded;
        }

        #endregion

        #region Commands

        #region ToggleExpandedCommand

        ICommand _ToggleExpandedCommand = null;

        public ICommand ToggleExpandedCommand
        {
            get { return _ToggleExpandedCommand ?? (_ToggleExpandedCommand = new DelegateCommand(ToggleExpanded)); }
        }

        #endregion

        #region PasteCommand

        ICommand _PasteCommand;

        public ICommand PasteCommand
        {
            get { return _PasteCommand ?? (_PasteCommand = new DelegateCommand(PasteCommand_Executed, PasteCommand_CanExecute)); }
        }

        void PasteCommand_Executed(object parameter)
        {
            Paste();

            if (!Category.IsPseudoCategory)
            {
                IsExpanded = true;
            }
        }

        bool PasteCommand_CanExecute(object parameter)
        {
            return CanPaste();
        }

        #endregion

        #region DropCommand

        ICommand _DropCommand = null;

        public ICommand DropCommand
        {
            get { return _DropCommand ?? (_DropCommand = new DelegateCommand(DropCommand_Executed)); }
        }

        private void DropCommand_Executed(object parameter)
        {
            var data = parameter as IDataObject;
            if (data == null)
            {
                return;
            }

            Paste(data);

            if (!Category.IsPseudoCategory)
            {
                IsExpanded = true;
            }
        }

        #endregion

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
            get
            {
                return IsExpanded ? (_Categories.Count + _SearchResults.Count) : 0;
            }
        }

        public ITreeNode this[int index]
        {
            get
            {
                if (!IsExpanded)
                {
                    throw new IndexOutOfRangeException();
                }

                if (index < _Categories.Count)
                {
                    return _Categories[index];
                }
                else
                {
                    index -= _Categories.Count;
                    return _SearchResults[index];
                }
            }
            set 
            { 
                throw new NotSupportedException(); 
            }
        }

        Object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (ITreeNode)value; }
        }

        public void Add(ITreeNode item)
        {
            throw new NotSupportedException();
        }

        int IList.Add(Object item)
        {
            int result = Count;
            Add((CategoryNode)item);
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
            int index = _Categories.IndexOf(value);
            if (index != -1)
            {
                return index;
            }

            index = _SearchResults.IndexOf(value);
            if (index != -1)
            {
                return _Categories.Count + index;
            }

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

        void Categories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsExpanded)
            {
                RaiseCollectionChanged(e);
            }
        }

        void SearchResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsExpanded)
            {
                return;
            }

            int newStartingIndex = (e.NewStartingIndex != -1) ? (_Categories.Count + e.NewStartingIndex) : -1;
            int oldStartingIndex = (e.OldStartingIndex != -1) ? (_Categories.Count + e.OldStartingIndex) : -1;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    RaiseCollectionReset();
                    break;
                case NotifyCollectionChangedAction.Add:
                    RaiseItemsAdded(e.NewItems, newStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
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
