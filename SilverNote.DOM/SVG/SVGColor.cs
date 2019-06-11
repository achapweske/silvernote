/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS;

namespace DOM.SVG
{
    public interface SVGColor : CSSValue
    {
        SVGColorType ColorType { get; }
        RGBColor RgbColor { get; }
        SVGICCColor IccColor { get; }

        void SetRGBColor(string rgbColor);
        void SetRGBColorICCColor(string rgbColor, string iccColor);
        void SetColor(SVGColorType colorType, string rgbColor, string iccColor);
    }
}
