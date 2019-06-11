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

namespace SilverNote.Editor
{
    public interface IFormattable
    {
        bool HasProperty(string name);
        object GetProperty(string name);
        void SetProperty(string name, object value);
        void ResetProperties();
        int ChangeProperty(string name, object oldValue, object newValue);
    }

    public static class Formattable
    {
        #region FormatChanged

        public static readonly RoutedEvent FormatChanged = EventManager.RegisterRoutedEvent(
            "FormatChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(Formattable)
        );

        public static void AddFormatChangedHandler(DependencyObject dep, RoutedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.AddHandler(FormatChanged, handler);
            }
        }

        public static void RemoveFormatChangedHandler(DependencyObject dep, RoutedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.RemoveHandler(FormatChanged, handler);
            }
        }

        #endregion

        public static bool HasProperty(object obj, string name)
        {
            if (obj is IFormattable)
            {
                return ((IFormattable)obj).HasProperty(name);
            }
            else
            {
                return false;
            }
        }

        public static object GetProperty(object obj, string name)
        {
            if (obj is IFormattable)
            {
                return ((IFormattable)obj).GetProperty(name);
            }
            else
            {
                return null;
            }
        }

        public static object GetProperty(IEnumerable<object> objs, string name)
        {
            var obj = objs.LastOrDefault();
            if (obj != null)
            {
                return GetProperty(obj, name);
            }
            else
            {
                return null;
            }
        }

        public static void SetProperty(object obj, string name, object value)
        {
            if (obj is IFormattable)
            {
                ((IFormattable)obj).SetProperty(name, value);
            }
        }

        public static void SetProperty(IEnumerable<object> objs, string name, object value)
        {
            foreach (var obj in objs)
            {
                SetProperty(obj, name, value);
            }
        }

        public static void ResetProperties(object obj)
        {
            if (obj is IFormattable)
            {
                ((IFormattable)obj).ResetProperties();
            }
        }

        public static void ResetProperties(IEnumerable<object> objs)
        {
            foreach (var obj in objs)
            {
                ResetProperties(obj);
            }
        }

        public static int ChangeProperty(object obj, string name, object oldValue, object newValue)
        {
            if (obj is IFormattable)
            {
                return ((IFormattable)obj).ChangeProperty(name, oldValue, newValue);
            }
            else
            {
                return 0;
            }
        }

        public static string GetStringProperty(this IFormattable self, string name, string defaultValue = default(string))
        {
            if (self.HasProperty(name))
            {
                return (string)self.GetProperty(name);
            }
            else
            {

                return defaultValue;
            }
        }

        public static double GetDoubleProperty(this IFormattable self, string name, double defaultValue = default(double))
        {
            if (self.HasProperty(name))
            {
                return (double)self.GetProperty(name);
            }
            else
            {

                return defaultValue;
            }
        }

        public static int GetIntProperty(this IFormattable self, string name, int defaultValue = default(int))
        {
            if (self.HasProperty(name))
            {
                return (int)self.GetProperty(name);
            }
            else
            {

                return defaultValue;
            }
        }

        public static bool GetBoolProperty(this IFormattable self, string name, bool defaultValue = default(bool))
        {
            
            if (self.HasProperty(name))
            {
                return (bool)self.GetProperty(name);
            }
            else
            {

                return defaultValue;
            }
        }
    }

    public class FormatChangedEventArgs : EventArgs
    {
        readonly string _PropertyName;

        public FormatChangedEventArgs()
        {
            _PropertyName = null;
        }

        public FormatChangedEventArgs(string propertyName)
        {
            _PropertyName = propertyName;
        }

        public string PropertyName
        {
            get { return _PropertyName; }
        }
    }
}
