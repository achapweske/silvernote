using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Data.Models
{
    public class NotebookDataEventArgs : EventArgs
    {
        public NotebookDataModel Notebook { get; set; }
    }
}
