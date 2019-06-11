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
    public interface SVGRectElement : SVGElement, SVGTests, SVGLangSpace, SVGExternalResourcesRequired, SVGStylable, SVGTransformable 
    {
        SVGAnimatedLength X { get; }
        SVGAnimatedLength Y { get; }
        SVGAnimatedLength Width { get; }
        SVGAnimatedLength Height { get; }
        SVGAnimatedLength RX { get; }
        SVGAnimatedLength RY { get; }
    }
}
