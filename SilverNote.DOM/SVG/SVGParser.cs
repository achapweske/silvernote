/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using DOM.SVG.Internal;

namespace DOM.SVG
{
    public static class SVGParser
    {
        #region Tokenizer

        private static StringBuilder _Buffer = new StringBuilder();

        private static StringBuilder Buffer
        {
            get
            {
                _Buffer.Clear();
                return _Buffer;
            }
        }

        // integer ::= [+-]? [0-9]+

        public static string ReadInteger(TextReader reader)
        {
            var result = Buffer;

            if (!TryReadInteger(reader, result))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            return result.ToString();
        }

        public static bool TryReadInteger(TextReader reader, StringBuilder result)
        {
            char c = (char)reader.Peek();

            // [+-]?
            if (c == '+' || c == '-')
            {
                reader.Read();
                result.Append(c);
                c = (char)reader.Peek();
            }

            int digitCount = 0;

            // [0-9]+
            while (Char.IsDigit(c))
            {
                reader.Read();
                result.Append(c);
                c = (char)reader.Peek();
                digitCount++;
            }

            return digitCount > 0;
        }

        // number ::= integer ([Ee] integer)?
        //            | [+-]? [0-9]* "." [0-9]+ ([Ee] integer)?
        //
        //          ~ [+-]? [0-9]* ("." [0-9]+)? ([Ee] integer)?

        public static string ReadNumber(TextReader reader)
        {
            var result = Buffer;

            if (!TryReadNumber(reader, result))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            return result.ToString();
        }

        public static bool TryReadNumber(TextReader reader, StringBuilder result)
        {
            char c = (char)reader.Peek();

            // [+-]?
            if (c == '+' || c == '-')
            {
                reader.Read();
                result.Append(c);
                c = (char)reader.Peek();
            }

            // [0-9]*
            while (Char.IsDigit(c))
            {
                reader.Read();
                result.Append(c);
                c = (char)reader.Peek();
            }

            // ("." [0-9]+)?
            if (c == '.')
            {
                reader.Read();
                result.Append(c);
                c = (char)reader.Peek();

                int digitCount = 0;

                while (Char.IsDigit(c))
                {
                    reader.Read();
                    result.Append(c);
                    c = (char)reader.Peek();
                    digitCount++;
                }

                if (digitCount == 0)
                {
                    return false;
                }
            }

            // ([Ee] integer)?

            if (c == 'E' || c == 'e')
            {
                reader.Read();
                result.Append(c);
                c = (char)reader.Peek();

                if (!TryReadInteger(reader, result))
                {
                    return false;
                }
            }

            return true;
        }

        // Read the name portion of a function such as uri(), rgb(), translate(), etc.

        public static bool TryReadFuncName(TextReader reader, StringBuilder result)
        {
            char c = (char)reader.Peek();

            if (!Char.IsLetter(c))
            {
                return false;
            }

            int charCount = 0;

            while (Char.IsLetterOrDigit(c) || c == '-')
            {
                reader.Read();
                result.Append(c);
                c = (char)reader.Peek();
                charCount++;
            }

            return charCount > 0;
        }

        // w	[ \t\r\n\f]*

        public static bool TryReadWhitespace(TextReader reader, StringBuilder buffer)
        {
            char c = (char)reader.Peek();
            if (!IsWhitespace(c))
            {
                return false;
            }

            do
            {
                reader.Read();
                buffer.Append(c);
                c = (char)reader.Peek();

            } while (IsWhitespace(c));

            return true;
        }

        public static bool SkipWhitespace(TextReader reader)
        {
            char c = (char)reader.Peek();
            if (!IsWhitespace(c))
            {
                return false;
            }

            do
            {
                reader.Read();
                c = (char)reader.Peek();

            } while (IsWhitespace(c));

            return true;
        }

        // [ \t\r\n\f]

