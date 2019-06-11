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
    /// Represents the TH and TD elements. See the TD element definition in HTML 4.01.
    /// http://www.w3.org/TR/2003/REC-DOM-Level-2-HTML-20030109/html.html#ID-82915075
    /// </summary>
    public interface HTMLTableCellElement : HTMLElement
    {
        int CellIndex { get; }
        string Abbr { get; set; }
        string Align { get; set; }
        string Axis { get; set; }
        string BgColor { get; set; }
        string Ch { get; set; }
        string ChOff { get; set; }
        int ColSpan { get; set; }
        string Headers { get; set; }
        string Height { get; set; }
        bool NoWrap { get; set; }
        int RowSpan { get; set; }
        string Scope { get; set; }
        string VAlign { get; set; }
        string Width { get; set; }
    }
}
