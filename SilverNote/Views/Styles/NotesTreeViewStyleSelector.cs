/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SilverNote.Views
{
    public class NotesTreeViewStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement frameworkElement = (FrameworkElement)container;
            if (item.GetType() == typeof(CategoryViewModel))
            {
                return frameworkElement.FindResource("NotesTreeViewCategoryStyle") as Style;
            }
            if (item.GetType() == typeof(SearchResultViewModel))
            {
                return frameworkElement.FindResource("NotesTreeViewNoteStyle") as Style;
            }
            return null;
        }
    }
}
