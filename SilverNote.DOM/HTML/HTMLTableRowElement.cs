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
    public interface HTMLTableRowElement : HTMLElement
    {
        int RowIndex { get; }
        int SectionRowIndex { get; }
        HTMLCollection Cells { get; }
        string Align { get; set; }
        string BgColor { get; set; }
        string Ch { get; set; }
        string ChOff { get; set; }
        string VAlign { get; set; }
        HTMLElement InsertCell(int index);
        void DeleteCell(int index);
    }
}
