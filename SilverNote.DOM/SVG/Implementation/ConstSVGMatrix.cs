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
    public class ConstSVGMatrix : MutableSVGMatrix
    {
        #region Constructors

        public ConstSVGMatrix()
            : this(0, 0, 0, 0, 0, 0)
        {

        }

        public ConstSVGMatrix(double a, double b, double c, double d, double e, double f)
            : base(a, b, c, d, e, f)
        {

        }

        #endregion

        #region Properties

        public override double A
        {
            get { return base.A; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        public override double B
        {
            get { return base.B; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        public override double C
        {
            get { return base.C; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        public override double D
        {
            get { return base.D; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        public override double E
        {
            get { return base.E; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        public override double F
        {
            get { return base.F; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        #endregion
    }
}
