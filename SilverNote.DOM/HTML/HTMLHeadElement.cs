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
    /// Document head information. See the HEAD element definition in HTML 4.01.
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-77253168
    /// </summary>
    public interface HTMLHeadElement : HTMLElement
    {
        string Profile { get; set; }
    }
}
