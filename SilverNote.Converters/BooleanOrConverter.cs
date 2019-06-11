/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SilverNote.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanOrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            foreach (object value in values)
            {
                if (value == DependencyProperty.UnsetValue)
                {
                    return DependencyProperty.UnsetValue;
                }

                result |= (bool)value;
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

