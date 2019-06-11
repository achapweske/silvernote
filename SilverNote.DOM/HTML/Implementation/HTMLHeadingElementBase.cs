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
    /// For the H1 to H6 elements. See the H1 element definition in HTML 4.01.
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-43345119
    /// </summary>
    public class HTMLHeadingElementBase : HTMLElementBase, HTMLHeadingElement
    {
        #region Constructors

        internal HTMLHeadingElementBase(string nodeName, HTMLDocumentImpl ownerDocument)
            : base(nodeName, ownerDocument)
        {

        }

        #endregion

        #region HTMLHeadingElement

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        #endregion

        #region INode

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
