/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Internal;

namespace DOM.Style.Internal
{
    /// <summary>
    /// The StyleSheetList interface provides the abstraction of an ordered collection of style sheets.
    /// </summary>
    public class StyleSheetListBase : DOMList<StyleSheet>, StyleSheetList
    {
        #region Constructors

        public StyleSheetListBase()
        {

        }

        public StyleSheetListBase(IEnumerable<StyleSheet> items)
            : base(items)
        {

        }

        #endregion

        #region Extensions

        public static readonly StyleSheetListBase Empty = new StyleSheetListBase();

        #endregion
    }
}
