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
    public class SVGAnimatedStringImpl : SVGAnimatedString
    {
        #region Fields

        readonly string _BaseVal;
        readonly Element _Element;
        readonly string _AttributeName;

        #endregion

        #region Constructors

        public SVGAnimatedStringImpl()
        {

        }

        public SVGAnimatedStringImpl(string baseVal)
        {
            _BaseVal = baseVal;
        }

        public SVGAnimatedStringImpl(Element element, string attributeName)
        {
            _Element = element;
            _AttributeName = attributeName;
        }

        #endregion

        #region Properties

        public virtual string BaseVal
        {
            get
            {
                if (_Element != null)
                {
                    return _Element.GetAttribute(_AttributeName);
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
                    _Element.SetAttribute(_AttributeName, value);
                }
                else
                {
                    throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
                }
            }
        }

        public virtual string AnimVal
        {
            get { return BaseVal; }
        }

        #endregion
    }
}
