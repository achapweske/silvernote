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

namespace DOM.Events.Internal
{
    /// <summary>
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Events-20001113/events.html#Events-UIEvent
    /// </summary>
    public class UIEventImpl : EventBase, UIEvent
    {
        #region Fields

        AbstractView _View;
        int _Detail;

        #endregion

        #region UIEvent

        public AbstractView View
        {
            get { return _View; }
        }

        public int Detail
        {
            get { return _Detail; }
        }

        public void InitUIEvent(string type, bool canBubble, bool cancelable, AbstractView view, int detail)
        {
            InitEvent(type, canBubble, cancelable);

            _View = view;
            _Detail = detail;
        }

        #endregion
    }
}
