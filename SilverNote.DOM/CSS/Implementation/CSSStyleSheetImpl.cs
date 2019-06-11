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
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Style-20001113/css.html#CSS-CSSStyleSheet
    /// </summary>
    public class CSSStyleSheetImpl : StyleSheetBase, CSSStyleSheet
    {
        #region Fields

        CSSRuleBase _OwnerRule;
        MutableCSSRuleList _CssRules;

        #endregion

        #region Constructors

        public CSSStyleSheetImpl()
            : base(null)
        {
            _CssRules = new MutableCSSRuleList();
        }

        public CSSStyleSheetImpl(string title, string media)
            : base(title, media)
        {

        }

        public CSSStyleSheetImpl(Node ownerNode)
            : base(ownerNode)
        {
            _OwnerRule = null;
            _CssRules = new MutableCSSRuleList();
        }

        public CSSStyleSheetImpl(CSSRuleBase ownerRule)
            : base(null)
        {
            _OwnerRule = ownerRule;
            _CssRules = new MutableCSSRuleList();
        }

        #endregion

        #region CSSStyleSheet

        /// <summary>
        /// Get the owning CSSImportRule, or null if none
        /// </summary>
        public CSSRule OwnerRule 
        {
            get { return _OwnerRule; }
        }
        
        /// <summary>
        /// All CSS rules contained within the style sheet. 
        /// 
        /// This includes both rule sets and at-rules.
        /// </summary>
        public CSSRuleList CssRules 
        {
            get { return _CssRules; }
        }

        /// <summary>
        /// Insert a new rule into the style sheet. 
        /// 
        /// The new rule now becomes part of the cascade.
        /// </summary>
        /// <param name="text">Parsable text representing the rule.</param>
        /// <param name="index">Index where the new rule will be inserted.</param>
        /// <returns>Index of the newly inserted rule.</returns>
        /// <exception cref="DOMException">
        /// HIERARCHY_REQUEST_ERR: Raised if the rule cannot be inserted at the specified index e.g. if an @import rule is inserted after a standard rule set or other at-rule.
        /// INDEX_SIZE_ERR: Raised if the specified index is not a valid insertion point.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this style sheet is readonly.
        /// SYNTAX_ERR: Raised if the specified rule has a syntax error and is unparsable.
        /// </exception>
        public int InsertRule(string text, int index)
        {
            CSSRuleBase rule = CSSParser.ParseStatement(text);

            return InsertRule(rule, index);
        }

        /// <summary>
        /// Delete a rule from the style sheet.
        /// </summary>
        /// <param name="index">Index of the rule to be deleted</param>
        /// <exception cref="DOMException">
        /// INDEX_SIZE_ERR: Raised if the specified index does not correspond to a rule in the style sheet's rule list.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this style sheet is readonly.</exception>
        public void DeleteRule(int index)
        {
            _CssRules.DeleteRule(index);
        }

        #endregion

        #region StyleSheet

        public override string Type
        {
            get
            {
                string type = base.Type;

                if (!String.IsNullOrEmpty(type))
                {
                    return type;
                }
                else
                {
                    return "text/css";
                }
            }
        }

        #endregion

        #region Extensions

        public int AppendRule(CSSRuleBase rule)
        {
            return InsertRule(rule, CssRules.Length);
        }

        public int InsertRule(CSSRuleBase rule, int index)
        {
            return _CssRules.InsertRule(rule, index);
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return CSSFormatter.FormatStyleSheet(this);
        }

        #endregion
    }
}
