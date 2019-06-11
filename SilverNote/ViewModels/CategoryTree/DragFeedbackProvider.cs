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
using System.Windows.Input;

namespace SilverNote.ViewModels.CategoryTree
{
    public class DragFeedbackProvider : IDragFeedbackProvider
    {
        public void DragStarted(object sender, MouseEventArgs e)
        {
            var listBoxItem = (ListBoxItem)sender;
            var listBox = LayoutHelper.GetAncestor<ListBox>(listBoxItem);
            foreach (var item in listBox.SelectedItems)
            {
                var container = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);
                container.Opacity = 0.5;
            }
        }

        public void GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

        }

        public void DragEnded(object sender)
        {
            var listBoxItem = (ListBoxItem)sender;
            var listBox = LayoutHelper.GetAncestor<ListBox>(listBoxItem);
            if (listBox != null)
            {
                foreach (var item in listBox.SelectedItems)
                {
                    var container = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);
                    container.Opacity = 1.0;
                }
            }
        }
    }
}
