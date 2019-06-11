/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Internal;

namespace DOM.HTML.Internal
{
    public class HTMLUListElementBase : HTMLElementBase, HTMLUListElement
    {
        #region Constructors

        internal HTMLUListElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.UL, ownerDocument)
        {

        }

        #endregion

        #region HTMLUListElement

        public bool Compact
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.COMPACT, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.COMPACT, value); }
        }

        public string Type
        {
            get { return GetAttribute(HTMLAttributes.TYPE); }
            set { SetAttribute(HTMLAttributes.TYPE, value); }
        }

        #endregion
    }
}
