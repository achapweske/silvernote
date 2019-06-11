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
    [ValueConversion(typeof(double), typeof(double))]
    public class AddConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double a = MultiplyConverter.ToDouble(value);
                double b = MultiplyConverter.ToDouble(parameter);
                return a + b;
            }
            catch
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double a = MultiplyConverter.ToDouble(value);
                double b = MultiplyConverter.ToDouble(parameter);
                return a - b;
            }
            catch
            {
                return Binding.DoNothing;
            }
        }
    }
}
