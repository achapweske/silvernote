/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using SilverNote.Data.Models;
using SilverNote.Common;
using SilverNote.Client;

namespace SilverNote.Models
{
    public class SearchModel : ModelBase
    {
        #region Fields
        
        readonly RepositoryModel _Repository;
        readonly NotebookModel _Notebook;
        CategoryModel _Category;
        List<SearchCategoryModel> _AdditionalCategories;
        string _SearchString;
        string _CreatedTimeFilter;
        string _ModifiedTimeFilter;
        string _ViewedTimeFilter;
        SearchSort _SortBy;
        SearchOrder _Order;
        bool _ReturnText;
        bool _IsSearching;
        SearchResultsModel _Results;
        
        #endregion

        #region Constructors

        public SearchModel(RepositoryModel repository, NotebookModel notebook, CategoryModel category)
        {
            _Repository = repository;
            _Notebook = notebook;
            _Category = category;
            _AdditionalCategories = new List<SearchCategoryModel>();
            _CreatedTimeFilter = "Any time";
            _ModifiedTimeFilter = "Any time";
            _ViewedTimeFilter = "Any time";
            _ReturnText = true;
            _Results = new SearchResultsModel(repository, this);
        }

        #endregion

        #region Properties

        public ClientManager Source
        {
            get { return Notebook.Source; }
        }

        public RepositoryModel Repository
        {
            get { return _Repository; }
        }

        public NotebookModel Notebook
        {
            get { return _Notebook; }
        }

        public CategoryModel Category
        {
            get 
            { 
                return _Category; 
            }
            set
            {
                if (value != _Category)
                {
                    _Category = value;
                    RaisePropertyChanged("Category");
                    Results.Invalidate();
                }
            }
        }

        public IList<SearchCategoryModel> AdditionalCategories
        {
            get { return _AdditionalCategories; }
        }

        public IEnumerable<CategoryModel> IncludedCategories
        {
            get
            {
                if (Category != null)
                {
                    yield return this.Category;
                }

                foreach (var category in AdditionalCategories)
                {
                    if (category.Operation.ToLower() == "and" || category.Operation.ToLower() == "or")
                    {
                        yield return category.Category;
                    }
                }

                yield break;
            }
        }

        public string Query
        {
            get
            {
                string text = String.Empty;

                if (Category != null && Category.ID != 0)
                {
                    text = String.Format("categoryID:{0}", Category.ID);
                }

                foreach (var category in AdditionalCategories)
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        text = String.Format("({0}) ", text);
                    }

                    text += String.Format("{0} categoryID:{1}", category.Operation, category.Category.ID);
                }

                if (!string.IsNullOrWhiteSpace(SearchString))
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        text = String.Format("({0}) ", text);
                    }

