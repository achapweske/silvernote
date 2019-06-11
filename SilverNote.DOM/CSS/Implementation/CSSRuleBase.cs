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
    public class CSSRuleBase : CSSRule
    {
        #region Fields

        CSSRuleType _Type;

        #endregion

        #region Constructors

        public CSSRuleBase(CSSRuleType type)
        {
            _Type = type;
        }

        #endregion

        #region CSSRule

        public CSSRuleType Type 
        {
            get { return _Type; }
        }

        public CSSStyleSheet ParentStyleSheet 
        {
            get { throw new NotImplementedException(); } 
        }

        public CSSRule ParentRule 
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string CssText { get; set; }

        #endregion

        #region Object

        public override string ToString()
        {
            return CssText;
        }

        #endregion
    }
}
