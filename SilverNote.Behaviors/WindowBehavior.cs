/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SilverNote.Behaviors
{
    public class WindowBehavior
    {
        #region SavedSize

        public static Size GetSavedSize(Window window)
        {
            return (Size)window.GetValue(SavedSizeProperty);
        }

        public static void SetSavedSize(Window window, Size value)
        {
            window.SetValue(SavedSizeProperty, value);
        }

        public static readonly DependencyProperty SavedSizeProperty = DependencyProperty.RegisterAttached(
            "SavedSize",
            typeof(Size),
            typeof(WindowBehavior),
            new FrameworkPropertyMetadata(default(Size), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSavedPropertyChanged)
        );

        #endregion

        #region SavedPosition

        public static Point GetSavedPosition(Window window)
        {
            return (Point)window.GetValue(SavedPositionProperty);
        }

        public static void SetSavedPosition(Window window, Point value)
        {
            window.SetValue(SavedPositionProperty, value);
        }

        public static readonly DependencyProperty SavedPositionProperty = DependencyProperty.RegisterAttached(
            "SavedPosition",
            typeof(Point),
            typeof(WindowBehavior),
            new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSavedPropertyChanged)
        );

        #endregion

        #region SavedState

        public static WindowState GetSavedState(Window window)
        {
            return (WindowState)window.GetValue(SavedStateProperty);
        }

        public static void SetSavedState(Window window, WindowState value)
        {
            window.SetValue(SavedStateProperty, value);
        }

        public static readonly DependencyProperty SavedStateProperty = DependencyProperty.RegisterAttached(
            "SavedState",
            typeof(WindowState),
            typeof(WindowBehavior),
            new FrameworkPropertyMetadata(default(WindowState), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSavedPropertyChanged)
        );

        #endregion

        #region SavedSettings

        public static ApplicationSettingsBase GetSavedSettings(Window window)
        {
            return (ApplicationSettingsBase)window.GetValue(SavedSettingsProperty);
        }

        public static void SetSavedSettings(Window window, ApplicationSettingsBase value)
        {
            window.SetValue(SavedSettingsProperty, value);
        }

        public static readonly DependencyProperty SavedSettingsProperty = DependencyProperty.RegisterAttached(
            "SavedSettings",
            typeof(ApplicationSettingsBase),
            typeof(WindowBehavior),
            new UIPropertyMetadata(null)
        );

        #endregion

        #region SavedProperty 

        static void OnSavedPropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs e)
        {
            var window = dep as Window;

            window.SourceInitialized -= Window_SourceInitialized;
            window.SourceInitialized += Window_SourceInitialized;
            window.Closing -= Window_Closing;
            window.Closing += Window_Closing;
        }

        static void Window_SourceInitialized(object sender, EventArgs e)
        {
            // Load window's state after HWND created, but before it's drawn:
            //
            // http://social.msdn.microsoft.com/Forums/hu-HU/wpf/thread/9df56308-e578-436f-b8d3-575243601443

            var window = sender as Window;

            // Size

            if (window.ReadLocalValue(SavedSizeProperty) != DependencyProperty.UnsetValue)
            {
                var size = GetSavedSize(window);

                double minWidth = 100;
                double minHeight = 100;
                double maxWidth = SystemParameters.VirtualScreenWidth;
                double maxHeight = SystemParameters.VirtualScreenHeight;

                window.Width = Math.Max(Math.Min(size.Width, maxWidth), minWidth);
                window.Height = Math.Max(Math.Min(size.Height, maxHeight), minHeight);
            }

            // Position

            if (window.ReadLocalValue(SavedPositionProperty) != DependencyProperty.UnsetValue)
            {
                var position = GetSavedPosition(window);

                double minX = 0;
                double minY = 0;
                double maxX = SystemParameters.VirtualScreenWidth - 100;
                double maxY = SystemParameters.VirtualScreenHeight - 100;

                window.Left = Math.Max(Math.Min(position.X, maxX), minX);
                window.Top = Math.Max(Math.Min(position.Y, maxY), minY);
            }

            // State

            if (window.ReadLocalValue(SavedStateProperty) != DependencyProperty.UnsetValue)
            {
                var state = GetSavedState(window);

                if (state == WindowState.Minimized)
                {
                    state = WindowState.Normal;
                }

                window.WindowState = state;
            }
        }

        static void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var window = sender as Window;

            if (window.WindowState != WindowState.Minimized)
            {
                SetSavedState(window, window.WindowState);

                if (window.WindowState != WindowState.Maximized)
                {
                    SetSavedPosition(window, new Point(window.Left, window.Top));
                    SetSavedSize(window, new Size(window.Width, window.Height));
                }

                var settings = GetSavedSettings(window);
                if (settings != null)
                {
                    settings.Save();
                }
            }

        }

        #endregion
    }
}
