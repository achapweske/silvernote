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
    public class PasswordBoxBehavior
    {
        #region SelectAllWhenGotFocus

        public static bool GetSelectAllWhenGotFocus(DependencyObject dep)
        {
            return (bool)dep.GetValue(SelectAllWhenGotFocusProperty);
        }

        public static void SetSelectAllWhenGotFocus(DependencyObject dep, bool value)
        {
            dep.SetValue(SelectAllWhenGotFocusProperty, value);
        }

        public static readonly DependencyProperty SelectAllWhenGotFocusProperty = DependencyProperty.RegisterAttached(
            "SelectAllWhenGotFocus",
            typeof(bool),
            typeof(PasswordBoxBehavior),
            new UIPropertyMetadata(false, OnSelectAllWhenGotFocusChanged)
        );

        static void OnSelectAllWhenGotFocusChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = dep as PasswordBox;

            if ((bool)e.NewValue)
            {
                passwordBox.GotFocus += SelectAllWhenGotFocusTarget_GotFocus;
            }
            else
            {
                passwordBox.GotFocus -= SelectAllWhenGotFocusTarget_GotFocus;
            }
        }

        static void SelectAllWhenGotFocusTarget_GotFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            passwordBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                passwordBox.SelectAll();
            }));
        }

        #endregion

    }
}
