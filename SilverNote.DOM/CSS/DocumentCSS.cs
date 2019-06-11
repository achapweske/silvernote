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
    /// <summary>
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Style-20001113/css.html#CSS-DocumentCSS
    /// </summary>
    public interface DocumentCSS : DocumentStyle
    {
        CSSStyleDeclaration GetOverrideStyle(Element elt, string pseudoElt);
        CSSStyleSheet UserStyleSheet { get; set; } // Extension
    }
}
