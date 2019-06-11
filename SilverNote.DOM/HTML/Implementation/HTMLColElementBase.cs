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
    public class HTMLColElementBase : HTMLElementBase, HTMLColElement
    {
        #region Constructors

        internal HTMLColElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.COL, ownerDocument)
        {

        }

        #endregion

        #region HTMLColElement

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        public string Ch
        {
            get { return GetAttribute(HTMLAttributes.CH); }
            set { SetAttribute(HTMLAttributes.CH, value); }
        }

        public string ChOff
        {
            get { return GetAttribute(HTMLAttributes.CHOFF); }
            set { SetAttribute(HTMLAttributes.CHOFF, value); }
        }

        public int Span
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.SPAN, 1); }
            set { this.SetAttributeAsInt(HTMLAttributes.SPAN, value); }
        }

        public string VAlign
        {
            get { return GetAttribute(HTMLAttributes.VALIGN); }
            set { SetAttribute(HTMLAttributes.VALIGN, value); }
        }

        public string Width
        {
            get { return GetAttribute(HTMLAttributes.WIDTH); }
            set { SetAttribute(HTMLAttributes.WIDTH, value); }
        }


        #endregion
    }
}
