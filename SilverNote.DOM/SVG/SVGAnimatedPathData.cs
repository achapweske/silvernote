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
    public interface SVGAnimatedPathData
    {
        ISVGPathSegList PathSegList { get; }
        ISVGPathSegList NormalizedPathSegList { get; }
        ISVGPathSegList AnimatedPathSegList { get; }
        ISVGPathSegList AnimatedNormalizedPathSegList { get; }
    }
}
