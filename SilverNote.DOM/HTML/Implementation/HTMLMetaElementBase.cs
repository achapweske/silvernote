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
    public class HTMLMetaElementBase : HTMLElementBase, HTMLMetaElement
    {
        #region Constructors

        internal HTMLMetaElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.META, ownerDocument)
        {

        }

        #endregion

        #region HTMLMetaElement

        public string Content
        {
            get { return GetAttribute(HTMLAttributes.CONTENT); }
            set { SetAttribute(HTMLAttributes.CONTENT, value); }
        }

        public string HTTPEquiv
        {
            get { return GetAttribute(HTMLAttributes.HTTP_EQUIV); }
            set { SetAttribute(HTMLAttributes.HTTP_EQUIV, value); }
        }

        public string Name
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
        }

        public string Scheme
        {
            get { return GetAttribute(HTMLAttributes.SCHEME); }
            set { SetAttribute(HTMLAttributes.SCHEME, value); }
        }

        #endregion
    }
}
