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
    public interface SVGTextPathElement : SVGTextContentElement, SVGURIReference
    {
        SVGAnimatedLength StartOffset { get; }
        SVGAnimatedEnumeration Method { get; }
        SVGAnimatedEnumeration Spacing { get; }
    }
}
