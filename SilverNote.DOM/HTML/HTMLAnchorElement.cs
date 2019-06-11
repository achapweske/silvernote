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
    /// The anchor element.
    /// 
    /// http://www.w3.org/TR/2003/REC-DOM-Level-2-HTML-20030109/html.html#ID-48250443
    /// </summary>
    public interface HTMLAnchorElement : HTMLElement
    {
        string AccessKey { get; set; }
        string Charset { get; set; }
        string Coords { get; set; }
        string HRef { get; set; }
        string HRefLang { get; set; }
        string Name { get; set; }
        string Rel { get; set; }
        string Rev { get; set; }
        string Shape { get; set; }
        int TabIndex { get; set; }
        string Target { get; set; }
        string Type { get; set; }
        void Blur();
        void Focus();
    }
}
