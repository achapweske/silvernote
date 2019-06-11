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
    public class HTMLHRElementBase : HTMLElementBase, HTMLHRElement
    {
        #region Constructors

        internal HTMLHRElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.HR, ownerDocument)
        {

        }

        #endregion

        #region HTMLHRElement

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        public bool NoShade
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.NOSHADE, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.NOSHADE, value); }
        }

        public string Size
        {
            get { return GetAttribute(HTMLAttributes.SIZE); }
            set { SetAttribute(HTMLAttributes.SIZE, value); }
        }

        public string Width
        {
            get { return GetAttribute(HTMLAttributes.WIDTH); }
            set { SetAttribute(HTMLAttributes.WIDTH, value); }
        }

        #endregion
    }
}
