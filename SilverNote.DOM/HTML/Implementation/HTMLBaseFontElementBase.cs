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
    public class HTMLBaseFontElementBase : HTMLElementBase, HTMLBaseFontElement
    {
        #region Constructors

        internal HTMLBaseFontElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.BASEFONT, ownerDocument)
        {

        }

        #endregion

        #region HTMLBaseFontElement

        public string Color
        {
            get { return GetAttribute(HTMLAttributes.COLOR); }
            set { SetAttribute(HTMLAttributes.COLOR, value); }
        }

        public string Face
        {
            get { return GetAttribute(HTMLAttributes.FACE); }
            set { SetAttribute(HTMLAttributes.FACE, value); }
        }

        public int Size
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.SIZE, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.SIZE, value); }
        }

        #endregion
    }
}
