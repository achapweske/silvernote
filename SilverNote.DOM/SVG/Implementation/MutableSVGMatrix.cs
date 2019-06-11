/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.SVG.Internal
{
    public class MutableSVGMatrix : SVGMatrix
    {
        #region Constructors

        public MutableSVGMatrix()
            : this(1, 0, 0, 1, 0, 0)
        {
            
        }

        public MutableSVGMatrix(double a, double b, double c, double d, double e, double f)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
            F = f;
        }

        #endregion

        #region Properties

        public virtual double A { get; set; }

        public virtual double B { get; set; }

        public virtual double C { get; set; }

        public virtual double D { get; set; }

        public virtual double E { get; set; }

        public virtual double F { get; set; }

        #endregion

        #region Operations

        public SVGMatrix Multiply(SVGMatrix secondMatrix)
        {
            return Multiply(secondMatrix.A, secondMatrix.B, secondMatrix.C, secondMatrix.D, secondMatrix.E, secondMatrix.F);
        }

        public SVGMatrix Inverse()
        {
            double determinant = Determinant();
            if (determinant == 0.0)
            {
                throw new SVGException(SVGException.SVG_MATRIX_NOT_INVERTABLE);
            }

            if (IsIdentity())
            {
                return new MutableSVGMatrix();
            }

            double a = D / determinant;
            double b = -B / determinant;
            double c = -C / determinant;
            double d = A / determinant;
            double e = (C * F - D * E) / determinant;
            double f = (B * E - A * F) / determinant;

            return new MutableSVGMatrix(a, b, c, d, e, f);
        }

        public SVGMatrix Translate(double x, double y)
        {
            double e = x * A + y * C + E;
            double f = x * B + y * D + F;
            
            return new MutableSVGMatrix(A, B, C, D, e, f);
        }

        public SVGMatrix Scale(double scaleFactor)
        {
            return ScaleNonUniform(scaleFactor, scaleFactor);
        }

        public SVGMatrix ScaleNonUniform(double scaleFactorX, double scaleFactorY)
        {
            double a = A * scaleFactorX;
            double b = B * scaleFactorX;
            double c = C * scaleFactorY;
            double d = D * scaleFactorY;
            
            return new MutableSVGMatrix(a, b, c, d, E, F);
        }

        public SVGMatrix Rotate(double angle)
        {
            double angleInRadians = DegToRad(angle);
            double cos = Math.Cos(angleInRadians);
            double sin = Math.Sin(angleInRadians);

            return Multiply(cos, sin, -sin, cos, 0, 0); 
        }

        public SVGMatrix RotateFromVector(double x, double y)
        {
            double angleInRadians = Math.Atan2(y, x);
            double cos = Math.Cos(angleInRadians);
            double sin = Math.Sin(angleInRadians);

            return Multiply(cos, sin, -sin, cos, 0, 0); 
        }

        public SVGMatrix FlipX()
        {
            return ScaleNonUniform(-1, 1);
        }

        public SVGMatrix FlipY()
        {
            return ScaleNonUniform(1, -1);
        }

        public SVGMatrix SkewX(double angle)
        {
            double angleInRadians = DegToRad(angle);
            double tangent = Math.Tan(angleInRadians);
            return Shear(tangent, 0);
        }

        public SVGMatrix SkewY(double angle)
        {
            double angleInRadians = DegToRad(angle);
            double tangent = Math.Tan(angleInRadians);
            return Shear(0, tangent);
        }

        #endregion

        #region Implementation

        private static double DegToRad(double degrees)
        {
            return (degrees * Math.PI) / 180.0;
        }

        public bool IsIdentity()
        {
            return (A == 1 && B == 0 && C == 0 && D == 1 && E == 0 && F == 0);
        }

        public double Determinant()
        {
            return A * D - B * C;
        }

        public SVGMatrix Multiply(double a, double b, double c, double d, double e, double f)
        {
            double newA = a * this.A + b * this.C;
            double newB = a * this.B + b * this.D;
            double newC = c * this.A + d * this.C;
            double newD = c * this.B + d * this.D;
            double newE = e * this.A + f * this.C + this.E;
            double newF = e * this.B + f * this.D + this.F;

            return new MutableSVGMatrix(newA, newB, newC, newD, newE, newF);
        }

        public SVGMatrix Shear(double x, double y)
        {
            double a = A + y * C;
            double b = B + y * D;
            double c = C + x * A;
            double d = D + x * B;

            return new MutableSVGMatrix(a, b, c, d, E, F);
        }

        #endregion
    }
}
