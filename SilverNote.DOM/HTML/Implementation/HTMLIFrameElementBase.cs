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
    public class HTMLIFrameElementBase : HTMLElementBase, HTMLIFrameElement
    {
        #region Constructors

        internal HTMLIFrameElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.IFRAME, ownerDocument)
        {

        }

        #endregion

        #region HTMLIFrameElement

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        public string FrameBorder
        {
            get { return GetAttribute(HTMLAttributes.FRAMEBORDER); }
            set { SetAttribute(HTMLAttributes.FRAMEBORDER, value); }
        }

        public string Height
        {
            get { return GetAttribute(HTMLAttributes.HEIGHT); }
            set { SetAttribute(HTMLAttributes.HEIGHT, value); }
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

        public string Width
        {
            get { return GetAttribute(HTMLAttributes.WIDTH); }
            set { SetAttribute(HTMLAttributes.WIDTH, value); }
        }

        public Document ContentDocument
        {
            get { throw new NotImplementedException(); }
        }


        #endregion
    }
}
