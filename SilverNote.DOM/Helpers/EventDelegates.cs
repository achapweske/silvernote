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

namespace DOM.Helpers
{
    public delegate void EventDelegate(Event evt);
    public delegate void UIEventDelegate(UIEvent evt);
    public delegate void MouseEventDelegate(MouseEvent evt);
    public delegate void MutationEventDelegate(MutationEvent evt);
}
