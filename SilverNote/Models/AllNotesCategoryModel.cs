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

namespace SilverNote.Models
{
    public class AllNotesCategoryModel : CategoryModel
    {
        public AllNotesCategoryModel(RepositoryModel repository, NotebookModel notebook)
            : base(repository, notebook, 0, true)
        {
            SetName("All Notes", false);
            SetParent(null, false);
            AddChildren(new CategoryModel[0], false);

            Search.Results.CollectionChanged += SearchResults_CollectionChanged;
        }

        private void SearchResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }
    }
}
