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
    public class HTMLOListElementBase : HTMLElementBase, HTMLOListElement
    {
        #region Constructors

        internal HTMLOListElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.OL, ownerDocument)
        {

        }

        #endregion

        #region HTMLOListElement

        public bool Compact
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.COMPACT, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.COMPACT, value); }
        }

        public int Start
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.START, 1); }
            set { this.SetAttributeAsInt(HTMLAttributes.START, value); }
        }

        public string Type
        {
            get { return GetAttribute(HTMLAttributes.TYPE); }
            set { SetAttribute(HTMLAttributes.TYPE, value); }
        }

        #endregion
    }
}
