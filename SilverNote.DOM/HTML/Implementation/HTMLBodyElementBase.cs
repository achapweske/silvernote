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
    /// The HTML document body. 
    ///
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-62018039
    /// </summary>
    public class HTMLBodyElementBase : HTMLElementBase, HTMLBodyElement
    {
        #region Constructors

        internal HTMLBodyElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.BODY, ownerDocument)
        {

        }

        #endregion

        #region HTMLBodyElement

        public string ALink
        {
            get { return GetAttribute(HTMLAttributes.ALINK); }
            set { SetAttribute(HTMLAttributes.ALINK, value); }
        }

        public string Background
        {
            get { return GetAttribute(HTMLAttributes.BACKGROUND); }
            set { SetAttribute(HTMLAttributes.BACKGROUND, value); }
        }

        public string BGColor
        {
            get { return GetAttribute(HTMLAttributes.BGCOLOR); }
            set { SetAttribute(HTMLAttributes.BGCOLOR, value); }
        }

        public string Link 
        {
            get { return GetAttribute(HTMLAttributes.LINK); }
            set { SetAttribute(HTMLAttributes.LINK, value); }
        }

        public string Text
        {
            get { return GetAttribute(HTMLAttributes.TEXT); }
            set { SetAttribute(HTMLAttributes.TEXT, value); }
        }

        public string VLink
        {
            get { return GetAttribute(HTMLAttributes.VLINK); }
            set { SetAttribute(HTMLAttributes.VLINK, value); }
        }

        #endregion

    }
}
