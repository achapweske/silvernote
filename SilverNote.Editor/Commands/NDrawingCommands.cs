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
    public static class NDrawingCommands
    {
        public static string GroupName
        {
            get { return "Drawing Commands"; }
        }

        public static readonly DynamicUICommand Select = new DynamicUICommand(
            "Select",
             "Select",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand Erase = new DynamicUICommand(
            "Erase",
             "Erase",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand InsertPath = new DynamicUICommand(
            "Insert path",
             "InsertPath",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand InsertLine = new DynamicUICommand(
            "Insert line",
             "InsertLine",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand InsertClipart = new DynamicUICommand(
            "Insert clipart",
             "InsertClipart",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand InsertTextBox = new DynamicUICommand(
            "Insert text box",
             "InsertTextBox",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand Stroke = new DynamicUICommand(
            "Line color",
             "Stroke",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand RotateRight = new DynamicUICommand(
            "Rotate right",
             "RotateRight",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand RotateLeft = new DynamicUICommand(
            "Rotate left",
             "RotateLeft",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand FlipHorizontal = new DynamicUICommand(
            "Flip horizontal",
             "FlipHorizontal",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand FlipVertical = new DynamicUICommand(
            "Flip vertical",
             "FlipVertical",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand BringForward = new DynamicUICommand(
            "Bring forward",
             "BringForward",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand SendBackward = new DynamicUICommand(
            "Send backward",
             "SendBackward",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand BringToFront = new DynamicUICommand(
            "Bring to front",
             "BringToFront",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand SendToBack = new DynamicUICommand(
            "Send to back",
             "SendToBack",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand Fill = new DynamicUICommand(
            "Fill color",
             "Fill",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand SetEffect = new DynamicUICommand(
            "Set effect",
             "SetEffect",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand Group = new DynamicUICommand(
            "Group",
             "Group",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand Ungroup = new DynamicUICommand(
            "Ungroup",
             "Ungroup",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand AddToLibrary = new DynamicUICommand(
            "Add to library",
             "AddToLibrary",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand Label = new DynamicUICommand(
            "Label",
             "Label",
             typeof(NDrawingCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand EditSource = new DynamicUICommand(
            "Edit Source",
             "EditSource",
             typeof(NDrawingCommands),
            isConfigurable: false
        );
    }
}
