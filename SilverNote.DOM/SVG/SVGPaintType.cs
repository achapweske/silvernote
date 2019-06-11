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
    public enum SVGPaintType : ushort
    {
        SVG_PAINTTYPE_UNKNOWN = 0,
        SVG_PAINTTYPE_RGBCOLOR = 1,
        SVG_PAINTTYPE_RGBCOLOR_ICCCOLOR = 2,
        SVG_PAINTTYPE_NONE = 101,
        SVG_PAINTTYPE_CURRENTCOLOR = 102,
        SVG_PAINTTYPE_URI_NONE = 103,
        SVG_PAINTTYPE_URI_CURRENTCOLOR = 104,
        SVG_PAINTTYPE_URI_RGBCOLOR = 105,
        SVG_PAINTTYPE_URI_RGBCOLOR_ICCCOLOR = 106,
        SVG_PAINTTYPE_URI = 107
    }
}
