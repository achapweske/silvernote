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
    public static class NodeExtensions
    {
        public static void AddEventListener(this Node node, string type, EventListener listener, bool useCapture)
        {
            if (node is EventTarget)
            {
                ((EventTarget)node).AddEventListener(type, listener, useCapture);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static void RemoveEventListener(this Node node, string type, EventListener listener, bool useCapture)
        {
            if (node is EventTarget)
            {
                ((EventTarget)node).RemoveEventListener(type, listener, useCapture);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool DispatchEvent(this Node node, Event evt)
        {
            if (node is EventTarget)
            {
                return ((EventTarget)node).DispatchEvent(evt);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
