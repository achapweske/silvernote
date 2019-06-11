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
    public class SVGAnimatedLengthImpl : SVGAnimatedLength
    {
        #region Fields
        
        SVGLength _BaseVal;
        SVGLength _AnimVal;

        #endregion

        #region Constructors

        public SVGAnimatedLengthImpl()
        {

        }

        public SVGAnimatedLengthImpl(SVGLength baseVal, SVGLength animVal)
        {
            _BaseVal = baseVal;
            _AnimVal = animVal;
        }

        public SVGAnimatedLengthImpl(SVGLength baseVal)
            : this(baseVal, baseVal)
        {

        }

        public SVGAnimatedLengthImpl(Element element, string attributeName)
            : this(new SVGLengthAttr(element, attributeName))
        {

        }

        #endregion

        #region Properties

        public virtual SVGLength BaseVal 
        {
            get { return _BaseVal; }
        }

        public virtual SVGLength AnimVal
        {
            get { return _AnimVal; }
        }

        #endregion
    }
}
