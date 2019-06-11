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
    public class ConstSVGNumber : MutableSVGNumber
    {
        #region Constructors

        public ConstSVGNumber(double value = 0)
            : base(value)
        {

        }

        #endregion

        #region Properties

        public override double Value
        {
            get { return base.Value; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        #endregion
    }
}
