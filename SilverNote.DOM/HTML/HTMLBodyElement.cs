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
    /// The HTML document body. 
    ///
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-62018039
    /// </summary>
    public interface HTMLBodyElement : HTMLElement
    {
        string ALink { get; set; }
        string Background { get; set; }
        string BGColor { get; set; }
        string Link { get; set; }
        string Text { get; set; }
        string VLink { get; set; }
    }
}
