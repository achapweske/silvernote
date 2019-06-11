/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace SilverNote.Converters
{
    [ValueConversion(typeof(bool), typeof(object))]
    public class SelectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = parameter as string;
            if (str == null)
            {
                return null;
            }

            string[] tokens = str.Split('|');
            if (tokens.Length == 0)
            {
                return "";
            }

            int index;

            if (value is bool)
            {
                index = (bool)value ? 1 : 0;
            }
            else if (value is int)
            {
                index = (int)value;
            }
            else
            {
                index = -1;
            }

            if (index >= 0 && index < tokens.Length)
            {
                return tokens[index];
            }
            else
            {
                return tokens[tokens.Length - 1];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
