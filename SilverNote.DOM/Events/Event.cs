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
    public interface Event
    {
        string Type { get; }
        EventTarget Target { get; }
        EventTarget CurrentTarget { get; }
        EventPhaseType EventPhase { get; }
        bool Bubbles { get; }
        bool Cancelable { get; }
        long Timestamp { get; }
        void StopPropagation();
        void PreventDefault();
        void InitEvent(string eventTypeArg, bool canBubbleArg, bool cancelableArg);
    }
}
