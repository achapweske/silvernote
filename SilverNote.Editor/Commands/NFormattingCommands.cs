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
    public static class NFormattingCommands
    {
        public static string GroupName
        {
            get { return "Formatting Commands"; }
        }

        public static readonly DynamicUICommand AlignLeft = new DynamicUICommand(
            "Align left",
            "AlignLeft",
            typeof(NFormattingCommands),
            new KeyGesture(Key.L, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand AlignCenter = new DynamicUICommand(
            "Align center",
            "AlignCenter",
            typeof(NFormattingCommands),
            new KeyGesture(Key.E, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand AlignRight = new DynamicUICommand(
            "Align right",
            "AlignRight",
            typeof(NFormattingCommands),
            new KeyGesture(Key.R, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand AlignJustify = new DynamicUICommand(
            "Align justify",
            "AlignJustify",
            typeof(NFormattingCommands),
            new KeyGesture(Key.J, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand AlignTop = new DynamicUICommand(
            "Align top",
            "AlignTop",
            typeof(NFormattingCommands)
        );

        public static readonly DynamicUICommand AlignMiddle = new DynamicUICommand(
            "Align middle",
            "AlignMiddle",
            typeof(NFormattingCommands)
        );

        public static readonly DynamicUICommand AlignBottom = new DynamicUICommand(
            "Align bottom",
            "AlignBottom",
            typeof(NFormattingCommands)
        );

        public static readonly DynamicUICommand SetFontFamily = new DynamicUICommand(
            "Set font",
            "SetFontFamily",
            typeof(NFormattingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand SetFontSize = new DynamicUICommand(
            "Set font size",
            "SetFontSize",
            typeof(NFormattingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand IncreaseFontSize = new DynamicUICommand(
            "Increase font size",
            "IncreaseFontSize",
            typeof(NFormattingCommands),
            new KeyGesture(Key.OemPeriod, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand DecreaseFontSize = new DynamicUICommand(
            "Decrease font size",
            "DecreaseFontSize",
            typeof(NFormattingCommands),
            new KeyGesture(Key.OemComma, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand IncreaseIndentation = new DynamicUICommand(
            "Increase indentation",
            "IncreaseIndentation",
            typeof(NFormattingCommands),
            new KeyGesture(Key.OemCloseBrackets, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand DecreaseIndentation = new DynamicUICommand(
            "Decrease indentation",
            "DecreaseIndentation",
            typeof(NFormattingCommands),
            new KeyGesture(Key.OemOpenBrackets, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand EnterLineBreak = new DynamicUICommand(
            "Line break",
            "EnterLineBreak",
            typeof(NFormattingCommands),
            new KeyGesture(Key.Enter, ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand SetTextColor = new DynamicUICommand(
            "Set text color",
            "SetTextColor",
            typeof(NFormattingCommands)
        );

        public static readonly DynamicUICommand Highlight = new DynamicUICommand(
            "Highlight",
            "Highlight",
            typeof(NFormattingCommands),
            new KeyGesture(Key.H, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand ToggleBold = new DynamicUICommand(
            "Bold",
            "ToggleBold",
            typeof(NFormattingCommands),
            new KeyGesture(Key.B, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand ToggleItalic = new DynamicUICommand(
            "Italic",
            "ToggleItalic",
            typeof(NFormattingCommands),
            new KeyGesture(Key.I, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand ToggleUnderline = new DynamicUICommand(
            "Underline",
            "ToggleUnderline",
            typeof(NFormattingCommands),
            new KeyGesture(Key.U, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand ToggleStrikethrough = new DynamicUICommand(
            "Strikethrough",
            "ToggleStrikethrough",
            typeof(NFormattingCommands),
            new KeyGesture(Key.U, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand ToggleSuperscript = new DynamicUICommand(
            "Superscript",
            "ToggleSuperscript",
            typeof(NFormattingCommands),
            new KeyGesture(Key.OemPlus, ModifierKeys.Control | ModifierKeys.Shift)
        );

        public static readonly DynamicUICommand ToggleSubscript = new DynamicUICommand(
            "Subscript",
            "ToggleSubscript",
            typeof(NFormattingCommands),
            new KeyGesture(Key.OemPlus, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand SetListStyle = new DynamicUICommand(
            "List style",
            "SetListStyle",
            typeof(NFormattingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand ToggleList = new DynamicUICommand(
            "Bulleted list",
            "ToggleList",
            typeof(NFormattingCommands),
            new KeyGesture(Key.OemPeriod, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand SetLineSpacing = new DynamicUICommand(
            "Line spacing",
            "SetLineSpacing",
            typeof(NFormattingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand SetLineSpacing1 = new DynamicUICommand(
            "Line spacing: 1",
            "SetLineSpacing1",
            typeof(NFormattingCommands),
            new KeyGesture(Key.D1, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand SetLineSpacing15 = new DynamicUICommand(
            "Line spacing: 1.5",
            "SetLineSpacing15",
            typeof(NFormattingCommands),
            new KeyGesture(Key.D5, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand SetLineSpacing2 = new DynamicUICommand(
            "Line spacing: 2",
            "SetLineSpacing2",
            typeof(NFormattingCommands),
            new KeyGesture(Key.D2, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand ClearFormatting = new DynamicUICommand(
            "Clear formatting",
            "ClearFormatting",
            typeof(NFormattingCommands),
            new KeyGesture(Key.Oem5, ModifierKeys.Control)  // Oem5 = backslash key
        );
    }
}
