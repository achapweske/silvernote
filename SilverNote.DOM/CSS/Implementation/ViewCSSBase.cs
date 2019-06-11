/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Views;
using DOM.Views.Internal;

namespace DOM.CSS.Internal
{
    public class ViewCSSBase : AbstractViewBase, ViewCSS
    {
        #region Constructors

        public ViewCSSBase(DocumentView document)
            : base(document)
        {
            
        }

        #endregion

        #region ViewCSS

        public CSSStyleDeclaration GetComputedStyle(Element elt, string pseudoElt)
        {
            if (elt is CSSElement)
            {
                return ((CSSElement)elt).ComputedStyle;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
