using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Data.Models
{
    public class SearchResultsDataEventArgs : EventArgs
    {
        public SearchResultsDataModel SearchResults { get; set; }
    }
}
