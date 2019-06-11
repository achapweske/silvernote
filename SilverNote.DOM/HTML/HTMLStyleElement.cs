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
    /// <summary>
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-16428977
    /// </summary>
    public interface HTMLStyleElement : HTMLElement
    {
        bool Disabled { get; set; }
        string Media { get; set; }
        string Type { get; set; }
    }
}
