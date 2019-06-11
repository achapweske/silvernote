/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SilverNote
{
    public class SafeConvert
    {
        public static object ToType(Type type, object input, object defaultValue = null)
        {
            if (type == typeof(String))
            {
                return ToString(input);
            }
            else if (type == typeof(bool?))
            {
                return ToBool(input);
            }
            else if (type == typeof(Int32))
            {
                return ToInt32(input);
            }
            else if (type == typeof(Int64))
            {
                return ToInt64(input);
            }
            else if (type == typeof(DateTime))
            {
                return ToDateTime(input);
            }
            else
            {
                return defaultValue;
            }
        }

        #region String

        public static string ToString(object input, string defaultValue = "")
        {
            if (input != null)
            {
                return input.ToString();
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region Int32

        public static Int32 ToInt32(object obj, Int32 defaultValue = 0)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            else if (obj is Int32)
            {
                return (Int32)obj;
            }
            else
            {
                return ToInt32(obj.ToString(), defaultValue);
            }
        }

        public static Int32 ToInt32(string input, Int32 defaultValue = 0)
        {
            if (input == null)
            {
                return defaultValue;
            }

            Int32 result;
            if (Int32.TryParse(input, out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region Int64

        public static Int64 ToInt64(object input, Int64 defaultValue = 0)
        {
            if (input != null)
                return ToInt64(input.ToString(), defaultValue);
            else
                return defaultValue;
        }

        public static Int64 ToInt64(string input, Int64 defaultValue = 0)
        {
            Int64 result;
            if (input == null)
                return defaultValue;
            else if (!Int64.TryParse(input, out result))
                result = defaultValue;
            return result;
        }

        #endregion

        #region Double

        public static double ToDouble(object input, double defaultValue = 0.0)
        {
            if (input != null)
            {
                return ToDouble(input.ToString(), defaultValue);
            }
            else
            {
                return defaultValue;
            }
        }

        public static double ToDouble(string input, double defaultValue = 0.0)
        {
            if (input == null)
            {
                return defaultValue;
            }

            double result;

            if (Double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        public static string ToString(double input, string defaultValue = "")
        {
            return input.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region DoubleCollection

        public static DoubleCollection ToDoubleCollection(string input, DoubleCollection defaultValue = null)
        {
            try
            {
                return DoubleCollection.Parse(input);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string ToString(DoubleCollection collection, string defaultValue = "")
        {
            if (collection != null)
                return collection.ToString();
            else
                return defaultValue;
        }

        #endregion

        #region Bool

        public static bool? ToBool(object input, bool? defaultValue = null)
        {
            if (input != null)
                return ToBool(input.ToString(), defaultValue);
            else
                return defaultValue;
        }

        public static bool? ToBool(string input, bool? defaultValue = null)
        {
            bool result;
            if (input == null)
                return defaultValue;
            else if (input == "0")
                return false;
            else if (input == "1")
                return true;
            else if (bool.TryParse(input, out result))
                return result;
            else
                return defaultValue;
        }

        #endregion

        #region Byte

        public static byte ToByte(object input, byte defaultValue = 0)
        {
            if (input != null)
                return ToByte(input.ToString(), defaultValue);
            else
                return defaultValue;
        }

        public static byte ToByte(string input, byte defaultValue = 0)
        {
            byte result;
            if (input == null)
                return defaultValue;
            else if (!Byte.TryParse(input, out result))
                result = defaultValue;
            return result;
        }

        #endregion

        #region DateTime

        public static DateTime ToDateTime(object input, DateTime defaultValue = new DateTime())
        {
            if (input != null)
                return ToDateTime(input.ToString(), defaultValue);
            else
                return defaultValue;
        }

        public static DateTime ToDateTime(string input, DateTime defaultValue = new DateTime())
        {
            DateTime result;
            if (input == null)
                result = defaultValue;
            else if (!DateTime.TryParse(input, out result))
                result = defaultValue;
            return result;
        }

        #endregion

        #region PointCollection

        public static PointCollection ToPointCollection(string input, PointCollection defaultValue = null)
        {
            try
            {
                return PointCollection.Parse(input);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string ToString(PointCollection collection, string defaultValue = "")
        {
            if (collection != null)
            {
                return collection.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region Matrix

        public static string ToString(Matrix input, string defaultValue = "")
        {
            return "matrix(" +
                input.M11 + " " +
                input.M12 + " " +
                input.M21 + " " +
                input.M22 + " " +
                input.OffsetX + " " +
                input.OffsetY + ")";
        }

        public static Matrix ToMatrix(string input, Matrix defaultValue = new Matrix())
        {
            input = input.Trim();
            if (!input.StartsWith("matrix"))
                return defaultValue;
            input = input.Substring("matrix".Length);
            input = input.TrimStart(new char[] { '(' });
            input = input.TrimEnd(new char[] { ')' });
            string[] values = input.Split();
            if (values.Length != 6)
                return defaultValue;

            Matrix result = new Matrix(
                ToDouble(values[0]),
                ToDouble(values[1]),
                ToDouble(values[2]),
                ToDouble(values[3]),
                ToDouble(values[4]),
                ToDouble(values[5])
            );
            return result;
        }

        #endregion

        #region CSS Size

        public static double ToCssSize(object obj, double defaultValue = 0)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            else if (obj is double)
            {
                return (double)obj;
            }
            else
            {
                return ToCssSize(obj.ToString());
            }
        }

        public static double ToCssSize(string text, double defaultValue = 0)
        {
            text = text.ToLower();

            if (text.EndsWith("em"))
            {
                text = text.Remove(text.Length - 2);
                return defaultValue * ToDouble(text, 1.0);
            }
            else if (text.EndsWith("pt"))
            {
                text = text.Remove(text.Length - 2);
                return ToDouble(text, defaultValue) * 96.0 / 72.0;
            }
            else if (text.EndsWith("px"))
            {
                text = text.Remove(text.Length - 2);
                return ToDouble(text, defaultValue);
            }
            else if (text.EndsWith("%"))
            {
                text = text.Remove(text.Length - 1);
                return defaultValue * ToDouble(text, 1.0) / 100.0;
            }
            else
            {
                return ToDouble(text, defaultValue);
            }
        }

        #endregion

        #region Color

        public static Color ToColor(string input, Color defaultColor = default(Color))
        {
            input = input.Trim();
            if (!input.StartsWith("rgb"))
            {
                return defaultColor;
            }
            input = input.Substring("rgb".Length);
            input = input.TrimStart('(').TrimEnd(')');
            string[] values = input.Split(',');
            if (values.Length != 3)
            {
                return defaultColor;
            }

            byte r = ToByte(values[0].Trim());
            byte g = ToByte(values[1].Trim());
            byte b = ToByte(values[2].Trim());

            if (r == 0 && g == 0 && b == 0)
            {
                return Colors.Black;
            }
            else
            {
                return new Color { R = r, G = g, B = b, A = 255 }; ;
            }
        }

        public static string ToString(Color color, string defaultValue = "")
        {
            if (color.A != 0)
                return "rgb(" + color.R + "," + color.G + "," + color.B + ")";
            else
                return "none";
        }


        #endregion

        #region Brush

        public static Brush ToBrush(object obj, Brush defaultValue = null)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            else if (obj is Brush)
            {
                return (Brush)obj;
            }
            else
            {
                return ToBrush(obj.ToString(), defaultValue);
            }
        }

        public static Brush ToBrush(string str, Brush defaultValue = null)
        {
            if (str == null)
            {
                return defaultValue;
            }
            if (str == "none")
            {
                return null;
            }

            Color color = ToColor(str);
            if (color == Colors.Black)
            {
                return Brushes.Black;
            }
            else
            {
                var brush = new SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }
        }

        public static string ToString(Brush brush, string defaultValue = "")
        {
            if (brush is SolidColorBrush)
            {
                return ToString((SolidColorBrush)brush, defaultValue);
            }
            else
            {
                return defaultValue;
            }
        }

        public static string ToString(SolidColorBrush brush, string defaultValue = "")
        {
            if (brush != null)
            {
                return ToString(brush.Color, defaultValue);
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region FontFamily

        public static FontFamily ToFontFamily(object obj, FontFamily defaultValue = default(FontFamily))
        {
            if (obj == null)
            {
                return defaultValue;
            }
            else if (obj is FontFamily)
            {
                return (FontFamily)obj;
            }
            else
            {
                return ToFontFamily(obj.ToString());
            }
        }

        public static FontFamily ToFontFamily(string str, FontFamily defaultValue = default(FontFamily))
        {
            try
            {
                return new FontFamily(str);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string ToString(FontFamily fontFamily, string defaultValue = "")
        {
            if (fontFamily != null)
            {
                return fontFamily.ToString();
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region FontWeight

        public static FontWeight ToFontWeight(object obj, FontWeight defaultValue = default(FontWeight))
        {
            if (obj == null)
            {
                return defaultValue;
            }
            else if (obj is FontWeight)
            {
                return (FontWeight)obj;
            }
            else
            {
                return ToFontWeight(obj.ToString());
            }
        }

        public static FontWeight ToFontWeight(string text, FontWeight defaultValue = default(FontWeight))
        {
            switch (text.ToLower())
            {
                case "normal":
                    return FontWeights.Normal;
                case "bold":
                    return FontWeights.Bold;
                case "bolder":
                    return ToFontWeight(ToInt32(defaultValue) + 100);
                case "lighter":
                    return ToFontWeight(ToInt32(defaultValue) - 100);
                default:
                    return ToFontWeight(ToInt32(text, ToInt32(defaultValue)));
            }
        }

        public static string ToString(FontWeight fontWeight, string defaultValue = "")
        {
            if (fontWeight == FontWeights.Normal)
                return "normal";
            if (fontWeight == FontWeights.Bold)
                return "bold";
            return ToString(ToInt32(fontWeight), defaultValue);
        }

        public static FontWeight ToFontWeight(int weightClass)
        {
            // http://msdn.microsoft.com/en-us/library/system.windows.fontweights.aspx

            if (weightClass <= 100)
                return FontWeights.Thin;
            if (weightClass <= 200)
                return FontWeights.ExtraLight;
            if (weightClass <= 300)
                return FontWeights.Light;
            if (weightClass <= 400)
                return FontWeights.Normal;
            if (weightClass <= 500)
                return FontWeights.Medium;
            if (weightClass <= 600)
                return FontWeights.SemiBold;
            if (weightClass <= 700)
                return FontWeights.Bold;
            if (weightClass <= 800)
                return FontWeights.ExtraBold;
            if (weightClass <= 900)
                return FontWeights.Black;
            return FontWeights.ExtraBlack;
        }

        public static int ToInt32(FontWeight fontWeight, int defaultValue = 400)
        {
            // http://msdn.microsoft.com/en-us/library/system.windows.fontweights.aspx

            if (fontWeight == FontWeights.Thin)
                return 100;
            if (fontWeight == FontWeights.ExtraLight ||
                fontWeight == FontWeights.UltraLight)
                return 200;
            if (fontWeight == FontWeights.Light)
                return 300;
            if (fontWeight == FontWeights.Normal ||
                fontWeight == FontWeights.Regular)
                return 400;
            if (fontWeight == FontWeights.Medium)
                return 500;
            if (fontWeight == FontWeights.DemiBold ||
                fontWeight == FontWeights.SemiBold)
                return 600;
            if (fontWeight == FontWeights.Bold)
                return 700;
            if (fontWeight == FontWeights.ExtraBold ||
                fontWeight == FontWeights.UltraBold)
                return 800;
            if (fontWeight == FontWeights.Black ||
                fontWeight == FontWeights.Heavy)
                return 900;
            if (fontWeight == FontWeights.ExtraBlack ||
                fontWeight == FontWeights.UltraBlack)
                return 950;
            return defaultValue;
        }

        #endregion

        #region FontStyle

        public static FontStyle ToFontStyle(object obj, FontStyle defaultValue = default(FontStyle))
        {
            if (obj == null)
            {
                return defaultValue;
            }
            else if (obj is FontStyle)
            {
                return (FontStyle)obj;
            }
            else
            {
                return ToFontStyle(obj.ToString(), defaultValue);
            }
        }

        public static FontStyle ToFontStyle(string text, FontStyle defaultValue = default(FontStyle))
        {
            switch (text.ToLower())
            {
                case "normal":
                    return FontStyles.Normal;
                case "italic":
                    return FontStyles.Italic;
                case "oblique":
                    return FontStyles.Oblique;
                default:
                    return defaultValue;
            }
        }

        public static string ToString(FontStyle fontStyle, string defaultValue = "")
        {
            if (fontStyle == FontStyles.Normal)
                return "normal";
            if (fontStyle == FontStyles.Italic)
                return "italic";
            if (fontStyle == FontStyles.Oblique)
                return "oblique";
            return defaultValue;
        }

        #endregion

        #region TextDecoration

        public static TextDecoration ToTextDecoration(string text, TextDecoration defaultValue = null)
        {
            switch (text)
            {
                case "underline":
                    return TextDecorations.Underline[0];
                case "overline":
                    return TextDecorations.OverLine[0];
                case "line-through":
                    return TextDecorations.Strikethrough[0];
                default:
                    return defaultValue;
            }
        }

        public static string ToString(TextDecoration textDecoration, string defaultValue = "")
        {
            if (textDecoration == TextDecorations.Underline[0])
                return "underline";
            if (textDecoration == TextDecorations.OverLine[0])
                return "overline";
            if (textDecoration == TextDecorations.Strikethrough[0])
                return "line-through";
            return defaultValue;
        }

        #endregion

        #region TextDecorationCollection

        public static string ToString(TextDecorationCollection textDecorations, string defaultValue = "")
        {
            if (textDecorations == null)
                return defaultValue;
            if (textDecorations.Count == 0)
                return "none";

            StringBuilder result = new StringBuilder();
            foreach (TextDecoration textDecoration in textDecorations)
            {
                string token = ToString(textDecoration, null);
                if (token != null)
                {
                    if (result.Length > 0)
                        result.Append(' ');
                    result.Append(token);
                }
            }

            if (result.Length > 0)
                return result.ToString();
            else
                return defaultValue;
        }

        public static TextDecorationCollection ToTextDecorations(object obj, TextDecorationCollection defaultValue = null)
        {
            if (obj == null)
            {
                return null;
            }
            else if (obj is TextDecorationCollection)
            {
                return (TextDecorationCollection)obj;
            }
            else
            {
                return ToTextDecorations(obj.ToString(), defaultValue);
            }
        }

        public static TextDecorationCollection ToTextDecorations(string text, TextDecorationCollection defaultValue = null)
        {
            TextDecorationCollection result = new TextDecorationCollection();
            if (text == "none")
            {
                return result;
            }

            string[] tokens = text.Split();
            foreach (string token in tokens)
            {
                TextDecoration decoration = ToTextDecoration(token, null);
                if (decoration != null)
                {
                    result.Add(decoration);
                }
            }

            if (result.Count > 0)
                return result;
            else
                return defaultValue;
        }

        #endregion

        #region BaselineAlignment

        public static BaselineAlignment ToBaselineAlignment(object obj, BaselineAlignment defaultValue = BaselineAlignment.Baseline)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            else if (obj is BaselineAlignment)
            {
                return (BaselineAlignment)obj;
            }
            else
            {
                return ToBaselineAlignment(obj.ToString(), defaultValue);
            }
        }

        public static BaselineAlignment ToBaselineAlignment(string text, BaselineAlignment defaultValue = BaselineAlignment.Baseline)
        {
            switch (text.ToLower())
            {
                case "baseline":
                    return BaselineAlignment.Baseline;
                case "bottom":
                    return BaselineAlignment.Bottom;
                case "middle":
                    return BaselineAlignment.Center;
                case "super":
                    return BaselineAlignment.Superscript;
                case "sub":
                    return BaselineAlignment.Subscript;
                case "text-bottom":
                    return BaselineAlignment.TextBottom;
                case "text-top":
                    return BaselineAlignment.TextTop;
                case "top":
                    return BaselineAlignment.Top;
                default:
                    return defaultValue;
            }
        }

        public static string ToString(BaselineAlignment baselineAlignment, string defaultValue = "")
        {
            switch (baselineAlignment)
            {
                case BaselineAlignment.Baseline:
                    return "baseline";
                case BaselineAlignment.Bottom:
                    return "bottom";
                case BaselineAlignment.Center:
                    return "middle";
                case BaselineAlignment.Superscript:
                    return "super";
                case BaselineAlignment.Subscript:
                    return "sub";
                case BaselineAlignment.TextBottom:
                    return "text-bottom";
                case BaselineAlignment.TextTop:
                    return "text-top";
                case BaselineAlignment.Top:
                    return "top";
                default:
                    return defaultValue;
            }
        }

        #endregion

        #region TextAlignment

        public static TextAlignment ToTextAlignment(object obj, TextAlignment defaultValue = TextAlignment.Left)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            else if (obj is TextAlignment)
            {
                return (TextAlignment)obj;
            }
            else
            {
                return ToTextAlignment(obj.ToString());
            }
        }

        public static TextAlignment ToTextAlignment(string text, TextAlignment defaultValue = TextAlignment.Left)
        {
            switch (text.ToLower())
            {
                case "left":
                    return TextAlignment.Left;
                case "right":
                    return TextAlignment.Right;
                case "center":
                    return TextAlignment.Center;
                case "justify":
                    return TextAlignment.Justify;
                default:
                    return defaultValue;
            }
        }

        public static string ToString(TextAlignment textAlignment, string defaultValue = "")
        {
            switch (textAlignment)
            {
                case TextAlignment.Left:
                    return "left";
                case TextAlignment.Right:
                    return "right";
                case TextAlignment.Center:
                    return "center";
                case TextAlignment.Justify:
                    return "justify";
                default:
                    return defaultValue;
            }
        }

        #endregion

        #region TextWrapping

        public static TextWrapping ToTextWrapping(object obj, TextWrapping defaultValue = TextWrapping.Wrap)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            else if (obj is TextWrapping)
            {
                return (TextWrapping)obj;
            }
            else
            {
                return ToTextWrapping(obj.ToString());
            }
        }

        public static TextWrapping ToTextWrapping(string text, TextWrapping defaultValue = TextWrapping.Wrap)
        {
            switch (text.ToLower())
            {
                case "normal":
                    return TextWrapping.Wrap;
                case "nowrap":
                    return TextWrapping.NoWrap;
                case "overflow":
                    return TextWrapping.WrapWithOverflow;
                default:
                    return defaultValue;
            }
        }

        public static string ToString(TextWrapping TextWrapping, string defaultValue = "")
        {
            switch (TextWrapping)
            {
                case TextWrapping.Wrap:
                    return "normal";
                case TextWrapping.NoWrap:
                    return "nowrap";
                case TextWrapping.WrapWithOverflow:
                    return "overflow";
                default:
                    return defaultValue;
            }
        }

        #endregion


        #region VerticalAlignment

        public static VerticalAlignment ToVerticalAlignment(string text, VerticalAlignment defaultValue = VerticalAlignment.Top)
        {
            switch (text)
            {
                case "top":
                    return VerticalAlignment.Top;
                case "middle":
                    return VerticalAlignment.Center;
                case "bottom":
                    return VerticalAlignment.Bottom;
                default:
                    return defaultValue;
            }
        }

        public static string ToString(VerticalAlignment verticalAlignment, string defaultValue = "")
        {
            switch (verticalAlignment)
            {
                case VerticalAlignment.Top:
                    return "top";
                case VerticalAlignment.Center:
                    return "middle";
                case VerticalAlignment.Bottom:
                    return "bottom";
                default:
                    return defaultValue;
            }
        }

        #endregion

        #region FlowDirection

        public static FlowDirection ToFlowDirection(object obj, FlowDirection defaultValue = FlowDirection.LeftToRight)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            else if (obj is FlowDirection)
            {
                return (FlowDirection)obj;
            }
            else
            {
                return ToFlowDirection(obj.ToString());
            }
        }

        public static FlowDirection ToFlowDirection(string text, FlowDirection defaultValue = FlowDirection.LeftToRight)
        {
            switch (text.ToLower())
            {
                case "ltr":
                    return FlowDirection.LeftToRight;
                case "rtl":
                    return FlowDirection.RightToLeft;
                default:
                    return defaultValue;
            }
        }

        public static string ToString(FlowDirection flowDirection, string defaultValue = "")
        {
            switch (flowDirection)
            {
                case FlowDirection.LeftToRight:
                    return "ltr";
                case FlowDirection.RightToLeft:
                    return "rtl";
                default:
                    return defaultValue;
            }
        }

        #endregion

        #region PenLineCap

        public static PenLineCap ToPenLineCap(object obj, PenLineCap defaultValue)
        {
            if (obj is PenLineCap)
            {
                return (PenLineCap)obj;
            }
            else if (obj == null)
            {
                return defaultValue;
            }
            else
            {
                return ToPenLineCap(obj.ToString(), defaultValue);
            }
        }

        public static PenLineCap ToPenLineCap(string str, PenLineCap defaultValue)
        {
            switch (str.ToLower())
            {
                case "butt":
                    return PenLineCap.Flat;
                case "round":
                    return PenLineCap.Round;
                case "square":
                    return PenLineCap.Square;
                case "triangle":
                    return PenLineCap.Triangle;
                default:
                    return defaultValue;
            }
        }

        public static string ToString(PenLineCap plc, string defaultValue = "")
        {
            switch (plc)
            {
                case PenLineCap.Flat:
                    return "butt";
                case PenLineCap.Round:
                    return "round";
                case PenLineCap.Square:
                    return "square";
                case PenLineCap.Triangle:
                    return "triangle";
                default:
                    return defaultValue;
            }
        }

        #endregion

        #region PenLineJoin

        public static PenLineJoin ToPenLineJoin(object obj, PenLineJoin defaultValue)
        {
            if (obj is PenLineJoin)
            {
                return (PenLineJoin)obj;
            }
            else if (obj == null)
            {
                return defaultValue;
            }
            else
            {
                return ToPenLineJoin(obj.ToString(), defaultValue);
            }
        }

        public static PenLineJoin ToPenLineJoin(string str, PenLineJoin defaultValue)
        {
            switch (str.ToLower())
            {
                case "miter":
                    return PenLineJoin.Miter;
                case "round":
                    return PenLineJoin.Round;
                case "bevel":
                    return PenLineJoin.Bevel;
                default:
                    return defaultValue;
            }
        }

        public static string ToString(PenLineJoin plj, string defaultValue = "")
        {
            switch (plj)
            {
                case PenLineJoin.Miter:
                    return "miter";
                case PenLineJoin.Round:
                    return "round";
                case PenLineJoin.Bevel:
                    return "bevel";
                default:
                    return defaultValue;
            }
        }

        #endregion

    }
}
