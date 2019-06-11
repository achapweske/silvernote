/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace SilverNote.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class NotEqualsParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)(value != parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Binding.DoNothing;
            }
            else
            {
                return parameter;
            }
        }
    }
}
