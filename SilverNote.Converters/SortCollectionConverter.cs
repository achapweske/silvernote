/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.ComponentModel;
using System.Collections;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace SilverNote.Converters
{
    [ValueConversion(typeof(IEnumerable), typeof(System.Collections.IEnumerable))]
    public class SortCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as IList;
            if (collection == null)
            {
                return null;
            }

            var view = new ListCollectionView(collection);

            if (parameter != null)
            {
                var sort = new SortDescription(parameter.ToString(), ListSortDirection.Ascending);

                view.SortDescriptions.Add(sort);
            }

            return view;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
