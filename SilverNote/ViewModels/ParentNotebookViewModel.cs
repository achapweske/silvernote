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
    public class ParentNotebookViewModel : NotebookViewModel
    {
        #region Fields

        NotebookViewModel _ViewModel;

        #endregion

        #region Constructors

        public ParentNotebookViewModel(NotebookViewModel viewModel)
        {
            _ViewModel = viewModel;
            Initialize(viewModel.Model);
        }

        #endregion

        #region Properties

        public override IList<CategoryViewModel> Categories
        {
            get { return new CategoryViewModel[0]; }
        }

        public override string Name
        {
            get
            {
                if (base.Name != null)
                {
                    return String.Format("Notebook ({0})", base.Name);
                }
                else
                {
                    return "Notebook";
                }
            }
            set
            {
                base.Name = value;
            }
        }

        #endregion
    }
}
