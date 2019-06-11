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
    public static class CSSPrimitiveValueExtensions
    {
        /// <summary>
        /// http://www.w3.org/TR/CSS2/syndata.html#value-def-length
        /// </summary>
        public static bool IsLength(this CSSPrimitiveValue value)
        {
            switch (value.PrimitiveType)
            {
                case CSSPrimitiveType.CSS_NUMBER:
                    return value.GetFloatValue(CSSPrimitiveType.CSS_NUMBER) == 0;
                case CSSPrimitiveType.CSS_EMS:
                case CSSPrimitiveType.CSS_EXS:
                case CSSPrimitiveType.CSS_IN:
                case CSSPrimitiveType.CSS_CM:
                case CSSPrimitiveType.CSS_MM:
                case CSSPrimitiveType.CSS_PT:
                case CSSPrimitiveType.CSS_PC:
                case CSSPrimitiveType.CSS_PX:
                    return true;
                default:
                    return false;
            }
        }
    }
}
