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
    public enum SVGTransformType
    {
        SVG_TRANSFORM_UNKNOWN = 0,
        SVG_TRANSFORM_MATRIX = 1,
        SVG_TRANSFORM_TRANSLATE = 2,
        SVG_TRANSFORM_SCALE = 3,
        SVG_TRANSFORM_ROTATE = 4,
        SVG_TRANSFORM_SKEWX = 5,
        SVG_TRANSFORM_SKEWY = 6
    }
}
