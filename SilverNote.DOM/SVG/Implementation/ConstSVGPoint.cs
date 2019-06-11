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
    public class ConstSVGPoint : MutableSVGPoint
    {
        #region Constructors

        public ConstSVGPoint()
            : this(0, 0)
        {

        }

        public ConstSVGPoint(double x, double y)
            : base(x, y)
        {

        }

        #endregion

        #region Properties

        public override double X
        {
            get { return base.X; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        public override double Y
        {
            get { return base.Y; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        #endregion
    }
}
