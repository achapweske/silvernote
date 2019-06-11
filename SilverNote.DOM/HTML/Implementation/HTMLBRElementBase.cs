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
    public class HTMLBRElementBase : HTMLElementBase, HTMLBRElement
    {
        #region Constructors

        internal HTMLBRElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.BR, ownerDocument)
        {

        }

        #endregion

        #region HTMLBRElement

        public string Clear
        {
            get { return GetAttribute(HTMLAttributes.CLEAR); }
            set { SetAttribute(HTMLAttributes.CLEAR, value); }
        }

        #endregion

        #region INode

        public override string InnerText
        {
            get
            {
                return "\n";
            }
            set
            {
                base.InnerText = value;
            }
        }

        #endregion
    }
}
