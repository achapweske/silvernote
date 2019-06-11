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
    public static class NNavigationCommands
    {
        public static string GroupName
        {
            get { return "Navigation Commands"; }
        }

        public static readonly DynamicUICommand MoveUpByLine = new DynamicUICommand(
            "Move up",
            "MoveUpByLine",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Up)
        );

        public static readonly DynamicUICommand MoveDownByLine = new DynamicUICommand(
            "Move down",
            "MoveDownByLine",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Down)
        );

        public static readonly DynamicUICommand MoveLeftByCharacter = new DynamicUICommand(
            "Move left",
            "MoveLeftByCharacter",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Left)
        );

        public static readonly DynamicUICommand MoveRightByCharacter = new DynamicUICommand(
            "Move right",
            "MoveRightByCharacter",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Right)
        );

        public static readonly DynamicUICommand MoveLeftByWord = new DynamicUICommand(
            "Move left one word",
            "MoveLeftByWord",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Left, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand MoveRightByWord = new DynamicUICommand(
            "Move right one word",
            "MoveRightByWord",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Right, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand MoveUpByPage = new DynamicUICommand(
            "Move up one page",
            "MoveUpByPage",
            typeof(NNavigationCommands),
            new KeyGesture(Key.PageUp)
        );

        public static readonly DynamicUICommand MoveDownByPage = new DynamicUICommand(
            "Move down one page",
            "MoveDownByPage",
            typeof(NNavigationCommands),
            new KeyGesture(Key.PageDown)
        );

        public static readonly DynamicUICommand MoveToLineStart = new DynamicUICommand(
            "Move to beginning of line",
            "MoveToLineStart",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Home)
        );

        public static readonly DynamicUICommand MoveToLineEnd = new DynamicUICommand(
            "Move to end of line",
            "MoveToLineEnd",
            typeof(NNavigationCommands),
            new KeyGesture(Key.End)
        );

        public static readonly DynamicUICommand MoveToParagraphStart = new DynamicUICommand(
            "Move to beginning of paragraph",
            "MoveToParagraphStart",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Up, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand MoveToParagraphEnd = new DynamicUICommand(
            "Move to end of paragraph",
            "MoveToParagraphEnd",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Down, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand MoveToDocumentStart = new DynamicUICommand(
            "Move to beginning of document",
            "MoveToDocumentStart",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Home, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand MoveToDocumentEnd = new DynamicUICommand(
            "Move to end of document",
            "MoveToDocumentEnd",
            typeof(NNavigationCommands),
            new KeyGesture(Key.End, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand SelectUpByLine = new DynamicUICommand(
            "Select up",
            "SelectUpByLine",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Up, ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectDownByLine = new DynamicUICommand(
            "Select down",
            "SelectDownByLine",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Down, ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectLeftByCharacter = new DynamicUICommand(
            "Select left",
            "SelectLeftByCharacter",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Left, ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectRightByCharacter = new DynamicUICommand(
            "Select right",
            "SelectRightByCharacter",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Right, ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectLeftByWord = new DynamicUICommand(
            "Select left one word",
            "SelectLeftByWord",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Left, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectRightByWord = new DynamicUICommand(
            "Select right one word",
            "SelectRightByWord",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Right, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectToLineStart = new DynamicUICommand(
            "Select to beginning of line",
            "SelectToLineStart",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Home, ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectToLineEnd = new DynamicUICommand(
            "Select to end of line",
            "SelectToLineEnd",
            typeof(NNavigationCommands),
            new KeyGesture(Key.End, ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectToParagraphStart = new DynamicUICommand(
            "Select to beginning of paragraph",
            "SelectToParagraphStart",
            typeof(NNavigationCommands)
        );

        public static readonly DynamicUICommand SelectToParagraphEnd = new DynamicUICommand(
            "Select to end of paragraph",
            "SelectToParagraphEnd",
            typeof(NNavigationCommands)
        );

        public static readonly DynamicUICommand SelectUpByPage = new DynamicUICommand(
            "Select up one page",
            "SelectUpByPage",
            typeof(NNavigationCommands),
            new KeyGesture(Key.PageUp, ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectDownByPage = new DynamicUICommand(
            "Select down one page",
            "SelectDownByPage",
            typeof(NNavigationCommands),
            new KeyGesture(Key.PageDown, ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectToDocumentStart = new DynamicUICommand(
            "Select to beginning of document",
            "SelectToDocumentStart",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Home, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectToDocumentEnd = new DynamicUICommand(
            "Select to end of document",
            "SelectToDocumentEnd",
            typeof(NNavigationCommands),
            new KeyGesture(Key.End, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SelectTitle = new DynamicUICommand(
            "Select title",
            "SelectTitle",
            typeof(NNavigationCommands),
            new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand TabForward = new DynamicUICommand(
            "Tab forward",
            "TabForward",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Tab)
        );

        public static readonly DynamicUICommand TabBackward = new DynamicUICommand(
            "Tab backward",
            "TabBackward",
            typeof(NNavigationCommands),
            new KeyGesture(Key.Tab, ModifierKeys.Shift)
        );
    }
}
