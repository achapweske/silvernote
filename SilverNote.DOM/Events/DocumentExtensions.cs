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
    public static class DocumentExtensions
    {
        public static Event CreateEvent(this Document document, string eventType)
        {
            if (document is DocumentEvent)
            {
                return ((DocumentEvent)document).CreateEvent(eventType);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
