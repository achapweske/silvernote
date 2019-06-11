using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SilverNote.Data.Models
{
    [XmlType("search_results")]
    public class SearchResultsDataModel : DataModelBase
    {
        public SearchResultsDataModel()
        {
            Total = -1;
        }

        [XmlElement("notebook_id")]
        public Int64 NotebookID { get; set; }

        [XmlElement("search_string")]
        public string SearchString { get; set; }

        [XmlArray("results")]
        public SearchResultDataModel[] Results { get; set; }

        [XmlElement("offset")]
        public int Offset { get; set; }

        [XmlElement("total")]
        public int Total { get; set; }
    }
}
