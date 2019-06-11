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
    public interface SVGPathSegArcAbs : SVGPathSeg
    {
        double X { get; set; }
        double Y { get; set; }
        double R1 { get; set; }
        double R2 { get; set; }
        double Angle { get; set; }
        bool LargeArcFlag { get; set; }
        bool SweepFlag { get; set; }
    }
}
