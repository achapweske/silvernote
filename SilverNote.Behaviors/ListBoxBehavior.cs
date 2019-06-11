/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SilverNote.Behaviors
{
    public class ListBoxBehavior
    {
        #region SelectedItems

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(ListBoxBehavior),
            new UIPropertyMetadata(SelectedItemsProperty_Changed)
        );

        public static IList GetSelectedItems(ListBox element)
        {
            return (IList)element.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(ListBox element, IList value)
        {
            element.SetValue(SelectedItemsProperty, value);
        }

        static void SelectedItemsProperty_Changed(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var listBox = (ListBox)dep;

            IList oldValue = (IList)e.OldValue;
            if (oldValue != null)
            {
                listBox.SelectionChanged -= SelectedItemsTarget_SelectionChanged;   
            }

            IList newValue = (IList)e.NewValue;
            if (newValue != null)
            {
                listBox.SelectionChanged += SelectedItemsTarget_SelectionChanged;   
            }
        }

        static void SelectedItemsTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var items = GetSelectedItems(listBox);

            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    items.Remove(item);
                }
            }

            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    if (!items.Contains(item))
                    {
                        items.Add(item);
                    }
                }
            }
        }

        #endregion


    }
}
