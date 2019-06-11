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
    public class ComboBoxBehavior
    {
        #region FocusWhenDropDownClosed

        public static readonly DependencyProperty FocusWhenDropDownClosedProperty = DependencyProperty.RegisterAttached(
            "FocusWhenDropDownClosed",
            typeof(UIElement),
            typeof(ComboBoxBehavior),
            new UIPropertyMetadata(null, FocusWhenDropDownClosed_PropertyChanged)
        );


        public static UIElement GetFocusWhenDropDownClosed(DependencyObject element)
        {
            return (UIElement)element.GetValue(FocusWhenDropDownClosedProperty);
        }

        public static void SetFocusWhenDropDownClosed(DependencyObject element, UIElement value)
        {
            element.SetValue(FocusWhenDropDownClosedProperty, value);
        }

        static void FocusWhenDropDownClosed_PropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = (ComboBox)dep;
            var element = (UIElement)e.NewValue;

            if (element != null)
            {
                comboBox.DropDownClosed += FocusWhenDropDownClosedTarget_DropDownClosed;
            }
            else
            {
                comboBox.DropDownClosed -= FocusWhenDropDownClosedTarget_DropDownClosed;
            }
        }

        static void FocusWhenDropDownClosedTarget_DropDownClosed(object sender, EventArgs e)
        {
            UIElement element = GetFocusWhenDropDownClosed((DependencyObject)sender);

            if (element != null)
            {
                element.Focus();
            }
        }

        #endregion

        
    }
}
