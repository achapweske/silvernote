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
    public class HTMLInputElementBase : HTMLElementBase, HTMLInputElement
    {
        #region Constructors

        internal HTMLInputElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.INPUT, ownerDocument)
        {

        }

        #endregion

        #region HTMLInputElement

        public string DefaultValue
        {
            get { return GetAttribute(HTMLAttributes.VALUE); }
            set { SetAttribute(HTMLAttributes.VALUE, value); }
        }

        public bool DefaultChecked
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.CHECKED, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.CHECKED, value); }
        }

        public HTMLFormElement Form
        {
            get { return (HTMLFormElement)GetAncestorByTagName(HTMLElements.FORM); }
        }

        public string Accept
        {
            get { return GetAttribute(HTMLAttributes.ACCEPT); }
            set { SetAttribute(HTMLAttributes.ACCEPT, value); }
        }

        public string AccessKey
        {
            get { return GetAttribute(HTMLAttributes.ACCESSKEY); }
            set { SetAttribute(HTMLAttributes.ACCESSKEY, value); }
        }

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

        public bool Checked
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool Disabled
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.DISABLED, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.DISABLED, value); }
        }

        public int MaxLength
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.DISABLED, int.MaxValue); }
            set { this.SetAttributeAsInt(HTMLAttributes.DISABLED, value); }
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

        public int Size
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.SIZE, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.SIZE, value); }
        }

        public string Src
        {
            get { return GetAttribute(HTMLAttributes.SRC); }
            set { SetAttribute(HTMLAttributes.SRC, value); }
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

        public string UseMap
        {
            get { return GetAttribute(HTMLAttributes.USEMAP); }
            set { SetAttribute(HTMLAttributes.USEMAP, value); }
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

        public void Click()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
