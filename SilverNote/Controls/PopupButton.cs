/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace SilverNote.Controls
{
    [ContentProperty("Content")]
    public class PopupButton : ToggleButton
    {
        static PopupButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata ( 
                typeof(PopupButton), 
                new FrameworkPropertyMetadata(typeof(PopupButton))
            );
        }

        #region CloseOnClick

        public static readonly DependencyProperty CloseOnClickProperty = DependencyProperty.RegisterAttached(
            "CloseOnClick",
            typeof(bool),
            typeof(PopupButton),
            new UIPropertyMetadata(false, OnCloseOnClickChanged)
        );

        public static bool GetCloseOnClick(DependencyObject element)
        {
            return (bool)element.GetValue(CloseOnClickProperty);
        }

        public static void SetCloseOnClick(DependencyObject element, bool value)
        {
            element.SetValue(CloseOnClickProperty, value);
        }

        static void OnCloseOnClickChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var menuItem = dep as MenuItem;
            if (menuItem != null)
            {
                if ((bool)e.OldValue)
                {
                    menuItem.Click -= CloseOnClickTarget_Click;
                }

                if ((bool)e.NewValue)
                {
                    menuItem.Click += CloseOnClickTarget_Click;
                }
            }
        }

        static void CloseOnClickTarget_Click(object sender, RoutedEventArgs e)
        {
            var popupButton = GetPopupButton((DependencyObject)sender);
            if (popupButton != null)
            {
                popupButton.IsChecked = false;
            }
        }

        #endregion

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(object),
            typeof(PopupButton)
        );

        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static PopupButton GetPopupButton(DependencyObject dep)
        {
            do
            {
                dep = LogicalTreeHelper.GetParent(dep) ?? VisualTreeHelper.GetParent(dep);

            } while (dep != null && !(dep is PopupButton));

            return (PopupButton)dep;
        }

        bool? wasChecked = null;

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            wasChecked = IsChecked;

            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsChecked == wasChecked)
            {
                base.OnMouseLeftButtonDown(e);
            }
            else
            {
                e.Handled = true;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsChecked == wasChecked)
            {
                base.OnMouseLeftButtonUp(e);
            }

            if (!IsAncestorOf(e.OriginalSource as DependencyObject))
            {
                IsChecked = false;
            }
        }
    }
}
