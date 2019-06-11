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
    public interface SVGGradientElement : SVGElement, SVGURIReference, SVGExternalResourcesRequired, SVGStylable
    {
        SVGAnimatedEnumeration GradientUnits { get; }
        SVGAnimatedTransformList GradientTransform { get; }
        SVGAnimatedEnumeration SpreadMethod { get; }
    }
}
