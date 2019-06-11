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
    public interface SVGMatrix
    {
        double A { get; set; }
        double B { get; set; }
        double C { get; set; }
        double D { get; set; }
        double E { get; set; }
        double F { get; set; }

        SVGMatrix Multiply(SVGMatrix secondMatrix);
        SVGMatrix Inverse();
        SVGMatrix Translate(double x, double y);
        SVGMatrix Scale(double scaleFactor);
        SVGMatrix ScaleNonUniform(double scaleFactorX, double scaleFactorY);
        SVGMatrix Rotate(double angle);
        SVGMatrix RotateFromVector(double x, double y);
        SVGMatrix FlipX();
        SVGMatrix FlipY();
        SVGMatrix SkewX(double angle);
        SVGMatrix SkewY(double angle);
    }
}
