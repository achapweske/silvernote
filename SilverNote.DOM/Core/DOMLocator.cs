/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM
{
    public interface DOMLocator
    {
        int LineNumber { get; }
        int ColumnNumber { get; }
        int ByteOffset { get; }
        int UTF16Offset { get; }
        Node RelatedNode { get; }
        string URI { get; }
    }
}
