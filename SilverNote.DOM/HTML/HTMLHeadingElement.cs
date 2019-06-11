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
    /// For the H1 to H6 elements. See the H1 element definition in HTML 4.01.
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-43345119
    /// </summary>
    public interface HTMLHeadingElement : HTMLElement
    {
        string Align { get; set; }
    }
}
