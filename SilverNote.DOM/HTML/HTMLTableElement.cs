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
    public interface HTMLTableElement : HTMLElement
    {
        HTMLTableCaptionElement Caption { get; set; }
        HTMLTableSectionElement THead { get; set; }
        HTMLTableSectionElement TFoot { get; set; }
        HTMLCollection Rows { get; }
        HTMLCollection TBodies { get; }
        string Align { get; set; }
        string BgColor { get; set; }
        string Border { get; set; }
        string CellPadding { get; set; }
        string CellSpacing { get; set; }
        string Frame { get; set; }
        string Rules { get; set; }
        string Summary { get; set; }
        string Width { get; set; }
        HTMLElement CreateTHead();
        void DeleteTHead();
        HTMLElement CreateTFoot();
        void DeleteTFoot();
        HTMLElement CreateCaption();
        void DeleteCaption();
        HTMLElement InsertRow(int index);
        void DeleteRow(int index);
    }
}
