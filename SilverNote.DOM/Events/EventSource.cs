/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Events
{
    public class EventSource : IEventSource
    {
        #region Fields

        string _Type;
        bool _Bubbles;
        bool _Cancelable;
        bool _IsCanceled;

        #endregion

        #region Constructors

        public EventSource()
        {

        }

        #endregion

        #region Properties

        public string Type
        {
            get { return _Type; }
        }

        public bool Bubbles
        {
            get { return _Bubbles; }
        }

        public bool Cancelable
        {
            get { return _Cancelable; }
        }

        #endregion

        #region Operations

        public void InitEvent(string eventType, bool canBubble, bool cancelable)
        {
            _IsCanceled = false;
            _Type = eventType;
            _Bubbles = canBubble;
            _Cancelable = cancelable;
        }

        #endregion

        #region IEventSource

        public bool IsCanceled
        {
            get { return _IsCanceled; }
        }

        public virtual void Dispatch(Node node)
        {
            Document document = node.OwnerDocument;
            if (document != null)
            {
                Event evt = document.CreateEvent(EventTypes.HTMLEvents);
                evt.InitEvent(Type, Bubbles, Cancelable);
                if (!node.DispatchEvent(evt))
                {
                    Cancel();
                }
            }
        }

        #endregion

        #region Implementation

        protected void Cancel()
        {
            _IsCanceled = true;
        }

        #endregion
    }
}
