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
    public interface SVGPaint : SVGColor
    {
        SVGPaintType PaintType { get; }
        string Uri { get; }
        void SetUri(string uri);
        void SetPaint(SVGPaintType paintType, string uri, string rgbColor, string iccColor);
    }
}
