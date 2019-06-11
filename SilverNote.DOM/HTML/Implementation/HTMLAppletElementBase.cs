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
    public class HTMLAppletElementBase : HTMLElementBase, HTMLAppletElement
    {
        #region Constructors

        internal HTMLAppletElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.APPLET, ownerDocument)
        {

        }

        #endregion

        #region HTMLAppletElement

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        public string Alt
        {
            get { return GetAttribute(HTMLAttributes.ALT); }
            set { SetAttribute(HTMLAttributes.ALT, value); }
        }

        public string Archive
        {
            get { return GetAttribute(HTMLAttributes.ARCHIVE); }
            set { SetAttribute(HTMLAttributes.ARCHIVE, value); }
        }

        public string Code
        {
            get { return GetAttribute(HTMLAttributes.CODE); }
            set { SetAttribute(HTMLAttributes.CODE, value); }
        }

        public string CodeBase
        {
            get { return GetAttribute(HTMLAttributes.CODEBASE); }
            set { SetAttribute(HTMLAttributes.CODEBASE, value); }
        }

        public string Height
        {
            get { return GetAttribute(HTMLAttributes.HEIGHT); }
            set { SetAttribute(HTMLAttributes.HEIGHT, value); }
        }

        public int HSpace
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.HSPACE, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.HSPACE, value); }
        }

        public string Name
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
        }

        public string Object
        {
            get { return GetAttribute(HTMLAttributes.OBJECT); }
            set { SetAttribute(HTMLAttributes.OBJECT, value); }
        }

        public int VSpace
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.VSPACE, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.VSPACE, value); }
        }

        public string Width
        {
            get { return GetAttribute(HTMLAttributes.WIDTH); }
            set { SetAttribute(HTMLAttributes.WIDTH, value); }
        }


        #endregion
    }
}
