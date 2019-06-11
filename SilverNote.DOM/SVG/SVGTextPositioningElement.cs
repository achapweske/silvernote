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
    public interface SVGTextPositioningElement : SVGTextContentElement
    {
        SVGAnimatedLengthList X { get; }
        SVGAnimatedLengthList Y { get; }
        SVGAnimatedLengthList DX { get; }
        SVGAnimatedLengthList DY { get; }
        SVGAnimatedNumberList Rotate { get; }
    }
}
