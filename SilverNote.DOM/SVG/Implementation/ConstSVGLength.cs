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
    public class ConstSVGLength : MutableSVGLength
    {
        #region Constructors

        public ConstSVGLength(double value = 0, SVGLengthType unitType = SVGLengthType.SVG_LENGTHTYPE_NUMBER)
            : base(value, unitType)
        {

        }

        #endregion

        #region Properties

        public override double Value 
        {
            get { return base.Value; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        public override double ValueInSpecifiedUnits 
        {
            get { return base.ValueInSpecifiedUnits; }
            set { throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR); }
        }

        #endregion

        #region Operations

        public override void NewValueSpecifiedUnits(SVGLengthType unitType, double valueInSpecifiedUnits)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        public override void ConvertToSpecifiedUnits(SVGLengthType unitType)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        #endregion
    }
}