        public static bool IsWhitespace(char c)
        {
            return (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\f');
        }

        private static char ReadChar(TextReader reader, char c)
        {
            if ((char)reader.Peek() != c)
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            return (char)reader.Read();
        }

        private static bool TryReadChar(TextReader reader, out char c)
        {
            int i = reader.Peek();
            if (i != -1)
            {
                c = (char)i;
                return true;
            }
            else
            {
                c = default(char);
                return false;
            }
        }

        private static bool TryReadChar(TextReader reader, StringBuilder result, char c)
        {
            if ((char)reader.Peek() == c)
            {
                result.Append((char)reader.Read());
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool SkipChar(TextReader reader, char c)
        {
            if ((char)reader.Peek() == c)
            {
                reader.Read();
                return true;
            }
            else
            {
                return false;
            }
        }

        // comma-wsp:
        //    (wsp+ comma? wsp*) | (comma wsp*)
        // comma:
        //    ","

        private static void ReadCommaWsp(TextReader reader)
        {
            // (comma wsp*)
            if (reader.Peek() == ',')
            {
                // comma
                reader.Read();
                // wsp*
                SkipWhitespace(reader);
            }
            // (wsp+ comma? wsp*)
            else if (IsWhitespace((char)reader.Peek()))
            {
                SkipWhitespace(reader);
                if (reader.Peek() == ',')
                {
                    reader.Read();
                    SkipWhitespace(reader);
                }
            }
            else
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }
        }

        #endregion

        #region Grammer

        #region number

        public static double ParseNumber(string str)
        {
            double value;
            if (!Double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value))
            {
                throw new DOMException(DOMException.SYNTAX_ERR, "Syntax error: \"" + str + "\" is not a valid number");
            }
            return value;
        }

        #endregion

        #region <length>

        // length ::= number ("em" | "ex" | "px" | "in" | "cm" | "mm" | "pt" | "pc" | "%")?

        public static SVGLength ParseLength(string text, SVGLength result = null)
        {
            using (var reader = new StringReader(text))
            {
                return ParseLength(reader, result);
            }
        }

        public static SVGLength ParseLength(TextReader reader, SVGLength result = null)
        {
            string valueString = ReadNumber(reader);
            double value = ParseNumber(valueString);

            var unitString = new StringBuilder();
            char c = (char)reader.Peek();
            while (Char.IsLetter(c) || c == '%')
            {
                reader.Read();
                unitString.Append(c);
                c = (char)reader.Peek();
            }

            SVGLengthType units = LengthTypeFromString(unitString.ToString());

            if (result != null)
            {
                result.NewValueSpecifiedUnits(units, value);
            }
            else
            {
                result = new MutableSVGLength(value, units);
            }

            return result;
        }

        static SVGLengthType LengthTypeFromString(string str)
        {
            switch (str.ToLower())
            {
                case "%":
                    return SVGLengthType.SVG_LENGTHTYPE_PERCENTAGE;
                case "em":
                    return SVGLengthType.SVG_LENGTHTYPE_EMS;
                case "ex":
                    return SVGLengthType.SVG_LENGTHTYPE_EXS;
                case "px":
                    return SVGLengthType.SVG_LENGTHTYPE_PX;
                case "cm":
                    return SVGLengthType.SVG_LENGTHTYPE_CM;
                case "mm":
                    return SVGLengthType.SVG_LENGTHTYPE_MM;
                case "in":
                    return SVGLengthType.SVG_LENGTHTYPE_IN;
                case "pt":
                    return SVGLengthType.SVG_LENGTHTYPE_PT;
                case "pc":
                    return SVGLengthType.SVG_LENGTHTYPE_PC;
                case "":
                    return SVGLengthType.SVG_LENGTHTYPE_NUMBER;
                default:
                    return SVGLengthType.SVG_LENGTHTYPE_UNKNOWN;
            }
        }

        #endregion

        #region <angle>

        // angle ::= number ("deg" | "grad" | "rad")?

        public static SVGAngle ParseAngle(string text, SVGAngle result = null)
        {
            using (var reader = new StringReader(text))
            {
                return ParseAngle(reader, result);
            }
        }

        public static SVGAngle ParseAngle(TextReader reader, SVGAngle result = null)
        {
            string valueString = ReadNumber(reader);

            double value;
            if (!Double.TryParse(valueString, out value))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            var unitString = new StringBuilder();
            char c = (char)reader.Peek();
            while (Char.IsLetter(c))
            {
                reader.Read();
                unitString.Append(c);
                c = (char)reader.Peek();
            }

            SVGAngleType units = AngleTypeFromString(unitString.ToString());

            if (result != null)
            {
                result.NewValueSpecifiedUnits(units, value);
            }
            else
            {
                result = new MutableSVGAngle(value, units);
            }

            return result;
        }

        static SVGAngleType AngleTypeFromString(string str)
        {
            switch (str.ToLower())
            {
                case "deg":
                    return SVGAngleType.SVG_ANGLETYPE_DEG;
                case "grad":
                    return SVGAngleType.SVG_ANGLETYPE_GRAD;
                case "rad":
                    return SVGAngleType.SVG_ANGLETYPE_RAD;
                case "":
                    return SVGAngleType.SVG_ANGLETYPE_UNSPECIFIED;
                default:
                    return SVGAngleType.SVG_ANGLETYPE_UNKNOWN;
            }
        }

        #endregion

        #region color

        // currentColor | <color> <icccolor> | inherit

        public static SVGColor ParseColor(string str)
        {
            SVGColor result;

            if (TryParseColor(str, out result))
            {
                return result;
            }
            else
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }
        }

