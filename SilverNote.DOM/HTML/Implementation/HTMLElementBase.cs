/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using DOM.CSS;
using DOM.CSS.Internal;

namespace DOM.HTML.Internal
{
    public class HTMLElementBase : CSSElement, HTMLElement, ElementCSSInlineStyle
    {
        #region Constructors

        public HTMLElementBase(string nodeName, HTMLDocumentImpl ownerDocument)
            : base(ownerDocument, nodeName)
        {

        }

        #endregion

        #region HTMLElement

        public string ID 
        {
            get { return GetAttribute(HTMLAttributes.ID); }
            set { SetAttribute(HTMLAttributes.ID, value); }
        }

        public string Title
        {
            get { return GetAttribute(HTMLAttributes.TITLE); }
            set { SetAttribute(HTMLAttributes.TITLE, value); }
        }

        public string Lang
        {
            get { return GetAttribute(HTMLAttributes.LANG); }
            set { SetAttribute(HTMLAttributes.LANG, value); }
        }

        public string Dir
        {
            get { return GetAttribute(HTMLAttributes.DIR); }
            set { SetAttribute(HTMLAttributes.DIR, value); }
        }

        public string ClassName
        {
            get { return GetAttribute(HTMLAttributes.CLASS); }
            set { SetAttribute(HTMLAttributes.CLASS, value); }
        }

        #endregion

        #region ElementCSSInlineStyle

        /// <summary>
        /// This element's inline style
        /// </summary>
        public CSS3StyleDeclaration Style
        {
            get { return InlineStyle; } 
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return HTMLFormatter.FormatElement(this);
        }

        #endregion
    }
}
