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
    public interface HTMLIFrameElement : HTMLElement
    {
        string Align { get; set; }
        string FrameBorder { get; set; }
        string Height { get; set; }
        string LongDesc { get; set; }
        string MarginHeight { get; set; }
        string MarginWidth { get; set; }
        string Name { get; set; }
        string Scrolling { get; set; }
        string Src { get; set; }
        string Width { get; set; }
        Document ContentDocument { get; }
    }
}
