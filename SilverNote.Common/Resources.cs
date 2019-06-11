/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SilverNote
{
    public static class Resources
    {
        const string CURSORS_PATH = "/SilverNote;component/Cursors/";

        public static Cursor GetCursor(string name)
        {
            if (!name.StartsWith("/"))
            {
                name = CURSORS_PATH + name;
            }

            CursorConverter converter = new CursorConverter();

            return (Cursor)converter.ConvertFrom(name);
        }
    }
}
