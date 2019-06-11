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
    public static class NViewCommands
    {
        public static string GroupName
        {
            get { return "View Commands"; }
        }

        public static readonly DynamicUICommand NewTab = new DynamicUICommand(
            "New tab",
            "NewTab",
            typeof(NViewCommands),
            new KeyGesture(Key.T, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand FloatTab = new DynamicUICommand(
            "Float tab",
            "FloatTab",
            typeof(NViewCommands)
        );

        public static readonly DynamicUICommand CloseTab = new DynamicUICommand(
            "Close tab",
            "CloseTab",
            typeof(NViewCommands),
            new KeyGesture(Key.W, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand CloseOtherTabs = new DynamicUICommand(
            "Close other tabs",
            "CloseOtherTabs",
            typeof(NViewCommands)
        );

        public static readonly DynamicUICommand ToggleLeftPane = new DynamicUICommand(
            "Show/hide left pane",
            "ToggleLeftPane",
            typeof(NViewCommands),
            new KeyGesture(Key.F1)
        );

        public static readonly DynamicUICommand ToggleRightPane = new DynamicUICommand(
            "Show/hide right pane",
            "ToggleRightPane",
            typeof(NViewCommands),
            new KeyGesture(Key.F2)
        );

        public static readonly DynamicUICommand SearchNotebook = new DynamicUICommand(
            "Search notebook",
            "SearchNotebook",
            typeof(NViewCommands),
            new KeyGesture(Key.Space, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand ToggleGrid = new DynamicUICommand(
            "Show/hide grid",
            "ToggleGrid",
            typeof(NViewCommands),
            new KeyGesture(Key.OemQuotes, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand ToggleGuidelines = new DynamicUICommand(
            "Show/hide guidelines",
            "ToggleGuidelines",
            typeof(NViewCommands),
            new KeyGesture(Key.OemSemicolon, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand ZoomIn = new DynamicUICommand(
            "Zoom in",
            "ZoomIn",
            typeof(NViewCommands)
        );

        public static readonly DynamicUICommand ZoomOut = new DynamicUICommand(
            "Zoom out",
            "ZoomOut",
            typeof(NViewCommands)
        );
    }
}
