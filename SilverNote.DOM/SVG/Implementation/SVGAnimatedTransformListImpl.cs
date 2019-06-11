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
    public class SVGAnimatedTransformListImpl : SVGAnimatedTransformList
    {
        #region Fields
        
        SVGTransformList _BaseVal;
        SVGTransformList _AnimVal;

        #endregion

        #region Constructors

        public SVGAnimatedTransformListImpl()
        {

        }

        public SVGAnimatedTransformListImpl(SVGTransformList baseVal, SVGTransformList animVal)
        {
            _BaseVal = baseVal;
            _AnimVal = animVal;
        }

        public SVGAnimatedTransformListImpl(SVGTransformList baseVal)
            : this(baseVal, baseVal)
        {

        }

        #endregion

        #region Properties

        public virtual SVGTransformList BaseVal 
        {
            get { return _BaseVal; }
        }

        public virtual SVGTransformList AnimVal
        {
            get { return _AnimVal; }
        }

        #endregion
    }
}
