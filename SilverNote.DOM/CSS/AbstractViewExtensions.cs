/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM;
using DOM.Views;

namespace DOM.CSS
{
    public static class AbstractViewExtensions
    {
        public static CSSStyleDeclaration GetComputedStyle(this AbstractView view, Element elt, string pseudoElt)
        {
            if (view is ViewCSS)
            {
                return ((ViewCSS)view).GetComputedStyle(elt, pseudoElt);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
