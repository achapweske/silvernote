/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Views;

namespace DOM.Windows.Internal
{
    public class WindowBase : Window
    {
        #region Fields

        DocumentView _Document;
        Dictionary<string, object> _Properties = new Dictionary<string,object>();

        #endregion

        #region Constructors

        public WindowBase()
        {

        }

        public WindowBase(DocumentView document)
        {
            _Document = document;
        }

        #endregion

        #region Window

        public Window Window
        {
            get { return this; }
        }

        public virtual Location Location
        {
            get { throw new NotImplementedException(); }
        }

        public virtual int SetTimeout(TimerListener listener, int milliseconds)
        {
            throw new NotImplementedException();
        }

        public virtual void ClearTimeout(int timerID)
        {
            throw new NotImplementedException();
        }

        public virtual int SetInterval(TimerListener listener, int milliseconds)
        {
            throw new NotImplementedException();
        }

        public virtual void ClearInterval(int timerID)
        {
            throw new NotImplementedException();
        }

        public virtual void SetProperty(string name, object value)
        {
            _Properties[name] = value;
        }

        public virtual object GetProperty(string name)
        {
            object value;
            if (_Properties.TryGetValue(name, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public virtual bool HasProperty(string name)
        {
            return _Properties.ContainsKey(name);
        }

        public virtual void Alert(string message)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region AbstractView

        public virtual DocumentView Document
        {
            get { return _Document; }
        }

        #endregion

    }
}
