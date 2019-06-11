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
using System.Windows.Input;
using SilverNote.Common;
using SilverNote.Models;
using System.ComponentModel;

namespace SilverNote.ViewModels
{
    public class SearchViewModel : ViewModelBase<SearchModel, SearchViewModel>
    {
        #region Initialization

        protected override void OnInitialize()
        {
            Model.WhenPropertyChanged("SortBy", SortBy_Changed);
        }

        #endregion

        #region Properties

        public RepositoryViewModel Repository
        {
            get { return RepositoryViewModel.FromModel(Model.Repository); }
        }

        public NotebookViewModel Notebook
        {
            get { return NotebookViewModel.FromModel(Model.Notebook); }
        }

        public CategoryViewModel Category
        {
            get { return CategoryViewModel.FromModel(Model.Category); }
            set { Model.Category = value.Model; }
        }

        public IList<SearchCategoryViewModel> AdditionalCategories
        {
            get { return SearchCategoryViewModel.FromCollection(Model.AdditionalCategories); }
        }

        public string SearchString
        {
            get { return Model.SearchString; }
            set { Model.SearchString = value; }
        }

        public string SearchPattern
        {
            get { return Model.SearchPattern; }
        }

        public RegexOptions SearchOptions
        {
            get { return Model.SearchOptions; }
        }

        public string CreatedTimeFilter
        {
            get { return Model.CreatedTimeFilter; }
            set { Model.CreatedTimeFilter = value; }
        }

        public string ModifiedTimeFilter
        {
            get { return Model.ModifiedTimeFilter; }
            set { Model.ModifiedTimeFilter = value; }
        }

        public string ViewedTimeFilter
        {
            get { return Model.ViewedTimeFilter; }
            set { Model.ViewedTimeFilter = value; }
        }

        private const string SORTBY_TITLE = "Title";
        private const string SORTBY_CREATED = "Created time";
        private const string SORTBY_MODIFIED = "Last edited";
        private const string SORTBY_VIEWED = "Last viewed";

        private static readonly string[] _SortByOptions = new string[]
		{
			SORTBY_TITLE,
			SORTBY_CREATED,
			SORTBY_MODIFIED,
			SORTBY_VIEWED
		};

        public string[] SortByOptions
        {
            get
            {
                return _SortByOptions;
            }
        }

        public string SortBy
        {
            get
            {
                switch (base.Model.SortBy)
                {
                    case SearchSort.ViewedAt:
                        return SORTBY_VIEWED;
                    case SearchSort.CreatedAt:
                        return SORTBY_CREATED;
                    case SearchSort.ModifiedAt:
                        return SORTBY_MODIFIED;
                    case SearchSort.Title:
                        return SORTBY_TITLE;
                    default:
                        return null;
                }
            }
            set
            {
                SearchSort oldValue = Model.SortBy;
                switch (value)
                {
                    case SORTBY_VIEWED:
                        Model.SortBy = SearchSort.ViewedAt;
                        break;
                    case SORTBY_CREATED:
                        Model.SortBy = SearchSort.CreatedAt;
                        break;
                    case SORTBY_MODIFIED:
                        Model.SortBy = SearchSort.ModifiedAt;
                        break;
                    case SORTBY_TITLE:
                        Model.SortBy = SearchSort.Title;
                        break;
                    default:
                        break;
                }

                if (oldValue == SearchSort.Title && Model.SortBy != SearchSort.Title ||
                    oldValue != SearchSort.Title && Model.SortBy == SearchSort.Title)
                {
                    ToggleOrder();
                }
            }
        }

        private const string ORDER_ALPHA_ASC = "Alphabetic";
        private const string ORDER_ALPHA_DESC = "Reverse";
        private const string ORDER_DATETIME_ASC = "Most recent last";
        private const string ORDER_DATETIME_DESC = "Most recent first";

        private static readonly string[] _OrderAlphaOptions = new string[]
		{
			ORDER_ALPHA_ASC,
			ORDER_ALPHA_DESC
		};

