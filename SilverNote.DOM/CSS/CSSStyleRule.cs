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
    /// <summary>
    /// The CSSStyleRule interface represents a single rule set in a CSS style sheet.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Style-20001113/css.html#CSS-CSSStyleRule
    /// </summary>
    public interface CSSStyleRule : CSSRule
    {
        string SelectorText { get; set; }
        CSSStyleDeclaration Style { get; }
    }
}
