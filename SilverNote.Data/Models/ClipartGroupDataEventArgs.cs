using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Data.Models
{
    public class ClipartGroupDataEventArgs : EventArgs
    {
        public ClipartGroupDataModel ClipartGroup { get; set; }
    }
}
