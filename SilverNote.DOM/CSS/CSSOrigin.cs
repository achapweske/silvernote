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
    public enum CSSOrigin
    {
        None = 0,
        Inline = 0x01,
        Document = 0x02,
        Author = Inline | Document,
        User = 0x04,
        UserAgent = 0x08,
        Initial = 0x20,
        All = Author | User | UserAgent | Initial
    }
}
