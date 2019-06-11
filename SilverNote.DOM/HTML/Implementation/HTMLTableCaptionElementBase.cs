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
    /// Table caption See the CAPTION element definition in HTML 4.01.
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-12035137
    /// </summary>
    public class HTMLTableCaptionElementBase : HTMLElementBase, HTMLTableCaptionElement
    {
        #region Constructors

        internal HTMLTableCaptionElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.CAPTION, ownerDocument)
        {

        }

        #endregion

        #region HTMLTableCaptionElement

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        #endregion

        #region Node

        public override string InnerText
        {
            get
            {
                return base.InnerText + "\n";
            }
            set
            {
                base.InnerText = value;
            }
        }

        #endregion
    }
}
