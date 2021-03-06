﻿/*
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
    public interface SVGCircleElement : SVGElement, SVGTests, SVGLangSpace, SVGExternalResourcesRequired, SVGStylable, SVGTransformable
    {
        SVGAnimatedLength CX { get; }
        SVGAnimatedLength CY { get; }
        SVGAnimatedLength R { get; }
    }
}
