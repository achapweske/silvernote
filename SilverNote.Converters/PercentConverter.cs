/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics;

namespace SilverNote.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class PercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                Debug.WriteLine("PercentConverter Error: input cannot be null");
                return Binding.DoNothing;
            }
            else if (value is double && targetType == typeof(String))
            {
                return DoubleToString((double)value, culture);
            }
            else
            {
                Debug.WriteLine("PercentConverter Error: unsupported conversion (type={0}, targetType={1})", value.GetType(), targetType);
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                Debug.WriteLine("PercentConverter Error: input cannot be null");
                return Binding.DoNothing;
            }
            else if (value is string && targetType == typeof(double))
            {
                try
                {
                    return StringToDouble((string)value, culture);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("PercentConverter Error: " + e.Message);
                    return Binding.DoNothing;
                }
            }
            else
            {
                Debug.WriteLine("PercentConverter Error: unsupported conversion (type={0}, targetType={1})", value.GetType(), targetType);
                return Binding.DoNothing;
            }
        }

        private static string DoubleToString(double value, CultureInfo culture)
        {
            return String.Format(culture, "{0:P0}", value);
        }

        private static double StringToDouble(string value, CultureInfo culture)
        {
            value = value.Replace(culture.NumberFormat.PercentSymbol, "");
            value = value.Trim();
            return Double.Parse(value, culture) / 100.0;
        }
    }
}
