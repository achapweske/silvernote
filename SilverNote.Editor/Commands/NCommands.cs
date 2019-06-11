/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using SilverNote.Common;

namespace SilverNote.Commands
{
    public static class NCommands
    {
        public static Type[] Classes = new[] {
            typeof(NApplicationCommands),
            typeof(NEditingCommands),
            typeof(NTextCommands),
            typeof(NNavigationCommands),
            typeof(NViewCommands),
            typeof(NFormattingCommands),
            typeof(NDrawingCommands),
            typeof(NInsertionCommands),
            typeof(NImageCommands),
            typeof(NFileCommands),
            typeof(NTableCommands),
            typeof(NUtilityCommands),
            typeof(NDevelopmentCommands)
        };

        private static IEnumerable<DynamicUICommand> _AllCommands;

        public static IEnumerable<DynamicUICommand> AllCommands
        {
            get
            {
                if (_AllCommands == null)
                {
                    _AllCommands = GetAllCommands(configurableOnly: true);
                }
                return _AllCommands;
            }
        }

        private static IEnumerable<DynamicUICommand> GetAllCommands(bool configurableOnly)
        {
            var results = new List<DynamicUICommand>();

            foreach (Type classType in Classes)
            {
                var fields = classType.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(DynamicUICommand))
                    {
                        var command = (DynamicUICommand)field.GetValue(null);
                        if (!configurableOnly || command.IsConfigurable)
                        {
                            results.Add(command);
                        }
                    }
                }
            }

            return results;
        }

        public static DynamicUICommand GetCommand(string command)
        {
            string ownerType, name;
            TryParseCommand(command, out ownerType, out name);

            foreach (Type classType in Classes)
            {
                if (ownerType == null || ownerType == classType.Name)
                {
                    var field = classType.GetField(name);
                    if (field != null)
                    {
                        return (DynamicUICommand)field.GetValue(null);
                    }
                }
            }

            return null;
        }

        public static NShortcutCollection ToShortcuts()
        {
            var shortcuts = new NShortcutCollection();

            foreach (var command in AllCommands)
            {
                string gesture = command.InputGestureText;
                string name = command.OwnerType.Name + "." + command.Name;
                var shortcut = new NShortcut(gesture, name);
                shortcuts.Add(shortcut);
            }

            return shortcuts;
        }

        public static void LoadShortcuts(NShortcutCollection shortcuts)
        {
            foreach (var shortcut in shortcuts)
            {
                var command = GetCommand(shortcut.Command);
                if (command != null)
                {
                    command.InputGestureText = shortcut.Gesture;
                }
            }
        }

        public static bool TryParseCommand(string str, out string ownerType, out string name)
        {
            string[] tokens = str.Split('.');

            if (tokens.Length > 0)
            {
                name = tokens[tokens.Length - 1];
            }
            else
            {
                name = null;
            }

            if (tokens.Length > 1)
            {
                ownerType = tokens[tokens.Length - 2];
            }
            else
            {
                ownerType = null;
            }

            return name != null;
        }
    }
}
