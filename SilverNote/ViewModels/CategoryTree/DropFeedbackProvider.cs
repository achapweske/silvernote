/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SilverNote.ViewModels.CategoryTree
{
    public class DropFeedbackProvider : IDropFeedbackProvider
    {
        bool _OriginalIsSelected;

        public void DragEnter(object sender, DragEventArgs e)
        {
            var expander = sender as Expander;
            if (expander != null)
            {
                expander.Opacity = 0.5;
                return;
            }

            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem != null)
            {
                listBoxItem = GetCategory(listBoxItem);
                _OriginalIsSelected = listBoxItem.IsSelected;
                listBoxItem.IsSelected = true;
            }
        }

        public void DragLeave(object sender, DragEventArgs e)
        {
            var expander = sender as Expander;
            if (expander != null)
            {
                expander.Opacity = 1;
                return;
            }

            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem != null)
            {
                listBoxItem = GetCategory((ListBoxItem)sender);
                listBoxItem.IsSelected = _OriginalIsSelected;
            }
        }

        public void DragOver(object sender, DragEventArgs e)
        {
            
        }

        public void Drop(object sender, DragEventArgs e)
        {
            DragLeave(sender, e);
        }

        private static ListBoxItem GetCategory(ListBoxItem item)
        {
            if (item.DataContext is CategoryNode)
            {
                return item;
            }

            var listBox = LayoutHelper.GetAncestor<ListBox>(item);
            var categoryNode = ((SearchResultNode)item.DataContext).Parent;
            return (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(categoryNode);
        }

    }
}
