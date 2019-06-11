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
    public static class NInsertionCommands
    {
        public static string GroupName
        {
            get { return "Insertion Commands"; }
        }

        public static readonly DynamicUICommand Insert = new DynamicUICommand(
            "Insert",
            "Insert",
            typeof(NInsertionCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand InsertSymbol = new DynamicUICommand(
            "Insert symbol",
            "InsertSymbol",
            typeof(NInsertionCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand InsertStickyNote = new DynamicUICommand(
            "Insert sticky note",
            "InsertStickyNote",
            typeof(NInsertionCommands)
        );

        public static readonly DynamicUICommand InsertHyperlink = new DynamicUICommand(
            "Insert hyperlink",
            "InsertHyperlink",
            typeof(NInsertionCommands)
        );

        public static readonly DynamicUICommand RemoveHyperlink = new DynamicUICommand(
            "Remove hyperlink",
            "RemoveHyperlink",
            typeof(NInsertionCommands)
        );

        public static readonly DynamicUICommand InsertSection = new DynamicUICommand(
            "Insert section",
            "InsertSection",
            typeof(NInsertionCommands)
        );

        public static readonly DynamicUICommand InsertSubsection = new DynamicUICommand(
            "Insert sub-section",
            "InsertSubsection",
            typeof(NInsertionCommands)
        );

        public static readonly DynamicUICommand InsertTable = new DynamicUICommand(
            "Insert table",
            "InsertTable",
            typeof(NInsertionCommands)
        );

        public static readonly DynamicUICommand InsertImage = new DynamicUICommand(
            "Insert image",
            "InsertImage",
            typeof(NInsertionCommands)
        );

        public static readonly DynamicUICommand InsertScreenshot = new DynamicUICommand(
            "Insert screenshot",
            "InsertScreenshot",
            typeof(NInsertionCommands)
        );

        public static readonly DynamicUICommand CaptureScreen = new DynamicUICommand(
            "Capture screen",
            "CaptureScreen",
            typeof(NInsertionCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand CaptureWindow = new DynamicUICommand(
            "Capture window",
            "CaptureWindow",
            typeof(NInsertionCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand CaptureRegion = new DynamicUICommand(
            "Capture region",
            "CaptureRegion",
            typeof(NInsertionCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand InsertAudio = new DynamicUICommand(
            "Insert audio",
            "InsertAudio",
            typeof(NInsertionCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand InsertVideo = new DynamicUICommand(
            "Insert video",
            "InsertVideo",
            typeof(NInsertionCommands),
            isConfigurable: false
        );

        public static readonly DynamicUICommand InsertFile = new DynamicUICommand(
            "Insert file",
            "InsertFile",
            typeof(NInsertionCommands)
        );

    }
}
