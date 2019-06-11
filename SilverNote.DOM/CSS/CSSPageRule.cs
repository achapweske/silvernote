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
    public interface CSSPageRule : CSSRule
    {
        string SelectorText { get; set; }
        CSSStyleDeclaration Style { get; }
    }
}
