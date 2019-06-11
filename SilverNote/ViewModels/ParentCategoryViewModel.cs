/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.ViewModels
{
    public class ParentCategoryViewModel : CategoryViewModel
    {
        #region Fields

        CategoryViewModel _ViewModel;

        #endregion

        #region Constructors

        public ParentCategoryViewModel(CategoryViewModel viewModel)
        {
            _ViewModel = viewModel;
            Initialize(viewModel.Model);
        }

        #endregion

        #region Properties

        public override IList<CategoryViewModel> Children
        {
            get { return new CategoryViewModel[0]; }
        }

        public override string Name
        {
            get
            {
                if (base.Name != null)
                {
                    return String.Format("Parent ({0})", base.Name);
                }
                else
                {
                    return "Parent";
                }
            }
            set
            {
                base.Name = value;
            }
        }

        #endregion

        #region Operations

        #endregion
    }
}
