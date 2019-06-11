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
    /// Represents a @charset rule in a CSS style sheet.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Style-20001113/css.html#CSS-CSSCharsetRule
    /// </summary>
    public class CSSCharsetRuleBase : CSSRuleBase, CSSCharsetRule
    {
        #region Constructors

        public CSSCharsetRuleBase()
            : base(CSSRuleType.CHARSET_RULE)
        {

        }

        #endregion

        #region CSSCharsetRule

        public string Encoding
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}
