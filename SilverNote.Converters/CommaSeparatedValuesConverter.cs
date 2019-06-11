/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace SilverNote.Converters
{
    public class CommaSeparatedValuesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = values[0];
            var collection = values[1] as IEnumerable;

            bool isLastItem = false;
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    isLastItem = (item == value);
                }
            }

            string propertyName = parameter as string;

            if (propertyName != null)
            {
                var property = value.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    value = property.GetValue(value, null);
                }
            }

            if (value == null)
            {
                return String.Empty;
            }

            if (isLastItem)
            {
                return value.ToString();
            }
            else
            {
                return String.Format("{0}, ", value);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
