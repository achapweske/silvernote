/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Events;

namespace DOM.Helpers
{
    public class EventHandler : EventListener
    {
        readonly EventDelegate _Delegate;

        public EventHandler(EventDelegate callback)
        {
            _Delegate = callback;
        }

        public void HandleEvent(Event evt)
        {
            _Delegate(evt);
        }

        public override bool Equals(object obj)
        {
            var other = obj as EventHandler;
            if (other != null)
            {
                return _Delegate.Equals(other._Delegate);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return _Delegate.GetHashCode();
        }
    }

    public class MouseEventHandler : EventListener
    {
        readonly MouseEventDelegate _Delegate;

        public MouseEventHandler(MouseEventDelegate callback)
        {
            _Delegate = callback;
        }

        public void HandleEvent(Event evt)
        {
            _Delegate((MouseEvent)evt);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MouseEventHandler;
            if (other != null)
            {
                return _Delegate.Equals(other._Delegate);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return _Delegate.GetHashCode();
        }
    }
}
