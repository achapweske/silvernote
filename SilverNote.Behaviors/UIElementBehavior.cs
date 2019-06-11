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
using System.Diagnostics;

namespace SilverNote.Behaviors
{
    public static class UIElementBehavior
    {
        #region FocusWhenEnterPressed

        public static readonly DependencyProperty FocusWhenEnterReleasedProperty = DependencyProperty.RegisterAttached(
            "FocusWhenEnterReleased",
            typeof(UIElement),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, FocusWhenEnterReleased_PropertyChanged)
        );

        public static UIElement GetFocusWhenEnterReleased(DependencyObject element)
        {
            return (UIElement)element.GetValue(FocusWhenEnterReleasedProperty);
        }

        public static void SetFocusWhenEnterReleased(DependencyObject element, UIElement value)
        {
            element.SetValue(FocusWhenEnterReleasedProperty, value);
        }

        static void FocusWhenEnterReleased_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = (ComboBox)dep;
            var element = (UIElement)e.NewValue;

            if (element != null)
            {
                comboBox.KeyUp += FocusWhenEnterReleasedTarget_EnterReleased;
            }
            else
            {
                comboBox.KeyUp -= FocusWhenEnterReleasedTarget_EnterReleased;
            }
        }

        static void FocusWhenEnterReleasedTarget_EnterReleased(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            UIElement element = GetFocusWhenEnterReleased((DependencyObject)sender);

            if (element != null)
            {
                element.Focus();
            }
        }

        #endregion

        #region IsFocused

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached(
            "IsFocused",
            typeof(bool),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(false, OnIsFocusedChanged)
        );

        static void OnIsFocusedChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = dep as UIElement;

            if (!(e.NewValue is bool))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                element.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (GetIsFocused(element))
                    {
                        element.Focus();
                    }
                }));
            }
        }

        public static bool GetIsFocused(UIElement element)
        {
            return (bool)element.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(UIElement element, bool value)
        {
            element.SetValue(IsFocusedProperty, value);
        }

        #endregion

        #region IsFocusedTwoWay

        public static readonly DependencyProperty IsFocusedTwoWayProperty = DependencyProperty.RegisterAttached(
            "IsFocusedTwoWay",
            typeof(bool),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(false, OnIsFocusedTwoWayChanged)
        );

        static void OnIsFocusedTwoWayChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = dep as UIElement;

            if (!(e.NewValue is bool))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                element.GotFocus += IsFocusedTwoWayTarget_FocusChanged;
                element.LostFocus += IsFocusedTwoWayTarget_FocusChanged;
            }
            else
            {
                element.GotFocus -= IsFocusedTwoWayTarget_FocusChanged;
                element.LostFocus -= IsFocusedTwoWayTarget_FocusChanged;
            }

            SetIsFocused(element, element.IsFocused);
        }

        private static void IsFocusedTwoWayTarget_FocusChanged(object sender, RoutedEventArgs e)
        {
            UIElement element = (UIElement)sender;

            SetIsFocused(element, element.IsFocused);
        }

        public static bool GetIsFocusedTwoWay(UIElement element)
        {
            return (bool)element.GetValue(IsFocusedTwoWayProperty);
        }

        public static void SetIsFocusedTwoWay(UIElement element, bool value)
        {
            element.SetValue(IsFocusedTwoWayProperty, value);
        }

        #endregion

        #region LostFocusCommand

        public static ICommand GetLostFocusCommand(UIElement element)
        {
            return (ICommand)element.GetValue(LostFocusCommandProperty);
        }

        public static void SetLostFocusCommand(UIElement element, ICommand value)
        {
            element.SetValue(LostFocusCommandProperty, value);
        }

        public static readonly DependencyProperty LostFocusCommandProperty = DependencyProperty.RegisterAttached (
            "LostFocusCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnLostFocusCommandChanged)
        );

        static void OnLostFocusCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.LostFocus -= LostFocusCommandTarget_LostFocus;
            }

            if (e.NewValue != null)
            {
                commandTarget.LostFocus += LostFocusCommandTarget_LostFocus;
            }
        }

        static void LostFocusCommandTarget_LostFocus(object sender, RoutedEventArgs e)
        {
            var element = sender as UIElement;
            var command = GetLostFocusCommand(element);
            var parameter = GetLostFocusCommandParameter(element);

            command.Execute(parameter);
        }

        #endregion // IsBroughtIntoViewWhenSelected

        #region LostFocusCommandParameter

        public static object GetLostFocusCommandParameter(UIElement element)
        {
            return element.GetValue(LostFocusCommandParameterProperty);
        }

        public static void SetLostFocusCommandParameter(UIElement element, object value)
        {
            element.SetValue(LostFocusCommandParameterProperty, value);
        }

        public static readonly DependencyProperty LostFocusCommandParameterProperty = DependencyProperty.RegisterAttached(
            "LostFocusCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        #endregion

        #region LostKeyboardFocusCommand

        public static ICommand GetLostKeyboardFocusCommand(UIElement element)
        {
            return (ICommand)element.GetValue(LostKeyboardFocusCommandProperty);
        }

        public static void SetLostKeyboardFocusCommand(UIElement element, ICommand value)
        {
            element.SetValue(LostKeyboardFocusCommandProperty, value);
        }

        public static readonly DependencyProperty LostKeyboardFocusCommandProperty = DependencyProperty.RegisterAttached(
            "LostKeyboardFocusCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnLostKeyboardFocusCommandChanged)
        );

        static void OnLostKeyboardFocusCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.LostKeyboardFocus -= LostKeyboardFocusCommandTarget_LostKeyboardFocus;
            }

            if (e.NewValue != null)
            {
                commandTarget.LostKeyboardFocus += LostKeyboardFocusCommandTarget_LostKeyboardFocus;
            }
        }

        static void LostKeyboardFocusCommandTarget_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            var element = sender as UIElement;
            var command = GetLostKeyboardFocusCommand(element);
            var parameter = GetLostKeyboardFocusCommandParameter(element);

            command.Execute(parameter);
        }

        #endregion

        #region LostKeyboardFocusCommandParameter

        public static object GetLostKeyboardFocusCommandParameter(UIElement element)
        {
            return element.GetValue(LostKeyboardFocusCommandParameterProperty);
        }

        public static void SetLostKeyboardFocusCommandParameter(UIElement element, object value)
        {
            element.SetValue(LostKeyboardFocusCommandParameterProperty, value);
        }

        public static readonly DependencyProperty LostKeyboardFocusCommandParameterProperty = DependencyProperty.RegisterAttached(
            "LostKeyboardFocusCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        #endregion

        #region FocusWhenMouseRightButtonDown

        public static readonly DependencyProperty FocusWhenMouseRightButtonDownProperty = DependencyProperty.RegisterAttached("FocusWhenMouseRightButtonDown", typeof(bool), typeof(UIElementBehavior), new UIPropertyMetadata(false, new PropertyChangedCallback(UIElementBehavior.OnFocusWhenMouseRightButtonDownChanged)));
		
        public static bool GetFocusWhenMouseRightButtonDown(TreeViewItem element)
        {
            return (bool)element.GetValue(UIElementBehavior.FocusWhenMouseRightButtonDownProperty);
        }

        public static void SetFocusWhenMouseRightButtonDown(TreeViewItem element, bool value)
        {
            element.SetValue(UIElementBehavior.FocusWhenMouseRightButtonDownProperty, value);
        }

        private static void OnFocusWhenMouseRightButtonDownChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement uIElement = dep as UIElement;
            if (uIElement == null)
            {
                return;
            }
            if (!(e.NewValue is bool))
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                uIElement.MouseRightButtonDown += new MouseButtonEventHandler(UIElementBehavior.FocusWhenMouseRightButtonDownTarget_MouseRightButtonDown);
            }
        }
        private static void FocusWhenMouseRightButtonDownTarget_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement uIElement = (UIElement)sender;
            uIElement.Focus();
            e.Handled = true;
        }

        #endregion

        #region MouseLeftButtonDownCommand

        public static ICommand GetMouseLeftButtonDownCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(MouseLeftButtonDownCommandProperty);
        }

        public static void SetMouseLeftButtonDownCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(MouseLeftButtonDownCommandProperty, value);
        }

        public static readonly DependencyProperty MouseLeftButtonDownCommandProperty = DependencyProperty.RegisterAttached(
            "MouseLeftButtonDownCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnMouseLeftButtonDownCommandChanged)
        );

        static void OnMouseLeftButtonDownCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.MouseLeftButtonDown -= MouseLeftButtonDownCommandTarget_MouseLeftButtonDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.MouseLeftButtonDown += MouseLeftButtonDownCommandTarget_MouseLeftButtonDown;
            }
        }

        static void MouseLeftButtonDownCommandTarget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;
            var command = GetMouseLeftButtonDownCommand(element);
            var parameter = GetMouseLeftButtonDownCommandParameter(element);

            command.Execute(parameter);
        }

        #endregion

        #region MouseLeftButtonDownCommandParameter

        public static object GetMouseLeftButtonDownCommandParameter(DependencyObject element)
        {
            return element.GetValue(MouseLeftButtonDownCommandParameterProperty);
        }

        public static void SetMouseLeftButtonDownCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(MouseLeftButtonDownCommandParameterProperty, value);
        }

        public static readonly DependencyProperty MouseLeftButtonDownCommandParameterProperty = DependencyProperty.RegisterAttached(
            "MouseLeftButtonDownCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        #endregion

        #region PreviewMouseLeftButtonDownCommand

        public static ICommand GetPreviewMouseLeftButtonDownCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(PreviewMouseLeftButtonDownCommandProperty);
        }

        public static void SetPreviewMouseLeftButtonDownCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(PreviewMouseLeftButtonDownCommandProperty, value);
        }

        public static readonly DependencyProperty PreviewMouseLeftButtonDownCommandProperty = DependencyProperty.RegisterAttached(
            "PreviewMouseLeftButtonDownCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnPreviewMouseLeftButtonDownCommandChanged)
        );

        static void OnPreviewMouseLeftButtonDownCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.PreviewMouseLeftButtonDown -= PreviewMouseLeftButtonDownCommandTarget_PreviewMouseLeftButtonDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDownCommandTarget_PreviewMouseLeftButtonDown;
            }
        }

        static void PreviewMouseLeftButtonDownCommandTarget_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;
            var command = GetPreviewMouseLeftButtonDownCommand(element);
            var parameter = GetPreviewMouseLeftButtonDownCommandParameter(element);

            command.Execute(parameter);
        }

        #endregion

        #region PreviewMouseLeftButtonDownCommandParameter

        public static object GetPreviewMouseLeftButtonDownCommandParameter(DependencyObject element)
        {
            return element.GetValue(PreviewMouseLeftButtonDownCommandParameterProperty);
        }

        public static void SetPreviewMouseLeftButtonDownCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(PreviewMouseLeftButtonDownCommandParameterProperty, value);
        }

        public static readonly DependencyProperty PreviewMouseLeftButtonDownCommandParameterProperty = DependencyProperty.RegisterAttached(
            "PreviewMouseLeftButtonDownCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        #endregion

        #region MouseDoubleClickCommand

        public static ICommand GetMouseDoubleClickCommand(DependencyObject dep)
        {
            return (ICommand)dep.GetValue(MouseDoubleClickCommandProperty);
        }

        public static void SetMouseDoubleClickCommand(DependencyObject dep, ICommand value)
        {
            dep.SetValue(MouseDoubleClickCommandProperty, value);
        }

        public static readonly DependencyProperty MouseDoubleClickCommandProperty = DependencyProperty.RegisterAttached(
            "MouseDoubleClickCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnMouseDoubleClickCommandChanged)
        );

        static void OnMouseDoubleClickCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            if (!(dep is UIElement))
            {
                Debug.Write("Error: " + e.Property + " cannot be applied to " + dep.GetType());
                return;
            }

            var commandTarget = (UIElement)dep;

            if (e.OldValue != null)
            {
                commandTarget.MouseLeftButtonDown -= MouseDoubleClickCommandTarget_MouseLeftButtonDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.MouseLeftButtonDown += MouseDoubleClickCommandTarget_MouseLeftButtonDown;
            }
        }

        static void MouseDoubleClickCommandTarget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && !e.Handled)
            {
                var target = sender as DependencyObject;
                var command = GetMouseDoubleClickCommand(target);
                var parameter = GetMouseDoubleClickCommandParameter(target);

                command.Execute(parameter);

                e.Handled = true;
            }
        }

        #endregion

        #region MouseDoubleClickCommandParameter

        public static object GetMouseDoubleClickCommandParameter(DependencyObject dep)
        {
            return dep.GetValue(MouseDoubleClickCommandParameterProperty);
        }

        public static void SetMouseDoubleClickCommandParameter(DependencyObject dep, object value)
        {
            dep.SetValue(MouseDoubleClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty MouseDoubleClickCommandParameterProperty = DependencyProperty.RegisterAttached(
            "MouseDoubleClickCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        #endregion

        #region PreviewMouseDoubleClickCommand

        public static readonly DependencyProperty PreviewMouseDoubleClickCommandProperty = DependencyProperty.RegisterAttached(
            "PreviewMouseDoubleClickCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnPreviewMouseDoubleClickCommandChanged)
        );

        static void OnPreviewMouseDoubleClickCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            if (!(dep is UIElement))
            {
                Debug.Write("Error: " + e.Property + " cannot be applied to " + dep.GetType());
                return;
            }

            var commandTarget = (UIElement)dep;

            if (e.OldValue != null)
            {
                commandTarget.PreviewMouseLeftButtonDown -= PreviewMouseDoubleClickCommandTarget_PreviewMouseLeftButtonDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewMouseLeftButtonDown += PreviewMouseDoubleClickCommandTarget_PreviewMouseLeftButtonDown;
            }
        }

        static void PreviewMouseDoubleClickCommandTarget_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && !e.Handled)
            {
                var target = sender as DependencyObject;
                var command = GetPreviewMouseDoubleClickCommand(target);
                var parameter = GetPreviewMouseDoubleClickCommandParameter(target);

                command.Execute(parameter);

                e.Handled = true;
            }
        }

        public static ICommand GetPreviewMouseDoubleClickCommand(DependencyObject dep)
        {
            return (ICommand)dep.GetValue(PreviewMouseDoubleClickCommandProperty);
        }

        public static void SetPreviewMouseDoubleClickCommand(DependencyObject dep, ICommand value)
        {
            dep.SetValue(PreviewMouseDoubleClickCommandProperty, value);
        }

        #endregion

        #region PreviewMouseDoubleClickCommandParameter

        public static object GetPreviewMouseDoubleClickCommandParameter(DependencyObject dep)
        {
            return dep.GetValue(PreviewMouseDoubleClickCommandParameterProperty);
        }

        public static void SetPreviewMouseDoubleClickCommandParameter(DependencyObject dep, object value)
        {
            dep.SetValue(PreviewMouseDoubleClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty PreviewMouseDoubleClickCommandParameterProperty = DependencyProperty.RegisterAttached(
            "PreviewMouseDoubleClickCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        #endregion


        #region PreviewEscapeCommand

        public static readonly DependencyProperty PreviewEscapeCommandProperty = DependencyProperty.RegisterAttached(
            "PreviewEscapeCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnPreviewEscapeCommandChanged)
        );

        public static ICommand GetPreviewEscapeCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(PreviewEscapeCommandProperty);
        }

        public static void SetPreviewEscapeCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(PreviewEscapeCommandProperty, value);
        }

        static void OnPreviewEscapeCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.PreviewKeyDown -= PreviewEscapeCommandTarget_KeyDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewKeyDown += PreviewEscapeCommandTarget_KeyDown;
            }
        }

        static void PreviewEscapeCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var element = sender as UIElement;
                var command = GetPreviewEscapeCommand(element);
                var parameter = GetPreviewEscapeCommandParameter(element);

                command.Execute(parameter);

                e.Handled = true;
            }
        }

        #endregion

        #region PreviewEscapeCommandParameter

        public static readonly DependencyProperty PreviewEscapeCommandParameterProperty = DependencyProperty.RegisterAttached(
            "PreviewEscapeCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        public static object GetPreviewEscapeCommandParameter(DependencyObject element)
        {
            return element.GetValue(PreviewEscapeCommandParameterProperty);
        }

        public static void SetPreviewEscapeCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(PreviewEscapeCommandParameterProperty, value);
        }

        #endregion

        #region PreviewEnterCommand

        public static readonly DependencyProperty PreviewEnterCommandProperty = DependencyProperty.RegisterAttached(
            "PreviewEnterCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnPreviewEnterCommandChanged)
        );

        public static ICommand GetPreviewEnterCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(PreviewEnterCommandProperty);
        }

        public static void SetPreviewEnterCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(PreviewEnterCommandProperty, value);
        }

        static void OnPreviewEnterCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.PreviewKeyDown -= PreviewEnterCommandTarget_KeyDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewKeyDown += PreviewEnterCommandTarget_KeyDown;
            }
        }

        static void PreviewEnterCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                var element = sender as UIElement;
                var command = GetPreviewEnterCommand(element);
                var parameter = GetPreviewEnterCommandParameter(element);

                command.Execute(parameter);

                e.Handled = true;
            }
        }

        #endregion

        #region PreviewEnterCommandParameter

        public static readonly DependencyProperty PreviewEnterCommandParameterProperty = DependencyProperty.RegisterAttached(
            "PreviewEnterCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        public static object GetPreviewEnterCommandParameter(DependencyObject element)
        {
            return element.GetValue(PreviewEnterCommandParameterProperty);
        }

        public static void SetPreviewEnterCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(PreviewEnterCommandParameterProperty, value);
        }

        #endregion

        #region PreviewShiftEnterCommand

        public static readonly DependencyProperty PreviewShiftEnterCommandProperty = DependencyProperty.RegisterAttached(
            "PreviewShiftEnterCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnPreviewShiftEnterCommandChanged)
        );

        public static ICommand GetPreviewShiftEnterCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(PreviewShiftEnterCommandProperty);
        }

        public static void SetPreviewShiftEnterCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(PreviewShiftEnterCommandProperty, value);
        }

        static void OnPreviewShiftEnterCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.PreviewKeyDown -= PreviewShiftEnterCommandTarget_KeyDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewKeyDown += PreviewShiftEnterCommandTarget_KeyDown;
            }
        }

        static void PreviewShiftEnterCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var element = sender as UIElement;
                var command = GetPreviewShiftEnterCommand(element);
                var parameter = GetPreviewShiftEnterCommandParameter(element);

                command.Execute(parameter);

                e.Handled = true;
            }
        }

        #endregion

        #region PreviewShiftEnterCommandParameter

        public static readonly DependencyProperty PreviewShiftEnterCommandParameterProperty = DependencyProperty.RegisterAttached(
            "PreviewShiftEnterCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        public static object GetPreviewShiftEnterCommandParameter(DependencyObject element)
        {
            return element.GetValue(PreviewShiftEnterCommandParameterProperty);
        }

        public static void SetPreviewShiftEnterCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(PreviewShiftEnterCommandParameterProperty, value);
        }

        #endregion

        #region PreviewUpCommand

        public static readonly DependencyProperty PreviewUpCommandProperty = DependencyProperty.RegisterAttached(
            "PreviewUpCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnPreviewUpCommandChanged)
        );

        public static ICommand GetPreviewUpCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(PreviewUpCommandProperty);
        }

        public static void SetPreviewUpCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(PreviewUpCommandProperty, value);
        }

        static void OnPreviewUpCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.PreviewKeyDown -= PreviewUpCommandTarget_KeyDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewKeyDown += PreviewUpCommandTarget_KeyDown;
            }
        }

        static void PreviewUpCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                var element = sender as UIElement;
                var command = GetPreviewUpCommand(element);
                var parameter = GetPreviewUpCommandParameter(element);

                command.Execute(parameter);

                e.Handled = true;
            }
        }

        #endregion

        #region PreviewUpCommandParameter

        public static readonly DependencyProperty PreviewUpCommandParameterProperty = DependencyProperty.RegisterAttached(
            "PreviewUpCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        public static object GetPreviewUpCommandParameter(DependencyObject element)
        {
            return element.GetValue(PreviewUpCommandParameterProperty);
        }

        public static void SetPreviewUpCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(PreviewUpCommandParameterProperty, value);
        }

        #endregion

        #region PreviewDownCommand

        public static readonly DependencyProperty PreviewDownCommandProperty = DependencyProperty.RegisterAttached(
            "PreviewDownCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnPreviewDownCommandChanged)
        );

        public static ICommand GetPreviewDownCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(PreviewDownCommandProperty);
        }

        public static void SetPreviewDownCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(PreviewDownCommandProperty, value);
        }

        static void OnPreviewDownCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.PreviewKeyDown -= PreviewDownCommandTarget_KeyDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewKeyDown += PreviewDownCommandTarget_KeyDown;
            }
        }

        static void PreviewDownCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                var element = sender as UIElement;
                var command = GetPreviewDownCommand(element);
                var parameter = GetPreviewDownCommandParameter(element);

                command.Execute(parameter);

                e.Handled = true;
            }
        }

        #endregion

        #region PreviewDownCommandParameter

        public static readonly DependencyProperty PreviewDownCommandParameterProperty = DependencyProperty.RegisterAttached(
            "PreviewDownCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        public static object GetPreviewDownCommandParameter(DependencyObject element)
        {
            return element.GetValue(PreviewDownCommandParameterProperty);
        }

        public static void SetPreviewDownCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(PreviewDownCommandParameterProperty, value);
        }

        #endregion

        #region PreviewShiftUpCommand

        public static readonly DependencyProperty PreviewShiftUpCommandProperty = DependencyProperty.RegisterAttached(
            "PreviewShiftUpCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnPreviewShiftUpCommandChanged)
        );

        public static ICommand GetPreviewShiftUpCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(PreviewShiftUpCommandProperty);
        }

        public static void SetPreviewShiftUpCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(PreviewShiftUpCommandProperty, value);
        }

        static void OnPreviewShiftUpCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.PreviewKeyDown -= PreviewShiftUpCommandTarget_KeyDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewKeyDown += PreviewShiftUpCommandTarget_KeyDown;
            }
        }

        static void PreviewShiftUpCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                var element = sender as UIElement;
                var command = GetPreviewShiftUpCommand(element);
                var parameter = GetPreviewShiftUpCommandParameter(element);

                command.Execute(parameter);

                e.Handled = true;
            }
        }

        #endregion

        #region PreviewShiftUpCommandParameter

        public static readonly DependencyProperty PreviewShiftUpCommandParameterProperty = DependencyProperty.RegisterAttached(
            "PreviewShiftUpCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        public static object GetPreviewShiftUpCommandParameter(DependencyObject element)
        {
            return element.GetValue(PreviewShiftUpCommandParameterProperty);
        }

        public static void SetPreviewShiftUpCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(PreviewShiftUpCommandParameterProperty, value);
        }

        #endregion

        #region PreviewShiftDownCommand

        public static readonly DependencyProperty PreviewShiftDownCommandProperty = DependencyProperty.RegisterAttached(
            "PreviewShiftDownCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, OnPreviewShiftDownCommandChanged)
        );

        public static ICommand GetPreviewShiftDownCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(PreviewShiftDownCommandProperty);
        }

        public static void SetPreviewShiftDownCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(PreviewShiftDownCommandProperty, value);
        }

        static void OnPreviewShiftDownCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement commandTarget = dep as UIElement;

            if (e.OldValue != null)
            {
                commandTarget.PreviewKeyDown -= PreviewShiftDownCommandTarget_KeyDown;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewKeyDown += PreviewShiftDownCommandTarget_KeyDown;
            }
        }

        static void PreviewShiftDownCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                var element = sender as UIElement;
                var command = GetPreviewShiftDownCommand(element);
                var parameter = GetPreviewShiftDownCommandParameter(element);

                command.Execute(parameter);

                e.Handled = true;
            }
        }

        #endregion

        #region PreviewShiftDownCommandParameter

        public static readonly DependencyProperty PreviewShiftDownCommandParameterProperty = DependencyProperty.RegisterAttached(
            "PreviewShiftDownCommandParameter",
            typeof(object),
            typeof(UIElementBehavior)
        );

        public static object GetPreviewShiftDownCommandParameter(DependencyObject element)
        {
            return element.GetValue(PreviewShiftDownCommandParameterProperty);
        }

        public static void SetPreviewShiftDownCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(PreviewShiftDownCommandParameterProperty, value);
        }

        #endregion

        #region ScrollOnDrag

        public static readonly DependencyProperty ScrollOnDragProperty = DependencyProperty.RegisterAttached(
            "ScrollOnDrag",
            typeof(bool),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(false, ScrollOnDrag_PropertyChanged)
        );

        static void ScrollOnDrag_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = dep as UIElement;

            if (element == null)
            {
                return;
            }

            if (!(e.NewValue is bool))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                element.MouseLeave += ScrollOnDragTarget_MouseLeave;
            }
            else
            {
                element.MouseLeave -= ScrollOnDragTarget_MouseLeave;
            }
        }

        static void ScrollOnDragTarget_MouseLeave(object sender, MouseEventArgs e)
        {
            UIElement element = (UIElement)sender;

            var scrollViewer = LayoutHelper.GetAncestor<ScrollViewer>(element);
            if (scrollViewer == null)
            {
                return;
            }

            Point position = e.GetPosition(scrollViewer);

            if (e.LeftButton.HasFlag(MouseButtonState.Pressed) && !IsPointInViewport(scrollViewer, position))
            {
                element.MouseLeave -= ScrollOnDragTarget_MouseLeave;

                element.CaptureMouse();

                element.MouseLeftButtonUp += ScrollOnDragTarget_MouseLeftButtonUp;
                element.MouseMove += ScrollOnDragTarget_MouseMove;
            }
        }

        static void ScrollOnDragTarget_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            UIElement element = (UIElement)sender;

            element.MouseLeftButtonUp -= ScrollOnDragTarget_MouseLeftButtonUp;
            element.MouseMove -= ScrollOnDragTarget_MouseMove;

            element.ReleaseMouseCapture();

            element.MouseLeave += ScrollOnDragTarget_MouseLeave;
        }

        static void ScrollOnDragTarget_MouseMove(object sender, MouseEventArgs e)
        {
            UIElement element = (UIElement)sender;

            var scrollViewer = LayoutHelper.GetAncestor<ScrollViewer>(element);
            if (scrollViewer == null)
            {
                return;
            }

            Point position = e.GetPosition(scrollViewer);

            if (IsPointInViewport(scrollViewer, position))
            {
                element.MouseLeftButtonUp -= ScrollOnDragTarget_MouseLeftButtonUp;
                element.MouseMove -= ScrollOnDragTarget_MouseMove;

                element.ReleaseMouseCapture();

                element.MouseLeave += ScrollOnDragTarget_MouseLeave;
            }
        }

        static bool IsPointInViewport(ScrollViewer scrollViewer, Point point)
        {
            if (point.X > scrollViewer.BorderThickness.Left + scrollViewer.Padding.Left &&
                point.X < scrollViewer.ViewportWidth &&
                point.Y > scrollViewer.BorderThickness.Top + scrollViewer.Padding.Top &&
                point.Y < scrollViewer.ViewportHeight)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool GetScrollOnDrag(UIElement element)
        {
            return (bool)element.GetValue(ScrollOnDragProperty);
        }

        public static void SetScrollOnDrag(UIElement element, bool value)
        {
            element.SetValue(ScrollOnDragProperty, value);
        }

        #endregion

        #region DeleteCommand

        public static readonly DependencyProperty DeleteCommandProperty = DependencyProperty.RegisterAttached("DeleteCommand", typeof(ICommand), typeof(UIElementBehavior), new UIPropertyMetadata(null, new PropertyChangedCallback(UIElementBehavior.DeleteCommandProperty_PropertyChanged)));

        public static ICommand GetDeleteCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(UIElementBehavior.DeleteCommandProperty);
        }

        public static void SetDeleteCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(UIElementBehavior.DeleteCommandProperty, value);
        }

        private static void DeleteCommandProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement uIElement = dep as UIElement;
            if (e.OldValue != null)
            {
                uIElement.KeyDown -= new KeyEventHandler(UIElementBehavior.DeleteCommandTarget_KeyDown);
            }
            if (e.NewValue != null)
            {
                uIElement.KeyDown += new KeyEventHandler(UIElementBehavior.DeleteCommandTarget_KeyDown);
            }
        }

        private static void DeleteCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                UIElement element = sender as UIElement;
                ICommand deleteCommand = UIElementBehavior.GetDeleteCommand(element);
                object deleteCommandParameter = UIElementBehavior.GetDeleteCommandParameter(element);
                deleteCommand.Execute(deleteCommandParameter);
                e.Handled = true;
            }
        }
	
        #endregion

        #region DeleteCommandParameter
        
        public static readonly DependencyProperty DeleteCommandParameterProperty = DependencyProperty.RegisterAttached("DeleteCommandParameter", typeof(object), typeof(UIElementBehavior));

        public static object GetDeleteCommandParameter(DependencyObject element)
        {
            return element.GetValue(UIElementBehavior.DeleteCommandParameterProperty);
        }

        public static void SetDeleteCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(UIElementBehavior.DeleteCommandParameterProperty, value);
        }
        
        #endregion        
        
        #region EnterCommand
        
        public static readonly DependencyProperty EnterCommandProperty = DependencyProperty.RegisterAttached(
            "EnterCommand", 
            typeof(ICommand), 
            typeof(UIElementBehavior), 
            new UIPropertyMetadata(null, EnterCommandProperty_PropertyChanged));

        public static ICommand GetEnterCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(UIElementBehavior.EnterCommandProperty);
        }

        public static void SetEnterCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(UIElementBehavior.EnterCommandProperty, value);
        }
        
        private static void EnterCommandProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = dep as UIElement;
            if (element == null)
            {
                throw new Exception("EnterCommand target must be a UIElement");
            }

            if (e.OldValue != null)
            {
                element.KeyDown -= EnterCommandTarget_KeyDown;
            }
            if (e.NewValue != null)
            {
                element.KeyDown += EnterCommandTarget_KeyDown;
            }
        }
        
        private static void EnterCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                var element = (UIElement)sender;
                var command = GetEnterCommand(element);
                object parameter = GetEnterCommandParameter(element);
                command.Execute(parameter);
                e.Handled = true;
            }
        }

        #endregion        
        
        #region EnterCommandParameter

        public static readonly DependencyProperty EnterCommandParameterProperty = DependencyProperty.RegisterAttached("EnterCommandParameter", typeof(object), typeof(UIElementBehavior));

        public static object GetEnterCommandParameter(DependencyObject element)
        {
            return element.GetValue(UIElementBehavior.EnterCommandParameterProperty);
        }

        public static void SetEnterCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(UIElementBehavior.EnterCommandParameterProperty, value);
        }

        #endregion        
        
        #region MinusKeyDownCommand

        public static readonly DependencyProperty MinusKeyDownCommandProperty = DependencyProperty.RegisterAttached("MinusKeyDownCommand", typeof(ICommand), typeof(UIElementBehavior), new UIPropertyMetadata(null, new PropertyChangedCallback(UIElementBehavior.MinusKeyDownCommandProperty_PropertyChanged)));

        public static ICommand GetMinusKeyDownCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(UIElementBehavior.MinusKeyDownCommandProperty);
        }
        
        public static void SetMinusKeyDownCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(UIElementBehavior.MinusKeyDownCommandProperty, value);
        }

        private static void MinusKeyDownCommandProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement uIElement = dep as UIElement;
            if (e.OldValue != null)
            {
                uIElement.KeyDown -= new KeyEventHandler(UIElementBehavior.MinusKeyDownCommandTarget_KeyDown);
            }
            if (e.NewValue != null)
            {
                uIElement.KeyDown += new KeyEventHandler(UIElementBehavior.MinusKeyDownCommandTarget_KeyDown);
            }
        }
        
        private static void MinusKeyDownCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemMinus && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                UIElement element = sender as UIElement;
                ICommand minusKeyDownCommand = UIElementBehavior.GetMinusKeyDownCommand(element);
                object minusKeyDownCommandParameter = UIElementBehavior.GetMinusKeyDownCommandParameter(element);
                minusKeyDownCommand.Execute(minusKeyDownCommandParameter);
                e.Handled = true;
            }
        }

        #endregion        
        
        #region MinusKeyDownCommandParameter
        
        public static readonly DependencyProperty MinusKeyDownCommandParameterProperty = DependencyProperty.RegisterAttached("MinusKeyDownCommandParameter", typeof(object), typeof(UIElementBehavior));

        public static object GetMinusKeyDownCommandParameter(DependencyObject element)
        {
            return element.GetValue(UIElementBehavior.MinusKeyDownCommandParameterProperty);
        }
        public static void SetMinusKeyDownCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(UIElementBehavior.MinusKeyDownCommandParameterProperty, value);
        }

        #endregion		

        #region CtrlXCommand

        public static readonly DependencyProperty CtrlXCommandProperty = DependencyProperty.RegisterAttached(
            "CtrlXCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, CtrlXCommandProperty_PropertyChanged));

        public static ICommand GetCtrlXCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(UIElementBehavior.CtrlXCommandProperty);
        }

        public static void SetCtrlXCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(UIElementBehavior.CtrlXCommandProperty, value);
        }

        private static void CtrlXCommandProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = dep as UIElement;
            if (element == null)
            {
                throw new Exception("CtrlXCommand target must be a UIElement");
            }

            if (e.OldValue != null)
            {
                element.KeyDown -= CtrlXCommandTarget_KeyDown;
            }
            if (e.NewValue != null)
            {
                element.KeyDown += CtrlXCommandTarget_KeyDown;
            }
        }

        private static void CtrlXCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.X && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                var element = (UIElement)sender;
                var command = GetCtrlXCommand(element);
                object parameter = GetCtrlXCommandParameter(element);
                command.Execute(parameter);
                e.Handled = true;
            }
        }

        #endregion

        #region CtrlXCommandParameter

        public static readonly DependencyProperty CtrlXCommandParameterProperty = DependencyProperty.RegisterAttached("CtrlXCommandParameter", typeof(object), typeof(UIElementBehavior));

        public static object GetCtrlXCommandParameter(DependencyObject element)
        {
            return element.GetValue(UIElementBehavior.CtrlXCommandParameterProperty);
        }

        public static void SetCtrlXCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(UIElementBehavior.CtrlXCommandParameterProperty, value);
        }

        #endregion        

        #region CtrlCCommand

        public static readonly DependencyProperty CtrlCCommandProperty = DependencyProperty.RegisterAttached(
            "CtrlCCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, CtrlCCommandProperty_PropertyChanged));

        public static ICommand GetCtrlCCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(UIElementBehavior.CtrlCCommandProperty);
        }

        public static void SetCtrlCCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(UIElementBehavior.CtrlCCommandProperty, value);
        }

        private static void CtrlCCommandProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = dep as UIElement;
            if (element == null)
            {
                throw new Exception("CtrlCCommand target must be a UIElement");
            }

            if (e.OldValue != null)
            {
                element.KeyDown -= CtrlCCommandTarget_KeyDown;
            }
            if (e.NewValue != null)
            {
                element.KeyDown += CtrlCCommandTarget_KeyDown;
            }
        }

        private static void CtrlCCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                var element = (UIElement)sender;
                var command = GetCtrlCCommand(element);
                object parameter = GetCtrlCCommandParameter(element);
                command.Execute(parameter);
                e.Handled = true;
            }
        }

        #endregion

        #region CtrlCCommandParameter

        public static readonly DependencyProperty CtrlCCommandParameterProperty = DependencyProperty.RegisterAttached("CtrlCCommandParameter", typeof(object), typeof(UIElementBehavior));

        public static object GetCtrlCCommandParameter(DependencyObject element)
        {
            return element.GetValue(UIElementBehavior.CtrlCCommandParameterProperty);
        }

        public static void SetCtrlCCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(UIElementBehavior.CtrlCCommandParameterProperty, value);
        }

        #endregion        

        #region CtrlVCommand

        public static readonly DependencyProperty CtrlVCommandProperty = DependencyProperty.RegisterAttached(
            "CtrlVCommand",
            typeof(ICommand),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, CtrlVCommandProperty_PropertyChanged));

        public static ICommand GetCtrlVCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(UIElementBehavior.CtrlVCommandProperty);
        }

        public static void SetCtrlVCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(UIElementBehavior.CtrlVCommandProperty, value);
        }

        private static void CtrlVCommandProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = dep as UIElement;
            if (element == null)
            {
                throw new Exception("CtrlVCommand target must be a UIElement");
            }

            if (e.OldValue != null)
            {
                element.KeyDown -= CtrlVCommandTarget_KeyDown;
            }
            if (e.NewValue != null)
            {
                element.KeyDown += CtrlVCommandTarget_KeyDown;
            }
        }

        private static void CtrlVCommandTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                var element = (UIElement)sender;
                var command = GetCtrlVCommand(element);
                object parameter = GetCtrlVCommandParameter(element);
                command.Execute(parameter);
                e.Handled = true;
            }
        }

        #endregion

        #region CtrlVCommandParameter

        public static readonly DependencyProperty CtrlVCommandParameterProperty = DependencyProperty.RegisterAttached("CtrlVCommandParameter", typeof(object), typeof(UIElementBehavior));

        public static object GetCtrlVCommandParameter(DependencyObject element)
        {
            return element.GetValue(UIElementBehavior.CtrlVCommandParameterProperty);
        }

        public static void SetCtrlVCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(UIElementBehavior.CtrlVCommandParameterProperty, value);
        }

        #endregion        

        #region CommandBindings

        public static readonly DependencyProperty CommandBindingsProperty = DependencyProperty.RegisterAttached(
            "CommandBindings", 
            typeof(CommandBindingCollection), 
            typeof(UIElementBehavior), 
            new UIPropertyMetadata(null, CommandBindingsProperty_PropertyChanged));

        public static CommandBindingCollection GetCommandBindings(DependencyObject element)
        {
            return (CommandBindingCollection)element.GetValue(CommandBindingsProperty);
        }

        public static void SetCommandBindings(DependencyObject element, CommandBindingCollection value)
        {
            element.SetValue(CommandBindingsProperty, value);
        }

        private static void CommandBindingsProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var element = dep as FrameworkElement;

            var oldValue = e.OldValue as CommandBindingCollection;
            if (oldValue != null)
            {
                foreach (CommandBinding binding in oldValue)
                {
                    element.CommandBindings.Remove(binding);
                }
            }

            if (!element.IsInitialized)
            {
                element.Initialized -= CommandBindingsTarget_Initialized;
                element.Initialized += CommandBindingsTarget_Initialized;
                return;
            }

            CommandBindingsTarget_Update(element);
        }

        private static void CommandBindingsTarget_Initialized(object sender, EventArgs e)
        {
            CommandBindingsTarget_Update((DependencyObject)sender);
        }

        private static void CommandBindingsTarget_Update(DependencyObject dep)
        {
            var element = dep as FrameworkElement;

            var newValue = GetCommandBindings(dep);
            if (newValue != null)
            {
                foreach (CommandBinding binding in newValue)
                {
                    element.CommandBindings.Add(binding);
                }
            }
        }

        #endregion

        #region InputBindings

        public static readonly DependencyProperty InputBindingsProperty = DependencyProperty.RegisterAttached(
            "InputBindings",
            typeof(InputBindingCollection),
            typeof(UIElementBehavior),
            new UIPropertyMetadata(null, InputBindingsProperty_PropertyChanged));

        public static InputBindingCollection GetInputBindings(DependencyObject element)
        {
            return (InputBindingCollection)element.GetValue(InputBindingsProperty);
        }

        public static void SetInputBindings(DependencyObject element, InputBindingCollection value)
        {
            element.SetValue(InputBindingsProperty, value);
        }

        private static void InputBindingsProperty_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var element = dep as FrameworkElement;

            var oldValue = e.OldValue as InputBindingCollection;
            if (oldValue != null)
            {
                foreach (InputBinding binding in oldValue)
                {
                    element.InputBindings.Remove(binding);
                }
            }

            if (!element.IsInitialized)
            {
                element.Initialized -= InputBindingsTarget_Initialized;
                element.Initialized += InputBindingsTarget_Initialized;
                return;
            }

            InputBindingsTarget_Update(element);
        }

        private static void InputBindingsTarget_Initialized(object sender, EventArgs e)
        {
            InputBindingsTarget_Update((DependencyObject)sender);
        }

        private static void InputBindingsTarget_Update(DependencyObject dep)
        {
            var element = dep as FrameworkElement;

            var newValue = GetInputBindings(dep);
            if (newValue != null)
            {
                foreach (InputBinding binding in newValue)
                {
                    element.InputBindings.Add(binding);
                }
            }
        }

        #endregion
    }
}
