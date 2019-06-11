/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.CSS
{
    public interface CSSPrimitiveValue : CSSValue
    {
        CSSPrimitiveType PrimitiveType { get; }
        void SetFloatValue(CSSPrimitiveType unitType, double floatValue);
        double GetFloatValue(CSSPrimitiveType unitType);
        void SetStringValue(CSSPrimitiveType stringType, string stringValue);
        string GetStringValue();
        CSSCounter GetCounterValue();
        CSSRect GetRectValue();
        RGBColor GetRGBColorValue();
    }
}
