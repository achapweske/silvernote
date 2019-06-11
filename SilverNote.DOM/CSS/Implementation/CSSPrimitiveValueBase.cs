/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.CSS.Internal
{
    public class CSSPrimitiveValueBase : CSSValueBase, CSSPrimitiveValue
    {
        #region Fields

        CSSPrimitiveType _PrimitiveType;
        double _FloatValue;
        string _StringValue;
        CSSRectBase _RectValue;
        RGBColor _RGBColorValue;
        string _CssText;

        #endregion

        #region Constructors

        public CSSPrimitiveValueBase(double value, CSSPrimitiveType units)
            : base(CSSValueType.CSS_PRIMITIVE_VALUE)
        {
            _PrimitiveType = units;
            _FloatValue = value;
        }

        public CSSPrimitiveValueBase(string value, CSSPrimitiveType units)
            : base(CSSValueType.CSS_PRIMITIVE_VALUE)
        {
            _PrimitiveType = units;
            _StringValue = value;
        }

        public CSSPrimitiveValueBase(RGBColor value)
            : base(CSSValueType.CSS_PRIMITIVE_VALUE)
        {
            _PrimitiveType = CSSPrimitiveType.CSS_RGBCOLOR;
            _RGBColorValue = value;
        }

        public CSSPrimitiveValueBase(CSSRectBase value)
            : base(CSSValueType.CSS_PRIMITIVE_VALUE)
        {
            _PrimitiveType = CSSPrimitiveType.CSS_RECT;
            _RectValue = value;
        }

        protected CSSPrimitiveValueBase(CSSValueType unitType)
            : base(unitType)
        {

        }

        #endregion

        #region CSSPrimitiveValue

        public CSSPrimitiveType PrimitiveType 
        {
            get { return _PrimitiveType; } 
        }

        public void SetFloatValue(CSSPrimitiveType unitType, double floatValue)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        public double GetFloatValue(CSSPrimitiveType unitType)
        {
            double result;

            if (!IsFloatUnits(PrimitiveType))
            {
                throw new DOMException(DOMException.INVALID_ACCESS_ERR);
            }

            if (ConvertFloat(PrimitiveType, unitType, _FloatValue, out result))
            {
                return result;
            }
            else if (_FloatValue == 0)
            {
                return 0;
            }
            else
            {
                throw new DOMException(DOMException.INVALID_ACCESS_ERR);
            }
        }

        public void SetStringValue(CSSPrimitiveType unitType, string stringValue)
        {
            throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
        }

        public string GetStringValue()
        {
            if (IsStringUnits(PrimitiveType))
            {
                return _StringValue;
            }
            else
            {
                throw new DOMException(DOMException.INVALID_ACCESS_ERR);
            }
        }

        public CSSCounter GetCounterValue()
        {
            throw new NotImplementedException();
        }

        public CSSRect GetRectValue()
        {
            if (PrimitiveType == CSSPrimitiveType.CSS_RECT)
            {
                return _RectValue;
            }
            else
            {
                throw new DOMException(DOMException.INVALID_ACCESS_ERR);
            }
        }

        public RGBColor GetRGBColorValue()
        {
            if (PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
            {
                return _RGBColorValue;
            }
            else
            {
                throw new DOMException(DOMException.INVALID_ACCESS_ERR);
            }
        }

        public override string CssText
        {
            get
            {
                if (_CssText == null)
                {
                    _CssText = CSSFormatter.FormatPrimitiveValue(this);
                }

                return _CssText;
            }
            set
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }
        }

        #endregion

        #region Extensions

        /// <summary>
        /// A CSSPrimitiveValue representing the value "inherit"
        /// </summary>
        public static readonly CSSPrimitiveValueBase Inherit = new CSSPrimitiveValueBase(CSSValueType.CSS_INHERIT);

        private static Dictionary<string, CSSPrimitiveValueBase> _Idents = new Dictionary<string, CSSPrimitiveValueBase>();

        public static CSSPrimitiveValueBase Ident(string name)
        {
            CSSPrimitiveValueBase result;
            if (_Idents.TryGetValue(name, out result))
            {
                return result;
            }
            else
            {
                result = new CSSPrimitiveValueBase(name, CSSPrimitiveType.CSS_IDENT);
                _Idents[name] = result;
                return result;
            }
        }

        public static bool AreEquivalent(CSSPrimitiveValueBase a, CSSPrimitiveValueBase b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            var unitsA = CanonicalUnits(a.PrimitiveType);
            var unitsB = CanonicalUnits(b.PrimitiveType);

            if (IsFloatUnits(unitsA) && 
                IsFloatUnits(unitsB) &&
                a.GetFloatValue(unitsA) == 0 &&
                b.GetFloatValue(unitsB) == 0 &&
                (unitsA == unitsB || unitsA == CSSPrimitiveType.CSS_NUMBER || unitsB == CSSPrimitiveType.CSS_NUMBER))
            {
                return true;
            }

            if (unitsA != unitsB)
            {
                return false;
            }

            var units = unitsA; // = unitsB

            if (IsFloatUnits(units))
            {
                double valueA = Math.Round(a.GetFloatValue(units), 3);
                double valueB = Math.Round(b.GetFloatValue(units), 3);

                return valueA == valueB;
            }
            else
            {
                return a.Equals(b);
            }
        }

        #endregion

        #region Implementation

        private static bool ConvertFloat(CSSPrimitiveType fromType, CSSPrimitiveType toType, double fromValue, out double toValue)
        {
            if (fromType == toType)
            {
                toValue = fromValue;
                return true;
            }
            else if (CanonicalUnits(fromType) == CanonicalUnits(toType))
            {
                toValue = (fromValue * ScaleFactor(toType)) / ScaleFactor(fromType);
                return true;
            }
            else
            {
                toValue = 0;
                return false;
            }
        }

        private static CSSPrimitiveType CanonicalUnits(CSSPrimitiveType unitType)
        {
            switch (unitType)
            {
                // Number
                case CSSPrimitiveType.CSS_NUMBER:
                    return CSSPrimitiveType.CSS_NUMBER;
                // Percentage
                case CSSPrimitiveType.CSS_PERCENTAGE:
                    return CSSPrimitiveType.CSS_PERCENTAGE;
                // Length
                case CSSPrimitiveType.CSS_PX:
                case CSSPrimitiveType.CSS_CM:
                case CSSPrimitiveType.CSS_MM:
                case CSSPrimitiveType.CSS_IN:
                case CSSPrimitiveType.CSS_PT:
                case CSSPrimitiveType.CSS_PC:
                    return CSSPrimitiveType.CSS_PX;
                // Angle
                case CSSPrimitiveType.CSS_DEG:
                case CSSPrimitiveType.CSS_RAD:
                case CSSPrimitiveType.CSS_GRAD:
                    return CSSPrimitiveType.CSS_DEG;
                // Time
                case CSSPrimitiveType.CSS_MS:
                case CSSPrimitiveType.CSS_S:
                    return CSSPrimitiveType.CSS_MS;
                // Frequency
                case CSSPrimitiveType.CSS_HZ:
                case CSSPrimitiveType.CSS_KHZ:
                    return CSSPrimitiveType.CSS_HZ;
                default:
                    return unitType;
            }
        }

        private static double ScaleFactor(CSSPrimitiveType unitType)
        {
            switch (unitType)
            {
                // Number
                case CSSPrimitiveType.CSS_NUMBER:
                    return 1.0;
                // Percentage
                case CSSPrimitiveType.CSS_PERCENTAGE:
                    return 1.0;
                // Length
                case CSSPrimitiveType.CSS_PX:
                    return 1.0;
                case CSSPrimitiveType.CSS_CM:
                    return 2.54 / 96.0;
                case CSSPrimitiveType.CSS_MM:
                    return 25.4 / 96.0;
                case CSSPrimitiveType.CSS_IN:
                    return 1.0 / 96.0;
                case CSSPrimitiveType.CSS_PT:
                    return 72.0 / 96.0;
                case CSSPrimitiveType.CSS_PC:
                    return 72.0 / 1152.0;
                // Angle
                case CSSPrimitiveType.CSS_DEG:
                    return 1.0;
                case CSSPrimitiveType.CSS_RAD:
                    return Math.PI / 180.0;
                case CSSPrimitiveType.CSS_GRAD:
                    return 1.0 / 0.9;
                // Time
                case CSSPrimitiveType.CSS_MS:
                    return 1.0;
                case CSSPrimitiveType.CSS_S:
                    return 0.001;
                // Frequency
                case CSSPrimitiveType.CSS_HZ:
                    return 1.0;
                case CSSPrimitiveType.CSS_KHZ:
                    return 0.001;
                default:
                    return 1.0;
            }
        }

        private static bool IsFloatUnits(CSSPrimitiveType unitType)
        {
            switch (unitType)
            {
                case CSSPrimitiveType.CSS_NUMBER: 
                case CSSPrimitiveType.CSS_PERCENTAGE: 
                case CSSPrimitiveType.CSS_EMS: 
                case CSSPrimitiveType.CSS_EXS: 
                case CSSPrimitiveType.CSS_PX: 
                case CSSPrimitiveType.CSS_CM: 
                case CSSPrimitiveType.CSS_MM: 
                case CSSPrimitiveType.CSS_IN: 
                case CSSPrimitiveType.CSS_PT: 
                case CSSPrimitiveType.CSS_PC: 
                case CSSPrimitiveType.CSS_DEG: 
                case CSSPrimitiveType.CSS_RAD: 
                case CSSPrimitiveType.CSS_GRAD: 
                case CSSPrimitiveType.CSS_MS: 
                case CSSPrimitiveType.CSS_S: 
                case CSSPrimitiveType.CSS_HZ: 
                case CSSPrimitiveType.CSS_KHZ: 
                case CSSPrimitiveType.CSS_DIMENSION:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsStringUnits(CSSPrimitiveType unitType)
        {
            switch (unitType)
            {
                case CSSPrimitiveType.CSS_STRING: 
                case CSSPrimitiveType.CSS_URI:
                case CSSPrimitiveType.CSS_IDENT:
                case CSSPrimitiveType.CSS_ATTR:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Parsing

        private static Dictionary<string, CSSPrimitiveValueBase> Cache = new Dictionary<string, CSSPrimitiveValueBase>();

        public static CSSPrimitiveValueBase Parse(string str)
        {
            CSSPrimitiveValueBase result;

            if (TryParse(str, out result))
            {
                return result;
            }
            else
            {
                throw new DOMException(DOMException.SYNTAX_ERR);
            }
        }

        public static bool TryParse(string str, out CSSPrimitiveValueBase result)
        {
            str = str.Trim();

            if (str.Length == 0)
            {
                result = null;
                return true;
            }

            if (Cache.TryGetValue(str, out result))
            {
                return true;
            }

            if (str == "inherit")
            {
                result = Inherit;
            }
            else if (
                !TryParseFloat(str, out result) &&
                !TryParseString(str, out result) &&
                !TryParseRGBColor(str, out result) &&
                !TryParseRect(str, out result) &&
                !TryParseURI(str, out result) &&
                !TryParseAttr(str, out result))
            {
                result = Ident(str);
            }

            if (Cache.Count > 1000)
            {
                Cache.Clear();
            }

            Cache[str] = result;

            return true;
        }


        private static bool TryParseFloat(string str, out CSSPrimitiveValueBase result)
        {
            if (str.Length == 0 || !"+-.0123456789".Contains(str[0]))
            {
                result = null;
                return false;
            }

            string valuePart;
            string unitsPart;

            int i = str.LastIndexOfAny(".0123456789".ToArray());
            if ((i != -1) && (i + 1 != str.Length))
            {
                valuePart = str.Remove(i + 1);
                unitsPart = str.Substring(i + 1);
            }
            else
            {
                valuePart = str;
                unitsPart = String.Empty;
            }

            double floatValue;
            if (!Double.TryParse(valuePart, NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue))
            {
                result = null;
                return false;
            }

            CSSPrimitiveType unitsType = CSSPrimitiveType.CSS_NUMBER;

            if (!String.IsNullOrEmpty(unitsPart))
            {
                unitsType = UnitsFromString(unitsPart);
            }

            result = new CSSPrimitiveValueBase(floatValue, unitsType);
            return true;
        }

        private static bool TryParseString(string str, out CSSPrimitiveValueBase result)
        {
            if (!str.StartsWith("\"") && !str.StartsWith("\'"))
            {
                result = null;
                return false;
            }

            str = str.Substring(1);

            if (str.EndsWith("\"") || str.EndsWith("\'"))
            {
                str = str.Remove(str.Length - 1);
            }

            result = new CSSPrimitiveValueBase(str, CSSPrimitiveType.CSS_STRING);
            return true;
        }

        private static bool TryParseRGBColor(string str, out CSSPrimitiveValueBase result)
        {
            RGBColor rgbColorValue;
            if (RGBColorBase.TryParse(str, out rgbColorValue))
            {
                result = new CSSPrimitiveValueBase(rgbColorValue);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseRect(string str, out CSSPrimitiveValueBase result)
        {
            CSSRectBase rect;
            if (CSSRectBase.TryParse(str, out rect))
            {
                result = new CSSPrimitiveValueBase(rect);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseURI(string str, out CSSPrimitiveValueBase result)
        {
            // http://www.w3.org/TR/css3-values/#urls

            string value;
            if (TryParseFunction(str, "url", out value))
            {
                result = new CSSPrimitiveValueBase(value, CSSPrimitiveType.CSS_URI);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseAttr(string str, out CSSPrimitiveValueBase result)
        {
            // http://www.w3.org/TR/css3-values/#attr

            string value;
            if (TryParseFunction(str, "attr", out value))
            {
                result = new CSSPrimitiveValueBase(value, CSSPrimitiveType.CSS_ATTR);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static bool TryParseFunction(string str, string name, out string value)
        {
            if (!name.EndsWith("("))
            {
                name = name + "(";
            }

            if (!str.StartsWith(name))
            {
                value = null;
                return false;
            }

            str = str.Remove(0, name.Length);

            if (str.EndsWith(")"))
            {
                str = str.Remove(str.Length - 1);
            }

            value = str;
            return true;
        }

        private static CSSPrimitiveType UnitsFromString(string str)
        {
            switch (str.ToLower())
            {
                case "%":
                    return CSSPrimitiveType.CSS_PERCENTAGE;
                case "em":
                    return CSSPrimitiveType.CSS_EMS;
                case "ex":
                    return CSSPrimitiveType.CSS_EXS;
                case "px":
                    return CSSPrimitiveType.CSS_PX;
                case "cm":
                    return CSSPrimitiveType.CSS_CM;
                case "mm":
                    return CSSPrimitiveType.CSS_MM;
                case "in":
                    return CSSPrimitiveType.CSS_IN;
                case "pt":
                    return CSSPrimitiveType.CSS_PT;
                case "pc":
                    return CSSPrimitiveType.CSS_PC;
                case "deg":
                    return CSSPrimitiveType.CSS_DEG;
                case "rad":
                    return CSSPrimitiveType.CSS_RAD;
                case "grad":
                    return CSSPrimitiveType.CSS_GRAD;
                case "ms":
                    return CSSPrimitiveType.CSS_MS;
                case "s":
                    return CSSPrimitiveType.CSS_S;
                case "hz":
                    return CSSPrimitiveType.CSS_HZ;
                case "khz":
                    return CSSPrimitiveType.CSS_KHZ;
                default:
                    return CSSPrimitiveType.CSS_UNKNOWN;
            }
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return CssText;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            var css = obj as CSSPrimitiveValueBase;

            if (css == null)
            {
                return false;
            }

            return this.CssText == css.CssText;
        }

        public override int GetHashCode()
        {
            return CssText.GetHashCode();
        }

        #endregion
    }

}
