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
    /// The document title. See the TITLE element definition in HTML 4.01.
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-79243169
    /// </summary>
    public class HTMLTitleElementBase : HTMLElementBase, HTMLTitleElement
    {
        #region Constructors

        internal HTMLTitleElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.TITLE, ownerDocument)
        {

        }

        #endregion

        #region HTMLTitleElement

        public string Text
        {
            get { return TextContent; }
            set { TextContent = value; }
        }

        #endregion
    }
}
