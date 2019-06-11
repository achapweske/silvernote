/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.HTML.Internal
{
    /// <summary>
    /// Root of an HTML document. 
    /// See the HTML element definition in HTML 4.01.
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-33759296
    /// </summary>
    public class HTMLHtmlElementBase : HTMLElementBase, HTMLHtmlElement
    {
        #region Constructors

        internal HTMLHtmlElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.HTML, ownerDocument)
        {

        }

        #endregion

        #region HTMLHtmlElement

        public string Version
        {
            get { return GetAttribute(HTMLAttributes.VERSION); }
            set { SetAttribute(HTMLAttributes.VERSION, value); }
        }

        #endregion
    }
}
