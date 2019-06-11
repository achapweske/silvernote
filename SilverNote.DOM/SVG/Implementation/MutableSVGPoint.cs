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
    public class MutableSVGPoint : SVGPoint
    {
        #region Constructors

        public MutableSVGPoint()
            : this(0, 0)
        {

        }

        public MutableSVGPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        #endregion

        #region Properties

        public virtual double X { get; set; }
        public virtual double Y { get; set; }

        #endregion

        #region Operations

        public SVGPoint MatrixTransform(SVGMatrix matrix)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
