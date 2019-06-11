/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Commands
{
    public static class NFileCommands
    {
        public static string GroupName
        {
            get { return "File Commands"; }
        }

        public static readonly DynamicUICommand Open = new DynamicUICommand(
            "Open",
            "Open",
            typeof(NFileCommands)
        );
    }
}
