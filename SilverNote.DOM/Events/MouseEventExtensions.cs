/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM.Events.Internal;
using DOM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Events
{
    public static class MouseEventExtensions
    {
        public static void InitMouseEvent(this MouseEvent evt,
                            string typeArg,
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
                            ushort buttonsArg,
                            EventTarget relatedTargetArg)
        {
            var mouseEvent = evt as MouseEventImpl;
            if (mouseEvent != null)
            {
                // include buttonsArg
                mouseEvent.InitMouseEvent(typeArg, canBubbleArg, cancelableArg, viewArg, detailArg, screenXArg, screenYArg, clientXArg, clientYArg, ctrlKeyArg, altKeyArg, shiftKeyArg, metaKeyArg, buttonArg, buttonsArg, relatedTargetArg);
            }
            else
            {
                // don't include buttonsArg
                evt.InitMouseEvent(typeArg, canBubbleArg, cancelableArg, viewArg, detailArg, screenXArg, screenYArg, clientXArg, clientYArg, ctrlKeyArg, altKeyArg, shiftKeyArg, metaKeyArg, buttonArg, relatedTargetArg);
            }
        }
    }
}
