/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using SilverNote.Common;
using SilverNote.Models;

namespace SilverNote.ViewModels
{
    public class SearchResultsViewModel : ViewModelBase<SearchResultsModel, SearchResultsViewModel>, IList<SearchResultViewModel>, IList, INotifyCollectionChanged
    {
        #region Fields

        SearchResultViewModel _SelectedResult;

        #endregion

        #region Initialization

        protected override void OnInitialize()
        {
            Model.CollectionChanged += Model_CollectionChanged;
        }

        #endregion

        #region Properties

        public RepositoryViewModel Repository
        {
            get { return RepositoryViewModel.FromModel(Model.Repository); }
        }

        public SearchViewModel Search
        {
            get { return SearchViewModel.FromModel(Model.Search); }
        }

        /// <summary>
        /// Get/set the currently selected search result
        /// </summary>
        public SearchResultViewModel SelectedResult
        {
            get 
            {
                return _SelectedResult; 
            }
            set 
            {
                if (value != _SelectedResult)
                {
                    _SelectedResult = value;
                    RaisePropertyChanged("SelectedResult");
                }
            }
        }

        /// <summary>
        /// Get all selected search results
        /// </summary>
        public IList<SearchResultViewModel> SelectedResults
        {
            get
            {
                return Model.ReferencedResults
                    .Select(SearchResultViewModel.FromModel)
                    .Where(result => result.IsSelected)
                    .ToArray();
            }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Select the first search result
        /// </summary>
        /// <returns>Index of the selected search result (this is always 0)</returns>
        public int FirstResult()
        {
            if (Count == 0)
            {
                return -1;
            }

            int index = 0;

            this[index].IsSelected = true;

            return index;
        }

        /// <summary>
        /// Select the last search result
        /// </summary>
        /// <returns>Index of the selected search result (Count - 1)</returns>
        public int LastResult()
        {
            if (Count == 0)
            {
                return -1;
            }

            int index = Count - 1;

            this[index].IsSelected = true;

            return index;
        }

        /// <summary>
        /// Determine which search result is currently selected and select the next one.
        /// 
        /// If no results are selected, this method selects the first search result.
        /// </summary>
        /// <returns>Index of the newly-selected search result</returns>
        public int NextResult()
        {
            int index = -1;

            if (SelectedResult != null)
            {
                index = IndexOf(SelectedResult);
            }

            return NextResult(index);
        }

        /// <summary>
        /// Determine which search result is currently selected and select the previous one.
        /// 
        /// If no results are selected, this method selects the last search result.
        /// </summary>
        /// <returns>Index of the newly-selected search result</returns>
        public int PreviousResult()
        {
            int index = -1;

            if (SelectedResult != null)
            {
                index = IndexOf(SelectedResult);
            }

            return PreviousResult(index);
        }

        /// <summary>
        /// Select the next search result
        /// 
        /// If no results are selected (index = -1), this method selects the first search result.
        /// </summary>
        /// <param name="index">Index of the currently-selected search result, or -1 if none</param>
        /// <returns>Index of the newly-selected search result</returns>
        public int NextResult(int index)
        {
            if (index >= 0 && index < Count)
            {
                // Unselect any selected search terms
                var result = this[index];
                result.UnselectAll();
                result.IsSelected = false;
            }

            if (index == -1 || ++index >= Count)
            {
                index = FirstResult();
            }

            if (index >= 0 && index < Count)
            {
                this[index].IsSelected = true;
            }

            return index;
        }

        /// <summary>
        /// Select the previous search result
        /// 
        /// If no results are selected (index = -1), this method selects the last search result.
        /// </summary>
        /// <param name="index">Index of the currently-selected search result, or -1 if none</param>
        /// <returns>Index of the newly-selected search result</returns>
        public int PreviousResult(int index)
        {
            if (index >= 0 && index < Count)
            {
                var result = this[index];
                result.UnselectAll();
                result.IsSelected = false;
            }

            if (index == -1 || --index < 0)
            {
                index = LastResult();
            }

            if (index >= 0 && index < Count)
            {
                this[index].IsSelected = true;
            }

            return index;
        }

        /// <summary>
        /// Select the first instance of a search term within the first search result
        /// </summary>
        public void FirstTerm()
        {
            int index = FirstResult();

            FirstTerm(index);
        }

        /// <summary>
        /// Select the last instance of a search term within the last search result
        /// </summary>
        public void LastTerm()
        {
            int index = LastResult();

            LastTerm(index);
        }

        /// <summary>
        /// Select the next instance of a search term
        /// </summary>
        public void NextTerm()
        {
            if (Count == 0)
            {
                return;
            }

            int selectedIndex;

            if (SelectedResult != null)
            {
                selectedIndex = IndexOf(SelectedResult);
            }
            else
            {
                selectedIndex = FirstResult();
            }

            NextTerm(selectedIndex);
        }

        /// <summary>
        /// Select the previous instance of the search term
        /// </summary>
        public void PreviousTerm()
        {
            if (Count == 0)
            {
                return;
            }

            int selectedIndex;

            if (SelectedResult != null)
            {
                selectedIndex = IndexOf(SelectedResult);
            }
            else
            {
                selectedIndex = LastResult();
            }

            PreviousTerm(selectedIndex);
        }

        /// <summary>
        /// Select the first instance of a search term within the given search result
        /// </summary>
        /// <param name="selectedIndex">Index of the target search result</param>
        public void FirstTerm(int selectedIndex)
        {
            var result = this[selectedIndex];

            result.SelectFirstTerm();
        }

        /// <summary>
        /// Select the last instance of a search term within the given search result
        /// </summary>
        /// <param name="selectedIndex">Index of the target search result</param>
        public void LastTerm(int selectedIndex)
        {
            var result = this[selectedIndex];

            result.SelectLastTerm();
        }

        /// <summary>
        /// Select the next instance of a search term within the given search result
        /// </summary>
        /// <param name="selectedIndex">Index of the target search result</param>
        public void NextTerm(int selectedIndex)
        {
            var selectedResult = this[selectedIndex];

            if (!selectedResult.SelectNextTerm())
            {
                selectedIndex = NextResult(selectedIndex);
                FirstTerm(selectedIndex);
            }
        }

        /// <summary>
        /// Select the previous instance of a search term within the given search result
        /// </summary>
        /// <param name="selectedIndex">Index of the target search result</param>
        public void PreviousTerm(int selectedIndex)
        {
            var selectedResult = this[selectedIndex];

            if (!selectedResult.SelectPreviousterm())
            {
                selectedIndex = PreviousResult(selectedIndex);
                LastTerm(selectedIndex);
            }
        }

        /// <summary>
        /// Open all selected notes.
        /// 
        /// If exactly one note is selected, then that note will also be activated.
        /// </summary>
        public void OpenSelectedNotes()
        {
            var selectedResults = SelectedResults;

            if (selectedResults.Count > 20)
            {
                string message = String.Format("You are about to open {0} notes.", selectedResults.Count);

                if (MessageBox.Show(message, "SilverNote", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            if (selectedResults.Count == 1)
            {
                selectedResults.First().Open();
            }
            else
            {
                foreach (var result in selectedResults)
                {
                    Search.Notebook.OpenNote(result.Note);
                }
            }
        }

        /// <summary>
        /// Delete all selected notes
        /// </summary>
        public void DeleteSelectedNotes()
        {
            var selectedNotes = (from result in SelectedResults select result.Note).ToArray();

            Search.Notebook.DeleteNotes(selectedNotes);
        }

        #endregion

        #region Commands

        private ICommand _OpenNotesCommand = null;

        public ICommand OpenNotesCommand
        {
            get
            {
                if (_OpenNotesCommand == null)
                {
                    _OpenNotesCommand = new DelegateCommand(o => OpenSelectedNotes());
                }
                return _OpenNotesCommand;
            }
        }

        private ICommand _DeleteNotesCommand = null;

        public ICommand DeleteNotesCommand
        {
            get
            {
                if (_DeleteNotesCommand == null)
                {
                    _DeleteNotesCommand = new DelegateCommand(o => DeleteSelectedNotes());
                }
                return _DeleteNotesCommand;
            }
        }

        private ICommand _NextResultCommand = null;

        public ICommand NextResultCommand
        {
            get
            {
                if (_NextResultCommand == null)
                {
                    _NextResultCommand = new DelegateCommand(o => NextResult());
                }
                return _NextResultCommand;
            }
        }

        private ICommand _PreviousResultCommand = null;

        public ICommand PreviousResultCommand
        {
            get
            {
                if (_PreviousResultCommand == null)
                {
                    _PreviousResultCommand = new DelegateCommand(o => PreviousResult());
                }
                return _PreviousResultCommand;
            }
        }

        private ICommand _NextTermCommand = null;

        public ICommand NextTermCommand
        {
            get
            {
                if (_NextTermCommand == null)
                {
                    _NextTermCommand = new DelegateCommand(o => NextTerm());
                }
                return _NextTermCommand;
            }
        }

        private ICommand _PreviousTermCommand = null;

        public ICommand PreviousTermCommand
        {
            get
            {
                if (_PreviousTermCommand == null)
                {
                    _PreviousTermCommand = new DelegateCommand(o => PreviousTerm());
                }
                return _PreviousTermCommand;
            }
        }
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
            get { return Model.Count; }
        }

        public SearchResultViewModel this[int index]
        {
            get { return SearchResultViewModel.FromModel(Model[index]); }
            set { throw new NotSupportedException(); }
        }

        Object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (SearchResultViewModel)value; }
        }

        public void Add(SearchResultViewModel item)
        {
            throw new NotSupportedException();
        }

        int IList.Add(Object item)
        {
            int result = Count;
            Add((SearchResultViewModel)item);
            return result;
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(SearchResultViewModel item)
        {
            return IndexOf(item) != -1;
        }

        bool IList.Contains(Object item)
        {
            return this.Contains((SearchResultViewModel)item);
        }

        public void CopyTo(SearchResultViewModel[] array, int arrayIndex)
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

        public IEnumerator<SearchResultViewModel> GetEnumerator()
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

        public int IndexOf(SearchResultViewModel value)
        {
            return Model.IndexOf(value.Model);
        }

        int IList.IndexOf(Object value)
        {
            return IndexOf((SearchResultViewModel)value);
        }

        public void Insert(int index, SearchResultViewModel item)
        {
            throw new NotSupportedException();
        }

        void IList.Insert(int index, Object value)
        {
            Insert(index, (SearchResultViewModel)value);
        }

        public bool Remove(SearchResultViewModel item)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(Object value)
        {
            Remove((SearchResultViewModel)value);
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

        protected void RaiseItemsAdded(IList<SearchResultViewModel> items, int startIndex)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)items, startIndex);

            RaiseCollectionChanged(e);
        }

