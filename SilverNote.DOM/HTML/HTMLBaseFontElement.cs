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
    public interface HTMLBaseFontElement : HTMLElement
    {
        string Color { get; set; }
        string Face { get; set; }
        int Size { get; set; }
    }
}
