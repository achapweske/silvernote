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
    public enum CSSPrimitiveType : ushort
    {
        // Unrecognized unit type
        CSS_UNKNOWN                    = 0,
        // Number with no units
        CSS_NUMBER                     = 1,
        CSS_PERCENTAGE                 = 2,
        CSS_EMS                        = 3,
        CSS_EXS                        = 4,
        CSS_PX                         = 5,
        CSS_CM                         = 6,
        CSS_MM                         = 7,
        CSS_IN                         = 8,
        CSS_PT                         = 9,
        CSS_PC                         = 10,
        CSS_DEG                        = 11,
        CSS_RAD                        = 12,
        CSS_GRAD                       = 13,
        CSS_MS                         = 14,
        CSS_S                          = 15,
        CSS_HZ                         = 16,
        CSS_KHZ                        = 17,
        CSS_DIMENSION                  = 18,
        CSS_STRING                     = 19,
        CSS_URI                        = 20,
        CSS_IDENT                      = 21,
        CSS_ATTR                       = 22,
        CSS_COUNTER                    = 23,
        CSS_RECT                       = 24,
        CSS_RGBCOLOR                   = 25
    }
}
