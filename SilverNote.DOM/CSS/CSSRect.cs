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
    public interface CSSRect
    {
        CSSPrimitiveValue Top { get; }
        CSSPrimitiveValue Right { get; }
        CSSPrimitiveValue Bottom { get; }
        CSSPrimitiveValue Left { get; }
    }
}
