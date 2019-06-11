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
    public interface CSSImportRule : CSSRule
    {
        string HRef { get; }
        MediaList Media { get; }
        CSSStyleSheet StyleSheet { get; }
    }
}
