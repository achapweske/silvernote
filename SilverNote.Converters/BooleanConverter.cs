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
    public class BooleanConverter : IValueConverter
    {
        public object TrueValue { get; set; }

        public object FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return TrueValue;
            }
            else
            {
                return FalseValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Equals(TrueValue))
            {
                return true;
            }
            else if (value.Equals(FalseValue))
            {
                return false;
            }
            else
            {
                return Binding.DoNothing;
            }
        }
    }
}
