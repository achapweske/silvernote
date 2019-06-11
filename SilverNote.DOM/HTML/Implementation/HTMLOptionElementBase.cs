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
    public class HTMLOptionElementBase : HTMLElementBase, HTMLOptionElement
    {
        #region Constructors

        internal HTMLOptionElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.OPTION, ownerDocument)
        {

        }

        #endregion

        #region HTMLOptionElement

        public HTMLFormElement Form
        {
            get { return (HTMLFormElement)GetAncestorByTagName(HTMLElements.FORM); }
        }

        public bool DefaultSelected
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.SELECTED, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.SELECTED, value); }
        }

        public string Text
        {
            get { return TextContent; }
        }

        public int Index
        {
            get { throw new NotImplementedException(); }
        }

        public bool Disabled
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.DISABLED, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.DISABLED, value); }
        }

        public string Label
        {
            get { return GetAttribute(HTMLAttributes.LABEL); }
            set { SetAttribute(HTMLAttributes.LABEL, value); }
        }

        public bool Selected
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string Value
        {
            get { return GetAttribute(HTMLAttributes.VALUE); }
            set { SetAttribute(HTMLAttributes.VALUE, value); }
        }

        #endregion
    }
}
