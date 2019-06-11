/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SilverNote.Behaviors
{
    public class TreeViewBehavior
    {
        #region WeakSelection

        public static bool GetWeakSelection(UIElement element)
        {
            return (bool)element.GetValue(WeakSelectionProperty);
        }

        public static void SetWeakSelection(UIElement element, bool value)
        {
            element.SetValue(WeakSelectionProperty, value);
        }

        public static readonly DependencyProperty WeakSelectionProperty = DependencyProperty.RegisterAttached(
            "WeakSelection",
            typeof(bool),
            typeof(TreeViewBehavior),
            new UIPropertyMetadata(false, OnWeakSelectionChanged)
        );

        static void OnWeakSelectionChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeView = dep as TreeView;

            if (!(e.NewValue is bool))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                treeView.MouseLeftButtonDown += WeakSelectionTarget_MouseLeftButtonDown;
            }
            else
            {
                treeView.MouseLeftButtonDown -= WeakSelectionTarget_MouseLeftButtonDown;
            }
        }

        static void WeakSelectionTarget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeView treeView = sender as TreeView;

            for (int i = 0; i < treeView.Items.Count; i++)
            {
                var item = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromIndex(i);
                if (item != null)
                {
                    UnselectTreeViewItem(item);
                }
            }
        }

        static void UnselectTreeViewItem(TreeViewItem item)
        {
            item.IsSelected = false;

            for (int i = 0; i < item.Items.Count; i++)
            {
                var child = (TreeViewItem)item.ItemContainerGenerator.ContainerFromIndex(i);
                if (child != null)
                {
                    UnselectTreeViewItem(child);
                }
            }
        }

        #endregion

    }
}
