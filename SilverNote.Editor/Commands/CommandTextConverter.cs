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
    public class CommandTextConverter : IValueConverter
    {
        public static CommandTextConverter Instance
        {
            get { return new CommandTextConverter(); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string command = value as string;

            if (value != null)
            {
                return GetCommandText(command);
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

        public static string GetCommandText(string commandName)
        {
            var command = NCommands.GetCommand(commandName);
            if (command != null)
            {
                return command.Text;
            }
            else
            {
                return null;
            }
        }
    }
}
