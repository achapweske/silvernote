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
    public class HTMLFormElementBase : HTMLElementBase, HTMLFormElement
    {
        #region Constructors

        internal HTMLFormElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.FORM, ownerDocument)
        {

        }

        #endregion

        #region HTMLFormElement

        public HTMLCollection Elements
        {
            get { throw new NotImplementedException(); }
        }

        public int Length
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
        }

        public string AcceptCharset
        {
            get { return GetAttribute(HTMLAttributes.ACCEPT_CHARSET); }
            set { SetAttribute(HTMLAttributes.ACCEPT_CHARSET, value); }
        }

        public string Action
        {
            get { return GetAttribute(HTMLAttributes.ACTION); }
            set { SetAttribute(HTMLAttributes.ACTION, value); }
        }

        public string EncType
        {
            get { return GetAttribute(HTMLAttributes.ENCTYPE); }
            set { SetAttribute(HTMLAttributes.ENCTYPE, value); }
        }

        public string Method
        {
            get { return GetAttribute(HTMLAttributes.METHOD); }
            set { SetAttribute(HTMLAttributes.METHOD, value); }
        }

        public string Target
        {
            get { return GetAttribute(HTMLAttributes.TARGET); }
            set { SetAttribute(HTMLAttributes.TARGET, value); }
        }

        public void Submit()
        {
            throw new NotImplementedException(); 
        }
        
        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
