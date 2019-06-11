/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SilverNote.Behaviors
{
    public static class TreeViewItemBehavior
    {
        #region ExpandWhen

        public static bool GetExpandWhen(TreeViewItem element)
        {
            return (bool)element.GetValue(ExpandWhenProperty);
        }

        public static void SetExpandWhen(TreeViewItem element, bool value)
        {
            element.SetValue(ExpandWhenProperty, value);
        }

        public static readonly DependencyProperty ExpandWhenProperty = DependencyProperty.RegisterAttached(
            "ExpandWhen",
            typeof(bool),
            typeof(TreeViewItemBehavior),
            new UIPropertyMetadata(false, OnExpandWhenChanged)
        );

        static void OnExpandWhenChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem treeViewItem = dep as TreeViewItem;

            if (!(e.NewValue is bool))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                treeViewItem.IsExpanded = true;
            }
        }

        #endregion

        #region SelectWhen

        public static bool GetSelectWhen(TreeViewItem element)
        {
            return (bool)element.GetValue(SelectWhenProperty);
        }

        public static void SetSelectWhen(TreeViewItem element, bool value)
        {
            element.SetValue(SelectWhenProperty, value);
        }

        public static readonly DependencyProperty SelectWhenProperty = DependencyProperty.RegisterAttached(
            "SelectWhen",
            typeof(bool),
            typeof(TreeViewItemBehavior),
            new UIPropertyMetadata(false, OnSelectWhenChanged)
        );

        static void OnSelectWhenChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem treeViewItem = dep as TreeViewItem;

            if (!(e.NewValue is bool))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                treeViewItem.IsSelected = true;
            }
        }

        #endregion

        #region SelectParentWhenEnterPressed

        public static bool GetSelectParentWhenEnterPressed(TreeViewItem element)
        {
            return (bool)element.GetValue(SelectParentWhenEnterPressedProperty);
        }

        public static void SetSelectParentWhenEnterPressed(TreeViewItem element, bool value)
        {
            element.SetValue(SelectParentWhenEnterPressedProperty, value);
        }

        public static readonly DependencyProperty SelectParentWhenEnterPressedProperty = DependencyProperty.RegisterAttached(
            "SelectParentWhenEnterPressed",
            typeof(bool),
            typeof(TreeViewItemBehavior),
            new UIPropertyMetadata(false, OnSelectParentWhenEnterPressedChanged)
        );

        static void OnSelectParentWhenEnterPressedChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var treeViewItem = dep as TreeViewItem;

            if (treeViewItem == null)
            {
                return;
            }

            if (!(e.NewValue is bool))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                treeViewItem.KeyDown += SelectParentWhenEnterPressedTarget_KeyDown;
            }
        }

        static void SelectParentWhenEnterPressedTarget_KeyDown(object sender, KeyEventArgs e)
        {
            var treeViewItem = sender as TreeViewItem;

            if (e.Key == Key.Enter)
            {
                treeViewItem.IsSelected = false;
                Keyboard.ClearFocus();

                var parentItem = ItemsControl.ItemsControlFromItemContainer(treeViewItem) as TreeViewItem;
                if (parentItem != null)
                {
                    parentItem.IsSelected = true;
                }

                e.Handled = true;
            }
        }

        #endregion

    }
}
