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
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using DOM.CSS.Internal;

namespace DOM.CSS
{
    public static class CSSConverter
    {
        #region Length

        public static double ToLength(CSSValue value, CSSPrimitiveType units = CSSPrimitiveType.CSS_PX, double defaultValue = 0.0)
        {
            var primitiveValue = value as CSSPrimitiveValue;
            if (primitiveValue != null && primitiveValue.IsLength())
            {
                return primitiveValue.GetFloatValue(units);
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region Float

        public static double ToFloat(CSSValue value, CSSPrimitiveType type = CSSPrimitiveType.CSS_NUMBER, double defaultValue = 0.0)
        {
            var primitiveValue = value as CSSPrimitiveValue;
            if (primitiveValue != null && primitiveValue.PrimitiveType == type)
            {
                return primitiveValue.GetFloatValue(type);
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region Rect

        public static CSSValue ToCSSValue(Rect rect)
        {
            return new CSSPrimitiveValueBase(CSSRectBase.FromRect(rect));
        }

        #endregion

        #region Color

        public static RGBColor ToRGBColor(Color color)
        {
            return RGBColorBase.FromColor(color);
        }

        public static Color ToColor(RGBColor rgbColor)
        {
            if (rgbColor is RGBColorBase)
            {
                return ((RGBColorBase)rgbColor).ToColor();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static CSSPrimitiveValue ToCSSValue(Color color)
        {
            if (color.A == 0)
            {
                return CSSValues.Transparent;
            }
            else
            {
                return new CSSPrimitiveValueBase(RGBColorBase.FromColor(color));
            }
        }

        public static Color ToColor(CSSPrimitiveValue rgbColorValue)
        {
            return ToColor(rgbColorValue.GetRGBColorValue());
        }

        public static double ToColorComponent(CSSPrimitiveValue value, bool clip)
        {
            double result;

            switch (value.PrimitiveType)
            {
                case CSSPrimitiveType.CSS_PERCENTAGE:
                    result = value.GetFloatValue(CSSPrimitiveType.CSS_PERCENTAGE) * 255.0 / 100.0;
                    break;
                case CSSPrimitiveType.CSS_NUMBER:
                    result = value.GetFloatValue(CSSPrimitiveType.CSS_NUMBER);
                    break;
                default:
                    throw new DOMException(DOMException.INVALID_ACCESS_ERR);
            }

            if (clip)
            {
                result = Math.Max(Math.Min(result, 255), 0);
            }

            return result;
        }

        #endregion

        #region Brush

        public static Brush ToBrush(string value, Brush defaultValue = null)
        {
            var cssValue = CSSPrimitiveValueBase.Parse(value);
            return ToBrush(cssValue, defaultValue);
        }

        public static Brush ToBrush(CSSValue value, Brush defaultValue = null)
        {
            var primitiveValue = value as CSSPrimitiveValue;
            if (primitiveValue != null && primitiveValue.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
            {
                RGBColor rgbColor = primitiveValue.GetRGBColorValue();
                return ToBrush(rgbColor);
            }
            else
            {
                return defaultValue;
            }
        }

        public static Brush ToBrush(RGBColor rgbColor)
        {
            if (rgbColor is RGBColorBase)
            {
                return ((RGBColorBase)rgbColor).ToBrush();
            }
            else
            {
                byte r = (byte)ToColorComponent(rgbColor.Red, clip: true);
                byte g = (byte)ToColorComponent(rgbColor.Green, clip: true);
                byte b = (byte)ToColorComponent(rgbColor.Blue, clip: true);

                Color color = Color.FromRgb(r, g, b);
                Brush brush = new SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }
        }

        public static CSSPrimitiveValue ToCSSValue(Brush brush, CSSPrimitiveValue defaultValue)
        {
            var solidBrush = brush as SolidColorBrush;
            if (solidBrush != null)
            {
                return ToCSSValue(solidBrush.Color);
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region FontFamily

        public static FontFamily ToFontFamily(string value)
        {
            return new FontFamily(value);
        }

        public static FontFamily ToFontFamily(CSSValue value)
        {
            return new FontFamily(value.CssText);
        }

        public static CSSValue ToCSSValue(FontFamily fontFamily, CSSValue defaultValue = null)
        {
            if (fontFamily != null)
            {
                return CSSValues.List(fontFamily.ToString(), ',');
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region FontWeight

        public static CSSValue ToCSSValue(FontWeight fontWeight)
        {
            if (fontWeight == FontWeights.Normal)
            {
                return CSSValues.Normal;
            }
            if (fontWeight == FontWeights.Bold)
            {
                return CSSValues.Bold;
            }

            return FontWeightToCSSValue(WeightClass(fontWeight));
        }

        public static CSSValue FontWeightToCSSValue(int weightClass)
        {
            return new CSSPrimitiveValueBase(weightClass, CSSPrimitiveType.CSS_NUMBER);
        }

        private static FontWeight FontWeightFromClass(int weightClass)
        {
            if (weightClass <= 100)
            {
                return FontWeights.Thin;
            }
            else if (weightClass <= 200)
            {
                return FontWeights.ExtraLight;
            }
            else if (weightClass <= 300)
            {
                return FontWeights.Light;
            }
            else if (weightClass <= 400)
            {
                return FontWeights.Normal;
            }
            else if (weightClass <= 500)
            {
                return FontWeights.Medium;
            }
            else if (weightClass <= 600)
            {
                return FontWeights.SemiBold;
            }
            else if (weightClass <= 700)
            {
                return FontWeights.Bold;
            }
            else if (weightClass <= 800)
            {
                return FontWeights.ExtraBold;
            }
            else if (weightClass <= 900)
            {
                return FontWeights.Black;
            }
            else
            {
                return FontWeights.ExtraBlack;
            }
        }

        private static int WeightClass(FontWeight fontWeight)
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

            return 400;
        }

        public static FontWeight ToFontWeight(string value, FontWeight defaultValue)
        {
            var cssValue = CSSPrimitiveValueBase.Parse(value);

            return ToFontWeight(cssValue, defaultValue);
        }

        public static FontWeight ToFontWeight(CSSValue value, FontWeight defaultValue)
        {
            CSSPrimitiveValue primitiveValue = value as CSSPrimitiveValue;
            if (primitiveValue == null)
            {
                return defaultValue;
            }

            if (primitiveValue.PrimitiveType == CSSPrimitiveType.CSS_NUMBER)
            {
                int weightClass = (int)primitiveValue.GetFloatValue(primitiveValue.PrimitiveType);
                return FontWeightFromClass(weightClass);
            }
            else if (value == CSSValues.Normal)
            {
                return FontWeights.Normal;
            }
            else if (value == CSSValues.Bold)
            {
                return FontWeights.Bold;
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region FontStyle

        public static CSSValue ToCSSValue(FontStyle fontStyle)
        {
            if (fontStyle == FontStyles.Normal)
            {
                return CSSValues.Normal;
            }
            if (fontStyle == FontStyles.Italic)
            {
                return CSSValues.Italic;
            }
            if (fontStyle == FontStyles.Oblique)
            {
                return CSSValues.Oblique;
            }

            return CSSValues.Normal;
        }

        public static FontStyle ToFontStyle(string value, FontStyle defaultValue)
        {
            var cssValue = CSSPrimitiveValueBase.Parse(value);

            return ToFontStyle(cssValue, defaultValue);
        }

        public static FontStyle ToFontStyle(CSSValue value, FontStyle defaultValue)
        {
            if (value == CSSValues.Normal)
            {
                return FontStyles.Normal;
            }
            else if (value == CSSValues.Italic)
            {
                return FontStyles.Italic;
            }
            else if (value == CSSValues.Oblique)
            {
                return FontStyles.Oblique;
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region ListStyleType


        #endregion

        #region TextAlignment

        public static TextAlignment ToTextAlignment(CSSValue value, TextAlignment defaultValue = TextAlignment.Left)
        {
            if (value == CSSValues.Left)
            {
                return TextAlignment.Left;
            }
            else if (value == CSSValues.Center)
            {
                return TextAlignment.Center;
            }
            else if (value == CSSValues.Right)
            {
                return TextAlignment.Right;
            }
            else if (value == CSSValues.Justify)
            {
                return TextAlignment.Justify;
            }
            else
            {
                return defaultValue;
            }
        }

        public static CSSValue ToCSSValue(TextAlignment textAlignment)
        {
            switch (textAlignment)
            {
                case TextAlignment.Left:
                    return CSSValues.Left;
                case TextAlignment.Right:
                    return CSSValues.Right;
                case TextAlignment.Center:
                    return CSSValues.Center;
                case TextAlignment.Justify:
                    return CSSValues.Justify;
                default:
                    return CSSValues.Left;
            }
        }

        #endregion

        #region TextDecorations

        public static CSSValue ToCSSValue(TextDecorationCollection textDecorations)
        {
            if (textDecorations == null || textDecorations.Count == 0)
            {
                return CSSValueListBase.Empty;
            }

            var results = textDecorations.Select(ToCSSValue);

            return new CSSValueListBase(results);
        }

        public static CSSValue ToCSSValue(TextDecoration textDecoration)
        {
            if (textDecoration.Location == TextDecorationLocation.Underline)
            {
                return CSSValues.Underline;
            }

            if (textDecoration.Location == TextDecorationLocation.OverLine)
            {
                return CSSValues.Overline;
            }

            if (textDecoration.Location == TextDecorationLocation.Strikethrough)
            {
                return CSSValues.LineThrough;
            }

            return CSSValues.Underline;
        }

        public static TextDecorationCollection ToTextDecorations(string values, TextDecorationCollection defaultValue)
        {
            var cssValues = CSSValueListBase.Parse(values);

            if (cssValues.Length > 0)
            {
                return ToTextDecorations(cssValues, defaultValue);
            }
            else
            {
                return null;
            }
        }

        public static TextDecorationCollection ToTextDecorations(CSSValue value, TextDecorationCollection defaultValue = null)
        {
            var valueList = value as CSSValueList;
            if (valueList == null)
            {
                return defaultValue;
            }

            bool error = false;

            var results = new TextDecorationCollection();

            foreach (var valueListItem in valueList)
            {
                var result = ToTextDecoration(valueListItem);

                if (result != null)
                {
                    results.Add(result);
                }
                else
                {
                    error = true;
                }
            }

            if (error && results.Count == 0)
            {
                results = defaultValue;
            }

            return results;
        }

        public static TextDecoration ToTextDecoration(CSSValue value)
        {
            if (value == CSSValues.Underline)
            {
                return System.Windows.TextDecorations.Underline[0];
            }
            if (value == CSSValues.Overline)
            {
                return System.Windows.TextDecorations.OverLine[0];
            }
            if (value == CSSValues.LineThrough)
            {
                return System.Windows.TextDecorations.Strikethrough[0];
            }

            return null;
        }

        #endregion

        #region VerticalAlign

        public static CSSValue ToCSSValue(VerticalAlignment verticalAlignment)
        {
            switch (verticalAlignment)
            {
                case VerticalAlignment.Top:
                    return CSSValues.Top;
                case VerticalAlignment.Center:
                    return CSSValues.Middle;
                case VerticalAlignment.Bottom:
                    return CSSValues.Bottom;
                default:
                    return CSSValues.Top;
            }
        }

        public static CSSValue ToCSSValue(BaselineAlignment alignment)
        {
            switch (alignment)
            {
                case BaselineAlignment.Baseline:
                    return CSSValues.Baseline;
                case BaselineAlignment.Bottom:
                    return CSSValues.Bottom;
                case BaselineAlignment.Center:
                    return CSSValues.Middle;
                case BaselineAlignment.Superscript:
                    return CSSValues.Super;
                case BaselineAlignment.Subscript:
                    return CSSValues.Sub;
                case BaselineAlignment.TextBottom:
                    return CSSValues.TextBottom;
                case BaselineAlignment.TextTop:
                    return CSSValues.TextTop;
                case BaselineAlignment.Top:
                    return CSSValues.Top;
                default:
                    return CSSValues.Baseline;
            }
        }

        public static VerticalAlignment ToVerticalAlignment(CSSValue value, VerticalAlignment defaultValue)
        {
            if (value == CSSValues.Top)
            {
                return VerticalAlignment.Top;
            }
            if (value == CSSValues.Middle)
            {
                return VerticalAlignment.Center;
            }
            if (value == CSSValues.Bottom)
            {
                return VerticalAlignment.Bottom;
            }

            return defaultValue;
        }

        public static BaselineAlignment ToBaselineAlignment(string value, BaselineAlignment defaultValue = BaselineAlignment.Baseline)
        {
            var cssValue = CSSPrimitiveValueBase.Parse(value);

            return ToBaselineAlignment(cssValue, defaultValue);
        }

        public static BaselineAlignment ToBaselineAlignment(CSSValue value, BaselineAlignment defaultValue = BaselineAlignment.Baseline)
        {
            if (value == CSSValues.Baseline)
            {
                return BaselineAlignment.Baseline;
            }
            if (value == CSSValues.Bottom)
            {
                return BaselineAlignment.Bottom;
            }
            if (value == CSSValues.Middle)
            {
                return BaselineAlignment.Center;
            }
            if (value == CSSValues.Super)
            {
                return BaselineAlignment.Superscript;
            }
            if (value == CSSValues.Sub)
            {
                return BaselineAlignment.Subscript;
            }
            if (value == CSSValues.TextBottom)
            {
                return BaselineAlignment.TextBottom;
            }
            if (value == CSSValues.TextTop)
            {
                return BaselineAlignment.TextTop;
            }
            if (value == CSSValues.Top)
            {
                return BaselineAlignment.Top;
            }

            return defaultValue;
        }

        #endregion

        #region Cursor

        public static Cursor ToCursor(CSSValue value, Cursor defaultValue = null)
        {
            var valueList = value as CSSValueList;
            if (valueList != null)
            {
                foreach (var item in valueList)
                {
                    var result = ToCursor(item);
                    if (result != null)
                    {
                        return result;
                    }
                }

                return defaultValue;
            }

            if (value == CSSValues.Crosshair)
            {
                return Cursors.Cross;
            }
            else if (value == CSSValues.Default)
            {
                return Cursors.Arrow;
            }
            else if (value == CSSValues.Pointer)
            {
                return Cursors.Hand;
            }
            else if (value == CSSValues.Move)
            {
                return Cursors.ScrollAll;
            }
            else if (value == CSSValues.EResize || value == CSSValues.WResize)
            {
                return Cursors.SizeWE;
            }
            else if (value == CSSValues.NResize || value == CSSValues.SResize)
            {
                return Cursors.SizeNS;
            }
            else if (value == CSSValues.NEResize || value == CSSValues.SWResize)
            {
                return Cursors.SizeNESW;
            }
            else if (value == CSSValues.NWResize || value == CSSValues.SEResize)
            {
                return Cursors.SizeNWSE;
            }
            else if (value == CSSValues.Text)
            {
                return Cursors.IBeam;
            }
            else if (value == CSSValues.Wait)
            {
                return Cursors.Wait;
            }
            else if (value == CSSValues.Help)
            {
                return Cursors.Help;
            }
            else
            {
                return defaultValue;
            }
        }

        public static CSSValue ToCSSValue(Cursor cursor)
        {
            if (cursor == Cursors.Cross)
            {
                return CSSValues.Crosshair;
            }
            else if (cursor == Cursors.Arrow)
            {
                return CSSValues.Default;
            }
            else if (cursor == Cursors.Hand)
            {
                return CSSValues.Pointer;
            }
            else if (cursor == Cursors.ScrollAll)
            {
                return CSSValues.Move;
            }
            else if (cursor == Cursors.SizeWE)
            {
                return CSSValues.EResize;
            }
            else if (cursor == Cursors.SizeNS)
            {
                return CSSValues.SResize;
            }
            else if (cursor == Cursors.SizeNESW)
            {
                return CSSValues.NEResize;
            }
            else if (cursor == Cursors.SizeNWSE)
            {
                return CSSValues.NWResize;
            }
            else if (cursor == Cursors.IBeam)
            {
                return CSSValues.Text;
            }
            else if (cursor == Cursors.Wait)
            {
                return CSSValues.Wait;
            }
            else if (cursor == Cursors.Help)
            {
                return CSSValues.Help;
            }
            else
            {
                return null;
            }
        }

        #endregion

    }
}
