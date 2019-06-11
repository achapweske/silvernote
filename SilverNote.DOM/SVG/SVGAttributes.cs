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
    public static class SVGAttributes
    {
        #region Attribute Names

        public const string BASELINE_SHIFT = "baseline-shift";
        public const string CLASS = "class";
        public const string CONTENT_SCRIPT_TYPE = "contentScriptType";
        public const string CONTENT_STYLE_TYPE = "contentStyleType";
        public const string CX = "cx";
        public const string CY = "cy";
        public const string D = "d";
        public const string DX = "dx";
        public const string DY = "dy";
        public const string EXTERNAL_RESOURCES_REQUIRED = "externalResourcesRequired";
        public const string FILL = "fill";
        public const string FILL_OPACITY = "fill-opacity";
        public const string FILTER = "filter";
        public const string FILTER_RES = "filterRes";
        public const string FILTER_UNITS = "filterUnits";
        public const string FONT_FAMILY = "font-family";
        public const string FONT_SIZE = "font-size";
        public const string FONT_STYLE = "font-style";
        public const string FONT_WEIGHT = "font-weight";
        public const string GRADIENT_TRANSFORM = "gradientTransform";
        public const string GRADIENT_UNITS = "gradientUnits";
        public const string HEIGHT = "height";
        public const string HREF = "href";  // should this be xlink:href?
        public const string ID = "id";
        public const string IN = "in";
        public const string LANG = "xml:lang";
        public const string MARKER_START = "marker-start";
        public const string MARKER_END = "marker-end";
        public const string MARKER_WIDTH = "markerWidth";
        public const string MARKER_HEIGHT = "markerHeight";
        public const string MARKER_UNITS = "markerUnits";
        public const string MEDIA = "media";
        public const string ORIENT = "orient";
        public const string OFFSET = "offset";
        public const string POINTS = "points";
        public const string PRIMITIVE_UNITS = "primitiveUnits";
        public const string REF_X = "refX";
        public const string REF_Y = "refY";
        public const string RESULT = "result";
        public const string RX = "rx";
        public const string RY = "ry";
        public const string SPACE = "xml:space";
        public const string SPREAD_METHOD = "spreadMethod";
        public const string STD_DEVIATION = "stdDeviation";
        public const string STOP_COLOR = "stop-color";
        public const string STROKE = "stroke";
        public const string STROKE_DASHARRAY = "stroke-dasharray";
        public const string STROKE_LINECAP = "stroke-linecap";
        public const string STROKE_LINEJOIN = "stroke-linejoin";
        public const string STROKE_OPACITY = "stroke-opacity";
        public const string STROKE_WIDTH = "stroke-width";
        public const string TEXT_DECORATION = "text-decoration";
        public const string TITLE = "title";
        public const string TRANSFORM = "transform";
        public const string TYPE = "type";
        public const string VERSION = "version";
        public const string WIDTH = "width";
        public const string X = "x";
        public const string XLINK_HREF = "xlink:href";
        public const string Y = "y";
        public const string X1 = "x1";
        public const string Y1 = "y1";
        public const string X2 = "x2";
        public const string Y2 = "y2";
        public const string X3 = "x3";
        public const string Y3 = "y3";

        #endregion
    }
}
