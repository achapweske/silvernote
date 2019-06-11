/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.ViewModels;
using SilverNote.ViewModels.CategoryTree;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SilverNote.Views
{
    public class NotesTreeListStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var frameworkElement = (FrameworkElement)container;
            if (item is CategoryNode)
            {
                return frameworkElement.FindResource("NotesTreeListCategoryStyle") as Style;
            }
            if (item is SearchResultNode)
            {
                return frameworkElement.FindResource("NotesTreeListNoteStyle") as Style;
            }
            return null;
        }
    }
}
