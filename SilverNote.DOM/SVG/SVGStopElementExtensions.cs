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
    public static class SVGStopElementExtensions
    {
        public static SVGColor GetStopColor(this SVGStopElement element)
        {
            SVGColor result;

            string stopColorAttr = element.GetAttribute(SVGAttributes.STOP_COLOR);
            if (stopColorAttr == null || !SVGParser.TryParseColor(stopColorAttr, out result))
            {
                result = SVGParser.ParseColor("black");
            }

            return result;
        }

        public static void SetStopColor(this SVGStopElement element, SVGColor stopColor)
        {
            string str = SVGFormatter.FormatColor(stopColor);
            element.SetAttribute(SVGAttributes.STOP_COLOR, str);
        }
    }
}
