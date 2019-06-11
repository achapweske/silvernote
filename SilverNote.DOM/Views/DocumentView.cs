/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Views
{
    /// <summary>
    /// The DocumentView interface is implemented by Document objects in DOM 
    /// implementations supporting DOM Views. It provides an attribute to 
    /// retrieve the default view of a document.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Views-20001113/views.html
    /// </summary>
    public interface DocumentView
    {
        /// <summary>
        /// The default AbstractView for this Document, or null if none available.
        /// </summary>
        AbstractView DefaultView { get; }
    }
}
