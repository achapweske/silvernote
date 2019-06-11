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
    public class HTMLBaseElementBase : HTMLElementBase, HTMLBaseElement
    {
        #region Constructors

        internal HTMLBaseElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.BASE, ownerDocument)
        {

        }

        #endregion

        #region HTMLBaseElement

        public string HRef 
        {
            get { return GetAttribute(HTMLAttributes.HREF); }
            set { SetAttribute(HTMLAttributes.HREF, value); }
        }

        public string Target
        {
            get { return GetAttribute(HTMLAttributes.TARGET); }
            set { SetAttribute(HTMLAttributes.TARGET, value); }
        }

        #endregion
    }
}
