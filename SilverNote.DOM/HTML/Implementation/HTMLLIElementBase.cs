/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Internal;

namespace DOM.HTML.Internal
{
    /// <summary>
    /// List item. See the LI element definition in HTML 4.01.
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-74680021
    /// </summary>
    public class HTMLLIElementBase : HTMLElementBase, HTMLLIElement
    {
        #region Constructors

        internal HTMLLIElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.LI, ownerDocument)
        {

        }

        #endregion

        #region HTMLLIElement

        public string Type
        {
            get { return GetAttribute(HTMLAttributes.TYPE); }
            set { SetAttribute(HTMLAttributes.TYPE, value); }
        }

        public int Value
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.VALUE, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.VALUE, value); }
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
