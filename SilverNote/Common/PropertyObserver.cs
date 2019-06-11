/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace SilverNote.Common
{
    public class PropertyObserver : IWeakEventListener
    {
        #region Fields

        readonly WeakReference _PropertySource;
        readonly Dictionary<string, List<PropertyChangedEventHandler>> _Handlers;

        #endregion

        #region Constructors

        public PropertyObserver(INotifyPropertyChanged propertySource)
        {
            _PropertySource = new WeakReference(propertySource);
            _Handlers = new Dictionary<string, List<PropertyChangedEventHandler>>();
        }

        #endregion

        #region Properties

        public INotifyPropertyChanged PropertySource
        {
            get
            {
                try
                {
                    return (INotifyPropertyChanged)_PropertySource.Target;
                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion

        #region Methods

        public PropertyObserver AddHandler(string propertyName, PropertyChangedEventHandler handler)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            var propertySource = this.PropertySource;
            if (propertySource != null)
            {
                List<PropertyChangedEventHandler> handlers;
                if (!_Handlers.TryGetValue(propertyName, out handlers))
                {
                    handlers = _Handlers[propertyName] = new List<PropertyChangedEventHandler>();
                }

                handlers.Add(handler);

                if (handlers.Count == 1)
                {
                    PropertyChangedEventManager.AddListener(propertySource, this, propertyName);
                }
            }

            return this;
        }

        public PropertyObserver RemoveHandler(string propertyName, PropertyChangedEventHandler handler)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            var propertySource = this.PropertySource;
            if (propertySource != null)
            {
                List<PropertyChangedEventHandler> handlers;
                if (_Handlers.TryGetValue(propertyName, out handlers))
                {
                    if (handlers.Remove(handler))
                    {
                        if (handlers.Count == 0)
                        {
                            _Handlers.Remove(propertyName);
                            PropertyChangedEventManager.RemoveListener(propertySource, this, propertyName);
                        }
                    }
                }
            }

            return this;
        }

        #endregion

        #region IWeakEventListener

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
            {
                return false;
            }

            var propertySource = sender as INotifyPropertyChanged;
            if (propertySource == null)
            {
                return false;
            }

            var args = e as PropertyChangedEventArgs;
            if (args == null)
            {
                return false;
            }

            string propertyName = args.PropertyName;

            if (String.IsNullOrEmpty(propertyName))
            {
                // All properties have been invalidated

                foreach (var handlers in _Handlers.Values.ToArray())
                {
                    foreach (var handler in handlers.ToArray())
                    {
                        handler(propertySource, args);
                    }
                }
            }
            else
            {
                List<PropertyChangedEventHandler> handlers;

                if (_Handlers.TryGetValue(propertyName, out handlers))
                {
                    foreach (var handler in handlers.ToArray())
                    {
                        handler(propertySource, args);
                    }
                }
            }

            return true;
        }

        #endregion

    }
}