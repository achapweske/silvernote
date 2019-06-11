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
using System.Windows.Media.Effects;
using SilverNote;
using DOM.CSS;
using DOM.SVG.Internal;

namespace DOM.SVG
{
    public static class SVGConverter
    {
        #region Color

        public static SVGColor ToSVGColor(RGBColor rgbColor)
        {
            return new SVGColorImpl(SVGColorType.SVG_COLORTYPE_RGBCOLOR, rgbColor);
        }

        public static SVGColor ToSVGColor(Color color)
        {
            RGBColor rgbColor = CSSConverter.ToRGBColor(color);
            return ToSVGColor(rgbColor);
        }

        public static SVGPaint ToSVGPaint(RGBColor rgbColor)
        {
            return new SVGPaintImpl(SVGPaintType.SVG_PAINTTYPE_RGBCOLOR, rgbColor);
        }

        public static SVGPaint ToSVGPaint(Color color)
        {
            RGBColor rgbColor = CSSConverter.ToRGBColor(color);
            return ToSVGPaint(rgbColor);
        }

        #endregion

        #region Brush

        public static SVGPaint ToSVGPaint(Brush brush)
        {
            var solidBrush = brush as SolidColorBrush;

            if (solidBrush != null)
            {
                return ToSVGPaint(solidBrush.Color);
            }
            else
            {
                return SVGPaintImpl.None;
            }
        }

        #endregion

        #region LinearGradientBrush

        public static LinearGradientBrush ToLinearGradientBrush(SVGLinearGradientElement element)
        {
            var brush = new LinearGradientBrush();

            brush.StartPoint = new Point(element.X1.BaseVal.Value, element.Y1.BaseVal.Value);
            brush.EndPoint = new Point(element.X2.BaseVal.Value, element.Y2.BaseVal.Value);

            NodeList stopElements = element.GetElementsByTagNameNS(SVGElements.NAMESPACE, SVGElements.STOP);
            foreach (SVGStopElement stopElement in stopElements)
            {
                GradientStop gradientStop = ToGradientStop(stopElement);
                brush.GradientStops.Add(gradientStop);
            }

            brush.Freeze();
            return brush;
        }

        public static SVGLinearGradientElement ToLinearGradientElement(LinearGradientBrush brush, SVGDocument ownerDocument)
        {
            var element = (SVGLinearGradientElement)ownerDocument.CreateElementNS(SVGElements.NAMESPACE, SVGElements.LINEAR_GRADIENT);

            element.X1.BaseVal.Value = brush.StartPoint.X;
            element.Y1.BaseVal.Value = brush.StartPoint.Y;
            element.X2.BaseVal.Value = brush.EndPoint.X;
            element.Y2.BaseVal.Value = brush.EndPoint.Y;

            foreach (GradientStop gradientStop in brush.GradientStops)
            {
                SVGStopElement stopElement = ToStopElement(gradientStop, ownerDocument);
                element.AppendChild(stopElement);
            }

            return element;
        }

        #endregion

        #region GradientStop

        public static GradientStop ToGradientStop(SVGStopElement element)
        {
            GradientStop result = new GradientStop();

            result.Offset = element.Offset.BaseVal;

            SVGColor stopColor = element.GetStopColor();
            if (stopColor.ColorType == SVGColorType.SVG_COLORTYPE_RGBCOLOR)
            {
                result.Color = CSSConverter.ToColor(stopColor.RgbColor);
            }
            else
            {
                result.Color = Colors.Black;
            }

            result.Freeze();
            return result;
        }

        public static SVGStopElement ToStopElement(GradientStop gradientStop, SVGDocument ownerDocument)
        {
            var element = (SVGStopElement)ownerDocument.CreateElementNS(SVGElements.NAMESPACE, SVGElements.STOP);

            element.Offset.BaseVal = gradientStop.Offset;
            var stopColor = ToSVGColor(gradientStop.Color);
            element.SetStopColor(stopColor);

            return element;
        }

        #endregion

        #region Effect

        public static Effect ToEffect(SVGFilterElement element)
        {
            string[] classes = element.ClassName.BaseVal.Split();

            if (classes.Contains("dropShadow"))
            {
                return new DropShadowEffect() { ShadowDepth = 3, Opacity = 0.5 };
            }
            else
            {
                return null;
            }
        }

        public static SVGFilterElement ToFilterElement(Effect effect, SVGDocument ownerDocument)
        {
            if (effect is DropShadowEffect)
            {
                DropShadowEffect dropShadow = (DropShadowEffect)effect;

                var filter = (SVGFilterElement)ownerDocument.CreateElementNS(SVGElements.NAMESPACE, SVGElements.FILTER);
                filter.SetAttribute(SVGAttributes.CLASS, "dropShadow");
                {
                    var feGaussianBlur = ownerDocument.CreateElementNS(SVGElements.NAMESPACE, SVGElements.FE_GAUSSIAN_BLUR);
                    feGaussianBlur.SetAttribute(SVGAttributes.IN, "SourceAlpha");
                    feGaussianBlur.SetAttribute(SVGAttributes.STD_DEVIATION, "1");
                    feGaussianBlur.SetAttribute(SVGAttributes.RESULT, "blurResult");
                    filter.AppendChild(feGaussianBlur);

                    var feOffset = ownerDocument.CreateElementNS(SVGElements.NAMESPACE, SVGElements.FE_OFFSET);
                    feOffset.SetAttribute(SVGAttributes.IN, "blurResult");
                    feOffset.SetAttribute(SVGAttributes.DX, "2");
                    feOffset.SetAttribute(SVGAttributes.DY, "2");
                    feOffset.SetAttribute(SVGAttributes.RESULT, "offsetResult");
                    filter.AppendChild(feOffset);

                    var feMerge = ownerDocument.CreateElementNS(SVGElements.NAMESPACE, SVGElements.FE_MERGE);
                    {
                        var feMergeNode1 = ownerDocument.CreateElementNS(SVGElements.NAMESPACE, SVGElements.FE_MERGE_NODE);
                        feMergeNode1.SetAttribute(SVGAttributes.IN, "offsetResult");
                        feMerge.AppendChild(feMergeNode1);

                        var feMergeNode2 = ownerDocument.CreateElementNS(SVGElements.NAMESPACE, SVGElements.FE_MERGE_NODE);
                        feMergeNode2.SetAttribute(SVGAttributes.IN, "SourceGraphic");
                        feMerge.AppendChild(feMergeNode2);
                    }
                    filter.AppendChild(feMerge);
                }

                return filter;
            }

            return null;
        }

        #endregion

        #region Matrix

        public static Matrix ToMatrix(SVGMatrix svgMatrix)
        {
            return new Matrix(svgMatrix.A, svgMatrix.B, svgMatrix.C, svgMatrix.D, svgMatrix.E, svgMatrix.F);
        }

        #endregion

        #region Transform

        public static Transform ToTransform(SVGTransform svgTransform)
        {
            SVGMatrix svgMatrix = svgTransform.Matrix;
            return new MatrixTransform(svgMatrix.A, svgMatrix.B, svgMatrix.C, svgMatrix.D, svgMatrix.E, svgMatrix.F);
        }

        #endregion
    }
}
