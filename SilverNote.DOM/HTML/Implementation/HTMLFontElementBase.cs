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
    public class HTMLFontElementBase : HTMLElementBase, HTMLFontElement
    {
        #region Constructors

        internal HTMLFontElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.FONT, ownerDocument)
        {

        }

        #endregion

        #region HTMLFontElement

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

        public string Size
        {
            get { return GetAttribute(HTMLAttributes.SIZE); }
            set { SetAttribute(HTMLAttributes.SIZE, value); }
        }

        #endregion
    }
}
