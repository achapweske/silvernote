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
    public class NotesTreeViewTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var frameworkElement = (FrameworkElement)container;
            if (item.GetType() == typeof(CategoryViewModel))
            {
                return frameworkElement.FindResource("NotesTreeViewCategoryDataTemplate") as DataTemplate;
            }
            if (item.GetType() == typeof(SearchResultViewModel))
            {
                return frameworkElement.FindResource("NotesTreeViewNoteDataTemplate") as DataTemplate;
            }
            return null;
        }
    }
}
