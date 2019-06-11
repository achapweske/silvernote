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
    public class HTMLFrameElementBase : HTMLElementBase, HTMLFrameElement
    {
        #region Constructors

        internal HTMLFrameElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.FRAME, ownerDocument)
        {

        }

        #endregion

        #region HTMLFrameElement

        public string FrameBorder
        {
            get { return GetAttribute(HTMLAttributes.FRAMEBORDER); }
            set { SetAttribute(HTMLAttributes.FRAMEBORDER, value); }
        }

        public string LongDesc
        {
            get { return GetAttribute(HTMLAttributes.LONGDESC); }
            set { SetAttribute(HTMLAttributes.LONGDESC, value); }
        }

        public string MarginHeight
        {
            get { return GetAttribute(HTMLAttributes.MARGINHEIGHT); }
            set { SetAttribute(HTMLAttributes.MARGINHEIGHT, value); }
        }

        public string MarginWidth
        {
            get { return GetAttribute(HTMLAttributes.MARGINWIDTH); }
            set { SetAttribute(HTMLAttributes.MARGINWIDTH, value); }
        }

        public string Name
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
        }

        public bool NoResize
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.NORESIZE, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.NORESIZE, value); }
        }

        public string Scrolling
        {
            get { return GetAttribute(HTMLAttributes.SCROLLING); }
            set { SetAttribute(HTMLAttributes.SCROLLING, value); }
        }

        public string Src
        {
            get { return GetAttribute(HTMLAttributes.SRC); }
            set { SetAttribute(HTMLAttributes.SRC, value); }
        }

        public Document ContentDocument
        {
            get { throw new NotImplementedException(); }
        }


        #endregion
    }
}
