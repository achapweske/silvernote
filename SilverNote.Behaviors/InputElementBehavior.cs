/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SilverNote.Behaviors
{
    public static class InputElementBehavior
    {
        #region IsFocusScope

        public static readonly DependencyProperty IsFocusScopeProperty = DependencyProperty.RegisterAttached(
            "IsFocusScope",
            typeof(bool),
            typeof(InputElementBehavior),
            new UIPropertyMetadata(IsFocusScopeProperty_Changed)
        );

        public static bool GetIsFocusScope(DependencyObject target)
        {
            return (bool)target.GetValue(IsFocusScopeProperty);
        }

        public static void SetIsFocusScope(DependencyObject target, bool newValue)
        {
            target.SetValue(IsFocusScopeProperty, newValue);
        }

        static void IsFocusScopeProperty_Changed(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            if (true.Equals(e.NewValue))
            {
                AddGotFocusHandler(dep, IsFocusScopeProperty_GotFocus);
            }
            else
            {
                RemoveGotFocusHandler(dep, IsFocusScopeProperty_GotFocus);
            }
        }

        static void IsFocusScopeProperty_GotFocus(object sender, RoutedEventArgs e)
        {
            var focusScope = (DependencyObject)sender;

            if (e.OriginalSource == focusScope)
            {
                var focusedElement = GetFocusedElement(focusScope);
                if (focusedElement != null && LayoutHelper.IsDescendant(focusScope, (DependencyObject)focusedElement))
                {
                    focusedElement.Focus();
                }
            }
            else
            {
                var focusedElement = e.OriginalSource as IInputElement;
                SetFocusedElement(focusScope, focusedElement);
            }
        }

        static void AddGotFocusHandler(DependencyObject dep, RoutedEventHandler handler)
        {
            if (dep is UIElement)
            {
                ((UIElement)dep).GotFocus += handler;
            }
            else if (dep is ContentElement)
            {
                ((ContentElement)dep).GotFocus += handler;
            }
        }

        static void RemoveGotFocusHandler(DependencyObject dep, RoutedEventHandler handler)
        {
            if (dep is UIElement)
            {
                ((UIElement)dep).GotFocus -= handler;
            }
            else if (dep is ContentElement)
            {
                ((ContentElement)dep).GotFocus -= handler;
            }
        }

        #endregion

        #region FocusedElement

        public static readonly DependencyProperty FocusedElementProperty = DependencyProperty.RegisterAttached(
            "FocusedElement",
            typeof(IInputElement),
            typeof(InputElementBehavior),
            new UIPropertyMetadata(FocusedElementPropertyProperty_Changed)
        );

        public static IInputElement GetFocusedElement(DependencyObject target)
        {
            return (IInputElement)target.GetValue(FocusedElementProperty);
        }

        public static void SetFocusedElement(DependencyObject target, IInputElement element)
        {
            target.SetValue(FocusedElementProperty, element);
        }

        static void FocusedElementPropertyProperty_Changed(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            
        }

        #endregion
    }
}
