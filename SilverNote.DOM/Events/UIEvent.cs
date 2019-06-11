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
    public interface UIEvent : Event
    {
        AbstractView View { get; }
        int Detail { get; } 
        void InitUIEvent(string typeArg, bool canBubbleArg, bool cancelableArg, AbstractView viewArg, int detailArg);
    }
}
