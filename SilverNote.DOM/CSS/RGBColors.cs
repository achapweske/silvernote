/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS.Internal;

namespace DOM.CSS
{
    public static class RGBColors
    {
        public static readonly RGBColor AliceBlue = new RGBColorBase(240, 248, 255);
        public static readonly RGBColor AntiqueWhite = new RGBColorBase(250, 235, 215);
        public static readonly RGBColor Aqua = new RGBColorBase(0, 255, 255);
        public static readonly RGBColor Aquamarine = new RGBColorBase(127, 255, 212);
        public static readonly RGBColor Azure = new RGBColorBase(240, 255, 255);
        public static readonly RGBColor Beige = new RGBColorBase(245, 245, 220);
        public static readonly RGBColor Bisque = new RGBColorBase(255, 228, 196);
        public static readonly RGBColor Black = new RGBColorBase(0, 0, 0);
        public static readonly RGBColor BlanchedAlmond = new RGBColorBase(255, 235, 205);
        public static readonly RGBColor Blue = new RGBColorBase(0, 0, 255);
        public static readonly RGBColor BlueViolet = new RGBColorBase(138, 43, 226);
        public static readonly RGBColor Brown = new RGBColorBase(165, 42, 42);
        public static readonly RGBColor BurlyWood = new RGBColorBase(222, 184, 135);
        public static readonly RGBColor CadetBlue = new RGBColorBase(95, 158, 160);
        public static readonly RGBColor ChartReuse = new RGBColorBase(127, 255, 0);
        public static readonly RGBColor Chocolate = new RGBColorBase(210, 105, 30);
        public static readonly RGBColor Coral = new RGBColorBase(255, 127, 80);
        public static readonly RGBColor CornflowerBlue = new RGBColorBase(100, 149, 237);
        public static readonly RGBColor Cornsilk = new RGBColorBase(255, 248, 220);
        public static readonly RGBColor Crimson = new RGBColorBase(220, 20, 60);
        public static readonly RGBColor Cyan = new RGBColorBase(0, 255, 255);
        public static readonly RGBColor DarkBlue = new RGBColorBase(0, 0, 139);
        public static readonly RGBColor DarkCyan = new RGBColorBase(0, 139, 139);
        public static readonly RGBColor DarkGoldenRod = new RGBColorBase(184, 134, 11);
        public static readonly RGBColor DarkGray = new RGBColorBase(169, 169, 169);
        public static readonly RGBColor DarkGreen = new RGBColorBase(0, 100, 0);
        public static readonly RGBColor DarkGrey = new RGBColorBase(169, 169, 169);
        public static readonly RGBColor DarkKhaki = new RGBColorBase(189, 183, 107);
        public static readonly RGBColor DarkMagenta = new RGBColorBase(139, 0, 139);
        public static readonly RGBColor DarkOliveGreen = new RGBColorBase(85, 107, 47);
        public static readonly RGBColor DarkOrange = new RGBColorBase(255, 140, 0);
        public static readonly RGBColor DarkOrchid = new RGBColorBase(153, 50, 204);
        public static readonly RGBColor DarkRed = new RGBColorBase(139, 0, 0);
        public static readonly RGBColor DarkSalmon = new RGBColorBase(233, 150, 122);
        public static readonly RGBColor DarkSeaGreen = new RGBColorBase(143, 188, 143);
        public static readonly RGBColor DarkSlateBlue = new RGBColorBase(72, 61, 139);
        public static readonly RGBColor DarkSlateGray = new RGBColorBase(47, 79, 79);
        public static readonly RGBColor DarkSlateGrey = new RGBColorBase(47, 79, 79);
        public static readonly RGBColor DarkTurquoise = new RGBColorBase(0, 206, 209);
        public static readonly RGBColor DarkViolet = new RGBColorBase(148, 0, 211);
        public static readonly RGBColor DeepPink = new RGBColorBase(255, 20, 147);
        public static readonly RGBColor DeepSkyBlue = new RGBColorBase(0, 191, 255);
        public static readonly RGBColor DimGray = new RGBColorBase(105, 105, 105);
        public static readonly RGBColor DimGrey = new RGBColorBase(105, 105, 105);
        public static readonly RGBColor DodgerBlue = new RGBColorBase(30, 144, 255);
        public static readonly RGBColor FireBrick = new RGBColorBase(178, 34, 34);
        public static readonly RGBColor FloralWhite = new RGBColorBase(255, 250, 240);
        public static readonly RGBColor ForestGreen = new RGBColorBase(34, 139, 34);
        public static readonly RGBColor Fuchsia = new RGBColorBase(255, 0, 255);
        public static readonly RGBColor Gainsboro = new RGBColorBase(220, 220, 220);
        public static readonly RGBColor GhostWhite = new RGBColorBase(248, 248, 255);
        public static readonly RGBColor Gold = new RGBColorBase(255, 215, 0);
        public static readonly RGBColor GoldenRod = new RGBColorBase(218, 165, 32);
        public static readonly RGBColor Gray = new RGBColorBase(128, 128, 128);
        public static readonly RGBColor Grey = new RGBColorBase(128, 128, 128);
        public static readonly RGBColor Green = new RGBColorBase(0, 128, 0);
        public static readonly RGBColor GreenYellow = new RGBColorBase(173, 255, 47);
        public static readonly RGBColor Honeydew = new RGBColorBase(240, 255, 240);
        public static readonly RGBColor HotPink = new RGBColorBase(255, 105, 180);
        public static readonly RGBColor IndianRed = new RGBColorBase(205, 92, 92);
        public static readonly RGBColor Indigo = new RGBColorBase(75, 0, 130);
        public static readonly RGBColor Ivory = new RGBColorBase(255, 255, 240);
        public static readonly RGBColor Khaki = new RGBColorBase(240, 230, 140);
        public static readonly RGBColor Lavender = new RGBColorBase(230, 230, 250);
        public static readonly RGBColor LavenderBlush = new RGBColorBase(255, 240, 245);
        public static readonly RGBColor LawnGreen = new RGBColorBase(124, 252, 0);
        public static readonly RGBColor LemonChiffon = new RGBColorBase(255, 250, 205);
        public static readonly RGBColor LightBlue = new RGBColorBase(173, 216, 230);
        public static readonly RGBColor LightCoral = new RGBColorBase(240, 128, 128);
        public static readonly RGBColor LightCyan = new RGBColorBase(224, 255, 255);
        public static readonly RGBColor LightGoldenRodYellow = new RGBColorBase(250, 250, 210);
        public static readonly RGBColor LightGray = new RGBColorBase(211, 211, 211);
        public static readonly RGBColor LightGreen = new RGBColorBase(144, 238, 144);
        public static readonly RGBColor LightGrey = new RGBColorBase(211, 211, 211);
        public static readonly RGBColor LightPink = new RGBColorBase(255, 182, 193);
        public static readonly RGBColor LightSalmon = new RGBColorBase(255, 160, 122);
        public static readonly RGBColor LightSeaGreen = new RGBColorBase(32, 178, 170);
        public static readonly RGBColor LightSkyBlue = new RGBColorBase(135, 206, 250);
        public static readonly RGBColor LightSlateGray = new RGBColorBase(119, 136, 153);
        public static readonly RGBColor LightSlateGrey = new RGBColorBase(119, 136, 153);
        public static readonly RGBColor LightSteelBlue = new RGBColorBase(176, 196, 222);
        public static readonly RGBColor LightYellow = new RGBColorBase(255, 255, 224);
        public static readonly RGBColor Lime = new RGBColorBase(0, 255, 0);
        public static readonly RGBColor LimeGreen = new RGBColorBase(50, 205, 50);
        public static readonly RGBColor Linen = new RGBColorBase(250, 240, 230);
        public static readonly RGBColor Magenta = new RGBColorBase(255, 0, 255);
        public static readonly RGBColor Maroon = new RGBColorBase(128, 0, 0);
        public static readonly RGBColor MediumAquamarine = new RGBColorBase(102, 205, 170);
        public static readonly RGBColor MediumBlue = new RGBColorBase(0, 0, 205);
        public static readonly RGBColor MediumOrchid = new RGBColorBase(186, 85, 211);
        public static readonly RGBColor MediumPurple = new RGBColorBase(147, 112, 219);
        public static readonly RGBColor MediumSeaGreen = new RGBColorBase(60, 179, 113);
        public static readonly RGBColor MediumSlateBlue = new RGBColorBase(123, 104, 238);
        public static readonly RGBColor MediumSpringGreen = new RGBColorBase(0, 250, 154);
        public static readonly RGBColor MediumTurquoise = new RGBColorBase(72, 209, 204);
        public static readonly RGBColor MediumVioletRed = new RGBColorBase(199, 21, 133);
        public static readonly RGBColor MidnightBlue = new RGBColorBase(25, 25, 112);
        public static readonly RGBColor MintCream = new RGBColorBase(245, 255, 250);
        public static readonly RGBColor MistyRose = new RGBColorBase(255, 228, 225);
        public static readonly RGBColor Moccasin = new RGBColorBase(255, 228, 181);
        public static readonly RGBColor NavajoWhite = new RGBColorBase(255, 222, 173);
        public static readonly RGBColor Navy = new RGBColorBase(0, 0, 128);
        public static readonly RGBColor OldLace = new RGBColorBase(253, 245, 230);
        public static readonly RGBColor Olive = new RGBColorBase(128, 128, 0);
        public static readonly RGBColor OliveDrab = new RGBColorBase(107, 142, 35);
        public static readonly RGBColor Orange = new RGBColorBase(255, 165, 0);
        public static readonly RGBColor OrangeRed = new RGBColorBase(255, 69, 0);
        public static readonly RGBColor Orchid = new RGBColorBase(218, 112, 214);
        public static readonly RGBColor PaleGoldenRod = new RGBColorBase(238, 232, 170);
        public static readonly RGBColor PaleGreen = new RGBColorBase(152, 251, 152);
        public static readonly RGBColor PaleTurquoise = new RGBColorBase(175, 238, 238);
        public static readonly RGBColor PaleVioletRed = new RGBColorBase(219, 112, 147);
        public static readonly RGBColor PapayaWhip = new RGBColorBase(255, 239, 213);
        public static readonly RGBColor PeachPuff = new RGBColorBase(255, 218, 185);
        public static readonly RGBColor Peru = new RGBColorBase(205, 133, 63);
        public static readonly RGBColor Pink = new RGBColorBase(255, 192, 203);
        public static readonly RGBColor Plum = new RGBColorBase(221, 160, 221);
        public static readonly RGBColor PowderBlue = new RGBColorBase(176, 224, 230);
        public static readonly RGBColor Purple = new RGBColorBase(128, 0, 128);
        public static readonly RGBColor Red = new RGBColorBase(255, 0, 0);
        public static readonly RGBColor RosyBrown = new RGBColorBase(188, 143, 143);
        public static readonly RGBColor RoyalBlue = new RGBColorBase(65, 105, 225);
        public static readonly RGBColor SaddleBrown = new RGBColorBase(139, 69, 19);
        public static readonly RGBColor Salmon = new RGBColorBase(250, 128, 114);
        public static readonly RGBColor SandyBrown = new RGBColorBase(244, 164, 96);
        public static readonly RGBColor SeaGreen = new RGBColorBase(46, 139, 87);
        public static readonly RGBColor Seashell = new RGBColorBase(255, 245, 238);
        public static readonly RGBColor Sienna = new RGBColorBase(160, 82, 45);
        public static readonly RGBColor Silver = new RGBColorBase(192, 192, 192);
        public static readonly RGBColor SkyBlue = new RGBColorBase(135, 206, 235);
        public static readonly RGBColor SlateBlue = new RGBColorBase(106, 90, 205);
        public static readonly RGBColor SlateGray = new RGBColorBase(112, 128, 144);
        public static readonly RGBColor SlateGrey = new RGBColorBase(112, 128, 144);
        public static readonly RGBColor Snow = new RGBColorBase(255, 250, 250);
        public static readonly RGBColor SpringGreen = new RGBColorBase(0, 255, 127);
        public static readonly RGBColor SteelBlue = new RGBColorBase(70, 130, 180);
        public static readonly RGBColor Tan = new RGBColorBase(210, 180, 140);
        public static readonly RGBColor Teal = new RGBColorBase(0, 128, 128);
        public static readonly RGBColor Thistle = new RGBColorBase(216, 191, 216);
        public static readonly RGBColor Tomato = new RGBColorBase(255, 99, 71);
        public static readonly RGBColor Turquoise = new RGBColorBase(64, 224, 208);
        public static readonly RGBColor Violet = new RGBColorBase(238, 130, 238);
        public static readonly RGBColor Wheat = new RGBColorBase(245, 222, 179);
        public static readonly RGBColor White = new RGBColorBase(255, 255, 255);
        public static readonly RGBColor WhiteSmoke = new RGBColorBase(245, 245, 245);
        public static readonly RGBColor Yellow = new RGBColorBase(255, 255, 0);
        public static readonly RGBColor YellowGreen = new RGBColorBase(154, 205, 50);

