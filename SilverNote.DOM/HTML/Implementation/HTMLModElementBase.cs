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
    public class HTMLModElementBase : HTMLElementBase, HTMLModElement
    {
        #region Constructors

        internal HTMLModElementBase(string nodeName, HTMLDocumentImpl ownerDocument)
            : base(nodeName, ownerDocument)
        {

        }

        #endregion

        #region HTMLModElement

        public string Cite
        {
            get { return GetAttribute(HTMLAttributes.CITE); }
            set { SetAttribute(HTMLAttributes.CITE, value); }
        }

        public string DateTime
        {
            get { return GetAttribute(HTMLAttributes.DATETIME); }
            set { SetAttribute(HTMLAttributes.DATETIME, value); }
        }

        #endregion
    }
}
