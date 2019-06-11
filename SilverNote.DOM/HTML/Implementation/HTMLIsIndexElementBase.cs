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
    public class HTMLIsIndexElementBase : HTMLElementBase, HTMLIsIndexElement
    {
        #region Constructors

        internal HTMLIsIndexElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.ISINDEX, ownerDocument)
        {

        }

        #endregion

        #region HTMLIsIndexElement

        public HTMLFormElement Form
        {
            get { return (HTMLFormElement)GetAncestorByTagName(HTMLElements.FORM); }
        }

        public string Prompt
        {
            get { return GetAttribute(HTMLAttributes.PROMPT); }
            set { SetAttribute(HTMLAttributes.PROMPT, value); }
        }

        #endregion
    }
}
