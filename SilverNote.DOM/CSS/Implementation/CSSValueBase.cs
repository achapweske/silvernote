/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.CSS.Internal
{
    /// <summary>
    /// This class implements the CSSValue interface introduced in DOM Level 2:
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Style-20001113/css.html#CSS-CSSValue
    /// </summary>
    public class CSSValueBase : CSSValue
    {
        #region Fields

        private CSSValueType _CssValueType;
        private string _CssText;

        #endregion

        #region Constructors

        public CSSValueBase(CSSValueType cssValueType)
        {
            _CssValueType = cssValueType;
        }

        public CSSValueBase(CSSValueType cssValueType, string cssText)
        {
            _CssValueType = cssValueType;
            _CssText = cssText;
        }

        #endregion

        #region CSSValue

        public virtual string CssText
        {
            get
            {
                if (CssValueType == CSSValueType.CSS_INHERIT)
                {
                    return "inherit";
                }
                else
                {
                    return _CssText;
                }
            }
            set
            {
                _CssText = value;
            }
        }

        public virtual CSSValueType CssValueType
        {
            get { return _CssValueType; }
        }

        #endregion

        #region Extensions

        public bool IsInherit
        {
            get { return CssValueType == CSSValueType.CSS_INHERIT; }
        }

        /// <summary>
        /// Determine if two CSSValues are equivalent.
        /// 
        /// Two values are equivalent if they are exactly equal to each other,
        /// or they are primitive values with compatible units and their
        /// converted values are exactly equal.
        /// </summary>
        public static bool AreEquivalent(CSSValue a, CSSValue b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            if (a.CssValueType == CSSValueType.CSS_INHERIT && 
                b.CssValueType == CSSValueType.CSS_INHERIT)
            {
                return true;
            }

            if (a.CssValueType == CSSValueType.CSS_PRIMITIVE_VALUE &&
                b.CssValueType == CSSValueType.CSS_PRIMITIVE_VALUE)
            {
                return CSSPrimitiveValueBase.AreEquivalent((CSSPrimitiveValueBase)a, (CSSPrimitiveValueBase)b);
            }

            if (a.CssValueType == CSSValueType.CSS_VALUE_LIST &&
                b.CssValueType == CSSValueType.CSS_VALUE_LIST)
            {
                return CSSValueListBase.AreEquivalent((CSSValueListBase)a, (CSSValueListBase)b);
            }

            return false;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return CssText;
        }

        #endregion
    }

}
