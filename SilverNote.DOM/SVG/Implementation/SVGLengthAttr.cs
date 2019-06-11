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
    public class SVGLengthAttr : MutableSVGLength
    {
        #region Fields

        Element _Element;
        string _AttributeName;

        #endregion

        #region Constructors

        public SVGLengthAttr(Element element, string attributeName)
        {
            _Element = element;
            _AttributeName = attributeName;
        }

        #endregion

        #region SVGLength

        public override SVGLengthType UnitType
        {
            get
            {
                string str = this.ValueAsString;
                if (!String.IsNullOrEmpty(str))
                {
                    return SVGParser.ParseLength(str).UnitType;
                }
                else
                {
                    return SVGLengthType.SVG_LENGTHTYPE_NUMBER;
                }
            }
        }

        public override double ValueInSpecifiedUnits
        {
            get
            {
                string str = this.ValueAsString;
                if (!String.IsNullOrEmpty(str))
                {
                    return SVGParser.ParseLength(str).ValueInSpecifiedUnits;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                var newLength = new ConstSVGLength(value, UnitType);
                ValueAsString = SVGFormatter.FormatLength(newLength);
            }
        }

        public override string ValueAsString
        {
            get
            {
                return _Element.GetAttribute(_AttributeName);
            }
            set
            {
                _Element.SetAttribute(_AttributeName, value);
            }
        }

        public override void NewValueSpecifiedUnits(SVGLengthType unitType, double valueInSpecifiedUnits)
        {
            var newLength = new ConstSVGLength(valueInSpecifiedUnits, unitType);
            ValueAsString = SVGFormatter.FormatLength(newLength);
        }

        #endregion
    }
}
