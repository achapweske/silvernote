/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.CSS
{
    /// <summary>
    /// Provides a read only access to the computed values of an element.
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-Style/css.html#CSS-OverrideAndComputed
    /// </summary>
    public interface ViewCSS : DOM.Views.AbstractView
    {
        /// <summary>
        /// This method is used to get the computed style as it is defined in [CSS2].
        /// </summary>
        /// <param name="elt">The element whose style is to be computed. This parameter cannot be null.</param>
        /// <param name="pseudoElt">The pseudo-element or null if none.</param>
        /// <returns>The computed style. The CSSStyleDeclaration is read-only and contains only absolute values.</returns>
        CSSStyleDeclaration GetComputedStyle(Element elt, string pseudoElt);
    }
}
