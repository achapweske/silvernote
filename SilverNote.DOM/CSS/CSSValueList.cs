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
    public interface CSSValueList : CSSValue, IEnumerable<CSSValue>
    {
        int Length { get; }
        CSSValue this[int index] { get; }
    }
}
