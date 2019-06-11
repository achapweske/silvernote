/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.CSS
{
    public interface DOMImplementationCSS : DOMImplementation
    {
        CSSStyleSheet CreateCSSStyleSheet(string title, string media);
    }
}
