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
    public interface HTMLScriptElement : HTMLElement
    {
        string Text { get; set; }
        string Charset { get; set; }
        bool Defer { get; set; }
        string Src { get; set; }
        string Type { get; set; }
    }
}
