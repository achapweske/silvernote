/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.ViewModels;
using SilverNote.ViewModels.CategoryTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SilverNote.Views.Styles
{
    partial class NotesTreeListStyle : ResourceDictionary
    {
        public NotesTreeListStyle()
        {
            InitializeComponent();
        }

        void NotesTreeListStyle_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var listBox = (ListBox)e.Source;
            var categories = (CategoryTreeViewModel)listBox.DataContext;
            listBox.ContextMenu = SelectContextMenu(categories);
            listBox.ContextMenu.DataContext = listBox.DataContext;
            listBox.ContextMenu.IsOpen = true;
            e.Handled = true;
        }

        private ContextMenu SelectContextMenu(CategoryTreeViewModel categories)
        {
            if (categories.SelectedItems.Count == 0)
            {
                // No items selected
                return (ContextMenu)this["NotesTreeListContextMenu"];
            }
            else if (categories.SelectedItems.Count == 1)
            {
                if (categories.SelectedItems[0] is SearchResultNode)
                {
                    // Single note selected
                    return (ContextMenu)this["NotesTreeListContextMenu_SingleNote"];
                }
                else if (categories.SelectedItems[0] is CategoryNode && ((CategoryNode)categories.SelectedItems[0]).Category.IsPseudoCategory)
                {
                    // Pseudo-category selected
                    return (ContextMenu)this["NotesTreeListContextMenu_PseudoCategory"];
                }
                else if (categories.SelectedItems[0] is CategoryNode)
                {
                    // Single category selected
                    return (ContextMenu)this["NotesTreeListContextMenu_SingleCategory"];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (categories.SelectedItems.All(item => item is SearchResultNode))
                {
                    // Multiple notes selected
                    return (ContextMenu)this["NotesTreeListContextMenu_MultipleNotes"];
                }
                else if (categories.SelectedItems.All(item => item is CategoryNode))
                {
                    // Multiple categories selected
                    return (ContextMenu)this["NotesTreeListContextMenu_MultipleCategories"];
                }
                else
                {
                    // Note(s) and category(s) selected
                    return (ContextMenu)this["NotesTreeListContextMenu_NotesAndCategories"];
                }
            }
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var tree = ((FrameworkElement)sender).DataContext as CategoryTreeViewModel;
            if (tree != null)
            {
                tree.OpenSelectedItems();
            }
        }
    }

    public class MoveToMenuItemContainerStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var frameworkElement = (FrameworkElement)container;
            if (item is NotebookViewModel)
            {
                return frameworkElement.FindResource("MoveToNotebookMenuItemContainerStyle") as Style;
            }
            if (item is CategoryViewModel)
            {
                return frameworkElement.FindResource("MoveToCategoryMenuItemContainerStyle") as Style;
            }
            return null;
        }
    }
}
