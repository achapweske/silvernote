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
    public class SVGTextPositioningElementImpl : SVGTextContentElementImpl, SVGTextPositioningElement
    {
        #region Constructors

        public SVGTextPositioningElementImpl(SVGDocument ownerDocument, string namespaceURI, string qualifiedName)
            : base(ownerDocument, namespaceURI, qualifiedName)
        {

        }

        #endregion

        #region SVGTextPositioningElement

        public SVGAnimatedLengthList X
        {
            get { throw new NotImplementedException(); }
        }

        public SVGAnimatedLengthList Y
        {
            get { throw new NotImplementedException(); }
        }

        public SVGAnimatedLengthList DX
        {
            get { throw new NotImplementedException(); }
        }

        public SVGAnimatedLengthList DY
        {
            get { throw new NotImplementedException(); }
        }

        public SVGAnimatedNumberList Rotate
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
