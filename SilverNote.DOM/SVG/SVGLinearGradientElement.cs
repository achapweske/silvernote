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
    public interface SVGLinearGradientElement : SVGGradientElement
    {
        SVGAnimatedLength X1 { get; }
        SVGAnimatedLength Y1 { get; }
        SVGAnimatedLength X2 { get; }
        SVGAnimatedLength Y2 { get; }
    }
}
