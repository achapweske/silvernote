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
    public enum SVGSpreadMethodTypes : ushort
    {
        SVG_SPREADMETHOD_UNKNOWN = 0,
        SVG_SPREADMETHOD_PAD = 1,
        SVG_SPREADMETHOD_REFLECT = 2,
        SVG_SPREADMETHOD_REPEAT = 3
    }
}

namespace DOM.SVG.Internal
{
    public class SVGSpreadMethodTypesParser : ISVGEnumerationParser
    {
        static SVGSpreadMethodTypesParser _Instance;

        public static SVGSpreadMethodTypesParser Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new SVGSpreadMethodTypesParser();
                }
                return _Instance;
            }
        }

        public ushort Parse(string str)
        {
            switch (str.ToLower())
            {
                case "pad":
                    return (ushort)SVGSpreadMethodTypes.SVG_SPREADMETHOD_PAD;
                case "reflect":
                    return (ushort)SVGSpreadMethodTypes.SVG_SPREADMETHOD_REFLECT;
                case "repeat":
                    return (ushort)SVGSpreadMethodTypes.SVG_SPREADMETHOD_REPEAT;
                default:
                    return (ushort)SVGSpreadMethodTypes.SVG_SPREADMETHOD_UNKNOWN;
            }
        }

        public string Format(ushort value)
        {
            switch ((SVGSpreadMethodTypes)value)
            {
                case SVGSpreadMethodTypes.SVG_SPREADMETHOD_PAD:
                    return "pad";
                case SVGSpreadMethodTypes.SVG_SPREADMETHOD_REFLECT:
                    return "reflect";
                case SVGSpreadMethodTypes.SVG_SPREADMETHOD_REPEAT:
                    return "repeat";
                case SVGSpreadMethodTypes.SVG_SPREADMETHOD_UNKNOWN:
                default:
                    return "";
            }
        }
    }

}
