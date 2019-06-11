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
    public class SVGAnimatedBooleanImpl : SVGAnimatedBoolean
    {
        #region Fields

        readonly bool _BaseVal;
        readonly Element _Element;
        readonly string _AttributeName;

        #endregion

        #region Constructors

        public SVGAnimatedBooleanImpl()
        {

        }

        public SVGAnimatedBooleanImpl(bool baseVal)
        {
            _BaseVal = baseVal;
        }

        public SVGAnimatedBooleanImpl(Element element, string attributeName)
        {
            _Element = element;
            _AttributeName = attributeName;
        }

        #endregion

        #region Properties

        public virtual bool BaseVal
        {
            get
            {
                if (_Element != null)
                {
                    return _Element.GetAttributeAsBool(_AttributeName, false);
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
                    _Element.SetAttributeAsBool(_AttributeName, value);
                }
                else
                {
                    throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
                }
            }
        }

        public virtual bool AnimVal
        {
            get { return BaseVal; }
        }

        #endregion
    }
}
