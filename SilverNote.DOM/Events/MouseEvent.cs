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
    public interface MouseEvent : UIEvent
    {
        int ScreenX { get; }
        int ScreenY { get; }
        int ClientX { get; }
        int ClientY { get; }
        bool CtrlKey { get; }
        bool ShiftKey { get; }
        bool AltKey { get; }
        bool MetaKey { get; }
        ushort Button { get; }
        ushort Buttons { get; }
        EventTarget RelatedTarget { get; }
        void InitMouseEvent(string typeArg,
                            bool canBubbleArg,
                            bool cancelableArg,
                            AbstractView viewArg,
                            int detailArg,
                            int screenXArg,
                            int screenYArg,
                            int clientXArg,
                            int clientYArg,
                            bool ctrlKeyArg,
                            bool altKeyArg,
                            bool shiftKeyArg,
                            bool metaKeyArg,
                            ushort buttonArg,
                            EventTarget relatedTargetArg);
    }
}
