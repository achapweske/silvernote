/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace SilverNote.Editor
{
    public interface IHasResources
    {
        IEnumerable<string> ResourceNames { get; }
        void GetResource(string url, Stream stream);
        void SetResource(string url, Stream stream);
    }

    public static class ResourceContainer
    {
        #region ResourceChanged

        public static readonly RoutedEvent ResourceChangedEvent = EventManager.RegisterRoutedEvent(
            "ResourceChanged",
            RoutingStrategy.Bubble,
            typeof(ResourceEventHandler),
            typeof(ResourceContainer)
        );

        public static void AddResourceChangedHandler(DependencyObject dep, ResourceEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.AddHandler(ResourceChangedEvent, handler);
            }
        }

        public static void RemoveResourceChangedHandler(DependencyObject dep, ResourceEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.RemoveHandler(ResourceChangedEvent, handler);
            }
        }

        #endregion

        #region ResourceRequested

        public static readonly RoutedEvent ResourceRequestedEvent = EventManager.RegisterRoutedEvent(
            "ResourceRequested",
            RoutingStrategy.Bubble,
            typeof(ResourceEventHandler),
            typeof(ResourceContainer)
        );

        public static void AddResourceRequestedHandler(DependencyObject dep, ResourceEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.AddHandler(ResourceRequestedEvent, handler);
            }
        }

        public static void RemoveResourceRequestedHandler(DependencyObject dep, ResourceEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.RemoveHandler(ResourceRequestedEvent, handler);
            }
        }

        #endregion

        #region Extension Methods

        public static byte[] GetResourceData(this IHasResources container, string url)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    container.GetResource(url, stream);
                    stream.Flush();
                    return stream.ToArray();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                return null;
            }
        }

        public static bool SetResourceData(this IHasResources container, string url, byte[] data)
        {
            try
            {
                using (var stream = new MemoryStream(data))
                {
                    container.SetResource(url, stream);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                return false;
            }
        }

        public static string GetResourceString(this IHasResources container, string url, Encoding encoding)
        {
            byte[] dataBytes = container.GetResourceData(url);

            if (dataBytes == null)
            {
                return null;
            }

            byte[] preamble = encoding.GetPreamble();
            if (dataBytes.Length > preamble.Length && preamble.SequenceEqual(dataBytes.Take(preamble.Length)))
            {
                return encoding.GetString(dataBytes, preamble.Length, dataBytes.Length - preamble.Length);
            }
            else
            {
                return encoding.GetString(dataBytes);
            }
        }

        public static bool SetResourceString(this IHasResources container, string url, string dataString, Encoding encoding)
        {
            byte[] dataBytes = encoding.GetBytes(dataString);

            return container.SetResourceData(url, dataBytes);
        }

        #endregion
    }

    public delegate void ResourceEventHandler(object sender, ResourceEventArgs e);

    public class ResourceEventArgs : RoutedEventArgs
    {
        public ResourceEventArgs(string url)
        {
            Url = url;
        }

        public ResourceEventArgs(RoutedEvent routedEvent, string url)
            : base(routedEvent)
        {
            Url = url;
        }

        public ResourceEventArgs(RoutedEvent routedEvent, object source, string url)
            : base(routedEvent, source)
        {
            Url = url;
        }

        public string Url { get; set; }
    }


}
