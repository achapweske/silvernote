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
    public class MutableSVGLength : SVGLength
    {
        #region Fields

        SVGLengthType _UnitType;
        double _Value;

        #endregion

        #region Constructors

        public MutableSVGLength(double value = 0, SVGLengthType unitType = SVGLengthType.SVG_LENGTHTYPE_NUMBER)
        {
            _Value = value;
            _UnitType = unitType;
        }

        public MutableSVGLength(SVGLength other)
            : this(other.Value, other.UnitType)
        {

        }

        #endregion

        #region Properties

        public virtual SVGLengthType UnitType 
        {
            get { return _UnitType; } 
        }

        public virtual double Value 
        {
            get { return ConvertValue(ValueInSpecifiedUnits, UnitType, SVGLengthType.SVG_LENGTHTYPE_NUMBER); }
            set { ValueInSpecifiedUnits = ConvertValue(value, SVGLengthType.SVG_LENGTHTYPE_NUMBER, UnitType); }
        }

        public virtual double ValueInSpecifiedUnits 
        {
            get { return _Value; }
            set { NewValueSpecifiedUnits(UnitType, value); }
        }

        public virtual string ValueAsString 
        {
            get { return SVGFormatter.FormatLength(this); }
            set { SVGParser.ParseLength(value, this); }
        }

        #endregion

        #region Operations

        public virtual void NewValueSpecifiedUnits(SVGLengthType unitType, double valueInSpecifiedUnits)
        {
            _UnitType = unitType;
            _Value = valueInSpecifiedUnits;
        }

        public virtual void ConvertToSpecifiedUnits(SVGLengthType unitType)
        {
            double newValue = ConvertValue(ValueInSpecifiedUnits, UnitType, unitType);
            NewValueSpecifiedUnits(unitType, newValue);
        }

        #endregion

        #region Implementation

        protected double ConvertValue(double value, SVGLengthType fromType, SVGLengthType toType)
        {
            if (fromType == toType)
            {
                return value;
            }
            
            return (value * ScaleFactor(toType)) / ScaleFactor(fromType);
        }

        protected double ScaleFactor(SVGLengthType unitType)
        {
            switch (unitType)
            {
                // Number
                case SVGLengthType.SVG_LENGTHTYPE_NUMBER:
                    return 1.0;
                // Percentage
                case SVGLengthType.SVG_LENGTHTYPE_PERCENTAGE:
                    return 1.0;
                // Font size
                case SVGLengthType.SVG_LENGTHTYPE_EMS:
                case SVGLengthType.SVG_LENGTHTYPE_EXS:
                    return 1.0;
                // Length
                case SVGLengthType.SVG_LENGTHTYPE_PX:
                    return 1.0;
                case SVGLengthType.SVG_LENGTHTYPE_CM:
                    return 2.54 / 96.0;
                case SVGLengthType.SVG_LENGTHTYPE_MM:
                    return 25.4 / 96.0;
                case SVGLengthType.SVG_LENGTHTYPE_IN:
                    return 1.0 / 96.0;
                case SVGLengthType.SVG_LENGTHTYPE_PT:
                    return 72.0 / 96.0;
                case SVGLengthType.SVG_LENGTHTYPE_PC:
                    return 72.0 / 1152.0;
                default:
                    return 1.0;
            }
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return ValueAsString;
        }

        #endregion
    }
}
