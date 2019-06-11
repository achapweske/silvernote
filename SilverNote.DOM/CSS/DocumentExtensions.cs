/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Style;

namespace DOM.CSS
{
    public static class DocumentExtensions
    {
        public static StyleSheetList GetStyleSheets(this Document document)
        {
            if (document is DocumentCSS)
            {
                return ((DocumentCSS)document).StyleSheets;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static CSSStyleDeclaration GetOverrideStyle(this Document document, Element elt, string pseudoElt)
        {
            if (document is DocumentCSS)
            {
                return ((DocumentCSS)document).GetOverrideStyle(elt, pseudoElt);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static CSSStyleSheet GetUserStyleSheet(this Document document)
        {
            if (document is DocumentCSS)
            {
                return ((DocumentCSS)document).UserStyleSheet;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static void SetUserStyleSheet(this Document document, CSSStyleSheet styleSheet)
        {
            if (document is DocumentCSS)
            {
                ((DocumentCSS)document).UserStyleSheet = styleSheet;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
