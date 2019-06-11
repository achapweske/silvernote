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
    /// <summary>
    /// Document head information. See the HEAD element definition in HTML 4.01.
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-77253168
    /// </summary>
    public class HTMLHeadElementBase : HTMLElementBase, HTMLHeadElement
    {
        #region Constructors

        internal HTMLHeadElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.HEAD, ownerDocument)
        {

        }

        #endregion

        #region HTMLHeadElement

        public string Profile
        {
            get { return GetAttribute(HTMLAttributes.PROFILE); }
            set { SetAttribute(HTMLAttributes.PROFILE, value); }
        }

        #endregion
    }
}
