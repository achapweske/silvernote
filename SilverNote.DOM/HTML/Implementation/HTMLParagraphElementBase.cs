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
    /// Paragraphs. See the P element definition in HTML 4.01.
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-84675076
    /// </summary>
    public class HTMLParagraphElementBase : HTMLElementBase, HTMLParagraphElement
    {
        #region Constructors

        internal HTMLParagraphElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.P, ownerDocument)
        {

        }

        #endregion

        #region HTMLParagraphElement

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        #endregion

        #region Node

        /// <summary>
        /// Overriden from base class
        /// </summary>
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
