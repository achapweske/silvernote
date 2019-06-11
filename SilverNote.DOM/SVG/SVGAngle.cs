/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.SVG
{
    public interface SVGAngle
    {
        SVGAngleType UnitType { get; }
        double Value { get; set; }
        double ValueInSpecifiedUnits { get; set; }
        string ValueAsString { get; set; }

        void NewValueSpecifiedUnits(SVGAngleType UnitType, double valueInSpecifiedUnits);
        void ConvertToSpecifiedUnits(SVGAngleType unitType);
    }
}
