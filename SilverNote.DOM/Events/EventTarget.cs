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
    /// <summary>
    /// Implemented by all Nodes in an implementation which supports the DOM Event Model.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Events-20001113/events.html#Events-EventTarget
    /// </summary>
    public interface EventTarget
    {
        void AddEventListener(string type, EventListener listener, bool useCapture);
        void RemoveEventListener(string type, EventListener listener, bool useCapture);
        bool DispatchEvent(Event evt);
    }
}
