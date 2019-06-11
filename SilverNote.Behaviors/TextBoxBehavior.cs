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
    public static class TextBoxBehavior
    {
        #region FilterExpression

        public const string INT_FILTER = @"^\d*$";

        public static string GetFilterExpression(TextBox textBox)
        {
            return (string)textBox.GetValue(FilterExpressionProperty);
        }

        public static void SetFilterExpression(TextBox textBox, string value)
        {
            textBox.SetValue(FilterExpressionProperty, value);
        }

        public static readonly DependencyProperty FilterExpressionProperty = DependencyProperty.RegisterAttached(
            "FilterExpression",
            typeof(string),
            typeof(TextBoxBehavior),
            new UIPropertyMetadata(null, OnFilterExpressionChanged)
        );

        static void OnFilterExpressionChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = dep as TextBox;

            string expression = e.NewValue as string;

            if (!String.IsNullOrEmpty(expression))
            {
                textBox.PreviewTextInput += FilterExpressionTarget_PreviewTextInput;
            }
            else
            {
                textBox.PreviewTextInput -= FilterExpressionTarget_PreviewTextInput;
            }
        }

        static void FilterExpressionTarget_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text + e.Text;
            string expression = GetFilterExpression(textBox);

            if (!Regex.IsMatch(newText, expression))
            {
                e.Handled = true;
            }
        }

        #endregion

        #region SelectAllWhen

        public static bool GetSelectAllWhen(UIElement element)
        {
            return (bool)element.GetValue(SelectAllWhenProperty);
        }

        public static void SetSelectAllWhen(UIElement element, bool value)
        {
            element.SetValue(SelectAllWhenProperty, value);
        }

        public static readonly DependencyProperty SelectAllWhenProperty = DependencyProperty.RegisterAttached(
            "SelectAllWhen",
            typeof(bool),
            typeof(TextBoxBehavior),
            new UIPropertyMetadata(false, OnSelectAllWhenChanged)
        );

        static void OnSelectAllWhenChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = dep as TextBox;

            if (!(e.NewValue is bool))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.SelectAll();
                }));
            }
        }

        #endregion

        #region SelectAllWhenGotFocus

        public static bool GetSelectAllWhenGotFocus(TextBox textBox)
        {
            return (bool)textBox.GetValue(SelectAllWhenGotFocusProperty);
        }

        public static void SetSelectAllWhenGotFocus(TextBox textBox, bool value)
        {
            textBox.SetValue(SelectAllWhenGotFocusProperty, value);
        }

        public static readonly DependencyProperty SelectAllWhenGotFocusProperty = DependencyProperty.RegisterAttached(
            "SelectAllWhenGotFocus",
            typeof(bool),
            typeof(TextBoxBehavior),
            new UIPropertyMetadata(false, OnSelectAllWhenGotFocusChanged)
        );

        static void OnSelectAllWhenGotFocusChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var textBox = dep as TextBox;

            if ((bool)e.NewValue)
            {
                textBox.GotFocus += SelectAllWhenGotFocusTarget_GotFocus;
            }
            else
            {
                textBox.GotFocus -= SelectAllWhenGotFocusTarget_GotFocus;
            }
        }

        static void SelectAllWhenGotFocusTarget_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.SelectAll();
            }));
        }

        #endregion

        #region WeakFocus

        public static bool GetWeakFocus(UIElement element)
        {
            return (bool)element.GetValue(WeakFocusProperty);
        }

        public static void SetWeakFocus(UIElement element, bool value)
        {
            element.SetValue(WeakFocusProperty, value);
        }

        public static readonly DependencyProperty WeakFocusProperty = DependencyProperty.RegisterAttached(
            "WeakFocus",
            typeof(bool),
            typeof(TextBoxBehavior),
            new UIPropertyMetadata(false, OnWeakFocusChanged)
        );

        static void OnWeakFocusChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = dep as TextBox;

            if (textBox == null)
            {
                return;
            }

            if (!(e.NewValue is bool))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                textBox.GotKeyboardFocus += WeakFocusTarget_GotKeyboardFocus;
                textBox.LostKeyboardFocus += WeakFocusTarget_LostKeyboardFocus;
            }
            else
            {
                textBox.GotKeyboardFocus -= WeakFocusTarget_GotKeyboardFocus;
                textBox.LostKeyboardFocus -= WeakFocusTarget_LostKeyboardFocus;
            }
        }

        static void WeakFocusTarget_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            textBox.CaptureMouse();

            textBox.PreviewMouseLeftButtonDown += WeakFocusTarget_MouseLeftButtonDown;
            textBox.KeyDown += WeakFocusTarget_KeyDown;
        }

        static void WeakFocusTarget_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            textBox.ReleaseMouseCapture();

            textBox.PreviewMouseLeftButtonDown -= WeakFocusTarget_MouseLeftButtonDown;
            textBox.KeyDown -= WeakFocusTarget_KeyDown;
        }

        static void WeakFocusTarget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            Point point = e.GetPosition(textBox);

            if (point.X < 0 || point.Y < 0 || point.X > textBox.ActualWidth || point.Y > textBox.ActualHeight)
            {
                Keyboard.ClearFocus();
            }
        }

        static void WeakFocusTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();

                if (GetHandleEnter((UIElement)sender))
                {
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region HandleEnter

        public static bool GetHandleEnter(UIElement element)
        {
            return (bool)element.GetValue(HandleEnterProperty);
        }

        public static void SetHandleEnter(UIElement element, bool value)
        {
            element.SetValue(HandleEnterProperty, value);
        }

        public static readonly DependencyProperty HandleEnterProperty = DependencyProperty.RegisterAttached(
            "HandleEnter",
            typeof(bool),
            typeof(TextBoxBehavior),
            new UIPropertyMetadata(true)
        );

        #endregion

    }
}
