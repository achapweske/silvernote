/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Style
{
    /// <summary>
    /// The DocumentStyle interface provides a mechanism by which the style sheets embedded in a document can be retrieved.
    /// </summary>
    public interface DocumentStyle
    {
        StyleSheetList StyleSheets { get; }
    }
}
