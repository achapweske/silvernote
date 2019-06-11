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
    public interface CSSMediaRule : CSSRule
    {
        MediaList Media { get; }
        CSSRuleList CssRules { get; }
        int InsertRule(string rule, int index);
        void DeleteRule(int index);
    }
}
