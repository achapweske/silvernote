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
    public class ControlBehavior
    {
        #region PreviewMouseDoubleClickCommand

        public static ICommand GetPreviewMouseDoubleClickCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(PreviewMouseDoubleClickCommandProperty);
        }

        public static void SetPreviewMouseDoubleClickCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(PreviewMouseDoubleClickCommandProperty, value);
        }

        public static readonly DependencyProperty PreviewMouseDoubleClickCommandProperty = DependencyProperty.RegisterAttached(
            "PreviewMouseDoubleClickCommand",
            typeof(ICommand),
            typeof(ControlBehavior),
            new UIPropertyMetadata(null, OnPreviewMouseDoubleClickCommandChanged)
        );

        static void OnPreviewMouseDoubleClickCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            Control commandTarget = dep as Control;

            if (e.OldValue != null)
            {
                commandTarget.PreviewMouseDoubleClick -= PreviewMouseDoubleClickCommandTarget_PreviewMouseDoubleClick;
            }

            if (e.NewValue != null)
            {
                commandTarget.PreviewMouseDoubleClick += PreviewMouseDoubleClickCommandTarget_PreviewMouseDoubleClick;
            }
        }

        static void PreviewMouseDoubleClickCommandTarget_PreviewMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (!e.Handled)
            {
                var target = sender as DependencyObject;
                var command = GetPreviewMouseDoubleClickCommand(target);
                var parameter = GetPreviewMouseDoubleClickCommandParameter(target);

                command.Execute(parameter);

                e.Handled = true;
            }
        }

        #endregion

        #region PreviewMouseDoubleClickCommandParameter

        public static object GetPreviewMouseDoubleClickCommandParameter(DependencyObject element)
        {
            return element.GetValue(PreviewMouseDoubleClickCommandParameterProperty);
        }

        public static void SetPreviewMouseDoubleClickCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(PreviewMouseDoubleClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty PreviewMouseDoubleClickCommandParameterProperty = DependencyProperty.RegisterAttached(
            "PreviewMouseDoubleClickCommandParameter",
            typeof(object),
            typeof(ControlBehavior)
        );

        #endregion

        #region MouseDoubleClickCommand

        public static ICommand GetMouseDoubleClickCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(MouseDoubleClickCommandProperty);
        }

        public static void SetMouseDoubleClickCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(MouseDoubleClickCommandProperty, value);
        }

        public static readonly DependencyProperty MouseDoubleClickCommandProperty = DependencyProperty.RegisterAttached(
            "MouseDoubleClickCommand",
            typeof(ICommand),
            typeof(ControlBehavior),
            new UIPropertyMetadata(null, OnMouseDoubleClickCommandChanged)
        );

        static void OnMouseDoubleClickCommandChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            Control commandTarget = dep as Control;

            if (e.OldValue != null)
            {
                commandTarget.MouseDoubleClick -= MouseDoubleClickCommandTarget_MouseDoubleClick;
            }

            if (e.NewValue != null)
            {
                commandTarget.MouseDoubleClick += MouseDoubleClickCommandTarget_MouseDoubleClick;
            }
        }

        static void MouseDoubleClickCommandTarget_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (!e.Handled)
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

        public static object GetMouseDoubleClickCommandParameter(DependencyObject element)
        {
            return element.GetValue(MouseDoubleClickCommandParameterProperty);
        }

        public static void SetMouseDoubleClickCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(MouseDoubleClickCommandParameterProperty, value);
        }

        public static readonly DependencyProperty MouseDoubleClickCommandParameterProperty = DependencyProperty.RegisterAttached(
            "MouseDoubleClickCommandParameter",
            typeof(object),
            typeof(ControlBehavior)
        );

        #endregion

    }
}
