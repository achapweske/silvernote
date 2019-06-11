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
    public class ConstSVGRect : MutableSVGRect
    {
        #region Constructors

        public ConstSVGRect()
            : this(0, 0, 0, 0)
        {

        }

        public ConstSVGRect(double x, double y, double width, double height)
            : base(x, y, width, height)
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

        public override double Width
        {
            get { return base.Width; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        public override double Height
        {
            get { return base.Height; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        #endregion
    }
}
