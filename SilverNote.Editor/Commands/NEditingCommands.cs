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
    public static class NEditingCommands
    {
        public static string GroupName
        {
            get { return "Editing Commands"; }
        }

        public static readonly DynamicUICommand Undo = new DynamicUICommand(
            "Undo",
            "Undo",
            typeof(NEditingCommands),
            new KeyGesture(Key.Z, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand Redo = new DynamicUICommand(
            "Redo",
            "Redo",
            typeof(NEditingCommands),
            new KeyGesture(Key.Y, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand Cut = new DynamicUICommand(
            "Cut",
            "Cut",
            typeof(NEditingCommands),
            new KeyGesture(Key.X, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand Copy = new DynamicUICommand(
            "Copy",
            "Copy",
            typeof(NEditingCommands),
            new KeyGesture(Key.C, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand Paste = new DynamicUICommand(
            "Paste",
            "Paste",
            typeof(NEditingCommands),
            new KeyGesture(Key.V, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand PasteSpecial = new DynamicUICommand(
            "Paste Special",
            "PasteSpecial",
            typeof(NEditingCommands),
            isConfigurable: false);

        public static readonly DynamicUICommand DeleteForward = new DynamicUICommand(
            "Delete forward",
            "DeleteForward",
            typeof(NEditingCommands),
            new KeyGesture(Key.Delete)
        );

        public static readonly DynamicUICommand DeleteBack = new DynamicUICommand(
            "Delete back",
            "DeleteBack",
            typeof(NEditingCommands),
            new KeyGesture(Key.Back)
        );

        public static readonly DynamicUICommand SelectAll = new DynamicUICommand(
            "Select all",
            "SelectAll",
            typeof(NEditingCommands),
            new KeyGesture(Key.A, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand Find = new DynamicUICommand(
            "Find",
            "Find",
            typeof(NEditingCommands),
            new KeyGesture(Key.F, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand FindNext = new DynamicUICommand(
            "Find next",
            "FindNext",
            typeof(NEditingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand FindPrevious = new DynamicUICommand(
            "Find previous",
            "FindPrevious",
            typeof(NEditingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand Replace = new DynamicUICommand(
            "Replace",
            "Replace",
            typeof(NEditingCommands),
            new KeyGesture(Key.H, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand ReplaceOnce = new DynamicUICommand(
            "Replace once",
            "ReplaceOnce",
            typeof(NEditingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand ReplaceAll = new DynamicUICommand(
            "Replace all",
            "ReplaceAll",
            typeof(NEditingCommands),
            isConfigurable: false
        );
    }

}
