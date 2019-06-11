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
    public interface HTMLFrameElement : HTMLElement
    {
        string FrameBorder { get; set; }
        string LongDesc { get; set; }
        string MarginHeight { get; set; }
        string MarginWidth { get; set; }
        string Name { get; set; }
        bool NoResize { get; set; }
        string Scrolling { get; set; }
        string Src { get; set; }
        Document ContentDocument { get; }
    }
}
