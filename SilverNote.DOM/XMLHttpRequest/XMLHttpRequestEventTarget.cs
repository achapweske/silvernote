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

namespace DOM.XHR
{
    public interface XMLHttpRequestEventTarget : EventTarget
    {
        EventHandler OnLoadStart { get; set; }
        EventHandler OnProgress { get; set; }
        EventHandler OnAbort { get; set; }
        EventHandler OnError { get; set; }
        EventHandler OnLoad { get; set; }
        EventHandler OnTimeout { get; set; }
        EventHandler OnLoadEnd { get; set; }
    }
}
