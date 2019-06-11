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
    public interface SVGTransform
    {
        SVGTransformType Type { get; }
        SVGMatrix Matrix { get; }
        double Angle { get; }

        void SetMatrix(SVGMatrix matrix);
        void SetTranslate(double tx, double ty);
        void SetScale(double sx, double sy);
        void SetRotate(double angle, double cx, double cy);
        void SetSkewX(double angle);
        void SetSkewY(double angle);
    }
}
