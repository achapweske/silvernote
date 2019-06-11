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
    public class HTMLScriptElementBase : HTMLElementBase, HTMLScriptElement
    {
        #region Constructors

        internal HTMLScriptElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.SCRIPT, ownerDocument)
        {

        }

        #endregion

        #region HTMLScriptElement

        public string Text
        {
            get { return GetAttribute(HTMLAttributes.TEXT); }
            set { SetAttribute(HTMLAttributes.TEXT, value); }
        }

        public string Charset
        {
            get { return GetAttribute(HTMLAttributes.CHARSET); }
            set { SetAttribute(HTMLAttributes.CHARSET, value); }
        }

        public bool Defer
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.DEFER, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.DEFER, value); }
        }

        public string Src
        {
            get { return GetAttribute(HTMLAttributes.SRC); }
            set { SetAttribute(HTMLAttributes.SRC, value); }
        }

        public string Type
        {
            get { return GetAttribute(HTMLAttributes.TYPE); }
            set { SetAttribute(HTMLAttributes.TYPE, value); }
        }


        #endregion
    }
}