                    text += SearchString;
                }
                return text;
            }
        }

        public string SearchString
        {
            get 
            { 
                return _SearchString; 
            }
            set
            {
                if (value != _SearchString)
                {
                    _SearchString = value;
                    RaisePropertyChanged("SearchString");
                    Results.Invalidate();
                }
            }
        }

        public string SearchPattern
        {
            get 
            {
                if (!String.IsNullOrEmpty(SearchString))
                {
                    return GetRegexPattern(SearchString);
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        public RegexOptions SearchOptions
        {
            get 
            {
                if (!String.IsNullOrEmpty(SearchString))
                {
                    return GetRegexOptions(SearchString);
                }
                else
                {
                    return RegexOptions.None;
                }
            }
        }

        public string CreatedTimeFilter
        {
            get { return _CreatedTimeFilter; }
            set
            {
                if (value != _CreatedTimeFilter)
                {
                    _CreatedTimeFilter = value;
                    RaisePropertyChanged("CreatedTimeFilter");
                    Results.Invalidate();
                }
            }
        }

        public Tuple<DateTime, DateTime> CreatedTimeRange
        {
            get
            {
                DateTime start, end;
                SearchModel.ParseDateTimeRange(CreatedTimeFilter, out start, out end);
                return Tuple.Create<DateTime, DateTime>(start, end);
            }
        }

        public string ModifiedTimeFilter
        {
            get { return _ModifiedTimeFilter; }
            set
            {
                if (value != _ModifiedTimeFilter)
                {
                    _ModifiedTimeFilter = value;
                    RaisePropertyChanged("ModifiedTimeFilter");
                    Results.Invalidate();
                }
            }
        }

        public Tuple<DateTime, DateTime> ModifiedTimeRange
        {
            get
            {
                DateTime start, end;
                SearchModel.ParseDateTimeRange(ModifiedTimeFilter, out start, out end);
                return Tuple.Create<DateTime, System.DateTime>(start, end);
            }
        }

        public string ViewedTimeFilter
        {
            get { return _ViewedTimeFilter; }
            set
            {
                if (value != _ViewedTimeFilter)
                {
                    _ViewedTimeFilter = value;
                    RaisePropertyChanged("ViewedTimeFilter");
                    Results.Invalidate();
                }
            }
        }

        public Tuple<DateTime, DateTime> ViewedTimeRange
        {
            get
            {
                DateTime start, end;
                SearchModel.ParseDateTimeRange(ViewedTimeFilter, out start, out end);
                return Tuple.Create<DateTime, DateTime>(start, end);
            }
        }

        public SearchSort SortBy
        {
            get { return _SortBy; }
            set
            {
                if (value != _SortBy)
                {
                    _SortBy = value;
                    RaisePropertyChanged("SortBy");
                    Results.Invalidate();
                }
            }
        }

        public SearchOrder Order
        {
            get { return _Order; }
            set
            {
                if (value != _Order)
                {
                    _Order = value;
                    RaisePropertyChanged("Order");
                    Results.Invalidate();
                }
            }
        }

        public bool ReturnText
        {
            get
            {
                return _ReturnText;
            }
            set
            {
                if (value != _ReturnText)
                {
                    _ReturnText = value;
                    RaisePropertyChanged("ReturnText");
                    Results.Invalidate();
                }
            }
        }

        public bool IsSearching
        {
            get
            {
                return _IsSearching;
            }
            protected set
            {
                if (value != _IsSearching)
                {
                    _IsSearching = value;
                    RaisePropertyChanged("IsSearching");
                }
            }
        }

        public SearchResultsModel Results
        {
            get { return _Results; }
        }

        #endregion

        #region Operations

        public SearchCategoryModel AddCategory(string operation, CategoryModel category)
        {
            var searchCategory = new SearchCategoryModel(this)
            {
                Operation = operation,
                Category = category
            };

            _AdditionalCategories.Add(searchCategory);
            searchCategory.PropertyChanged += AdditionalCategory_PropertyChanged;
            RaisePropertyChanged("AdditionalCategories");
            Results.Invalidate();

            return searchCategory;
        }

        public void RemoveCategory(SearchCategoryModel category)
        {
            if (category != null && _AdditionalCategories.Contains(category))
            {
                _AdditionalCategories.Remove(category);
                category.PropertyChanged -= AdditionalCategory_PropertyChanged;
                RaisePropertyChanged("AdditionalCategories");
                Results.Invalidate();
            }
        }

        void AdditionalCategory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Results.Invalidate();
        }

        private HashSet<IAsyncResult> _PendingSearches = new HashSet<IAsyncResult>();

        public void Search(int offset = 0, int limit = -1)
        {
            IsSearching = true;
            Messenger.Instance.Notify("Flush", Notebook);

            var search = Source.Client.BeginFindNotes(
                Notebook.ID, 
                Query, 
                CreatedTimeRange.Item1, CreatedTimeRange.Item2, 
                ModifiedTimeRange.Item1, ModifiedTimeRange.Item2, 
                ViewedTimeRange.Item1, ViewedTimeRange.Item2, 
                (NoteClient.NoteSort)SortBy, 
                (NoteClient.NoteOrder)Order, 
                offset, limit, ReturnText, 
                Search_Completed, null);

            _PendingSearches.Add(search);
        }

        protected void Search_Completed(IAsyncResult result)
        {
            _PendingSearches.Remove(result);
            IsSearching = _PendingSearches.Count > 0;

            SearchResultsDataModel resultsData;
            try
            {
                resultsData = Source.Client.EndFindNotes(result);
            }
            catch (NoteClientException)
            {
                Source.Client.Resume();
                return;
            }

            UpdateResults(resultsData);
            
            if (resultsData.SearchString == Query)
            {
                var categories = (
                    from c in IncludedCategories
                    where !Notebook.SpecialCategories.Contains(c)
                    select c).ToArray();

                if (categories.Length > 0)
                {
                    foreach (var resultData in resultsData.Results)
                    {
                        var note = Notebook.GetNote(resultData.Note.ID, true);

                        foreach (var category in categories)
                        {
                            note.AddToCategory(category, false);
                        }
                    }
                }
            }
        }
		
        static void ParseDateTimeRange(string str, out DateTime first, out DateTime second)
        {
            switch (str.ToLower())
            {
                case "today":
                    first = NoteClient.Now.ToLocalTime().Date.ToUniversalTime();
                    second = DateTime.MaxValue;
                    break;
                case "yesterday":
                    second = NoteClient.Now.ToLocalTime().Date.ToUniversalTime();
                    first = second.AddDays(-1.0);
                    break;
                case "past week":
                    first = NoteClient.Now.ToLocalTime().Date.AddDays(-7.0).ToUniversalTime();
                    second = DateTime.MaxValue;
                    break;
                case "past month":
                    first = NoteClient.Now.ToLocalTime().Date.AddMonths(-1).ToUniversalTime();
                    second = DateTime.MaxValue;
                    break;
                case "past year":
                    first = NoteClient.Now.ToLocalTime().Date.AddYears(-1).ToUniversalTime();
                    second = DateTime.MaxValue;
                    break;
                default:
                    first = default(DateTime);
                    second = default(DateTime);
                    break;
            }
        }

        #endregion

        #region Data Model

        public void UpdateResults(SearchResultsDataModel newResults)
        {
            Results.Update(newResults);
        }

        #endregion

        #region Implementation

        private static string GetRegexPattern(string str)
        {
            return @"\b" + Regex.Escape(str);
        }

        private static RegexOptions GetRegexOptions(string str)
        {
            if (str == str.ToLower())
            {
                return RegexOptions.IgnoreCase;
            }
            else
            {
                return RegexOptions.None;
            }
        }

        #endregion
    }

    public enum SearchOrder
    {
        Descending,
        Ascending
    }

    public enum SearchSort
    {
        ViewedAt,
        CreatedAt,
        ModifiedAt,
        Title
    }
}
