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
using DOM.Style.Internal;

namespace DOM.CSS.Internal
{
    /// <summary>
    /// Represents a @media rule in a CSS style sheet. 
    /// 
    /// A @media rule can be used to delimit style rules for specific media types.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Style-20001113/css.html#CSS-CSSMediaRule
    /// </summary>
    public class CSSMediaRuleBase : CSSRuleBase, CSSMediaRule
    {
        #region Fields

        MediaList _Media;
        CSSRuleList _CssRules;

        #endregion

        #region Constructors

        public CSSMediaRuleBase()
            : base(CSSRuleType.MEDIA_RULE)
        {
            _Media = new MediaListBase();
            _CssRules = new CSSRuleListBase();
        }

        #endregion

        #region CssRule


        #endregion

        #region CSSMediaRule

        public MediaList Media
        {
            get { return _Media; }
        }

        public CSSRuleList CssRules
        {
            get { return _CssRules; }
        }

        public int InsertRule(string rule, int index)
        {
            throw new NotImplementedException();
        }

        public void DeleteRule(int index)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