        protected void RaiseItemsRemoved(IList<SearchResultViewModel> items, int startIndex)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)items, startIndex);

            RaiseCollectionChanged(e);
        }

        protected void RaiseItemsMoved(IList<SearchResultViewModel> items, int index, int oldIndex)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, (IList)items, index, oldIndex);

            RaiseCollectionChanged(e);
        }

        protected void RaiseItemsReplaced(IList<SearchResultViewModel> newItems, IList<SearchResultViewModel> oldItems, int index)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList)oldItems, (IList)newItems, index);

            RaiseCollectionChanged(e);
        }

        #endregion

        #region Implementation

        public SearchResultViewModel GetResult(int index)
        {
            var result = Model.GetResult(index, true);

            return SearchResultViewModel.FromModel(result);
        }

        private void Model_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IList<SearchResultViewModel> newItems = null;

            if (e.NewItems != null)
            {
                newItems = SearchResultViewModel.FromCollection(e.NewItems.OfType<SearchResultModel>().ToList());
            }

            IList<SearchResultViewModel> oldItems = null;

            if (e.OldItems != null)
            {
                oldItems = SearchResultViewModel.FromCollection(e.OldItems.OfType<SearchResultModel>().ToList());
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
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

            if (SelectedResult == null)
            {
                SelectedResult = this.FirstOrDefault();
            }
        }

        #endregion
    }
}
