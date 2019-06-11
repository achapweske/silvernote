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
    /// Embedded image. See the IMG element definition in HTML 4.01.
    /// http://www.w3.org/TR/2003/REC-DOM-Level-2-HTML-20030109/html.html#ID-17701901
    /// </summary>
    public interface HTMLImageElement : HTMLElement
    {
        string Name { get; set; }
        string Align { get; set; }
        string Alt { get; set; }
        string Border { get; set; }
        int Height { get; set; }
        int HSpace { get; set; }
        bool IsMap { get; set; }
        string LongDesc { get; set; }
        string Src { get; set; }
        string UseMap { get; set; }
        int VSpace { get; set; }
        int Width { get; set; }
    }
}
