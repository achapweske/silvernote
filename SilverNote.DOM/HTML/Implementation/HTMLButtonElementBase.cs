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
    public class HTMLButtonElementBase : HTMLElementBase, HTMLButtonElement
    {
        #region Constructors

        internal HTMLButtonElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.BUTTON, ownerDocument)
        {

        }

        #endregion

        #region HTMLButtonElement

        public HTMLFormElement Form
        {
            get { return (HTMLFormElement)GetAncestorByTagName(HTMLElements.FORM); }
        }

        public string AccessKey
        {
            get { return GetAttribute(HTMLAttributes.ACCESSKEY); }
            set { SetAttribute(HTMLAttributes.ACCESSKEY, value); }
        }

        public bool Disabled
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.DISABLED, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.DISABLED, value); }
        }

        public string Name
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
        }

        public int TabIndex
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.TABINDEX, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.TABINDEX, value); }
        }

        public string Type
        {
            get { return GetAttribute(HTMLAttributes.TYPE); }
        }

        public string Value
        {
            get { return TextContent; }
            set { TextContent = value; }
        }

        #endregion
    }
}
