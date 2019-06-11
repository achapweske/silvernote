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
    public class SVGAnimatedLengthListImpl : SVGAnimatedLengthList
    {
        #region Fields

        SVGLengthList _BaseVal;
        SVGLengthList _AnimVal;

        #endregion

        #region Constructors

        public SVGAnimatedLengthListImpl()
        {

        }

        public SVGAnimatedLengthListImpl(SVGLengthList baseVal, SVGLengthList animVal)
        {
            _BaseVal = baseVal;
            _AnimVal = animVal;
        }

        public SVGAnimatedLengthListImpl(SVGLengthList baseVal)
            : this(baseVal, baseVal)
        {

        }

        #endregion

        #region Properties

        public virtual SVGLengthList BaseVal
        {
            get { return _BaseVal; }
        }

        public virtual SVGLengthList AnimVal
        {
            get { return _AnimVal; }
        }

        #endregion
    }
}
