using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Data.Models
{
    public class FileDataEventArgs : EventArgs
    {
        public FileDataModel File { get; set; }
    }
}
