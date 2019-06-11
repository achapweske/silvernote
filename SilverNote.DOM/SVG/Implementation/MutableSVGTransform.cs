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
    public class MutableSVGTransform : SVGTransform
    {
        #region Fields

        SVGTransformType _Type;
        MutableSVGMatrix _Matrix;
        double _Angle;

        #endregion

        #region Constructors

        public MutableSVGTransform()
        {
            _Type = SVGTransformType.SVG_TRANSFORM_UNKNOWN;
            _Matrix = new MutableSVGMatrix();
            _Angle = 0;
        }

        public MutableSVGTransform(SVGMatrix matrix)
            : this()
        {
            SetMatrix(matrix);
        }

        #endregion

        #region Properties

        public SVGTransformType Type
        {
            get { return _Type; }
        }

        public SVGMatrix Matrix
        {
            get { return _Matrix; }
        }

        public double Angle
        {
            get { return _Angle; }
        }

        #endregion

        #region Operations

        public void SetMatrix(SVGMatrix matrix)
        {
            _Type = SVGTransformType.SVG_TRANSFORM_MATRIX;
            _Angle = 0;

            _Matrix.A = matrix.A;
            _Matrix.B = matrix.B;
            _Matrix.C = matrix.C;
            _Matrix.D = matrix.D;
            _Matrix.E = matrix.E;
            _Matrix.F = matrix.F;
        }

        public void SetTranslate(double tx, double ty)
        {
            _Type = SVGTransformType.SVG_TRANSFORM_TRANSLATE;
            _Angle = 0;

            _Matrix.A = 1;
            _Matrix.B = 0;
            _Matrix.C = 0;
            _Matrix.D = 1;
            _Matrix.E = tx;
            _Matrix.F = ty;
        }

        public void SetScale(double sx, double sy)
        {
            _Type = SVGTransformType.SVG_TRANSFORM_SCALE;
            _Angle = 0;

            _Matrix.A = sx;
            _Matrix.B = 0;
            _Matrix.C = 0;
            _Matrix.D = sy;
            _Matrix.E = 0;
            _Matrix.F = 0;
        }

        public void SetRotate(double angle, double cx, double cy)
        {
            _Type = SVGTransformType.SVG_TRANSFORM_ROTATE;
            _Angle = angle;

            var matrix = new MutableSVGMatrix();
            matrix.Translate(cx, cy);
            matrix.Rotate(angle);
            matrix.Translate(-cx, -cy);

            _Matrix.A = matrix.A;
            _Matrix.B = matrix.B;
            _Matrix.C = matrix.C;
            _Matrix.D = matrix.D;
            _Matrix.E = matrix.E;
            _Matrix.F = matrix.F;
        }

        public void SetSkewX(double angle)
        {
            _Type = SVGTransformType.SVG_TRANSFORM_SKEWX;
            _Angle = angle;

            double angleInRadians = DegToRad(angle);
            double tangent = Math.Tan(angleInRadians);

            _Matrix.A = 1;
            _Matrix.B = 0;
            _Matrix.C = Math.Tan(angleInRadians);
            _Matrix.D = 1;
            _Matrix.E = 0;
            _Matrix.F = 0;
        }

        public void SetSkewY(double angle)
        {
            _Type = SVGTransformType.SVG_TRANSFORM_SKEWY;
            _Angle = angle;

            double angleInRadians = DegToRad(angle);
            double tangent = Math.Tan(angleInRadians);

            _Matrix.A = 1;
            _Matrix.B = Math.Tan(angleInRadians);
            _Matrix.C = 0;
            _Matrix.D = 1;
            _Matrix.E = 0;
            _Matrix.F = 0;
        }

        #endregion

        #region Implementation

        private static double DegToRad(double degrees)
        {
            return (degrees * Math.PI) / 180.0;

        }
        #endregion

        #region Object

        public override string ToString()
        {
            return SVGFormatter.FormatTransform(this);
        }

        #endregion
    }
}
