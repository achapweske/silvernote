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
    public interface DocumentEvent
    {
        /// <summary>
        /// Create an event
        /// </summary>
        /// <param name="eventType">Event type (e.g. "MouseEvents")</param>
        /// <returns>The newly-created event</returns>
        /// <exception cref="DOMException">
        /// NOT_SUPPORTED_ERR: Raised if the implementation does not support the type of Event interface requested
        /// </exception>
        Event CreateEvent(string eventType);
    }
}
