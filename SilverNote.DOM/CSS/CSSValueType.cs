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
    public enum CSSValueType : ushort
    {
        CSS_INHERIT                    = 0,
        CSS_PRIMITIVE_VALUE            = 1,
        CSS_VALUE_LIST                 = 2,
        CSS_CUSTOM                     = 3
    }
}
