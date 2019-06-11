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
    public static class ListBoxItemBehavior
    {
        #region BringIntoViewWhenSelected

        public static readonly DependencyProperty BringIntoViewWhenSelectedProperty = DependencyProperty.RegisterAttached(
            "BringIntoViewWhenSelected",
            typeof(bool),
            typeof(ListBoxItemBehavior),
            new UIPropertyMetadata(false, OnBringIntoViewWhenSelectedChanged)
        );

        public static bool GetBringIntoViewWhenSelected(ListBoxItem element)
        {
            return (bool)element.GetValue(BringIntoViewWhenSelectedProperty);
        }

        public static void SetBringIntoViewWhenSelected(ListBoxItem element, bool value)
        {
            element.SetValue(BringIntoViewWhenSelectedProperty, value);
        }

        static void OnBringIntoViewWhenSelectedChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var listBoxItem = dep as ListBoxItem;

            if (listBoxItem == null)
            {
                return;
            }

            if (!(e.NewValue is bool))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                listBoxItem.Selected += BringIntoViewWhenSelectedTarget_Selected;
            }
            else
            {
                listBoxItem.Selected -= BringIntoViewWhenSelectedTarget_Selected;
            }
        }

        static void BringIntoViewWhenSelectedTarget_Selected(object sender, EventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;

            listBoxItem.BringIntoView();
        }

        #endregion

        #region Command

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(ListBoxItemBehavior),
            new UIPropertyMetadata(null, CommandProperty_Changed)
        );

        public static ICommand GetCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(CommandProperty, value);
        }

        static void CommandProperty_Changed(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            Control commandTarget = dep as Control;

            if (e.OldValue != null)
            {
                commandTarget.PreviewMouseLeftButtonUp -= CommandTarget_ItemClicked;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewMouseLeftButtonUp += CommandTarget_ItemClicked;
            }
        }

        static void CommandTarget_ItemClicked(object sender, RoutedEventArgs e)
        {
            if (!e.Handled)
            {
                var target = sender as DependencyObject;
                var command = GetCommand(target);
                var parameter = GetCommandParameter(target);

                command.Execute(parameter);
            }
        }

        #endregion

        #region CommandParameter

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(object),
            typeof(ListBoxItemBehavior)
        );

        public static object GetCommandParameter(DependencyObject element)
        {
            return element.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(CommandParameterProperty, value);
        }

        #endregion

    }
}
