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
    public class HTMLParamElementBase : HTMLElementBase, HTMLParamElement
    {
        #region Constructors

        internal HTMLParamElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.PARAM, ownerDocument)
        {

        }

        #endregion

        #region HTMLParamElement

        public string Name
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
        }

        public string Type
        {
            get { return GetAttribute(HTMLAttributes.TYPE); }
            set { SetAttribute(HTMLAttributes.TYPE, value); }
        }

        public string Value
        {
            get { return GetAttribute(HTMLAttributes.VALUE); }
            set { SetAttribute(HTMLAttributes.VALUE, value); }
        }

        public string ValueType
        {
            get { return GetAttribute(HTMLAttributes.VALUETYPE); }
            set { SetAttribute(HTMLAttributes.VALUETYPE, value); }
        }


        #endregion
    }
}
