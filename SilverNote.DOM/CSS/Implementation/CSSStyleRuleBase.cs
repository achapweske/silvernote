/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DOM.CSS.Internal
{
    public class CSSStyleRuleBase : CSSRuleBase, CSSStyleRule
    {
        #region Fields

        CSSStyleDeclarationBase _Style;
        CSSSelectorGroup _Selector;

        #endregion

        #region Constructors

        public CSSStyleRuleBase()
            : base(CSSRuleType.STYLE_RULE)
        {
            _Style = new CSSStyleDeclarationBase();
            _Selector = new CSSSelectorGroup();
        }

        #endregion

        #region CSSRule

        public override string CssText
        {
            get { return CSSFormatter.FormatStyleRule(this); }
            set { CSSParser.ParseRuleSet(value, this); }
        }

        #endregion

        #region CSSStyleRule

        /// <summary>
        /// The textual representation of the selector for the rule set.
        /// </summary>
        /// <exception cref="DOMException">
        /// SYNTAX_ERR: Raised if the specified CSS string value has a syntax error and is unparsable.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this rule is readonly.
        /// </exception>
        public string SelectorText
        {
            get { return CSSFormatter.FormatSelectorGroup(Selector); }
            set { CSSParser.ParseSelectorGroup(value, Selector); }
        }

        /// <summary>
        /// The declaration-block of this rule set.
        /// </summary>
        public CSSStyleDeclaration Style
        {
            get { return _Style; }
        }

        #endregion

        #region Implementation

        internal CSSSelectorGroup Selector
        {
            get { return _Selector; }
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
