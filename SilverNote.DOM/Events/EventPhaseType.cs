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
    public enum EventPhaseType : ushort
    {
        CAPTURING_PHASE                = 1,
        AT_TARGET                      = 2,
        BUBBLING_PHASE                 = 3
    }
}
