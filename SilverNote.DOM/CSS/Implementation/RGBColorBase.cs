/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Reflection;

namespace DOM.CSS.Internal
{
    public class RGBColorBase : RGBColor
    {
        #region Fields

        private CSSPrimitiveValue _Red;
        private CSSPrimitiveValue _Green;
        private CSSPrimitiveValue _Blue;

        #endregion

        #region Constructors

        public RGBColorBase(CSSPrimitiveValue r, CSSPrimitiveValue g, CSSPrimitiveValue b)
        {
            _Red = r;
            _Green = g;
            _Blue = b;
        }

        public RGBColorBase(double r, double g, double b)
        {
            _Red = new CSSPrimitiveValueBase(r, CSSPrimitiveType.CSS_NUMBER);
            _Green = new CSSPrimitiveValueBase(g, CSSPrimitiveType.CSS_NUMBER);
            _Blue = new CSSPrimitiveValueBase(b, CSSPrimitiveType.CSS_NUMBER);
        }

        #endregion

        #region RGBColor

        public CSSPrimitiveValue Red
        {
            get { return _Red; }
        }

        public CSSPrimitiveValue Green
        {
            get { return _Green; }
        }

        public CSSPrimitiveValue Blue
        {
            get { return _Blue; }
        }

        #endregion

        #region Extensions

        public static RGBColorBase FromColor(Color fromColor)
        {
            return new RGBColorBase(fromColor.R, fromColor.G, fromColor.B);
        }

        private Color? _Color;

        public Color ToColor()
        {
            if (_Color == null)
            {
                byte r = (byte)CSSConverter.ToColorComponent(Red, clip: true);
                byte g = (byte)CSSConverter.ToColorComponent(Green, clip: true);
                byte b = (byte)CSSConverter.ToColorComponent(Blue, clip: true);

                _Color = Color.FromRgb(r, g, b);
            }

            return _Color.Value;
        }

        private SolidColorBrush _Brush;

        public SolidColorBrush ToBrush()
        {
            if (_Brush == null)
            {
                Color color = this.ToColor();

                if (!StandardBrushes.TryGetValue(color, out _Brush))
                {
                    _Brush = new SolidColorBrush(color);
                    _Brush.Freeze();
                }
            }

            return _Brush;
        }

        private static Dictionary<Color, SolidColorBrush> StandardBrushes = GetStandardBrushes();

        private static Dictionary<Color, SolidColorBrush> GetStandardBrushes()
        {
            var results = new Dictionary<Color, SolidColorBrush>();

            var properties = typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(SolidColorBrush))
                {
                    var brush = (SolidColorBrush)property.GetValue(null, null);

                    results[brush.Color] = brush;
                }
            }

            return results;
        }

        #endregion

        #region Parsing

        public static RGBColor Parse(string str)
        {
            RGBColor result;
            if (TryParse(str, out result))
            {
                return result;
            }
            else
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }
        }

        public static bool TryParse(string str, out RGBColor result)
        {
            // http://www.w3.org/TR/SVG/types.html#DataTypeColor
            //
            // color    ::= "#" hexdigit hexdigit hexdigit (hexdigit hexdigit hexdigit)?
            //             | "rgb(" wsp* integer comma integer comma integer wsp* ")"
            //             | "rgb(" wsp* integer "%" comma integer "%" comma integer "%" wsp* ")"
            //             | color-keyword
            // hexdigit ::= [0-9A-Fa-f]
            // comma    ::= wsp* "," wsp*

            if (str.StartsWith("#"))
            {
                str = str.Substring(1);
                result = FromHex(str);
            }
            else if (str.StartsWith("rgb("))
            {
                str = str.Substring(4);
                str = str.TrimEnd(')');
                result = FromCSV(str);
            }
            else
            {
                result = RGBColors.FromKeyword(str);
            }

            return result != null;
        }

        private static RGBColorBase FromHex(string str)
        {
            if (!str.All("0123456789abcdefABCDEF".Contains))
            {
                return null;
            }

            if (str.Length == 3)
            {
                int r = FromHex(str[0]);
                int g = FromHex(str[1]);
                int b = FromHex(str[2]);

                r = r * 16 + r;
                g = g * 16 + g;
                b = b * 16 + b;

                return new RGBColorBase(r, g, b);
            }

            if (str.Length == 6)
            {
                int r = FromHex(str[0]) * 16 + FromHex(str[1]);
                int g = FromHex(str[2]) * 16 + FromHex(str[3]);
                int b = FromHex(str[4]) * 16 + FromHex(str[5]);

                return new RGBColorBase(r, g, b);
            }

            return null;
        }

        private static int FromHex(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (c - '0');
            }
            else if (c >= 'a' && c <= 'f')
            {
                return 10 + (c - 'a');
            }
            else if (c >= 'A' && c <= 'F')
            {
                return 10 + (c - 'A');
            }
            else
            {
                throw new ArgumentException("c");
            }
        }

        private static RGBColorBase FromCSV(string str)
        {
            string[] values = str.Trim().Split(',');

            if (values.Length != 3)
            {
                return null;
            }

            double rValue, gValue, bValue;
            CSSPrimitiveType rUnits, gUnits, bUnits;

            if (!TryParseComponent(values[0], out rValue, out rUnits) ||
                !TryParseComponent(values[1], out gValue, out gUnits) ||
                !TryParseComponent(values[2], out bValue, out bUnits))
            {
                return null;
            }

            var r = new CSSPrimitiveValueBase(rValue, rUnits);
            var g = new CSSPrimitiveValueBase(gValue, gUnits);
            var b = new CSSPrimitiveValueBase(bValue, bUnits);

            return new RGBColorBase(r, g, b);
        }

        private static bool TryParseComponent(string str, out double value, out CSSPrimitiveType units)
        {
            str = str.Trim();

            if (str.EndsWith("%"))
            {
                units = CSSPrimitiveType.CSS_PERCENTAGE;
                str = str.Remove(str.Length - 1);
            }
            else
            {
                units = CSSPrimitiveType.CSS_NUMBER;
            }

            return Double.TryParse(str, out value);
        }

        #endregion

        #region Keywords


        #endregion

        #region Object

        public override string ToString()
        {
            return CSSFormatter.FormatColor(this, true);
        }

        #endregion
    }
}
