/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DOM.SVG
{
    public static class SVGFormatter
    {
        #region <length>

        // length ::= number ("em" | "ex" | "px" | "in" | "cm" | "mm" | "pt" | "pc" | "%")?

        public static string FormatLength(SVGLength value)
        {
            return FormatLength(value.ValueInSpecifiedUnits, value.UnitType);
        }

        public static void FormatLength(TextWriter writer, SVGLength value)
        {
            FormatLength(writer, value.ValueInSpecifiedUnits, value.UnitType);
        }

        public static string FormatLength(double value, SVGLengthType unitType = SVGLengthType.SVG_LENGTHTYPE_NUMBER)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatLength(writer, value, unitType);
                return writer.ToString();
            }
        }

        public static void FormatLength(TextWriter writer, double value, SVGLengthType unitType = SVGLengthType.SVG_LENGTHTYPE_NUMBER)
        {
            switch (unitType)
            {
                case SVGLengthType.SVG_LENGTHTYPE_UNKNOWN:
                    break;
                case SVGLengthType.SVG_LENGTHTYPE_NUMBER:
                    writer.Write(value);
                    break;
                case SVGLengthType.SVG_LENGTHTYPE_PERCENTAGE:
                    writer.Write(value);
                    writer.Write("%");
                    break;
                case SVGLengthType.SVG_LENGTHTYPE_EMS:
                    writer.Write(value);
                    writer.Write("em");
                    break;
                case SVGLengthType.SVG_LENGTHTYPE_EXS:
                    writer.Write(value);
                    writer.Write("ex");
                    break;
                case SVGLengthType.SVG_LENGTHTYPE_PX:
                    writer.Write(value);
                    writer.Write("px");
                    break;
                case SVGLengthType.SVG_LENGTHTYPE_CM :
                    writer.Write(value);
                    writer.Write("cm");
                    break;
                case SVGLengthType.SVG_LENGTHTYPE_MM:
                    writer.Write(value);
                    writer.Write("mm");
                    break;
                case SVGLengthType.SVG_LENGTHTYPE_IN:
                    writer.Write(value);
                    writer.Write("in");
                    break;
                case SVGLengthType.SVG_LENGTHTYPE_PT:
                    writer.Write(value);
                    writer.Write("pt");
                    break;
                case SVGLengthType.SVG_LENGTHTYPE_PC:
                    writer.Write(value);
                    writer.Write("pc");
                    break;
            }
        }

        #endregion

        #region <angle>

        // angle ::= number ("deg" | "grad" | "rad")?

        public static string FormatAngle(SVGAngle value)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatAngle(writer, value);
                return writer.ToString();
            }
        }

        public static void FormatAngle(TextWriter writer, SVGAngle value)
        {
            switch (value.UnitType)
            {
                case SVGAngleType.SVG_ANGLETYPE_DEG:
                    writer.Write(value.ValueInSpecifiedUnits);
                    writer.Write("deg");
                    break;
                case SVGAngleType.SVG_ANGLETYPE_RAD:
                    writer.Write(value.ValueInSpecifiedUnits);
                    writer.Write("rad");
                    break;
                case SVGAngleType.SVG_ANGLETYPE_GRAD:
                    writer.Write(value.ValueInSpecifiedUnits);
                    writer.Write("grad");
                    break;
                case SVGAngleType.SVG_ANGLETYPE_UNSPECIFIED:
                    writer.Write(value.ValueInSpecifiedUnits);
                    break;
                case SVGAngleType.SVG_ANGLETYPE_UNKNOWN:
                default:
                    break;
            }
        }

        #endregion

        #region color

        // currentColor | <color> <icccolor> | inherit

        public static string FormatColor(SVGColor value, bool useKeywords = true)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatColor(writer, value, useKeywords);
                return writer.ToString();
            }
        }

        public static void FormatColor(TextWriter writer, SVGColor value, bool useKeywords = true)
        {
            if (value.CssValueType == CSS.CSSValueType.CSS_INHERIT)
            {
                writer.Write("inherit");
                return;
            }

            switch (value.ColorType)
            {
                case SVGColorType.SVG_COLORTYPE_CURRENTCOLOR:
                    writer.Write("currentColor");
                    return;
                case SVGColorType.SVG_COLORTYPE_RGBCOLOR:
                    CSS.CSSFormatter.FormatColor(writer, value.RgbColor, useKeywords);
                    return;
                case SVGColorType.SVG_COLORTYPE_RGBCOLOR_ICCCOLOR:
                    CSS.CSSFormatter.FormatColor(writer, value.RgbColor, useKeywords);
                    // TODO
                    return;
                case SVGColorType.SVG_COLORTYPE_UNKNOWN:
                default:
                    return;
            }
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

        public static string FormatPaint(SVGPaint value, bool useKeywords = true)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatPaint(writer, value, useKeywords);
                return writer.ToString();
            }
        }

        public static void FormatPaint(TextWriter writer, SVGPaint value, bool useKeywords = true)
        {
            if (value.CssValueType == CSS.CSSValueType.CSS_INHERIT)
            {
                writer.Write("inherit");
                return;
            }

            switch (value.PaintType)
            {
                case SVGPaintType.SVG_PAINTTYPE_URI:
                    writer.Write("url(" + value.Uri + ")");
                    break;
                case SVGPaintType.SVG_PAINTTYPE_URI_NONE:
                case SVGPaintType.SVG_PAINTTYPE_URI_CURRENTCOLOR:
                case SVGPaintType.SVG_PAINTTYPE_URI_RGBCOLOR:
                case SVGPaintType.SVG_PAINTTYPE_URI_RGBCOLOR_ICCCOLOR:
                    writer.Write("url(" + value.Uri + ")");
                    writer.Write(' ');
                    break;
            }

            switch (value.PaintType)
            {
                case SVGPaintType.SVG_PAINTTYPE_NONE:
                case SVGPaintType.SVG_PAINTTYPE_URI_NONE:
                    writer.Write("none");
                    return;
                case SVGPaintType.SVG_PAINTTYPE_CURRENTCOLOR:
                case SVGPaintType.SVG_PAINTTYPE_URI_CURRENTCOLOR:
                    writer.Write("currentColor");
                    return;
                case SVGPaintType.SVG_PAINTTYPE_RGBCOLOR:
                case SVGPaintType.SVG_PAINTTYPE_URI_RGBCOLOR:
                    CSS.CSSFormatter.FormatColor(writer, value.RgbColor, useKeywords);
                    return;
                case SVGPaintType.SVG_PAINTTYPE_RGBCOLOR_ICCCOLOR:
                case SVGPaintType.SVG_PAINTTYPE_URI_RGBCOLOR_ICCCOLOR:
                    CSS.CSSFormatter.FormatColor(writer, value.RgbColor, useKeywords);
                    // TODO
                    return;
                default:
                    return;
            }
        }

        #endregion

        #region transform

        public static string FormatTransform(SVGTransform value)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatTransform(writer, value);
                return writer.ToString();
            }
        }

        // transform:
        //     matrix
        //     | translate
        //     | scale
        //     | rotate
        //     | skewX
        //     | skewY

        public static void FormatTransform(TextWriter writer, SVGTransform value)
        {
            switch (value.Type)
            {
                case SVGTransformType.SVG_TRANSFORM_MATRIX:
                    FormatTransform_Matrix(writer, value);
                    break;
                case SVGTransformType.SVG_TRANSFORM_TRANSLATE:
                    FormatTransform_Translate(writer, value);
                    break;
                case SVGTransformType.SVG_TRANSFORM_SCALE:
                    FormatTransform_Scale(writer, value);
                    break;
                case SVGTransformType.SVG_TRANSFORM_ROTATE:
                    FormatTransform_Rotate(writer, value);
                    break;
                case SVGTransformType.SVG_TRANSFORM_SKEWX:
                    FormatTransform_SkewX(writer, value);
                    break;
                case SVGTransformType.SVG_TRANSFORM_SKEWY:
                    FormatTransform_SkewY(writer, value);
                    break;
                default:
                    break;
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

        public static void FormatTransform_Matrix(TextWriter writer, SVGTransform value)
        {
            writer.Write("matrix(");
            writer.Write(value.Matrix.A);
            writer.Write(',');
            writer.Write(value.Matrix.B);
            writer.Write(',');
            writer.Write(value.Matrix.C);
            writer.Write(',');
            writer.Write(value.Matrix.D);
            writer.Write(',');
            writer.Write(value.Matrix.E);
            writer.Write(',');
            writer.Write(value.Matrix.F);
            writer.Write(')');
        }

        // translate:
        //     "translate" wsp* "(" wsp* number ( comma-wsp number )? wsp* ")"

        public static void FormatTransform_Translate(TextWriter writer, SVGTransform value)
        {
            writer.Write("translate(");
            writer.Write(value.Matrix.E);
            if (value.Matrix.F != value.Matrix.E)
            {
                writer.Write(',');
                writer.Write(value.Matrix.F);
            }
            writer.Write(')');
        }

        // scale:
        //     "scale" wsp* "(" wsp* number ( comma-wsp number )? wsp* ")"

        public static void FormatTransform_Scale(TextWriter writer, SVGTransform value)
        {
            writer.Write("scale(");
            writer.Write(value.Matrix.A);
            if (value.Matrix.D != value.Matrix.A)
            {
                writer.Write(',');
                writer.Write(value.Matrix.D);
            }
            writer.Write(')');
        }

        // rotate:
        //     "rotate" wsp* "(" wsp* number ( comma-wsp number comma-wsp number )? wsp* ")"

        public static void FormatTransform_Rotate(TextWriter writer, SVGTransform value)
        {
            writer.Write("rotate(");
            writer.Write(value.Angle);
            if (value.Matrix.E != 0 || value.Matrix.F != 0)
            {
                writer.Write(',');
                writer.Write(value.Matrix.E);
                writer.Write(',');
                writer.Write(value.Matrix.F);
            }
            writer.Write(')');
        }

        // skewX:
        //     "skewX" wsp* "(" wsp* number wsp* ")"

        public static void FormatTransform_SkewX(TextWriter writer, SVGTransform value)
        {
            writer.Write("skewX(");
            writer.Write(value.Angle);
            writer.Write(')');
        }

        // skewY:
        //     "skewY" wsp* "(" wsp* number wsp* ")"

        public static void FormatTransform_SkewY(TextWriter writer, SVGTransform value)
        {
            writer.Write("skewY(");
            writer.Write(value.Angle);
            writer.Write(')');
        }

        #endregion

        #region transform-list

        public static string FormatTransformList(SVGTransformList value)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatTransformList(writer, value);
                return writer.ToString();
            }
        }

        // transform-list:
        //    wsp* transforms? wsp*
        // transforms:
        //    transform
        //    | transform comma-wsp+ transforms

        public static void FormatTransformList(TextWriter writer, SVGTransformList value)
        {
            for (int i = 0; i < value.NumberOfItems; i++)
            {
                if (i > 0)
                {
                    writer.Write(' ');
                }
                FormatTransform(writer, value.GetItem(i));
            }
        }

        #endregion
    }
}
