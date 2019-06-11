/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.SVG
{
    public enum SVGLengthType : ushort
    {
        SVG_LENGTHTYPE_UNKNOWN = 0,
        SVG_LENGTHTYPE_NUMBER = 1,
        SVG_LENGTHTYPE_PERCENTAGE = 2,
        SVG_LENGTHTYPE_EMS = 3,
        SVG_LENGTHTYPE_EXS = 4,
        SVG_LENGTHTYPE_PX = 5,
        SVG_LENGTHTYPE_CM = 6,
        SVG_LENGTHTYPE_MM = 7,
        SVG_LENGTHTYPE_IN = 8,
        SVG_LENGTHTYPE_PT = 9,
        SVG_LENGTHTYPE_PC = 10
    }
}
