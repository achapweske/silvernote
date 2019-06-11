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
    public class HTMLOptGroupElementBase : HTMLElementBase, HTMLOptGroupElement
    {
        #region Constructors

        internal HTMLOptGroupElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.OPTGROUP, ownerDocument)
        {

        }

        #endregion

        #region HTMLOptGroupElement

        public bool Disabled
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.DISABLED, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.DISABLED, value); }
        }

        public string Label
        {
            get { return GetAttribute(HTMLAttributes.LABEL); }
            set { SetAttribute(HTMLAttributes.LABEL, value); }
        }

        #endregion
    }
}