        public static bool TryParseColor(string str, out SVGColor result)
        {
            str = str.Trim();

            if (str == "inherit")
            {
                result = SVGColorImpl.Inherit;
                return true;
            }

            if (str == "currentcolor")
            {
                result = SVGPaintImpl.CurrentColor;
                return true;
            }

            int i = str.IndexOf("icc-color");
            if (i != -1)
            {
                // ignore icc-color
                str = str.Remove(i).Trim();
            }

            CSS.RGBColor rgbColor;
            if (CSS.Internal.RGBColorBase.TryParse(str, out rgbColor))
            {
                result = new SVGColorImpl(SVGColorType.SVG_COLORTYPE_RGBCOLOR, rgbColor);
                return true;
            }

            result = null;
            return false;
        }

        #endregion

        #region <paint>

        // <paint>: none |
        //          currentColor |
        //          <color> [<icccolor>] |
        //          <funciri> [ none | currentColor | <color> [<icccolor>] ] |
        //          inherit

        // color    ::= "#" hexdigit hexdigit hexdigit (hexdigit hexdigit hexdigit)?
        //          | "rgb(" wsp* integer comma integer comma integer wsp* ")"
        //          | "rgb(" wsp* integer "%" comma integer "%" comma integer "%" wsp* ")"
        //          | color-keyword
        // hexdigit ::= [0-9A-Fa-f]
        // comma    ::= wsp* "," wsp*
        // icccolor ::= "icc-color(" name (comma-wsp number)+ ")"
        // name     ::= [^,()#x20#x9#xD#xA] /* any char except ",", "(", ")" or wsp */

        public static SVGPaint ParsePaint(string str)
        {
            SVGPaint result;

            if (TryParsePaint(str, out result))
            {
                return result;
            }
            else
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }
        }

        public static bool TryParsePaint(string str, out SVGPaint result)
        {
            str = str.Trim();

            if (str == "none")
            {
                result = SVGPaintImpl.None;
                return true;
            }

            if (str == "inherit")
            {
                result = SVGPaintImpl.Inherit;
                return true;
            }

            if (str == "currentcolor")
            {
                result = SVGPaintImpl.CurrentColor;
                return true;
            }

            string uri = null;

            if (str.StartsWith("url"))
            {
                int i = str.IndexOf('(');
                if (i == -1)
                {
                    throw new DOMException(DOMException.SYNTAX_ERR);
                }

                int j = str.IndexOf(')', i + 1);
                if (j == -1)
                {
                    throw new DOMException(DOMException.SYNTAX_ERR);
                }

                uri = str.Substring(i + 1, j - i - 1).Trim();
                str = str.Remove(0, j + 1).Trim();

                SVGPaint fallback;
                if (!TryParsePaint(str, out fallback))
                {
                    result = new SVGPaintImpl(SVGPaintType.SVG_PAINTTYPE_URI, uri);
                    return true;
                }

                switch (fallback.PaintType)
                {
                    case SVGPaintType.SVG_PAINTTYPE_NONE:
                        result = new SVGPaintImpl(SVGPaintType.SVG_PAINTTYPE_URI_NONE, uri);
                        break;
                    case SVGPaintType.SVG_PAINTTYPE_CURRENTCOLOR:
                        result = new SVGPaintImpl(SVGPaintType.SVG_PAINTTYPE_URI_CURRENTCOLOR);
                        break;
                    case SVGPaintType.SVG_PAINTTYPE_RGBCOLOR:
                        result = new SVGPaintImpl(SVGPaintType.SVG_PAINTTYPE_URI_RGBCOLOR, fallback.RgbColor);
                        break;
                    case SVGPaintType.SVG_PAINTTYPE_RGBCOLOR_ICCCOLOR:
                        result = new SVGPaintImpl(SVGPaintType.SVG_PAINTTYPE_URI_RGBCOLOR_ICCCOLOR, fallback.RgbColor, fallback.IccColor);
                        break;
                    default:
                        result = new SVGPaintImpl(SVGPaintType.SVG_PAINTTYPE_URI, uri);
                        break;
                }

                return true;
            }

            int k = str.IndexOf("icc-color");
            if (k != -1)
            {
                str = str.Remove(k).Trim();
            }

            CSS.RGBColor rgbColor;
            if (CSS.Internal.RGBColorBase.TryParse(str, out rgbColor))
            {
                result = new SVGPaintImpl(SVGPaintType.SVG_PAINTTYPE_RGBCOLOR, rgbColor);
                return true;
            }

            result = null;
            return false;
        }

