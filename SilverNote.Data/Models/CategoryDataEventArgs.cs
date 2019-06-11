using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Data.Models
{
    public class CategoryDataEventArgs : EventArgs
    {
        public CategoryDataModel Category { get; set; }
    }
}
