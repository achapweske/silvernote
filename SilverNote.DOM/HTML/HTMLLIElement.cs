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
    /// List item. See the LI element definition in HTML 4.01.
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-74680021
    /// </summary>
    public interface HTMLLIElement : HTMLElement
    {
        string Type { get; set; }
        int Value { get; set; }
    }
}
