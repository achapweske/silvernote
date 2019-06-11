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
    public class HTMLAreaElementBase : HTMLElementBase, HTMLAreaElement
    {
        #region Constructors

        internal HTMLAreaElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.AREA, ownerDocument)
        {

        }

        #endregion

        #region HTMLAreaElement

        public string AccessKey
        {
            get { return GetAttribute(HTMLAttributes.ACCESSKEY); }
            set { SetAttribute(HTMLAttributes.ACCESSKEY, value); }
        }

        public string Alt
        {
            get { return GetAttribute(HTMLAttributes.ALT); }
            set { SetAttribute(HTMLAttributes.ALT, value); }
        }

        public string Coords
        {
            get { return GetAttribute(HTMLAttributes.COORDS); }
            set { SetAttribute(HTMLAttributes.COORDS, value); }
        }

        public string HRef
        {
            get { return GetAttribute(HTMLAttributes.HREF); }
            set { SetAttribute(HTMLAttributes.HREF, value); }
        }

        public bool NoHRef
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.NOHREF, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.NOHREF, value); }
        }

        public string Shape
        {
            get { return GetAttribute(HTMLAttributes.SHAPE); }
            set { SetAttribute(HTMLAttributes.SHAPE, value); }
        }

        public int TabIndex
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.TABINDEX, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.TABINDEX, value); }
        }

        public string Target
        {
            get { return GetAttribute(HTMLAttributes.TARGET); }
            set { SetAttribute(HTMLAttributes.TARGET, value); }
        }


        #endregion
    }
}
