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
using System.Windows.Input;

namespace SilverNote.Commands
{
    public static class NDevelopmentCommands
    {
        public static readonly DynamicUICommand Debug = new DynamicUICommand(
            "Debug",
            "Debug",
            typeof(NDevelopmentCommands),
            new KeyGesture(Key.D, ModifierKeys.Control | ModifierKeys.Shift),
            isConfigurable: false
        );

        public static readonly DynamicUICommand EditSource = new DynamicUICommand(
            "Edit source",
            "EditSource",
            typeof(NDevelopmentCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand ViewRequests = new DynamicUICommand(
            "View requests",
            "ViewRequests",
            typeof(NDevelopmentCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand OpenBrowser = new DynamicUICommand(
            "Open browser",
            "OpenBrowser",
            typeof(NDevelopmentCommands),
            isConfigurable: false
        );
    }
}
