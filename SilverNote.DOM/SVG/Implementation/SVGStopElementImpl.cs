/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS;

namespace DOM.SVG.Internal
{
    public class SVGStopElementImpl : SVGElementBase, SVGStopElement
    {
        #region Constructors

        public SVGStopElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.STOP)
        {

        }

        #endregion

        #region SVGStopElement

        SVGAnimatedNumber _Offset;

        public SVGAnimatedNumber Offset
        {
            get
            {
                if (_Offset == null)
                {
                    _Offset = new SVGAnimatedNumberImpl(this, SVGAttributes.OFFSET);
                }
                return _Offset;
            }
        }

        #endregion

        #region SVGStyleable

        SVGAnimatedString _ClassName;

        public SVGAnimatedString ClassName
        {
            get
            {
                if (_ClassName == null)
                {
                    _ClassName = new SVGAnimatedStringImpl(this, SVGAttributes.CLASS);
                }
                return _ClassName;
            }
        }

        public CSSValue GetPresentationAttribute(string name)
        {
            return Style.GetPropertyCSSValue(name);
        }

        #endregion
    }
}
