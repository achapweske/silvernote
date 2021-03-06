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
    public interface SVGPathSegCurvetoCubicSmoothRel : SVGPathSeg
    {
        double X { get; set; }
        double Y { get; set; }
        double X2 { get; set; }
        double Y2 { get; set; }
    }
}
