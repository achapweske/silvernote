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
    public static class NImageCommands
    {
        public static string GroupName
        {
            get { return "Image Commands"; }
        }

        public static readonly DynamicUICommand Open = new DynamicUICommand(
            "Open",
            "Open",
            typeof(NImageCommands)
        );

        public static readonly DynamicUICommand Edit = new DynamicUICommand(
            "Edit",
            "Edit",
            typeof(NImageCommands)
        );

        public static readonly DynamicUICommand OpenWith = new DynamicUICommand(
            "Open with",
            "OpenWith",
            typeof(NImageCommands)
        );

        public static readonly DynamicUICommand ResetSize = new DynamicUICommand(
            "Reset size",
            "ResetSize",
            typeof(NImageCommands)
        );

        public static readonly DynamicUICommand TogglePreserveAspectRatio = new DynamicUICommand(
            "Preserve aspect ratio",
            "TogglePreserveAspectRatio",
            typeof(NImageCommands)
        );
    }
}
