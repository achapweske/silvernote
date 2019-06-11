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
    public class MutableCSSRuleList : CSSRuleListBase
    {
        #region Fields

        private List<CSSRule> _Items;

        #endregion

        #region Constructors

        public MutableCSSRuleList()
        {
            _Items = new List<CSSRule>();
        }

        #endregion

        #region CSSRuleList

        public override int Length
        {
            get { return _Items.Count; }
        }

        public override CSSRule this[int index]
        {
            get
            {
                if (index >= 0 || index < _Items.Count)
                {
                    return _Items[index];
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region Extensions

        /// <summary>
        /// Insert a new rule into this collection. 
        /// </summary>
        /// <param name="rule">The rule to be inserted.</param>
        /// <param name="index">Index where the new rule will be inserted.</param>
        /// <returns>Index of the newly inserted rule.</returns>
        /// <exception cref="DOMException">
        /// INDEX_SIZE_ERR: Raised if the specified index is not a valid insertion point.
        /// </exception>
        public int InsertRule(CSSRuleBase rule, int index)
        {
            if (index < 0 || index > _Items.Count)
            {
                throw new DOMException(DOMException.INDEX_SIZE_ERR);
            }

            _Items.Insert(index, rule);

            RaiseCollectionChanged(null, rule);

            return index;
        }

        /// <summary>
        /// Delete a rule from the style sheet.
        /// </summary>
        /// <param name="index">Index of the rule to be deleted</param>
        /// <exception cref="DOMException">
        /// INDEX_SIZE_ERR: Raised if the specified index does not correspond to a rule in the style sheet's rule list.
        /// </exception>
        public void DeleteRule(int index)
        {
            if (index < 0 || index >= _Items.Count)
            {
                throw new DOMException(DOMException.INDEX_SIZE_ERR);
            }

            var oldItem = _Items[index];

            _Items.RemoveAt(index);

            RaiseCollectionChanged(oldItem, null);
        }

        #endregion
    }
}