        #endregion

        #region transform

        // transform:
        //     matrix
        //     | translate
        //     | scale
        //     | rotate
        //     | skewX
        //     | skewY

        public static SVGTransform ParseTransform(string text)
        {
            using (var reader = new StringReader(text))
            {
                return ParseTransform(reader);
            }
        }

        public static SVGTransform ParseTransform(TextReader reader)
        {
            StringBuilder buffer = Buffer;

            if (!TryReadFuncName(reader, buffer))
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }

            string name = buffer.ToString();

            switch (name.ToLower())
            {
                case "matrix":
                    return ParseTransform_Matrix(reader);
                case "translate":
                    return ParseTransform_Translate(reader);
                case "scale":
                    return ParseTransform_Scale(reader);
                case "rotate":
                    return ParseTransform_Rotate(reader);
                case "skewx":
                    return ParseTransform_SkewX(reader);
                case "skewy":
                    return ParseTransform_SkewY(reader);
                default:
                    throw new DOMException(DOMException.SYNTAX_ERR);
            }
        }

        // matrix:
        //     "matrix" wsp* "(" wsp*
        //        number comma-wsp
        //        number comma-wsp
        //        number comma-wsp
        //        number comma-wsp
        //        number comma-wsp
        //        number wsp* ")"

        public static SVGTransform ParseTransform_Matrix(TextReader reader)
        {
            // wsp*
            SkipWhitespace(reader);

            // "("
            ReadChar(reader, '(');

            // wsp*
            SkipWhitespace(reader);

            double a = ParseNumber(ReadNumber(reader));

            // comma-wsp
            ReadCommaWsp(reader);

            double b = ParseNumber(ReadNumber(reader));

            // comma-wsp
            ReadCommaWsp(reader);

            double c = ParseNumber(ReadNumber(reader));

            // comma-wsp
            ReadCommaWsp(reader);

            double d = ParseNumber(ReadNumber(reader));

            // comma-wsp
            ReadCommaWsp(reader);

            double e = ParseNumber(ReadNumber(reader));

            // comma-wsp
            ReadCommaWsp(reader);

            double f = ParseNumber(ReadNumber(reader));

            // wsp*
            SkipWhitespace(reader);

            // ")"
            ReadChar(reader, ')');

            SVGMatrix matrix = new MutableSVGMatrix(a, b, c, d, e, f);
            return new MutableSVGTransform(matrix);
        }

        // translate:
        //     "translate" wsp* "(" wsp* number ( comma-wsp number )? wsp* ")"

        public static SVGTransform ParseTransform_Translate(TextReader reader)
        {
            // wsp*
            SkipWhitespace(reader);

            // "("
            ReadChar(reader, '(');

            // wsp*
            SkipWhitespace(reader);

            // number
            double tx = ParseNumber(ReadNumber(reader));
            double ty = tx;

            // ( comma-wsp number )?

            SkipWhitespace(reader);
            if (reader.Peek() != ')')
            {
                if (reader.Peek() == ',')
                {
                    reader.Read();
                    SkipWhitespace(reader);
                }
                ty = ParseNumber(ReadNumber(reader));
            }

            // wsp*
            SkipWhitespace(reader);

            // ")"
            ReadChar(reader, ')');

            var result = new MutableSVGTransform();
            result.SetTranslate(tx, ty);
            return result;
        }

        // scale:
        //     "scale" wsp* "(" wsp* number ( comma-wsp number )? wsp* ")"

        public static SVGTransform ParseTransform_Scale(TextReader reader)
        {
            // wsp*
            SkipWhitespace(reader);

            // "("
            ReadChar(reader, '(');

            // wsp*
            SkipWhitespace(reader);

            // number
            double sx = ParseNumber(ReadNumber(reader));
            double sy = sx;

            // ( comma-wsp number )?

            SkipWhitespace(reader);
            if (reader.Peek() != ')')
            {
                if (reader.Peek() == ',')
                {
                    reader.Read();
                    SkipWhitespace(reader);
                }
                sy = ParseNumber(ReadNumber(reader));
            }

            // wsp*
            SkipWhitespace(reader);

            // ")"
            ReadChar(reader, ')');

            var result = new MutableSVGTransform();
            result.SetScale(sx, sy);
            return result;
        }

        // rotate:
        //     "rotate" wsp* "(" wsp* number ( comma-wsp number comma-wsp number )? wsp* ")"

        public static SVGTransform ParseTransform_Rotate(TextReader reader)
        {
            // wsp*
            SkipWhitespace(reader);

            // "("
            ReadChar(reader, '(');

            // wsp*
            SkipWhitespace(reader);

            // number
            double angle = ParseNumber(ReadNumber(reader));

            // ( comma-wsp number comma-wsp number )?

            double cx = 0, cy = 0;

            SkipWhitespace(reader);
            if (reader.Peek() != ')')
            {
                if (reader.Peek() == ',')
                {
                    reader.Read();
                    SkipWhitespace(reader);
                }
                cx = ParseNumber(ReadNumber(reader));
                ReadCommaWsp(reader);
                cy = ParseNumber(ReadNumber(reader));
            }

            // wsp*
            SkipWhitespace(reader);

            // ")"
            ReadChar(reader, ')');

            var result = new MutableSVGTransform();
            result.SetRotate(angle, cx, cy);
            return result;
        }

        // skewX:
        //     "skewX" wsp* "(" wsp* number wsp* ")"

        public static SVGTransform ParseTransform_SkewX(TextReader reader)
        {
            // wsp*
            SkipWhitespace(reader);

            // "("
            ReadChar(reader, '(');

            // wsp*
            SkipWhitespace(reader);

            // number
            double angle = ParseNumber(ReadNumber(reader));

            // wsp*
            SkipWhitespace(reader);

            // ")"
            ReadChar(reader, ')');

            var result = new MutableSVGTransform();
            result.SetSkewX(angle);
            return result;
        }

        // skewY:
        //     "skewY" wsp* "(" wsp* number wsp* ")"

        public static SVGTransform ParseTransform_SkewY(TextReader reader)
        {
            // wsp*
            SkipWhitespace(reader);

            // "("
            ReadChar(reader, '(');

            // wsp*
            SkipWhitespace(reader);

            // number
            double angle = ParseNumber(ReadNumber(reader));

            // wsp*
            SkipWhitespace(reader);

            // ")"
            ReadChar(reader, ')');

            var result = new MutableSVGTransform();
            result.SetSkewY(angle);
            return result;
        }

        #endregion

        #region transform-list

        // transform-list:
        //    wsp* transforms? wsp*
        // transforms:
        //    transform
        //    | transform comma-wsp+ transforms

        public static SVGTransformList ParseTransformList(string text)
        {
            using (var reader = new StringReader(text))
            {
                return ParseTransformList(reader);
            }
        }

        public static SVGTransformList ParseTransformList(TextReader reader)
        {
            var results = new MutableSVGTransformList();

            char c = (char)reader.Peek();

            while (Char.IsLetter(c) || IsWhitespace(c) || c == ',')
            {
                if (IsWhitespace(c) || c == ',')
                {
                    reader.Read();
                    c = (char)reader.Peek();
                    continue;
                }

                // transform
                SVGTransform transform = ParseTransform(reader);
                results.AppendItem(transform);

                c = (char)reader.Peek();
            }

            return results;
        }

        #endregion

        #endregion
    }
}
