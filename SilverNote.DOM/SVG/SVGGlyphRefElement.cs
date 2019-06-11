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
    public interface SVGGlyphRefElement : SVGElement, SVGURIReference, SVGStylable
    {
        string GlyphRef { get; set; }
        string Format { get; set; }
        double X { get; set; }
        double Y { get; set; }
        double DX { get; set; }
        double DY { get; set; }
    }
}
