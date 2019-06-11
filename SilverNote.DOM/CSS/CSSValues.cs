/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using DOM.CSS.Internal;

namespace DOM.CSS
{
    public static class CSSValues
    {
        #region Identifiers

        public static readonly CSSPrimitiveValue Absolute = Ident("absolute");
        public static readonly CSSPrimitiveValue Auto = Ident("auto");
        public static readonly CSSPrimitiveValue Armenian = Ident("armenian");
        public static readonly CSSPrimitiveValue Baseline = Ident("baseline");
        public static readonly CSSPrimitiveValue Block = Ident("block");
        public static readonly CSSPrimitiveValue Bold = Ident("bold");
        public static readonly CSSPrimitiveValue Bolder = Ident("bolder");
        public static readonly CSSPrimitiveValue BorderBox = Ident("border-box");
        public static readonly CSSPrimitiveValue Both = Ident("both");
        public static readonly CSSPrimitiveValue Bottom = Ident("bottom");
        public static readonly CSSPrimitiveValue Caption = Ident("caption");
        public static readonly CSSPrimitiveValue Center = Ident("center");
        public static readonly CSSPrimitiveValue Circle = Ident("circle");
        public static readonly CSSPrimitiveValue Collapse = Ident("collapse");
        public static readonly CSSPrimitiveValue ContentBox = Ident("content-box");
        public static readonly CSSPrimitiveValue Continuous = Ident("continuous");
        public static readonly CSSPrimitiveValue Clear = Ident("clear");
        public static readonly CSSPrimitiveValue Crosshair = Ident("crosshair");
        public static readonly CSSPrimitiveValue Dashed = Ident("dashed");
        public static readonly CSSPrimitiveValue Decimal = Ident("decimal");
        public static readonly CSSPrimitiveValue DecimalLeadingZero = Ident("decimal-leading-zero");
        public static readonly CSSPrimitiveValue Default = Ident("default");
        public static readonly CSSPrimitiveValue Disc = Ident("disc");
        public static readonly CSSPrimitiveValue Dotted = Ident("dotted");
        public static readonly CSSPrimitiveValue Double = Ident("double");
        public static readonly CSSPrimitiveValue EResize = Ident("e-resize");
        public static readonly CSSPrimitiveValue Fixed = Ident("fixed");
        public static readonly CSSPrimitiveValue Georgian = Ident("georgian");
        public static readonly CSSPrimitiveValue Groove = Ident("groove");
        public static readonly CSSPrimitiveValue Hebrew = Ident("hebrew");
        public static readonly CSSPrimitiveValue Help = Ident("help");
        public static readonly CSSPrimitiveValue Hidden = Ident("hidden");
        public static readonly CSSPrimitiveValue Icon = Ident("icon");
        public static readonly CSSPrimitiveValue Inline = Ident("inline");
        public static readonly CSSPrimitiveValue InlineBlock = Ident("inline-block");
        public static readonly CSSPrimitiveValue InlineTable = Ident("inline-table");
        public static readonly CSSPrimitiveValue Inset = Ident("inset");
        public static readonly CSSPrimitiveValue Invert = Ident("invert");
        public static readonly CSSPrimitiveValue Italic = Ident("italic");
        public static readonly CSSPrimitiveValue Justify = Ident("justify");
        public static readonly CSSPrimitiveValue Large = Ident("large");
        public static readonly CSSPrimitiveValue Larger = Ident("larger");
        public static readonly CSSPrimitiveValue Left = Ident("left");
        public static readonly CSSPrimitiveValue Level = Ident("level");
        public static readonly CSSPrimitiveValue Lighter = Ident("lighter");
        public static readonly CSSPrimitiveValue LineThrough = Ident("line-through");
        public static readonly CSSPrimitiveValue ListItem = Ident("list-item");
        public static readonly CSSPrimitiveValue LowerAlpha = Ident("lower-alpha");
        public static readonly CSSPrimitiveValue LowerGreek = Ident("lower-greek");
        public static readonly CSSPrimitiveValue LowerLatin = Ident("lower-latin");
        public static readonly CSSPrimitiveValue LowerRoman = Ident("lower-roman");
        public static readonly CSSPrimitiveValue Ltr = Ident("ltr");
        public static readonly CSSPrimitiveValue Medium = Ident("medium");
        public static readonly CSSPrimitiveValue Menu = Ident("menu");
        public static readonly CSSPrimitiveValue MessageBox = Ident("message-box");
        public static readonly CSSPrimitiveValue Middle = Ident("middle");
        public static readonly CSSPrimitiveValue Move = Ident("move");
        public static readonly CSSPrimitiveValue NResize = Ident("n-resize");
        public static readonly CSSPrimitiveValue NEResize = Ident("ne-resize");
        public static readonly CSSPrimitiveValue NWResize = Ident("nw-resize");
        public static readonly CSSPrimitiveValue None = Ident("none");
        public static readonly CSSPrimitiveValue NoRepeat = Ident("no-repeat");
        public static readonly CSSPrimitiveValue Nowrap = Ident("nowrap");
        public static readonly CSSPrimitiveValue Normal = Ident("normal");
        public static readonly CSSPrimitiveValue Oblique = Ident("oblique");
        public static readonly CSSPrimitiveValue Once = Ident("once");
        public static readonly CSSPrimitiveValue Outset = Ident("outset");
        public static readonly CSSPrimitiveValue Outside = Ident("outside");
        public static readonly CSSPrimitiveValue Overline = Ident("overline");
        public static readonly CSSPrimitiveValue PaddingBox = Ident("padding-box");
        public static readonly CSSPrimitiveValue Pointer = Ident("pointer");
        public static readonly CSSPrimitiveValue Pre = Ident("pre");
        public static readonly CSSPrimitiveValue PreLine = Ident("pre-line");
        public static readonly CSSPrimitiveValue PreWrap = Ident("pre-wrap");
        public static readonly CSSPrimitiveValue Relative = Ident("relative");
        public static readonly CSSPrimitiveValue Repeat = Ident("repeat");
        public static readonly CSSPrimitiveValue RepeatX = Ident("repeat-x");
        public static readonly CSSPrimitiveValue RepeatY = Ident("repeat-x");
        public static readonly CSSPrimitiveValue Ridge = Ident("ridge");
        public static readonly CSSPrimitiveValue Right = Ident("right");
        public static readonly CSSPrimitiveValue Rtl = Ident("rtl");
        public static readonly CSSPrimitiveValue SResize = Ident("s-resize");
        public static readonly CSSPrimitiveValue SEResize = Ident("se-resize");
        public static readonly CSSPrimitiveValue SWResize = Ident("sw-resize");
        public static readonly CSSPrimitiveValue Separate = Ident("separate");
        public static readonly CSSPrimitiveValue Scroll = Ident("scroll");
        public static readonly CSSPrimitiveValue Show = Ident("show");
        public static readonly CSSPrimitiveValue Solid = Ident("solid");
        public static readonly CSSPrimitiveValue Small = Ident("small");
        public static readonly CSSPrimitiveValue SmallCaps = Ident("small-caps");
        public static readonly CSSPrimitiveValue SmallCaption = Ident("small-caption");
        public static readonly CSSPrimitiveValue Smaller = Ident("smaller");
        public static readonly CSSPrimitiveValue Square = Ident("square");
        public static readonly CSSPrimitiveValue Static = Ident("static");
        public static readonly CSSPrimitiveValue StatusBar = Ident("status-bar");
        public static readonly CSSPrimitiveValue Sub = Ident("sub");
        public static readonly CSSPrimitiveValue Super = Ident("super");
        public static readonly CSSPrimitiveValue Table = Ident("table");
        public static readonly CSSPrimitiveValue TableCaption = Ident("table-caption");
        public static readonly CSSPrimitiveValue TableCell = Ident("table-cell");
        public static readonly CSSPrimitiveValue TableColumn = Ident("table-column");
        public static readonly CSSPrimitiveValue TableColumnGroup = Ident("table-column-group");
        public static readonly CSSPrimitiveValue TableFooterGroup = Ident("table-footer-group");
        public static readonly CSSPrimitiveValue TableHeaderGroup = Ident("table-header-group");
        public static readonly CSSPrimitiveValue TableRow = Ident("table-row");
        public static readonly CSSPrimitiveValue TableRowGroup = Ident("table-row-group");
        public static readonly CSSPrimitiveValue Text = Ident("text");
        public static readonly CSSPrimitiveValue TextBottom = Ident("text-bottom");
        public static readonly CSSPrimitiveValue TextTop = Ident("text-top");
        public static readonly CSSPrimitiveValue Thick = Ident("thick");
        public static readonly CSSPrimitiveValue Thin = Ident("thin");
        public static readonly CSSPrimitiveValue Top = Ident("top");
        public static readonly CSSPrimitiveValue UpperAlpha = Ident("upper-alpha");
        public static readonly CSSPrimitiveValue UpperLatin = Ident("upper-latin");
        public static readonly CSSPrimitiveValue UpperRoman = Ident("upper-roman");
        public static readonly CSSPrimitiveValue Transparent = Ident("transparent");
        public static readonly CSSPrimitiveValue Underline = Ident("underline");
        public static readonly CSSPrimitiveValue Visible = Ident("visible");
        public static readonly CSSPrimitiveValue Wait = Ident("wait");
        public static readonly CSSPrimitiveValue WResize = Ident("w-resize");
        public static readonly CSSPrimitiveValue XLarge = Ident("x-large");
        public static readonly CSSPrimitiveValue XSmall = Ident("x-small");
        public static readonly CSSPrimitiveValue XXLarge = Ident("xx-large");
        public static readonly CSSPrimitiveValue XXSmall = Ident("xx-small");

