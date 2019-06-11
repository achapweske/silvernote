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
    public interface StyleSheetList : IEnumerable<StyleSheet>
    {
        int Length { get; }
        StyleSheet this[int index] { get; }
    }
}
