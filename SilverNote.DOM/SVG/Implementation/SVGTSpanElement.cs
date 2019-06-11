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
    public class SVGTSpanElementImpl : SVGTextPositioningElementImpl, SVGTSpanElement
    {
        #region Constructors

        public SVGTSpanElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.TSPAN)
        {

        }

        #endregion
    }
}