        private static readonly string[] _OrderDateTimeOptions = new string[]
		{
			ORDER_DATETIME_ASC,
			ORDER_DATETIME_DESC
		};

        public string[] OrderOptions
        {
            get
            {
                switch (Model.SortBy)
                {
                    case SearchSort.ViewedAt:
                    case SearchSort.CreatedAt:
                    case SearchSort.ModifiedAt:
                        return _OrderDateTimeOptions;
                    case SearchSort.Title:
                        return _OrderAlphaOptions;
                    default:
                        return null;
                }
            }
        }

        public string Order
        {
            get
            {
                if (Model.SortBy == SearchSort.Title)
                {
                    switch (Model.Order)
                    {
                        case SearchOrder.Descending:
                            return ORDER_ALPHA_DESC;
                        case SearchOrder.Ascending:
                            return ORDER_ALPHA_ASC;
                        default:
                            return null;
                    }
                }
                else
                {
                    switch (base.Model.Order)
                    {
                        case SearchOrder.Descending:
                            return ORDER_DATETIME_DESC;
                        case SearchOrder.Ascending:
                            return ORDER_DATETIME_ASC;
                        default:
                            return null;
                    }
                }
            }
            set
            {
                if (value != null)
                {
                    if (value == ORDER_ALPHA_ASC || value == ORDER_DATETIME_ASC)
                    {
                        Model.Order = SearchOrder.Ascending;
                        return;
                    }
                    if (value == ORDER_ALPHA_DESC || value == ORDER_DATETIME_DESC)
                    {
                        Model.Order = SearchOrder.Descending;
                        return;
                    }
                }
            }
        }

        public SearchResultsViewModel Results
        {
            get { return SearchResultsViewModel.FromModel(Model.Results); }
        }

        #endregion

        #region Operations

        public void AddCategory(string operation, CategoryViewModel category)
        {
            Model.AddCategory(operation, category.Model);

            Notebook.IsSearching = true;
        }

        public void RemoveCategory(SearchCategoryViewModel category)
        {
            Model.RemoveCategory(category.Model);

            Notebook.IsSearching = true;
        }

        public void ToggleOrder()
        {
            if (Model.Order == SearchOrder.Ascending)
            {
                Model.Order = SearchOrder.Descending;
            }
            else
            {
                Model.Order = SearchOrder.Ascending;
            }
        }

        #endregion

        #region Commands

        private ICommand _AndCategoryCommand = null;

        public ICommand AndCategoryCommand
        {
            get
            {
                if (_AndCategoryCommand == null)
                {
                    _AndCategoryCommand = new DelegateCommand(o => AddCategory("AND", o as CategoryViewModel));
                }
                return _AndCategoryCommand;
            }
        }

        private ICommand _OrCategoryCommand = null;

        public ICommand OrCategoryCommand
        {
            get
            {
                if (_OrCategoryCommand == null)
                {
                    _OrCategoryCommand = new DelegateCommand(o => AddCategory("OR", o as CategoryViewModel));
                }
                return _OrCategoryCommand;
            }
        }

        private ICommand _ExceptCategoryCommand = null;

        public ICommand ExceptCategoryCommand
        {
            get
            {
                if (_ExceptCategoryCommand == null)
                {
                    _ExceptCategoryCommand = new DelegateCommand(o => AddCategory("EXCEPT", o as CategoryViewModel));
                }
                return _ExceptCategoryCommand;
            }
        }

        private ICommand _RemoveCategoryCommand = null;

        public ICommand RemoveCategoryCommand
        {
            get
            {
                if (_RemoveCategoryCommand == null)
                {
                    _RemoveCategoryCommand = new DelegateCommand(o => RemoveCategory(o as SearchCategoryViewModel));
                }
                return _RemoveCategoryCommand;
            }
        }

        #endregion

        #region Implementation

        private void SortBy_Changed(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("OrderOptions");
            RaisePropertyChanged("Order");
        }

        #endregion
    }
}
