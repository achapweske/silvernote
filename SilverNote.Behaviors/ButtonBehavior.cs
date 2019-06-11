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
using SilverNote.Common;

namespace SilverNote.Behaviors
{
    public class ButtonBehavior
    {
        #region DialogResult

        public static bool GetDialogResult(Button textBox)
        {
            return (bool)textBox.GetValue(DialogResultProperty);
        }

        public static void SetDialogResult(Button textBox, bool value)
        {
            textBox.SetValue(DialogResultProperty, value);
        }

        public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached(
            "DialogResult",
            typeof(bool),
            typeof(ButtonBehavior),
            new UIPropertyMetadata(false, OnDialogResultChanged)
        );

        static void OnDialogResultChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            Button button = dep as Button;

            if (e.NewValue is bool)
            {
                button.Click += DialogResultTarget_Click;
            }
            else
            {
                button.Click -= DialogResultTarget_Click;
            }
        }

        static void DialogResultTarget_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            var window = LayoutHelper.GetAncestor<Window>(button);
            if (window != null)
            {
                window.DialogResult = GetDialogResult(button);
            }
        }

        #endregion

    }
}
