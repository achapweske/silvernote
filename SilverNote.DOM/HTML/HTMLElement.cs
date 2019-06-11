/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS;

namespace DOM.HTML
{
    /// <summary>
    /// http://www.w3.org/TR/2003/REC-DOM-Level-2-HTML-20030109/html.html#ID-213157251
    /// </summary>
    public interface HTMLElement : Element, ElementCSSInlineStyle
    {
        string ID { get; set; }
        string Title { get; set; }
        string Lang { get; set; }
        string Dir { get; set; }
        string ClassName { get; set; }
    }
}
