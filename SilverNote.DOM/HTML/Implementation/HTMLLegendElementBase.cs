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
    public class HTMLLegendElementBase : HTMLElementBase, HTMLLegendElement
    {
        #region Constructors

        internal HTMLLegendElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.LEGEND, ownerDocument)
        {

        }

        #endregion

        #region HTMLLegendElement

        public HTMLFormElement Form
        {
            get { return (HTMLFormElement)GetAncestorByTagName(HTMLElements.FORM); }
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

        #endregion
    }
}
