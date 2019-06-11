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
    public class SVGAnimatedEnumerationImpl : SVGAnimatedEnumeration
    {
        #region Fields

        readonly ushort _BaseVal;
        readonly Element _Element;
        readonly string _PropertyName;
        readonly ISVGEnumerationParser _Parser;

        #endregion

        #region Constructors

        public SVGAnimatedEnumerationImpl()
        {

        }

        public SVGAnimatedEnumerationImpl(ushort baseVal)
        {
            _BaseVal = baseVal;
        }

        public SVGAnimatedEnumerationImpl(Element element, string propertyName, ISVGEnumerationParser parser)
        {
            _Element = element;
            _PropertyName = propertyName;
            _Parser = parser;
        }

        #endregion

        #region Properties

        public ushort BaseVal
        {
            get
            {
                if (_Element == null)
                {
                    return _BaseVal;
                }

                string propertyValue = _Element.GetAttribute(_PropertyName);
                return _Parser.Parse(propertyValue);
            }
            set
            {
                if (_Element == null)
                {
                    throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
                }

                string propertyValue = _Parser.Format(value);
                _Element.SetAttribute(_PropertyName, propertyValue);
            }
        }

        public ushort AnimVal
        {
            get { return BaseVal; }
        }

        #endregion
    }

    public interface ISVGEnumerationParser
    {
        string Format(ushort value);
        ushort Parse(string name);
    }
}
