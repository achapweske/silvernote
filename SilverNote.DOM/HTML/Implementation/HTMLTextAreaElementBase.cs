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
    public class HTMLTextAreaElementBase : HTMLElementBase, HTMLTextAreaElement
    {
        #region Constructors

        internal HTMLTextAreaElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.TEXTAREA, ownerDocument)
        {

        }

        #endregion

        #region HTMLTextAreaElement

        public string DefaultValue
        {
            get { return TextContent; }
            set { TextContent = value; }
        }

        public HTMLFormElement Form
        {
            get { return (HTMLFormElement)GetAncestorByTagName(HTMLElements.FORM); }
        }

        public string AccessKey
        {
            get { return GetAttribute(HTMLAttributes.ACCESSKEY); }
            set { SetAttribute(HTMLAttributes.ACCESSKEY, value); }
        }

        public int Cols
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.COLS, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.COLS, value); }
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

        public bool ReadOnly
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.READONLY, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.READONLY, value); }
        }

        public int Rows
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.ROWS, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.ROWS, value); }
        }

        public int TabIndex
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.TABINDEX, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.TABINDEX, value); }
        }

        public string Type
        {
            get { return GetAttribute(HTMLAttributes.TYPE); }
            set { SetAttribute(HTMLAttributes.TYPE, value); }
        }

        public string Value 
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Blur()
        {
            throw new NotImplementedException();
        }

        public void Focus()
        {
            throw new NotImplementedException();
        }

        public void Select()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
