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
    public interface HTMLHRElement : HTMLElement
    {
        string Align { get; set; }
        bool NoShade { get; set; }
        string Size { get; set; }
        string Width { get; set; }
    }
}
