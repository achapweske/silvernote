using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Models
{
    public class UncategorizedCategoryModel : CategoryModel
    {
        new public const Int64 ID = 1;

        public UncategorizedCategoryModel(RepositoryModel repository, NotebookModel notebook)
            : base(repository, notebook, ID, true)
        {
            SetName("Uncategorized", false);
            SetParent(null, false);
            AddChildren(new CategoryModel[0], false);
        }
    }
}
