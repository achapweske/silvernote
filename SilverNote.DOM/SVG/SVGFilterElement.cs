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
    public interface SVGFilterElement : SVGElement, SVGURIReference, SVGLangSpace, SVGExternalResourcesRequired, SVGStylable
    {
        SVGAnimatedEnumeration FilterUnits { get; }
        SVGAnimatedEnumeration PrimitiveUnits { get; }
        SVGAnimatedLength X { get; }
        SVGAnimatedLength Y { get; }
        SVGAnimatedLength Width { get; }
        SVGAnimatedLength Height { get; }
        SVGAnimatedInteger FilterResX { get; }
        SVGAnimatedInteger FilterResY { get; }

        void SetFilterRes(int filterResX, int filterResY);
    }
}
