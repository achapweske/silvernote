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
    public class HTMLDivElementBase : HTMLElementBase, HTMLDivElement
    {
        #region Constructors

        internal HTMLDivElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.DIV, ownerDocument)
        {

        }

        #endregion

        #region HTMLDivElement

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        #endregion
    }
}
