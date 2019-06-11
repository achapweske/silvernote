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
    public class SVGTextElementImpl : SVGTextPositioningElementImpl
    {
        #region Constructors

        internal SVGTextElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.TEXT)
        {

        }

        #endregion

        #region SVGTransformable

        public SVGAnimatedTransformList Transform
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
