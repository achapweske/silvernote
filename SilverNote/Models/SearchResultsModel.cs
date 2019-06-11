/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SilverNote.Data.Models;
using System.Windows;
using System.Windows.Threading;

namespace SilverNote.Models
{
    public class SearchResultsModel : ModelBase, INotifyCollectionChanged
    { 
        #region Fields

        RepositoryModel _Repository;
        SearchModel _Search;
        bool _IsInvalidatePending;
        bool _NeedSearch = true;
		
        #endregion

        #region Constructors

        public SearchResultsModel(RepositoryModel repository, SearchModel search)
        {
            _Repository = repository;
            _Search = search;
        }

        #endregion

        #region Properties

        public RepositoryModel Repository
        {
            get { return _Repository; }
        }

        public SearchModel Search
        {
            get { return _Search; }
        }

        public int Count
        {
            get { return GetResultCount(true); }
        }

        public SearchResultModel this[int index]
        {
            get
            {
                return GetResult(index, true);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public IEnumerable<SearchResultModel> ReferencedResults
        {
            get { return _Results.Values; }
        }

        #endregion

        #region Operations

        public int IndexOf(SearchResultModel item)
        {
            foreach (var result in _Results)
            {
                if (result.Value == item)
                {
                    return result.Key;
                }
            }
            return -1;
        }

        public void Remove(NoteModel oldNote)
        {
            var result = _Results.FirstOrDefault(r => r.Value.Note == oldNote);
            if (result.Value != null)
            {
                RemoveAt(result.Key);
            }
            else
            {
                Invalidate();
            }
        }
		
        public void Insert(int index, SearchResultModel newResult)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            if (Search.IsSearching)
            {
                Invalidate();
            }

            var newResults = new Dictionary<int, SearchResultModel>();

            foreach (var result in _Results)
            {
                if (result.Key < index)
                {
                    newResults.Add(result.Key, result.Value);
                }
                else
                {
                    if (result.Key >= index)
                    {
                        newResults.Add(result.Key + 1, result.Value);
                    }
                }
            }
            
            newResults.Add(index, newResult);
            _Results = newResults;
            _Count++;

            RaisePropertyChanged("Count");
            RaisePropertyChanged("Item[]");
            RaiseItemAdded(newResult, index);
        }

        public void RemoveAt(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException("Index cannot be negative");
            }
            else if (index >= _Count)
            {
                throw new IndexOutOfRangeException("Index must be less than collection size");
            }

            if (Search.IsSearching)
            {
                Invalidate();
            }

            SearchResultModel item = null;

            // Update index values for all elements in _Results

            var newResults = new Dictionary<int, SearchResultModel>();
            foreach (var result in _Results)
            {
                if (result.Key < index)
                {
                    newResults.Add(result.Key, result.Value);
                }
                else if (result.Key > index)
                {
                    newResults.Add(result.Key - 1, result.Value);
                }
                else /* result.Key == index */
                {
                    item = result.Value;
                }
            }

            _Results = newResults;
            _Count--;

            RaisePropertyChanged("Count");
            RaisePropertyChanged("Item[]");
            RaiseItemRemoved(item, index);
        }

