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
    public class HTMLLabelElementBase : HTMLElementBase, HTMLLabelElement
    {
        #region Constructors

        internal HTMLLabelElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.LABEL, ownerDocument)
        {

        }

        #endregion

        #region HTMLLabelElement

        public HTMLFormElement Form
        {
            get { return (HTMLFormElement)GetAncestorByTagName(HTMLElements.FORM); }
        }

        public string AccessKey
        {
            get { return GetAttribute(HTMLAttributes.ACCESSKEY); }
            set { SetAttribute(HTMLAttributes.ACCESSKEY, value); }
        }

        public string HtmlFor
        {
            get { return GetAttribute(HTMLAttributes.FOR); }
            set { SetAttribute(HTMLAttributes.FOR, value); }
        }

        #endregion
    }
}
