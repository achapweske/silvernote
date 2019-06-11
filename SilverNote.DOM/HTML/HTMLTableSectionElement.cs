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
    public interface HTMLTableSectionElement : HTMLElement
    {
        string Align { get; set; }
        string Ch { get; set; }
        string ChOff { get; set; }
        string VAlign { get; set; }
        HTMLCollection Rows { get; }
        HTMLElement InsertRow(int index);
        void DeleteRow(int index);
    }
}
