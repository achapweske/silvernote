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
    public interface SVGPathElement : SVGElement, SVGTests, SVGLangSpace, SVGExternalResourcesRequired, SVGStylable, SVGTransformable, SVGAnimatedPathData 
    {
        SVGAnimatedNumber PathLength { get; }

        double GetTotalLength();
        SVGPoint GetPointAtLength(double distance);
        int GetPathSegAtLength(double distance);
        SVGPathSegClosePath CreateSVGPathSegClosePath();
        SVGPathSegMovetoAbs CreateSVGPathSegMovetoAbs(double x, double y);
        SVGPathSegMovetoRel CreateSVGPathSegMovetoRel(double x, double y);
        SVGPathSegLinetoAbs CreateSVGPathSegLinetoAbs(double x, double y);
        SVGPathSegLinetoRel CreateSVGPathSegLinetoRel(double x, double y);
        SVGPathSegCurvetoCubicAbs CreateSVGPathSegCurvetoCubicAbs(double x, double y, double x1, double y1, double x2, double y2);
        SVGPathSegCurvetoCubicRel CreateSVGPathSegCurvetoCubicRel(double x, double y, double x1, double y1, double x2, double y2);
        SVGPathSegCurvetoQuadraticAbs CreateSVGPathSegCurvetoQuadraticAbs(double x, double y, double x1, double y1);
        SVGPathSegCurvetoQuadraticRel CreateSVGPathSegCurvetoQuadraticRel(double x, double y, double x1, double y1);
        SVGPathSegArcAbs CreateSVGPathSegArcAbs(double x, double y, double r1, double r2, double angle, bool largeArcFlag, bool sweepFlag);
        SVGPathSegArcRel CreateSVGPathSegArcRel(double x, double y, double r1, double r2, double angle, bool largeArcFlag, bool sweepFlag);
        SVGPathSegLinetoHorizontalAbs CreateSVGPathSegLinetoHorizontalAbs(double x);
        SVGPathSegLinetoHorizontalRel CreateSVGPathSegLinetoHorizontalRel(double x);
        SVGPathSegLinetoVerticalAbs CreateSVGPathSegLinetoVerticalAbs(double y);
        SVGPathSegLinetoVerticalRel CreateSVGPathSegLinetoVerticalRel(double y);
        SVGPathSegCurvetoCubicSmoothAbs CreateSVGPathSegCurvetoCubicSmoothAbs(double x, double y, double x2, double y2);
        SVGPathSegCurvetoCubicSmoothRel CreateSVGPathSegCurvetoCubicSmoothRel(double x, double y, double x2, double y2);
        SVGPathSegCurvetoQuadraticSmoothAbs CreateSVGPathSegCurvetoQuadraticSmoothAbs(double x, double y);
        SVGPathSegCurvetoQuadraticSmoothRel CreateSVGPathSegCurvetoQuadraticSmoothRel(double x, double y);
    }
}
