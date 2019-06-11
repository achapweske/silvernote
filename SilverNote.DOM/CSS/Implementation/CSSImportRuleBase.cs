/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Style;

namespace DOM.CSS.Internal
{
    /// <summary>
    /// Represents a @import rule within a CSS style sheet. 
    /// 
    /// The @import rule is used to import style rules from other style sheets.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Style-20001113/css.html#CSS-CSSImportRule
    /// </summary>
    public class CSSImportRuleBase : CSSRuleBase, CSSImportRule
    {
        #region Constructors

        public CSSImportRuleBase()
            : base(CSSRuleType.IMPORT_RULE)
        {

        }

        #endregion

        #region CSSImportRule

        public string HRef
        {
            get { throw new NotImplementedException(); }
        }

        public MediaList Media
        {
            get { throw new NotImplementedException(); }
        }

        public CSSStyleSheet StyleSheet
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
