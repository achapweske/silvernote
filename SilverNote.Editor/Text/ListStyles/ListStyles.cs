/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Editor
{
    public static class ListStyles
    {
        public static readonly IListStyle Check = new CheckListStyle();
        public static readonly IListStyle Circle = new CircleListStyle();
        public static readonly IListStyle OpenCircle = new OpenCircleListStyle();
        public static readonly IListStyle Square = new SquareListStyle();
        public static readonly IListStyle OpenSquare = new OpenSquareListStyle();
        public static readonly IListStyle Diamond = new DiamondListStyle();
        public static readonly IListStyle OpenDiamond = new OpenDiamondListStyle();
        public static readonly IListStyle Triangle = new TriangleListStyle();
        public static readonly IListStyle OpenTriangle = new OpenTriangleListStyle();
        public static readonly IListStyle Decimal = new DecimalListStyle();
        public static readonly IListStyle LowerAlpha = new LowerAlphaListStyle();
        public static readonly IListStyle LowerRoman = new LowerRomanListStyle();
        public static readonly IListStyle UpperAlpha = new UpperAlphaListStyle();
        public static readonly IListStyle UpperRoman = new UpperRomanListStyle();

        private static IList<IListStyle> _FilledStyles;

        public static IList<IListStyle> FilledStyles
        {
            get
            {
                if (_FilledStyles == null)
                {
                    _FilledStyles = new[] {
                        Check,
                        Circle,
                        Square,
                        Diamond,
                        Triangle
                    };
                }
                return _FilledStyles;
            }
        }

        private static IList<IListStyle> _OpenStyles;

        public static IList<IListStyle> OpenStyles
        {
            get
            {
                if (_OpenStyles == null)
                {
                    _OpenStyles = new[] {
                        OpenCircle,
                        OpenSquare,
                        OpenDiamond,
                        OpenTriangle
                    };
                }
                return _OpenStyles;
            }
        }

        private static IList<IListStyle> _UnorderedStyles;

        public static IList<IListStyle> UnorderedStyles
        {
            get
            {
                if (_UnorderedStyles == null)
                {
                    _UnorderedStyles = new[] {
                        Decimal,
                        LowerAlpha,
                        LowerRoman,
                        UpperAlpha,
                        UpperRoman
                    };
                }
                return _UnorderedStyles;
            }
        }

        public static IListStyle FromString(string name, IListStyle defaultStyle)
        {
            switch (name)
            {
                case "check":
                    return Check;
                case "disc":
                    return Circle;
                case "circle":
                    return OpenCircle;
                case "square":
                    return Square;
                case "open-square":
                    return OpenSquare;
                case "diamond":
                    return Diamond;
                case "open-diamond":
                    return OpenDiamond;
                case "triangle":
                    return Triangle;
                case "open-triangle":
                    return OpenTriangle;
                case "decimal":
                    return Decimal;
                case "lower-alpha":
                    return LowerAlpha;
                case "lower-roman":
                    return LowerRoman;
                case "upper-alpha":
                    return UpperAlpha;
                case "upper-roman":
                    return UpperRoman;
                case "none":
                    return null;
                default:
                    return defaultStyle;
            }
        }


    }
}
