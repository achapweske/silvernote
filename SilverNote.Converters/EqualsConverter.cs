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
    [ValueConversion(typeof(object), typeof(bool))]
    public class EqualsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = true;

            object firstValue = values[0];

            foreach (object value in values)
            {
                if (firstValue != null)
                {
                    result &= (firstValue.Equals(value));
                }
                else
                {
                    result &= (firstValue == value);
                }
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

