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
    public enum DOMOperationType : ushort
    {
        NODE_CLONED = 1,
        NODE_IMPORTED = 2,
        NODE_DELETED = 3,
        NODE_RENAMED = 4,
        NODE_ADOPTED = 5
    }
}
