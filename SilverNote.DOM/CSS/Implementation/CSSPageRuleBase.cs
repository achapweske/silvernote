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
    /// Represents a @page rule within a CSS style sheet. 
    /// 
    /// The @page rule is used to specify the style of a page box for paged media.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Style-20001113/css.html#CSS-CSSPageRule
    /// </summary>
    public class CSSPageRuleBase : CSSRuleBase, CSSPageRule
    {
        #region Constructors

        public CSSPageRuleBase()
            : base(CSSRuleType.PAGE_RULE)
        {

        }

        #endregion

        #region ICSSPageRule

        public string SelectorText 
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); } 
        }

        public CSSStyleDeclaration Style 
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
