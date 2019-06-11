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
    public interface HTMLMetaElement : HTMLElement
    {
        string Content { get; set; }
        string HTTPEquiv { get; set; }
        string Name { get; set; }
        string Scheme { get; set; }
    }
}