        public static IEnumerable<CSSPrimitiveValue> ListStyleTypes
        {
            get
            {
                return BulletListStyleTypes.Concat(NumericListStyleTypes);
            }
        }

        public static IEnumerable<CSSPrimitiveValue> BulletListStyleTypes
        {
            get
            {
                return new CSSPrimitiveValue[] {
                    CSSValues.Circle,
                    CSSValues.Disc,
                    CSSValues.Square
                };
            }
        }

        public static IEnumerable<CSSPrimitiveValue> NumericListStyleTypes
        {
            get 
            {
                return new CSSPrimitiveValue[] {
                    CSSValues.Armenian,
                    CSSValues.Decimal,
                    CSSValues.DecimalLeadingZero, 
                    CSSValues.Georgian,
                    CSSValues.Hebrew, 
                    CSSValues.LowerAlpha, 
                    CSSValues.LowerGreek,
                    CSSValues.LowerLatin, 
                    CSSValues.LowerRoman, 
                    CSSValues.UpperAlpha,
                    CSSValues.UpperLatin, 
                    CSSValues.UpperRoman
                };
            }
        }

        #endregion

        #region Colors

        public static readonly CSSPrimitiveValue AliceBlue = Color(RGBColors.AliceBlue);
        public static readonly CSSPrimitiveValue AntiqueWhite = Color(RGBColors.AntiqueWhite);
        public static readonly CSSPrimitiveValue Aqua = Color(RGBColors.Aqua);
        public static readonly CSSPrimitiveValue Aquamarine = Color(RGBColors.Aquamarine);
        public static readonly CSSPrimitiveValue Azure = Color(RGBColors.Azure);
        public static readonly CSSPrimitiveValue Beige = Color(RGBColors.Beige);
        public static readonly CSSPrimitiveValue Bisque = Color(RGBColors.Bisque);
        public static readonly CSSPrimitiveValue Black = Color(RGBColors.Black);
        public static readonly CSSPrimitiveValue BlanchedAlmond = Color(RGBColors.BlanchedAlmond);
        public static readonly CSSPrimitiveValue Blue = Color(RGBColors.Blue);
        public static readonly CSSPrimitiveValue BlueViolet = Color(RGBColors.BlueViolet);
        public static readonly CSSPrimitiveValue Brown = Color(RGBColors.Brown);
        public static readonly CSSPrimitiveValue BurlyWood = Color(RGBColors.BurlyWood);
        public static readonly CSSPrimitiveValue CadetBlue = Color(RGBColors.CadetBlue);
        public static readonly CSSPrimitiveValue ChartReuse = Color(RGBColors.ChartReuse);
        public static readonly CSSPrimitiveValue Chocolate = Color(RGBColors.Chocolate);
        public static readonly CSSPrimitiveValue Coral = Color(RGBColors.Coral);
        public static readonly CSSPrimitiveValue CornFlowerBlue = Color(RGBColors.CornflowerBlue);
        public static readonly CSSPrimitiveValue Cornsilk = Color(RGBColors.Cornsilk);
        public static readonly CSSPrimitiveValue Crimson = Color(RGBColors.Crimson);
        public static readonly CSSPrimitiveValue Cyan = Color(RGBColors.Cyan);
        public static readonly CSSPrimitiveValue DarkBlue = Color(RGBColors.DarkBlue);
        public static readonly CSSPrimitiveValue DarkCyan = Color(RGBColors.DarkCyan);
        public static readonly CSSPrimitiveValue DarkGoldenRod = Color(RGBColors.DarkGoldenRod);
        public static readonly CSSPrimitiveValue DarkGray = Color(RGBColors.DarkGray);
        public static readonly CSSPrimitiveValue DarkGreen = Color(RGBColors.DarkGreen);
        public static readonly CSSPrimitiveValue DarkGrey = Color(RGBColors.DarkGrey);
        public static readonly CSSPrimitiveValue DarkKhaki = Color(RGBColors.DarkKhaki);
        public static readonly CSSPrimitiveValue DarkMagenta = Color(RGBColors.DarkMagenta);
        public static readonly CSSPrimitiveValue DarkOliveGreen = Color(RGBColors.DarkOliveGreen);
        public static readonly CSSPrimitiveValue DarkOrange = Color(RGBColors.DarkOrange);
        public static readonly CSSPrimitiveValue DarkOrchid = Color(RGBColors.DarkOrchid);
        public static readonly CSSPrimitiveValue DarkRed = Color(RGBColors.DarkRed);
        public static readonly CSSPrimitiveValue DarkSalmon = Color(RGBColors.DarkSalmon);
        public static readonly CSSPrimitiveValue DarkSeagreen = Color(RGBColors.DarkSeaGreen);
        public static readonly CSSPrimitiveValue DarkSlateBlue = Color(RGBColors.DarkSlateBlue);
        public static readonly CSSPrimitiveValue DarkSlateGray = Color(RGBColors.DarkSlateGray);
        public static readonly CSSPrimitiveValue DarkSlateGrey = Color(RGBColors.DarkSlateGrey);
        public static readonly CSSPrimitiveValue DarkTurquoise = Color(RGBColors.DarkTurquoise);
        public static readonly CSSPrimitiveValue DarkViolet = Color(RGBColors.DarkViolet);
        public static readonly CSSPrimitiveValue DeepPink = Color(RGBColors.DeepPink);
        public static readonly CSSPrimitiveValue DeepSkyBlue = Color(RGBColors.DeepSkyBlue);
        public static readonly CSSPrimitiveValue DimGray = Color(RGBColors.DimGray);
        public static readonly CSSPrimitiveValue DimGrey = Color(RGBColors.DimGrey);
        public static readonly CSSPrimitiveValue DodgerBlue = Color(RGBColors.DodgerBlue);
        public static readonly CSSPrimitiveValue FireBrick = Color(RGBColors.FireBrick);
        public static readonly CSSPrimitiveValue FloralWhite = Color(RGBColors.FloralWhite);
        public static readonly CSSPrimitiveValue ForestGreen = Color(RGBColors.ForestGreen);
        public static readonly CSSPrimitiveValue Fuchsia = Color(RGBColors.Fuchsia);
        public static readonly CSSPrimitiveValue Gainsboro = Color(RGBColors.Gainsboro);
        public static readonly CSSPrimitiveValue GhostWhite = Color(RGBColors.GhostWhite);
        public static readonly CSSPrimitiveValue Gold = Color(RGBColors.Gold);
        public static readonly CSSPrimitiveValue GoldenRod = Color(RGBColors.GoldenRod);
        public static readonly CSSPrimitiveValue Gray = Color(RGBColors.Gray);
        public static readonly CSSPrimitiveValue Grey = Color(RGBColors.Grey);
        public static readonly CSSPrimitiveValue Green = Color(RGBColors.Green);
        public static readonly CSSPrimitiveValue GreenYellow = Color(RGBColors.GreenYellow);
        public static readonly CSSPrimitiveValue Honeydew = Color(RGBColors.Honeydew);
        public static readonly CSSPrimitiveValue HotPink = Color(RGBColors.HotPink);
        public static readonly CSSPrimitiveValue IndianRed = Color(RGBColors.IndianRed);
        public static readonly CSSPrimitiveValue Indigo = Color(RGBColors.Indigo);
        public static readonly CSSPrimitiveValue Ivory = Color(RGBColors.Ivory);
        public static readonly CSSPrimitiveValue Khaki = Color(RGBColors.Khaki);
        public static readonly CSSPrimitiveValue Lavender = Color(RGBColors.Lavender);
        public static readonly CSSPrimitiveValue LavenderBlush = Color(RGBColors.LavenderBlush);
        public static readonly CSSPrimitiveValue LawnGreen = Color(RGBColors.LawnGreen);
        public static readonly CSSPrimitiveValue LemonChiffon = Color(RGBColors.LemonChiffon);
        public static readonly CSSPrimitiveValue LightBlue = Color(RGBColors.LightBlue);
        public static readonly CSSPrimitiveValue LightCoral = Color(RGBColors.LightCoral);
        public static readonly CSSPrimitiveValue LightCyan = Color(RGBColors.LightCyan);
        public static readonly CSSPrimitiveValue LightGoldenRodYellow = Color(RGBColors.LightGoldenRodYellow);
        public static readonly CSSPrimitiveValue LightGray = Color(RGBColors.LightGray);
        public static readonly CSSPrimitiveValue LightGreen = Color(RGBColors.LightGreen);
        public static readonly CSSPrimitiveValue LightGrey = Color(RGBColors.LightGrey);
        public static readonly CSSPrimitiveValue LightPink = Color(RGBColors.LightPink);
        public static readonly CSSPrimitiveValue LightSalmon = Color(RGBColors.LightSalmon);
        public static readonly CSSPrimitiveValue LightSeaGreen = Color(RGBColors.LightSeaGreen);
        public static readonly CSSPrimitiveValue LightSkyBlue = Color(RGBColors.LightSkyBlue);
        public static readonly CSSPrimitiveValue LightSlateGray = Color(RGBColors.LightSlateGray);
        public static readonly CSSPrimitiveValue LightSlateGrey = Color(RGBColors.LightSlateGrey);
        public static readonly CSSPrimitiveValue LightSteelBlue = Color(RGBColors.LightSteelBlue);
        public static readonly CSSPrimitiveValue LightYellow = Color(RGBColors.LightYellow);
        public static readonly CSSPrimitiveValue Lime = Color(RGBColors.Lime);
        public static readonly CSSPrimitiveValue LimeGreen = Color(RGBColors.LimeGreen);
        public static readonly CSSPrimitiveValue Linen = Color(RGBColors.Linen);
        public static readonly CSSPrimitiveValue Magenta = Color(RGBColors.Magenta);
        public static readonly CSSPrimitiveValue Maroon = Color(RGBColors.Maroon);
        public static readonly CSSPrimitiveValue MediumaquaMarine = Color(RGBColors.MediumAquamarine);
        public static readonly CSSPrimitiveValue MediumBlue = Color(RGBColors.MediumBlue);
        public static readonly CSSPrimitiveValue MediumOrchid = Color(RGBColors.MediumOrchid);
        public static readonly CSSPrimitiveValue MediumPurple = Color(RGBColors.MediumPurple);
        public static readonly CSSPrimitiveValue MediumSeaGreen = Color(RGBColors.MediumSeaGreen);
        public static readonly CSSPrimitiveValue MediumSlateBlue = Color(RGBColors.MediumSlateBlue);
        public static readonly CSSPrimitiveValue MediumSpringGreen = Color(RGBColors.MediumSpringGreen);
        public static readonly CSSPrimitiveValue MediumTurquoise = Color(RGBColors.MediumTurquoise);
        public static readonly CSSPrimitiveValue MediumVioletRed = Color(RGBColors.MediumVioletRed);
        public static readonly CSSPrimitiveValue MidnightBlue = Color(RGBColors.MidnightBlue);
        public static readonly CSSPrimitiveValue MintCream = Color(RGBColors.MintCream);
        public static readonly CSSPrimitiveValue MistyRose = Color(RGBColors.MistyRose);
        public static readonly CSSPrimitiveValue Moccasin = Color(RGBColors.Moccasin);
        public static readonly CSSPrimitiveValue NavajoWhite = Color(RGBColors.NavajoWhite);
        public static readonly CSSPrimitiveValue Navy = Color(RGBColors.Navy);
        public static readonly CSSPrimitiveValue OldLace = Color(RGBColors.OldLace);
        public static readonly CSSPrimitiveValue Olive = Color(RGBColors.Olive);
        public static readonly CSSPrimitiveValue OliveDrab = Color(RGBColors.OliveDrab);
        public static readonly CSSPrimitiveValue Orange = Color(RGBColors.Orange);
        public static readonly CSSPrimitiveValue OrangeRed = Color(RGBColors.OrangeRed);
        public static readonly CSSPrimitiveValue Orchid = Color(RGBColors.Orchid);
        public static readonly CSSPrimitiveValue PaleGoldenRod = Color(RGBColors.PaleGoldenRod);
        public static readonly CSSPrimitiveValue PaleGreen = Color(RGBColors.PaleGreen);
        public static readonly CSSPrimitiveValue PaleTurquoise = Color(RGBColors.PaleTurquoise);
        public static readonly CSSPrimitiveValue PaleVioletRed = Color(RGBColors.PaleVioletRed);
        public static readonly CSSPrimitiveValue PapayaWhip = Color(RGBColors.PapayaWhip);
        public static readonly CSSPrimitiveValue PeachPuff = Color(RGBColors.PeachPuff);
        public static readonly CSSPrimitiveValue Peru = Color(RGBColors.Peru);
        public static readonly CSSPrimitiveValue Pink = Color(RGBColors.Pink);
        public static readonly CSSPrimitiveValue Plum = Color(RGBColors.Plum);
        public static readonly CSSPrimitiveValue PowderBlue = Color(RGBColors.PowderBlue);
        public static readonly CSSPrimitiveValue Purple = Color(RGBColors.Purple);
        public static readonly CSSPrimitiveValue Red = Color(RGBColors.Red);
        public static readonly CSSPrimitiveValue RosyBrown = Color(RGBColors.RosyBrown);
        public static readonly CSSPrimitiveValue RoyalBlue = Color(RGBColors.RoyalBlue);
        public static readonly CSSPrimitiveValue SaddleBrown = Color(RGBColors.SaddleBrown);
        public static readonly CSSPrimitiveValue Salmon = Color(RGBColors.Salmon);
        public static readonly CSSPrimitiveValue SandyBrown = Color(RGBColors.SandyBrown);
        public static readonly CSSPrimitiveValue SeaGreen = Color(RGBColors.SeaGreen);
        public static readonly CSSPrimitiveValue Seashell = Color(RGBColors.Seashell);
        public static readonly CSSPrimitiveValue Sienna = Color(RGBColors.Sienna);
        public static readonly CSSPrimitiveValue Silver = Color(RGBColors.Silver);
        public static readonly CSSPrimitiveValue SkyBlue = Color(RGBColors.SkyBlue);
        public static readonly CSSPrimitiveValue SlateBlue = Color(RGBColors.SlateBlue);
        public static readonly CSSPrimitiveValue SlateGray = Color(RGBColors.SlateGray);
        public static readonly CSSPrimitiveValue SlateGrey = Color(RGBColors.SlateGrey);
        public static readonly CSSPrimitiveValue Snow = Color(RGBColors.Snow);
        public static readonly CSSPrimitiveValue SpringGreen = Color(RGBColors.SpringGreen);
        public static readonly CSSPrimitiveValue SteelBlue = Color(RGBColors.SteelBlue);
        public static readonly CSSPrimitiveValue Tan = Color(RGBColors.Tan);
        public static readonly CSSPrimitiveValue Teal = Color(RGBColors.Teal);
        public static readonly CSSPrimitiveValue Thistle = Color(RGBColors.Thistle);
        public static readonly CSSPrimitiveValue Tomato = Color(RGBColors.Tomato);
        public static readonly CSSPrimitiveValue Turquoise = Color(RGBColors.Turquoise);
        public static readonly CSSPrimitiveValue Violet = Color(RGBColors.Violet);
        public static readonly CSSPrimitiveValue Wheat = Color(RGBColors.Wheat);
        public static readonly CSSPrimitiveValue White = Color(RGBColors.White);
        public static readonly CSSPrimitiveValue WhiteSmoke = Color(RGBColors.WhiteSmoke);
        public static readonly CSSPrimitiveValue Yellow = Color(RGBColors.Yellow);
        public static readonly CSSPrimitiveValue YellowGreen = Color(RGBColors.YellowGreen);

