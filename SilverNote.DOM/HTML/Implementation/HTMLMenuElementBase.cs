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
    public class HTMLMenuElementBase : HTMLElementBase, HTMLMenuElement
    {
        #region Constructors

        internal HTMLMenuElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.MENU, ownerDocument)
        {

        }

        #endregion

        #region HTMLMenuElement

        public bool Compact
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.COMPACT, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.COMPACT, value); }
        }

        #endregion
    }
}
