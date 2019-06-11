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
    public interface SVGAnimationElement : SVGElement, SVGTests, SVGExternalResourcesRequired, ElementTimeControl
    {
        SVGElement TargetElement { get; }
        double GetStartTime();
        double GetCurrentTime();
        double GetSimpleDuration();
    }
}
