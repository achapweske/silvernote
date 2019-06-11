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
    /// The delegate utilized by CSSRenderedStyleDeclaration
    /// </summary>
    public interface ICSSRenderer
    {
        /// <summary>
        /// Get the names of all styles supported by this object
        /// </summary>
        IList<string> GetSupportedStyles(ElementContext context);

        /// <summary>
        /// Get the currently-set value of the given CSS style.
        /// </summary>
        /// <param name="name">Name of the style to retrieve</param>
        /// <returns>The retrieved value, or null if not set.</returns>
        CSSValue GetStyleProperty(ElementContext context, string name);

        /// <summary>
        /// Set the given CSS style value.
        /// </summary>
        /// <param name="name">Name of the style to set</param>
        /// <param name="value">New style value</param>
        /// <param name="priority">New style priority, or String.Empty if none</param>
        void SetStyleProperty(ElementContext context, string name, CSSValue value);

    }
}
