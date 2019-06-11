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
    public class MultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double a = ToDouble(value);
                double b = ToDouble(parameter);
                return a * b;
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
                double a = ToDouble(value);
                double b = ToDouble(parameter);
                return a / b;
            }
            catch
            {
                return Binding.DoNothing;
            }
        }

        public static double ToDouble(object value)
        {
            if (value is string && ((string)value).Contains("/"))
            {
                string[] fraction = ((string)value).Split('/');

                return ToDouble(fraction[0]) / ToDouble(fraction[1]);
            }
            else
            {
                return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
        }
    }
}
