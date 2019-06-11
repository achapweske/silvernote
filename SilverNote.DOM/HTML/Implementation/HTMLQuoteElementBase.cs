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
    public class HTMLQuoteElementBase : HTMLElementBase, HTMLQuoteElement
    {
        #region Constructors

        internal HTMLQuoteElementBase(string nodeName, HTMLDocumentImpl ownerDocument)
            : base(nodeName, ownerDocument)
        {

        }

        #endregion

        #region HTMLQuoteElement

        public string Cite
        {
            get { return GetAttribute(HTMLAttributes.CITE); }
            set { SetAttribute(HTMLAttributes.CITE, value); }
        }

        #endregion
    }
}
