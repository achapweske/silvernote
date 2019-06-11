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
    /// Represents a @font-face rule in a CSS style sheet. 
    /// 
    /// The @font-face rule is used to hold a set of font descriptions.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Style-20001113/css.html#CSS-CSSFontFaceRule
    /// </summary>
    public class CSSFontFaceRuleBase : CSSRuleBase, CSSFontFaceRule
    {
        #region Constructors

        public CSSFontFaceRuleBase()
            : base(CSSRuleType.FONT_FACE_RULE)
        {

        }

        #endregion

        #region CSSFontFaceRule

        public CSSStyleDeclaration Style
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
