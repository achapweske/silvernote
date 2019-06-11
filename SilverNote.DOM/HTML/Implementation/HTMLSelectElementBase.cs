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
    public class HTMLSelectElementBase : HTMLElementBase, HTMLSelectElement
    {
        #region Constructors

        internal HTMLSelectElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.SELECT, ownerDocument)
        {

        }

        #endregion

        #region HTMLSelectElement

        public string Type
        {
            get
            {
                if (this.GetAttributeAsBool(HTMLAttributes.MULTIPLE, false))
                {
                    return "select-multiple";
                }
                else
                {
                    return "select-one";
                }
            }
        }

        public int SelectedIndex 
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string Value
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Length
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public HTMLFormElement Form
        {
            get { return (HTMLFormElement)GetAncestorByTagName(HTMLElements.FORM); }
        }

        public HTMLOptionsCollection Options
        {
            get { throw new NotImplementedException(); }
        }

        public bool Disabled
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.DISABLED, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.DISABLED, value); }
        }

        public bool Multiple
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.DISABLED, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.DISABLED, value); }
        }

        public string Name
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
        }

        public int Size
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.SIZE, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.SIZE, value); }
        }

        public int TabIndex
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.TABINDEX, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.TABINDEX, value); }
        }

        public void Add(HTMLElement element, HTMLElement before)
        {
            InsertBefore(element, before);
        }

        public void Remove(int index)
        {
            throw new NotImplementedException();
        }

        public void Blur()
        {
            throw new NotImplementedException();
        }

        public void Focus()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
