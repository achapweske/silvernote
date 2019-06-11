/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections;

namespace SilverNote.Common
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            VerifyProperty(propertyName);

            var handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        [Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            if (propertyName == "Item[]")
            {
                propertyName = "Item";
            }
            Type type = this.GetType();
            if (type.GetProperty(propertyName) == null)
            {
                string msg = String.Format("{0} is not a public property of {1}", propertyName, type.FullName);
                Debug.Fail(msg);
            }
        }

        PropertyObserver _Observer;

        public ObservableObject WhenPropertyChanged(string propertyName, PropertyChangedEventHandler handler, bool register = true)
        {
            VerifyProperty(propertyName);

            if (_Observer == null)
            {
                _Observer = new PropertyObserver(this);
            }

            if (register)
            {
                _Observer.AddHandler(propertyName, handler);
            }
            else
            {
                _Observer.RemoveHandler(propertyName, handler);
            }

            return this;
        }
    }
}
