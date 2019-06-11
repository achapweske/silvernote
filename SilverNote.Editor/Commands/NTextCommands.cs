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
    public static class NTextCommands
    {
        public static string GroupName
        {
            get { return "Text Commands"; }
        }

        public static readonly DynamicUICommand InsertParagraphBefore = new DynamicUICommand(
            "Insert paragraph above",
            "InsertParagraphBefore",
            typeof(NTextCommands),
            new KeyGesture(Key.Enter, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand InsertParagraphAfter = new DynamicUICommand(
            "Insert paragraph below",
            "InsertParagraphAfter",
            typeof(NTextCommands),
            new KeyGesture(Key.Enter, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand MoveParagraphUp = new DynamicUICommand(
            "Move paragraph up",
            "MoveParagraphUp",
            typeof(NTextCommands),
            new KeyGesture(Key.Up, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand MoveParagraphDown = new DynamicUICommand(
            "Move paragraph down",
            "MoveParagraphDown",
            typeof(NTextCommands),
            new KeyGesture(Key.Down, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectParagraph = new DynamicUICommand(
            "Select paragraph",
            "SelectParagraph",
            typeof(NTextCommands),
            new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand DuplicateParagraph = new DynamicUICommand(
            "Duplicate paragraph",
            "DuplicateParagraph",
            typeof(NTextCommands),
            new KeyGesture(Key.D, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand DeleteForwardByWord = new DynamicUICommand(
            "Delete next word",
            "DeleteForwardByWord",
            typeof(NTextCommands),
            new KeyGesture(Key.Delete, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand DeleteForwardByParagraph = new DynamicUICommand(
            "Delete next paragraph",
            "DeleteForwardByParagraph",
            typeof(NTextCommands)
        );

        public static readonly DynamicUICommand DeleteBackByWord = new DynamicUICommand(
            "Delete previous word",
            "DeleteBackByWord",
            typeof(NTextCommands),
            new KeyGesture(Key.Back, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand DeleteBackByParagraph = new DynamicUICommand(
            "Delete previous paragraph",
            "DeleteBackByParagraph",
            typeof(NTextCommands)
        );

        public static readonly DynamicUICommand DeleteParagraph = new DynamicUICommand(
            "Delete paragraph",
            "DeleteParagraph",
            typeof(NTextCommands),
            new KeyGesture(Key.K, ModifierKeys.Control | ModifierKeys.Shift)
        );
    }
}
