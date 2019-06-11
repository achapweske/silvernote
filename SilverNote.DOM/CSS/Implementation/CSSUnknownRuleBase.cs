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
    /// Represents an at-rule not supported by this user agent.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Style-20001113/css.html#CSS-CSSUnknownRule
    /// </summary>
    public class CSSUnknownRuleBase : CSSRuleBase, CSSUnknownRule
    {
        #region Constructors

        public CSSUnknownRuleBase()
            : base(CSSRuleType.UNKNOWN_RULE)
        {

        }

        #endregion
    }
}
