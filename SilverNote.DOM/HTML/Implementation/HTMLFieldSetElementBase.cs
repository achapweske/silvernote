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
    public class HTMLFieldSetElementBase : HTMLElementBase, HTMLFieldSetElement
    {
        #region Constructors

        internal HTMLFieldSetElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.FIELDSET, ownerDocument)
        {

        }

        #endregion

        #region HTMLFieldSetElement

        public HTMLFormElement Form
        {
            get { return (HTMLFormElement)GetAncestorByTagName(HTMLElements.FORM); }
        }

        #endregion
    }
}
