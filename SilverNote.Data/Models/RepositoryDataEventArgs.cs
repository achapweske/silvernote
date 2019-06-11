using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Data.Models
{
    public class RepositoryDataEventArgs : EventArgs
    {
        public RepositoryDataModel Repository { get; set; }
    }
}
