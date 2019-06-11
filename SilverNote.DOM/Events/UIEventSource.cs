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

namespace DOM.Events
{
    public class UIEventSource : EventSource
    {
        #region Fields

        AbstractView _View;
        int _Detail;

        #endregion

        #region Constructors

        public UIEventSource()
        {

        }

        #endregion

        #region Properties

        public AbstractView View
        {
            get { return _View; }
        }

        public int Detail
        {
            get { return _Detail; }
        }

        #endregion

        #region Operations

        public void InitUIEvent(string typeArg, bool canBubbleArg, bool cancelableArg, AbstractView viewArg, int detailArg)
        {
            InitEvent(typeArg, canBubbleArg, cancelableArg);
            _View = viewArg;
            _Detail = detailArg;
        }

        #endregion

        #region IEventSource

        public override void Dispatch(Node node)
        {
            Document document = node.OwnerDocument;
            if (document != null)
            {
                UIEvent evt = (UIEvent)document.CreateEvent(EventTypes.UIEvents);
                evt.InitUIEvent(Type, Bubbles, Cancelable, View, Detail);
                if (!node.DispatchEvent(evt))
                {
                    Cancel();
                }
            }
        }

        #endregion
    }
}
