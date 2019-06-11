using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Data.Models
{
    public class ClipartDataEventArgs : EventArgs
    {
        public ClipartDataModel Clipart { get; set; }
    }
}
