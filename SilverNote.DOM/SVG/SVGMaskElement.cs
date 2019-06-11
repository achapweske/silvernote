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
    public interface SVGMaskElement : SVGElement, SVGTests, SVGLangSpace, SVGExternalResourcesRequired, SVGStylable
    {
        SVGAnimatedEnumeration MaskUnits { get; }
        SVGAnimatedEnumeration MaskContentUnits { get; }
        SVGAnimatedLength X { get; }
        SVGAnimatedLength Y { get; }
        SVGAnimatedLength Width { get; }
        SVGAnimatedLength Height { get; }
    }
}
