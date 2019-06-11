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
    public static class NApplicationCommands
    {
        public static string GroupName
        {
            get { return "Application Commands"; }
        }

        public static readonly DynamicUICommand NewNote = new DynamicUICommand(
            "New note",
            "NewNote",
            typeof(NApplicationCommands),
            new KeyGesture(Key.N, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand NewNotebook = new DynamicUICommand(
            "New notebook",
            "NewNotebook",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand OpenNotebook = new DynamicUICommand(
            "Open notebook",
            "OpenNotebook",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand DeleteNotebook = new DynamicUICommand(
            "Delete notebook",
            "DeleteNotebook",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand RenameNotebook = new DynamicUICommand(
            "Rename notebook",
            "RenameNotebook",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand Save = new DynamicUICommand(
            "Save",
            "Save",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand SaveAsHTML = new DynamicUICommand(
            "Save as HTML",
            "SaveAsHTML",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand SaveAsRTF = new DynamicUICommand(
            "Save as RTF",
            "SaveAsRTF",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand SaveAsTXT = new DynamicUICommand(
            "Save as TXT",
            "SaveAsTXT",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand SaveAsDOCX = new DynamicUICommand(
            "Save as DOCX",
            "SaveAsDOCX",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand SaveImage = new DynamicUICommand(
            "Save selected image", 
            "SaveImage",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand SaveFile = new DynamicUICommand(
            "Save selected file",
            "SaveFile",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand Print = new DynamicUICommand(
            "Print",
            "Print",
            typeof(NApplicationCommands),
            new KeyGesture(Key.P, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand PrintPreview = new DynamicUICommand(
            "Print preview",
            "PrintPreview",
            typeof(NApplicationCommands),
            new KeyGesture(Key.F2, ModifierKeys.Control)
        );

        public static readonly DynamicUICommand ChangePassword = new DynamicUICommand(
            "Change password",
            "ChangePassword",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand Settings = new DynamicUICommand(
            "Settings",
            "Settings",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand Activate = new DynamicUICommand(
            "Activate",
            "Activate",
            typeof(NApplicationCommands)
        );

        public static readonly DynamicUICommand About = new DynamicUICommand(
            "About",
            "About",
            typeof(NApplicationCommands)
        );
    }

}
