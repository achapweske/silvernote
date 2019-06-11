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
    public static class NTableCommands
    {
        public static string GroupName
        {
            get { return "Table Commands"; }
        }

        public static readonly DynamicUICommand InsertRowAbove = new DynamicUICommand(
            "Insert row above",
            "InsertRowAbove",
            typeof(NTableCommands)
        );

        public static readonly DynamicUICommand InsertRowBelow = new DynamicUICommand(
            "Insert row below",
            "InsertRowBelow",
            typeof(NTableCommands)
        );

        public static readonly DynamicUICommand InsertColumnLeft = new DynamicUICommand(
            "Insert column left",
            "InsertColumnLeft",
            typeof(NTableCommands)
        );

        public static readonly DynamicUICommand InsertColumnRight = new DynamicUICommand(
            "Insert column right",
            "InsertColumnRight",
            typeof(NTableCommands)
        );

        public static readonly DynamicUICommand DeleteRow = new DynamicUICommand(
            "Delete row",
            "DeleteRow",
            typeof(NTableCommands)
        );

        public static readonly DynamicUICommand DeleteColumn = new DynamicUICommand(
            "Delete column",
            "DeleteColumn",
            typeof(NTableCommands)
        );

        public static readonly DynamicUICommand DeleteTable = new DynamicUICommand(
            "Delete table",
            "DeleteTable",
            typeof(NTableCommands)
        );

        public static readonly DynamicUICommand SelectRow = new DynamicUICommand(
            "Select row",
            "SelectRow",
            typeof(NTableCommands),
            new KeyGesture(Key.Space, ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectColumn = new DynamicUICommand(
            "Select column",
            "SelectColumn",
            typeof(NTableCommands),
            new KeyGesture(Key.Space, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectTable = new DynamicUICommand(
            "Select table",
            "SelectTable",
            typeof(NTableCommands)
        );

        public static readonly DynamicUICommand MergeCells = new DynamicUICommand(
            "Merge cells",
            "MergeCells",
            typeof(NTableCommands)
        );

        public static readonly DynamicUICommand SplitCells = new DynamicUICommand(
            "Split cells",
            "SplitCells",
            typeof(NTableCommands)
        );

        public static readonly DynamicUICommand SetBorderBrush = new DynamicUICommand(
            "Set border brush",
            "SetBorderBrush",
            typeof(NTableCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand SetBorderWidth = new DynamicUICommand(
            "Set border width",
            "SetBorderWidth",
            typeof(NTableCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand SetBackground = new DynamicUICommand(
            "Set background",
            "SetBackground",
            typeof(NTableCommands),
            isConfigurable: false
        );
    }
}
