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
    public class MutableSVGAngle : SVGAngle
    {
        #region Fields

        SVGAngleType _UnitType;
        double _Value;

        #endregion

        #region Constructors

        public MutableSVGAngle(double value = 0, SVGAngleType unitType = SVGAngleType.SVG_ANGLETYPE_UNSPECIFIED)
        {
            _Value = value;
            _UnitType = unitType;
        }

        public MutableSVGAngle(SVGAngle other)
            : this(other.Value, other.UnitType)
        {

        }

        #endregion

        #region Properties

        public virtual SVGAngleType UnitType
        {
            get { return _UnitType; }
        }

        public virtual double Value
        {
            get { return ConvertValue(ValueInSpecifiedUnits, UnitType, SVGAngleType.SVG_ANGLETYPE_DEG); }
            set { ValueInSpecifiedUnits = ConvertValue(value, SVGAngleType.SVG_ANGLETYPE_DEG, UnitType); }
        }

        public virtual double ValueInSpecifiedUnits
        {
            get { return _Value; }
            set { NewValueSpecifiedUnits(UnitType, value); }
        }

        public virtual string ValueAsString
        {
            get { return SVGFormatter.FormatAngle(this); }
            set { SVGParser.ParseAngle(value, this); }
        }

        #endregion

        #region Operations

        public virtual void NewValueSpecifiedUnits(SVGAngleType unitType, double valueInSpecifiedUnits)
        {
            _UnitType = unitType;
            _Value = valueInSpecifiedUnits;
        }

        public virtual void ConvertToSpecifiedUnits(SVGAngleType unitType)
        {
            double newValue = ConvertValue(ValueInSpecifiedUnits, UnitType, unitType);
            NewValueSpecifiedUnits(unitType, newValue);
        }

        #endregion

        #region Implementation

        protected double ConvertValue(double value, SVGAngleType fromType, SVGAngleType toType)
        {
            if (fromType == toType)
            {
                return value;
            }

            return (value * ScaleFactor(toType)) / ScaleFactor(fromType);
        }

        protected double ScaleFactor(SVGAngleType unitType)
        {
            switch (unitType)
            {
                // Degrees
                case SVGAngleType.SVG_ANGLETYPE_DEG:
                case SVGAngleType.SVG_ANGLETYPE_UNSPECIFIED:
                    return 1.0;
                case SVGAngleType.SVG_ANGLETYPE_RAD:
                    return Math.PI / 180.0;
                case SVGAngleType.SVG_ANGLETYPE_GRAD:
                    return 0.9;
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
