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
    public interface HTMLLinkElement : HTMLElement
    {
        bool Disabled { get; set; }
        string Charset { get; set; }
        string HRef { get; set; }
        string HRefLang { get; set; }
        string Media { get; set; }
        string Rel { get; set; }
        string Rev { get; set; }
        string Target { get; set; }
        string Type { get; set; }
    }
}