        #endregion

        #region Numbers

        public static readonly CSSPrimitiveValue Zero = Number(0);

        #endregion

        #region Methods

        public static CSSPrimitiveValue Ident(string name)
        {
            return CSSPrimitiveValueBase.Ident(name);
        }

        public static CSSPrimitiveValue String(string str)
        {
            return new CSSPrimitiveValueBase(str, CSSPrimitiveType.CSS_STRING);
        }

        public static CSSPrimitiveValueBase Number(double value)
        {
            return new CSSPrimitiveValueBase(value, CSSPrimitiveType.CSS_NUMBER);
        }

        public static CSSPrimitiveValue Pixels(double lengthInPixels)
        {
            return new CSSPrimitiveValueBase(lengthInPixels, CSSPrimitiveType.CSS_PX);
        }

        public static CSSPrimitiveValue Points(double lengthInPoints)
        {
            return new CSSPrimitiveValueBase(lengthInPoints, CSSPrimitiveType.CSS_PT);
        }

        public static CSSPrimitiveValue Color(string name)
        {
            var color = RGBColorBase.Parse(name);

            return new CSSPrimitiveValueBase(color);
        }

        public static CSSPrimitiveValue Color(RGBColor color)
        {
            return new CSSPrimitiveValueBase(color);
        }

        public static CSSValueList List(string str, char separator)
        {
            return CSSValueListBase.Parse(str, separator = ' ');
        }

        public static CSSValueList List(IEnumerable<CSSValue> values, char separator = ' ')
        {
            return new CSSValueListBase(values, separator);
        }

        #endregion
    }
}
