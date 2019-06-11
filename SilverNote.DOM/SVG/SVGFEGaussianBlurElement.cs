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
    public interface SVGFEGaussianBlurElement : SVGElement, SVGFilterPrimitiveStandardAttributes
    {
        SVGAnimatedString In1 { get; }
        SVGAnimatedNumber StdDeviationX { get; }
        SVGAnimatedNumber StdDeviationY { get; }

        void SetStdDeviation(double stdDeviationX, double stdDeviationY);
    }
}
