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
    public enum SVGRenderingIntent : ushort
    {
        RENDERING_INTENT_UNKNOWN = 0,
        RENDERING_INTENT_AUTO = 1,
        RENDERING_INTENT_PERCEPTUAL = 2,
        RENDERING_INTENT_RELATIVE_COLORIMETRIC = 3,
        RENDERING_INTENT_SATURATION = 4,
        RENDERING_INTENT_ABSOLUTE_COLORIMETRIC = 5
    }
}
