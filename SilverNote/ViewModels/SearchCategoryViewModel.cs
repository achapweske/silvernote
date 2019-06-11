/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SilverNote.Common;
using SilverNote.Models;

namespace SilverNote.ViewModels
{
    public class SearchCategoryViewModel : ViewModelBase<SearchCategoryModel, SearchCategoryViewModel>
    {
        #region Properties

        public SearchViewModel Search
        {
            get { return SearchViewModel.FromModel(Model.Search); }
        }

        public string Operation
        {
            get { return Model.Operation; }
            set { Model.Operation = value; }
        }

        public CategoryViewModel Category
        {
            get { return CategoryViewModel.FromModel(Model.Category); }
            set { Model.Category = value.Model; }
        }

        private ICommand _SetOperationCommand = null;

        public ICommand SetOperationCommand
        {
            get
            {
                if (_SetOperationCommand == null)
                {
                    _SetOperationCommand = new DelegateCommand(o => Operation = o as string);
                }
                return _SetOperationCommand;
            }
        }

        #endregion
    }
}
