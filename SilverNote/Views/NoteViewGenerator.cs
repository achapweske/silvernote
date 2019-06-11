/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using SilverNote.ViewModels;

namespace SilverNote.Views
{
    [ValueConversion(typeof(NoteViewModel), typeof(NoteView))]
    public class NoteViewGenerator : IValueConverter
    {
        public NoteViewGenerator()
        {
            _NoteViews = new Dictionary<NoteViewModel, NoteView>();
        }

        private Dictionary<NoteViewModel, NoteView> _NoteViews;

        public NoteView this[NoteViewModel vm]
        {
            get
            {
                NoteView result;
                if (!_NoteViews.TryGetValue(vm, out result))
                {
                    result = new NoteView();
                    result.DataContext = vm;
                    _NoteViews.Add(vm, result);
                }
                return result;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var noteViewModel = value as NoteViewModel;

            if (noteViewModel != null)
            {
                return this[noteViewModel];
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
