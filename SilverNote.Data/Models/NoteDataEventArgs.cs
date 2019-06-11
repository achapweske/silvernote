using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Data.Models
{
    public class NoteDataEventArgs : EventArgs
    {
        public NoteDataModel Note { get; set; }
    }
}
