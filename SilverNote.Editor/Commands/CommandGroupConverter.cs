/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SilverNote.Commands
{
    [ValueConversion(typeof(string), typeof(string))]
    public class CommandGroupConverter : IValueConverter
    {
        public static CommandGroupConverter Instance
        {
            get { return new CommandGroupConverter(); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string command = value as string;

            if (value != null)
            {
                return GetCommandGroup(command);
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public static string GetCommandGroup(string commandName)
        {
            var command = NCommands.GetCommand(commandName);
            if (command == null)
            {
                return null;
            }

            var property = command.OwnerType.GetProperty("GroupName");
            if (property == null)
            {
                return String.Empty;
            }

            return property.GetValue(null, null) as string;
        }
    }
}