        public static RGBColor FromKeyword(string keyword)
        {
            // http://www.w3.org/TR/SVG/types.html#ColorKeywords

            switch (keyword.ToLower().Trim())
            {
                case "aliceblue":
                    return RGBColors.AliceBlue;
                case "antiquewhite":
                    return RGBColors.AntiqueWhite;
                case "aqua":
                    return RGBColors.Aqua;
                case "aquamarine":
                    return RGBColors.Aquamarine;
                case "azure":
                    return RGBColors.Azure;
                case "beige":
                    return RGBColors.Beige;
                case "bisque":
                    return RGBColors.Bisque;
                case "black":
                    return RGBColors.Black;
                case "blanchedalmond":
                    return RGBColors.BlanchedAlmond;
                case "blue":
                    return RGBColors.Blue;
                case "blueviolet":
                    return RGBColors.BlueViolet;
                case "brown":
                    return RGBColors.Brown;
                case "burlywood":
                    return RGBColors.BurlyWood;
                case "cadetblue":
                    return RGBColors.CadetBlue;
                case "chartreuse":
                    return RGBColors.ChartReuse;
                case "chocolate":
                    return RGBColors.Chocolate;
                case "coral":
                    return RGBColors.Coral;
                case "cornflowerblue":
                    return RGBColors.CornflowerBlue;
                case "cornsilk":
                    return RGBColors.Cornsilk;
                case "crimson":
                    return RGBColors.Crimson;
                case "cyan":
                    return RGBColors.Cyan;
                case "darkblue":
                    return RGBColors.DarkBlue;
                case "darkcyan":
                    return RGBColors.DarkCyan;
                case "darkgoldenrod":
                    return RGBColors.DarkGoldenRod;
                case "darkgray":
                    return RGBColors.DarkGray;
                case "darkgreen":
                    return RGBColors.DarkGreen;
                case "darkgrey":
                    return RGBColors.DarkGrey;
                case "darkkhaki":
                    return RGBColors.DarkKhaki;
                case "darkmagenta":
                    return RGBColors.DarkMagenta;
                case "darkolivegreen":
                    return RGBColors.DarkOliveGreen;
                case "darkorange":
                    return RGBColors.DarkOrange;
                case "darkorchid":
                    return RGBColors.DarkOrchid;
                case "darkred":
                    return RGBColors.DarkRed;
                case "darksalmon":
                    return RGBColors.DarkSalmon;
                case "darkseagreen":
                    return RGBColors.DarkSeaGreen;
                case "darkslateblue":
                    return RGBColors.DarkSlateBlue;
                case "darkslategray":
                    return RGBColors.DarkSlateGray;
                case "darkslategrey":
                    return RGBColors.DarkSlateGrey;
                case "darkturquoise":
                    return RGBColors.DarkTurquoise;
                case "darkviolet":
                    return RGBColors.DarkViolet;
                case "deeppink":
                    return RGBColors.DeepPink;
                case "deepskyblue":
                    return RGBColors.DeepSkyBlue;
                case "dimgray":
                    return RGBColors.DimGray;
                case "dimgrey":
                    return RGBColors.DimGrey;
                case "dodgerblue":
                    return RGBColors.DodgerBlue;
                case "firebrick":
                    return RGBColors.FireBrick;
                case "floralwhite":
                    return RGBColors.FloralWhite;
                case "forestgreen":
                    return RGBColors.ForestGreen;
                case "fuchsia":
                    return RGBColors.Fuchsia;
                case "gainsboro":
                    return RGBColors.Gainsboro;
                case "ghostwhite":
                    return RGBColors.GhostWhite;
                case "gold":
                    return RGBColors.Gold;
                case "goldenrod":
                    return RGBColors.GoldenRod;
                case "gray":
                    return RGBColors.Gray;
                case "grey":
                    return RGBColors.Grey;
                case "green":
                    return RGBColors.Green;
                case "greenyellow":
                    return RGBColors.GreenYellow;
                case "honeydew":
                    return RGBColors.Honeydew;
                case "hotpink":
                    return RGBColors.HotPink;
                case "indianred":
                    return RGBColors.IndianRed;
                case "indigo":
                    return RGBColors.Indigo;
                case "ivory":
                    return RGBColors.Ivory;
                case "khaki":
                    return RGBColors.Khaki;
                case "lavender":
                    return RGBColors.Lavender;
                case "lavenderblush":
                    return RGBColors.LavenderBlush;
                case "lawngreen":
                    return RGBColors.LawnGreen;
                case "lemonchiffon":
                    return RGBColors.LemonChiffon;
                case "lightblue":
                    return RGBColors.LightBlue;
                case "lightcoral":
                    return RGBColors.LightCoral;
                case "lightcyan":
                    return RGBColors.LightCyan;
                case "lightgoldenrodyellow":
                    return RGBColors.LightGoldenRodYellow;
                case "lightgray":
                    return RGBColors.LightGray;
                case "lightgreen":
                    return RGBColors.LightGreen;
                case "lightgrey":
                    return RGBColors.LightGrey;
                case "lightpink":
                    return RGBColors.LightPink;
                case "lightsalmon":
                    return RGBColors.LightSalmon;
                case "lightseagreen":
                    return RGBColors.LightSeaGreen;
                case "lightskyblue":
                    return RGBColors.LightSkyBlue;
                case "lightslategray":
                    return RGBColors.LightSlateGray;
                case "lightslategrey":
                    return RGBColors.LightSlateGrey;
                case "lightsteelblue":
                    return RGBColors.LightSteelBlue;
                case "lightyellow":
                    return RGBColors.LightYellow;
                case "lime":
                    return RGBColors.Lime;
                case "limegreen":
                    return RGBColors.LimeGreen;
                case "linen":
                    return RGBColors.Linen;
                case "magenta":
                    return RGBColors.Magenta;
                case "maroon":
                    return RGBColors.Maroon;
                case "mediumaquamarine":
                    return RGBColors.MediumAquamarine;
                case "mediumblue":
                    return RGBColors.MediumBlue;
                case "mediumorchid":
                    return RGBColors.MediumOrchid;
                case "mediumpurple":
                    return RGBColors.MediumPurple;
                case "mediumseagreen":
                    return RGBColors.MediumSeaGreen;
                case "mediumslateblue":
                    return RGBColors.MediumSlateBlue;
                case "mediumspringgreen":
                    return RGBColors.MediumSpringGreen;
                case "mediumturquoise":
                    return RGBColors.MediumTurquoise;
                case "mediumvioletred":
                    return RGBColors.MediumVioletRed;
                case "midnightblue":
                    return RGBColors.MidnightBlue;
                case "mintcream":
                    return RGBColors.MintCream;
                case "mistyrose":
                    return RGBColors.MistyRose;
                case "moccasin":
                    return RGBColors.Moccasin;
                case "navajowhite":
                    return RGBColors.NavajoWhite;
                case "navy":
                    return RGBColors.Navy;
                case "oldlace":
                    return RGBColors.OldLace;
                case "olive":
                    return RGBColors.Olive;
                case "olivedrab":
                    return RGBColors.OliveDrab;
                case "orange":
                    return RGBColors.Orange;
                case "orangered":
                    return RGBColors.OrangeRed;
                case "orchid":
                    return RGBColors.Orchid;
                case "palegoldenrod":
                    return RGBColors.PaleGoldenRod;
                case "palegreen":
                    return RGBColors.PaleGreen;
                case "paleturquoise":
                    return RGBColors.PaleTurquoise;
                case "palevioletred":
                    return RGBColors.PaleVioletRed;
                case "papayawhip":
                    return RGBColors.PapayaWhip;
                case "peachpuff":
                    return RGBColors.PeachPuff;
                case "peru":
                    return RGBColors.Peru;
                case "pink":
                    return RGBColors.Pink;
                case "plum":
                    return RGBColors.Plum;
                case "powderblue":
                    return RGBColors.PowderBlue;
                case "purple":
                    return RGBColors.Purple;
                case "red":
                    return RGBColors.Red;
                case "rosybrown":
                    return RGBColors.RosyBrown;
                case "royalblue":
                    return RGBColors.RoyalBlue;
                case "saddlebrown":
                    return RGBColors.SaddleBrown;
                case "salmon":
                    return RGBColors.Salmon;
                case "sandybrown":
                    return RGBColors.SandyBrown;
                case "seagreen":
                    return RGBColors.SeaGreen;
                case "seashell":
                    return RGBColors.Seashell;
                case "sienna":
                    return RGBColors.Sienna;
                case "silver":
                    return RGBColors.Silver;
                case "skyblue":
                    return RGBColors.SkyBlue;
                case "slateblue":
                    return RGBColors.SlateBlue;
                case "slategray":
                    return RGBColors.SlateGray;
                case "slategrey":
                    return RGBColors.SlateGrey;
                case "snow":
                    return RGBColors.Snow;
                case "springgreen":
                    return RGBColors.SpringGreen;
                case "steelblue":
                    return RGBColors.SteelBlue;
                case "tan":
                    return RGBColors.Tan;
                case "teal":
                    return RGBColors.Teal;
                case "thistle":
                    return RGBColors.Thistle;
                case "tomato":
                    return RGBColors.Tomato;
                case "turquoise":
                    return RGBColors.Turquoise;
                case "violet":
                    return RGBColors.Violet;
                case "wheat":
                    return RGBColors.Wheat;
                case "white":
                    return RGBColors.White;
                case "whitesmoke":
                    return RGBColors.WhiteSmoke;
                case "yellow":
                    return RGBColors.Yellow;
                case "yellowgreen":
                    return RGBColors.YellowGreen;
                default:
                    return null;
            }
        }

    }
}
