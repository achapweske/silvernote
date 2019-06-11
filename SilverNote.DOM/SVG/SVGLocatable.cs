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
    public interface SVGLocatable
    {
        SVGElement NearestViewportElement { get; }
        SVGElement FarthestViewportElement { get; }
        SVGRect GetBBox();
        SVGMatrix GetCTM();
        SVGMatrix GetScreenCTM();
        SVGMatrix GetTransformToElement(SVGElement element);
    }
}
