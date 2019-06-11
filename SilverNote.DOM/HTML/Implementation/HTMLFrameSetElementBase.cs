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
    public class HTMLFrameSetElementBase : HTMLElementBase, HTMLFrameSetElement
    {
        #region Constructors

        internal HTMLFrameSetElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.FRAMESET, ownerDocument)
        {

        }

        #endregion

        #region HTMLFrameSetElement

        public string Cols
        {
            get { return GetAttribute(HTMLAttributes.COLS); }
            set { SetAttribute(HTMLAttributes.COLS, value); }
        }

        public string Rows
        {
            get { return GetAttribute(HTMLAttributes.COLS); }
            set { SetAttribute(HTMLAttributes.COLS, value); }
        }

        #endregion
    }
}
