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
    public enum SVGUnitTypes : ushort
    {
        SVG_UNIT_TYPE_UNKNOWN = 0,
        SVG_UNIT_TYPE_USERSPACEONUSE = 1,
        SVG_UNIT_TYPE_OBJECTBOUNDINGBOX = 2
    }
}

namespace DOM.SVG.Internal
{
    public class SVGUnitTypesParser : ISVGEnumerationParser
    {
        static SVGUnitTypesParser _Instance;

        public static SVGUnitTypesParser Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new SVGUnitTypesParser();
                }
                return _Instance;
            }
        }

        public ushort Parse(string str)
        {
            switch (str.ToLower())
            {
                case "userspaceonuse":
                    return (ushort)SVGUnitTypes.SVG_UNIT_TYPE_USERSPACEONUSE;
                case "objectboundingbox":
                    return (ushort)SVGUnitTypes.SVG_UNIT_TYPE_OBJECTBOUNDINGBOX;
                default:
                    return (ushort)SVGUnitTypes.SVG_UNIT_TYPE_UNKNOWN;
            }
        }

        public string Format(ushort value)
        {
            switch ((SVGUnitTypes)value)
            {
                case SVGUnitTypes.SVG_UNIT_TYPE_USERSPACEONUSE:
                    return "userSpaceOnUse";
                case SVGUnitTypes.SVG_UNIT_TYPE_OBJECTBOUNDINGBOX:
                    return "objectBoundingBox";
                case SVGUnitTypes.SVG_UNIT_TYPE_UNKNOWN:
                default:
                    return "";
            }
        }
    }
}
