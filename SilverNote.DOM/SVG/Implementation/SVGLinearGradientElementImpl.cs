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
    public class SVGLinearGradientElementImpl : SVGGradientElementImpl, SVGLinearGradientElement
    {
        #region Fields

        SVGAnimatedLength _X1;
        SVGAnimatedLength _X2;
        SVGAnimatedLength _Y1;
        SVGAnimatedLength _Y2;

        #endregion

        #region Constructors

        public SVGLinearGradientElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.LINEAR_GRADIENT)
        {

        }

        #endregion

        #region SVGLinearGradientElement

        public SVGAnimatedLength X1
        {
            get
            {
                if (_X1 == null)
                {
                    _X1 = new SVGAnimatedLengthImpl(this, SVGAttributes.X1);
                }
                return _X1;
            }
        }

        public SVGAnimatedLength Y1
        {
            get
            {
                if (_Y1 == null)
                {
                    _Y1 = new SVGAnimatedLengthImpl(this, SVGAttributes.Y1);
                }
                return _Y1;
            }
        }

        public SVGAnimatedLength X2
        {
            get
            {
                if (_X2 == null)
                {
                    _X2 = new SVGAnimatedLengthImpl(this, SVGAttributes.X2);
                }
                return _X2;
            }
        }

        public SVGAnimatedLength Y2
        {
            get
            {
                if (_Y2 == null)
                {
                    _Y2 = new SVGAnimatedLengthImpl(this, SVGAttributes.Y2);
                }
                return _Y2;
            }
        }

        #endregion
    }
}
