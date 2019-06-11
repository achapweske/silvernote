/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace SilverNote.Behaviors
{
    public static class MenuItemBehavior
    {
        #region UpdateItemsSourceWhenSubmenuOpened

        public static bool GetUpdateItemsSourceWhenSubmenuOpened(DependencyObject element)
        {
            return (bool)element.GetValue(UpdateItemsSourceWhenSubmenuOpenedProperty);
        }

        public static void SetUpdateItemsSourceWhenSubmenuOpened(DependencyObject element, bool value)
        {
            element.SetValue(UpdateItemsSourceWhenSubmenuOpenedProperty, value);
        }

        public static readonly DependencyProperty UpdateItemsSourceWhenSubmenuOpenedProperty = DependencyProperty.RegisterAttached(
            "UpdateItemsSourceWhenSubmenuOpened",
            typeof(bool),
            typeof(MenuItemBehavior),
            new UIPropertyMetadata(false, UpdateItemsSourceWhenSubmenuOpened_PropertyChanged)
        );

        static void UpdateItemsSourceWhenSubmenuOpened_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            MenuItem menuItem = dep as MenuItem;

            if (e.NewValue is bool && ((bool)e.NewValue))
            {
                menuItem.SubmenuOpened += UpdateItemsSourceTarget_SubmenuOpened;
            }
            else
            {
                menuItem.SubmenuOpened -= UpdateItemsSourceTarget_SubmenuOpened;
            }
        }

        static void UpdateItemsSourceTarget_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;

            var binding = menuItem.GetBindingExpression(MenuItem.ItemsSourceProperty);
            if (binding != null)
            {
                binding.UpdateTarget();
            }
        }

        #endregion

        #region ClickCommand

        public static ICommand GetClickCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(ClickCommandProperty);
        }

        public static void SetClickCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(ClickCommandProperty, value);
        }

        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.RegisterAttached(
            "ClickCommand",
            typeof(ICommand),
            typeof(MenuItemBehavior),
            new UIPropertyMetadata(null, OnClickCommandChanged)
        );

        static void OnClickCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            MenuItem commandTarget = dep as MenuItem;

            if (e.OldValue != null)
            {
                commandTarget.Click -= ClickCommandTarget_Click;
            }

            if (e.NewValue != null)
            {
                commandTarget.Click += ClickCommandTarget_Click;
            }
        }

        static void ClickCommandTarget_Click(object sender, RoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) &&
                    !Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                    !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    var target = sender as DependencyObject;
                    var command = GetClickCommand(target);
                    var parameter = GetClickCommandParameter(target);

                    command.Execute(parameter);

                    e.Handled = true;
                }
            }
        }

        #endregion

        #region ClickCommandParameter

        public static object GetClickCommandParameter(DependencyObject element)
        {
            return element.GetValue(ClickCommandParameterProperty);
        }

        public static void SetClickCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(ClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty ClickCommandParameterProperty = DependencyProperty.RegisterAttached(
            "ClickCommandParameter",
            typeof(object),
            typeof(MenuItemBehavior)
        );

        #endregion

        #region AltClickCommand

        public static ICommand GetAltClickCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(AltClickCommandProperty);
        }

        public static void SetAltClickCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(AltClickCommandProperty, value);
        }

        public static readonly DependencyProperty AltClickCommandProperty = DependencyProperty.RegisterAttached(
            "AltClickCommand",
            typeof(ICommand),
            typeof(MenuItemBehavior),
            new UIPropertyMetadata(null, OnAltClickCommandChanged)
        );

        static void OnAltClickCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            MenuItem commandTarget = dep as MenuItem;

            if (e.OldValue != null)
            {
                commandTarget.Click -= AltClickCommandTarget_Click;
            }

            if (e.NewValue != null)
            {
                commandTarget.Click += AltClickCommandTarget_Click;
            }
        }

        static void AltClickCommandTarget_Click(object sender, RoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                {
                    var target = sender as DependencyObject;
                    var command = GetAltClickCommand(target);
                    var parameter = GetAltClickCommandParameter(target);

                    command.Execute(parameter);

                    e.Handled = true;
                }
            }
        }

        #endregion

        #region AltClickCommandParameter

        public static object GetAltClickCommandParameter(DependencyObject element)
        {
            return element.GetValue(AltClickCommandParameterProperty);
        }

        public static void SetAltClickCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(AltClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty AltClickCommandParameterProperty = DependencyProperty.RegisterAttached(
            "AltClickCommandParameter",
            typeof(object),
            typeof(MenuItemBehavior)
        );

        #endregion

        #region CtrlClickCommand

        public static ICommand GetCtrlClickCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(CtrlClickCommandProperty);
        }

        public static void SetCtrlClickCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(CtrlClickCommandProperty, value);
        }

        public static readonly DependencyProperty CtrlClickCommandProperty = DependencyProperty.RegisterAttached(
            "CtrlClickCommand",
            typeof(ICommand),
            typeof(MenuItemBehavior),
            new UIPropertyMetadata(null, OnCtrlClickCommandChanged)
        );

        static void OnCtrlClickCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            MenuItem commandTarget = dep as MenuItem;

            if (e.OldValue != null)
            {
                commandTarget.Click -= CtrlClickCommandTarget_Click;
            }

            if (e.NewValue != null)
            {
                commandTarget.Click += CtrlClickCommandTarget_Click;
            }
        }

        static void CtrlClickCommandTarget_Click(object sender, RoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                    !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    var target = sender as DependencyObject;
                    var command = GetCtrlClickCommand(target);
                    var parameter = GetCtrlClickCommandParameter(target);

                    command.Execute(parameter);

                    e.Handled = true;
                }
            }
        }

        #endregion

        #region CtrlClickCommandParameter

        public static object GetCtrlClickCommandParameter(DependencyObject element)
        {
            return element.GetValue(CtrlClickCommandParameterProperty);
        }

        public static void SetCtrlClickCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(CtrlClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty CtrlClickCommandParameterProperty = DependencyProperty.RegisterAttached(
            "CtrlClickCommandParameter",
            typeof(object),
            typeof(MenuItemBehavior)
        );

        #endregion

        #region ShiftClickCommand

        public static ICommand GetShiftClickCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(ShiftClickCommandProperty);
        }

        public static void SetShiftClickCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(ShiftClickCommandProperty, value);
        }

        public static readonly DependencyProperty ShiftClickCommandProperty = DependencyProperty.RegisterAttached(
            "ShiftClickCommand",
            typeof(ICommand),
            typeof(MenuItemBehavior),
            new UIPropertyMetadata(null, OnShiftClickCommandChanged)
        );

        static void OnShiftClickCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            MenuItem commandTarget = dep as MenuItem;

            if (e.OldValue != null)
            {
                commandTarget.Click -= ShiftClickCommandTarget_Click;
            }

            if (e.NewValue != null)
            {
                commandTarget.Click += ShiftClickCommandTarget_Click;
            }
        }

        static void ShiftClickCommandTarget_Click(object sender, RoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) &&
                    !Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    var target = sender as DependencyObject;
                    var command = GetShiftClickCommand(target);
                    var parameter = GetShiftClickCommandParameter(target);

                    command.Execute(parameter);

                    e.Handled = true;
                }
            }
        }

        #endregion

        #region ShiftClickCommandParameter

        public static object GetShiftClickCommandParameter(DependencyObject element)
        {
            return element.GetValue(ShiftClickCommandParameterProperty);
        }

        public static void SetShiftClickCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(ShiftClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty ShiftClickCommandParameterProperty = DependencyProperty.RegisterAttached(
            "ShiftClickCommandParameter",
            typeof(object),
            typeof(MenuItemBehavior)
        );

        #endregion

        #region CtrlShiftClickCommand

        public static ICommand GetCtrlShiftClickCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(CtrlShiftClickCommandProperty);
        }

        public static void SetCtrlShiftClickCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(CtrlShiftClickCommandProperty, value);
        }

        public static readonly DependencyProperty CtrlShiftClickCommandProperty = DependencyProperty.RegisterAttached(
            "CtrlShiftClickCommand",
            typeof(ICommand),
            typeof(MenuItemBehavior),
            new UIPropertyMetadata(null, OnCtrlShiftClickCommandChanged)
        );

        static void OnCtrlShiftClickCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            MenuItem commandTarget = dep as MenuItem;

            if (e.OldValue != null)
            {
                commandTarget.Click -= CtrlShiftClickCommandTarget_Click;
            }

            if (e.NewValue != null)
            {
                commandTarget.Click += CtrlShiftClickCommandTarget_Click;
            }
        }

        static void CtrlShiftClickCommandTarget_Click(object sender, RoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                    Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    var target = sender as DependencyObject;
                    var command = GetCtrlShiftClickCommand(target);
                    var parameter = GetCtrlShiftClickCommandParameter(target);

                    command.Execute(parameter);

                    e.Handled = true;
                }
            }
        }

        #endregion

        #region CtrlShiftClickCommandParameter

        public static object GetCtrlShiftClickCommandParameter(DependencyObject element)
        {
            return element.GetValue(CtrlShiftClickCommandParameterProperty);
        }

        public static void SetCtrlShiftClickCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(CtrlShiftClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty CtrlShiftClickCommandParameterProperty = DependencyProperty.RegisterAttached(
            "CtrlShiftClickCommandParameter",
            typeof(object),
            typeof(MenuItemBehavior)
        );

        #endregion

    }
}