        /// <summary>
        /// Invalidate the current set of search results.
        /// 
        /// Whenever you modify a note in a way that may affect search results
        /// you must either update the results by calling Add() or Remove(), or
        /// you must trigger a new query by calling Invalidate().
        /// 
        /// Invalidate() raises a CollectionChanged "Reset" event to 
        /// notify client code that there has been a change. The next time
        /// this collection is accessed, a new query will be initiated.
        /// </summary>
        public void Invalidate()
        {
            if (_NeedCount)
            {
                return;     // There are no search results to be invalidated
            }

            // Take care to not make multiple consecutive calls to 
            // RaiseCollectionReset() as this disables virtualization on 
            // virtualizing panels.

            _IsInvalidatePending = true;    // Optimization

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_IsInvalidatePending)
                {
                    if (!_NeedSearch)
                    {
                        // Trigger a search next time Count is read
                        _Count = 0;
                        _NeedSearch = true;
                        RaiseCollectionReset();
                        RaisePropertyChanged("Count");
                        RaisePropertyChanged("Item[]");
                    }
                    _IsInvalidatePending = false;
                }
            }));
        }

        #endregion

        #region Implementation

        #region ResultCount

        int _Count;
        bool _NeedCount = true;

        private int GetResultCount(bool sync)
        {
            if (sync && (_NeedSearch || (_NeedCount && !Search.IsSearching)))
            {
                Search.Search();
                _NeedSearch = false;
            }

            return _Count;
        }

        private void SetResultCount(int value)
        {
            _Count = value;
            _NeedCount = false;
            RaiseCollectionReset();
            RaisePropertyChanged("Count");
            RaisePropertyChanged("Item[]");
        }

        #endregion

        #region Results

        Dictionary<int, SearchResultModel> _Results = new Dictionary<int, SearchResultModel>();

        public SearchResultModel GetResult(int index, bool fetch)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException("Index cannot be negative");
            }
            else if (index >= _Count)
            {
                throw new IndexOutOfRangeException("Index must be less than collection size");
            }

            SearchResultModel result;
            if (!_Results.TryGetValue(index, out result))
            {
                result = new SearchResultModel(Search);
                _Results.Add(index, result);
                if (fetch) Fetch(index);
            }
            return result;
        }

        #endregion

        #region Fetch

        List<int> _FetchQueue = new List<int>();

        protected void Fetch(int index)
        {
            _FetchQueue.Add(index);

            var application = Application.Current;
            if (application != null)
            {
                var dispatcher = application.Dispatcher;
                if (dispatcher != null)
                {
                    application.Dispatcher.BeginInvoke(
                        new Action(OnFetch),
                        DispatcherPriority.Background
                    );
                }
                else
                {
                    OnFetch();
                }
            }
        }

        protected void OnFetch()
        {
            if (_FetchQueue.Count > 0)
            {
                _FetchQueue.Sort();
                for (int i = 0; i < _FetchQueue.Count; )
                {
                    int fetchBegin = _FetchQueue[i];
                    int fetchEnd = fetchBegin;
                    while (++i < _FetchQueue.Count && _FetchQueue[i] < fetchBegin + 100 && _FetchQueue[i] < fetchEnd + 10)
                    {
                        fetchEnd = _FetchQueue[i];
                    }
                    int offset = fetchBegin;
                    int limit = (fetchEnd - fetchBegin) + 1;
                    Search.Search(offset, limit);
                }
                _FetchQueue.Clear();
            }
        }

        #endregion

        private int ComputeIndex(string title, DateTime createdAt, DateTime modifiedAt, DateTime viewedAt)
        {
            var results = (
                from r in _Results
                orderby r.Key
                select r).AsEnumerable();

            switch (Search.SortBy)
            {
                case SearchSort.ViewedAt:
                    if (Search.Order == SearchOrder.Descending)
                    {
                        results =
                            from r in results
                            where viewedAt >= r.Value.Note.ViewedAt
                            select r;
                    }
                    else
                    {
                        results =
                            from r in results
                            where viewedAt <= r.Value.Note.ViewedAt
                            select r;
                    }
                    break;
                case SearchSort.CreatedAt:
                    if (Search.Order == SearchOrder.Descending)
                    {
                        results =
                            from r in results
                            where createdAt >= r.Value.Note.CreatedAt
                            select r;
                    }
                    else
                    {
                        results =
                            from r in results
                            where createdAt <= r.Value.Note.CreatedAt
                            select r;
                    }
                    break;
                case SearchSort.ModifiedAt:
                    if (Search.Order == SearchOrder.Descending)
                    {
                        results =
                            from r in results
                            where modifiedAt >= r.Value.Note.ModifiedAt
                            select r;
                    }
                    else
                    {
                        results =
                            from r in results
                            where modifiedAt <= r.Value.Note.ModifiedAt
                            select r;
                    }
                    break;
                case SearchSort.Title:
                    if (Search.Order == SearchOrder.Descending)
                    {
                        results =
                            from r in results
                            where String.Compare(title, r.Value.Note.Title) >= 0
                            select r;
                    }
                    else
                    {
                        results =
                            from r in results
                            where String.Compare(title, r.Value.Note.Title) <= 0
                            select r;
                    }
                    break;
            }

            var result = results.FirstOrDefault();
            if (result.Value != null)
            {
                return result.Key;
            }
            else
            {
                return _Count;
            }
        }

        #endregion

        #region Data Model

        public void Update(SearchResultsDataModel newResults)
        {
            if (newResults.Total != -1)
            {
                _Results.Clear();
                _FetchQueue.Clear();
            }

            if (newResults.Results != null)
            {
                for (int i = 0; i < newResults.Results.Length; i++)
                {
                    UpdateResult(newResults.Offset + i, newResults.Results[i]);
                }
            }

            if (newResults.Total != -1)
            {
                SetResultCount(newResults.Total);
            }
        }

        public void UpdateResult(int index, SearchResultDataModel newResult)
        {
            GetResult(index, false).Update(newResult);
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

        protected void RaiseItemAdded(SearchResultModel item, int index)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);

            RaiseCollectionChanged(e);
        }

        protected void RaiseItemRemoved(SearchResultModel item, int index)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);

            RaiseCollectionChanged(e);
        }

        #endregion
    }
}
