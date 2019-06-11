/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Models
{
    public class SearchCategoryModel : ModelBase
    {
        public SearchCategoryModel(SearchModel search)
        {
            _Search = search;
        }

        private SearchModel _Search;

        public SearchModel Search
        {
            get { return _Search; }
        }

        private string _Operation;

        public string Operation
        {
            get { return _Operation; }
            set
            {
                _Operation = value;
                RaisePropertyChanged("Operation");
            }
        }

        private CategoryModel _Category;

        public CategoryModel Category
        {
            get { return _Category; }
            set
            {
                _Category = value;
                RaisePropertyChanged("Category");
            }
        }
    }
}
