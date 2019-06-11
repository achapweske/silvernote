/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Commands
{
    public class NShortcutCollection : List<NShortcut>
    {
        #region Constructors

        public NShortcutCollection()
        {

        }

        public NShortcutCollection(IEnumerable<NShortcut> collection)
            : base(collection)
        {

        }

        #endregion
    }
}
