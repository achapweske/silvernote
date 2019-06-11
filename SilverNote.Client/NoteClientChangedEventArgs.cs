/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Client
{
    public class NoteClientChangedEventArgs : EventArgs
    {
        private readonly NoteClient _OldValue;

        public NoteClientChangedEventArgs(NoteClient oldValue)
        {
            _OldValue = oldValue;
        }

        public NoteClient OldValue
        {
            get { return _OldValue; }
        }
    }
}
