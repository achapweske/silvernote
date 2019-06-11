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
    public class MutableSVGNumber : SVGNumber
    {
        #region Constructors

        public MutableSVGNumber(double value = 0)
        {
            Value = value;
        }

        #endregion

        #region Properties

        public virtual double Value { get; set; }

        #endregion
    }
}
