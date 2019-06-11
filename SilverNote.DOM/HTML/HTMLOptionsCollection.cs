/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.HTML
{
    public interface HTMLOptionsCollection
    {
        int Length { get; set; }
        Node this[int index] { get; }
        Node this[string name] { get; }
    }
}
