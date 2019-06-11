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
    public class HTMLPreElementBase : HTMLElementBase, HTMLPreElement
    {
        #region Constructors

        internal HTMLPreElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.PRE, ownerDocument)
        {

        }

        #endregion

        #region HTMLPreElement

        public int Width
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.WIDTH, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.WIDTH, value); }
        }

        #endregion
    }
}
