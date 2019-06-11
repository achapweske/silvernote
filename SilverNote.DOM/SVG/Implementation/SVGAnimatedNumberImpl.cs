/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Internal;

namespace DOM.SVG.Internal
{
    public class SVGAnimatedNumberImpl : SVGAnimatedNumber
    {
        #region Fields

        readonly double _BaseVal;
        readonly Element _Element;
        readonly string _AttributeName;

        #endregion

        #region Constructors

        public SVGAnimatedNumberImpl()
        {

        }

        public SVGAnimatedNumberImpl(double baseVal)
        {
            _BaseVal = baseVal;
        }

        public SVGAnimatedNumberImpl(Element element, string attributeName)
        {
            _Element = element;
            _AttributeName = attributeName;
        }

        #endregion

        #region Properties

        public virtual double BaseVal
        {
            get
            {
                if (_Element != null)
                {
                    return _Element.GetAttributeAsDouble(_AttributeName, 0.0);
                }
                else
                {
                    return _BaseVal;
                }
            }
            set
            {
                if (_Element != null)
                {
                    _Element.SetAttributeAsDouble(_AttributeName, value);
                }
                else
                {
                    throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
                }
            }
        }

        public virtual double AnimVal
        {
            get { return BaseVal; }
        }

        #endregion
    }
}
