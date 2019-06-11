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
    public static class NUtilityCommands
    {
        public static string GroupName
        {
            get { return "Miscellaneous Commands"; }
        }

        public static readonly DynamicUICommand DrillDown = new DynamicUICommand(
            "Drill down",
            "DrillDown",
            typeof(NUtilityCommands)
        );

        public static readonly DynamicUICommand DrillThrough = new DynamicUICommand(
            "Drill through",
            "DrillThrough",
            typeof(NUtilityCommands)
        );

        public static readonly DynamicUICommand OpenHyperlink = new DynamicUICommand(
            "Open hyperlink",
            "OpenHyperlink",
            typeof(NUtilityCommands)
        );

        public static readonly DynamicUICommand Lookup = new DynamicUICommand(
            "Lookup",
            "Lookup",
            typeof(NUtilityCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand WordCount = new DynamicUICommand(
            "Word count",
            "WordCount",
            typeof(NUtilityCommands)
        );
    }
}
