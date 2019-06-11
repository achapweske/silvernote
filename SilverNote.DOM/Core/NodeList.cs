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
    public interface NodeList : IEnumerable<Node>
    {
        int Length { get; }
        Node this[int index] { get; }
    }
}
