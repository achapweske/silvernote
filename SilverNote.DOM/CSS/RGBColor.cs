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
    public interface RGBColor
    {
        CSSPrimitiveValue Red { get; }
        CSSPrimitiveValue Green { get; }
        CSSPrimitiveValue Blue { get; }
    }
}
