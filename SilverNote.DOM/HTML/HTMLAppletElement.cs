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
    public interface HTMLAppletElement : HTMLElement
    {
        string Align { get; set; }
        string Alt { get; set; }
        string Archive { get; set; }
        string Code { get; set; }
        string CodeBase { get; set; }
        string Height { get; set; }
        int HSpace { get; set; }
        string Name { get; set; }
        string Object { get; set; }
        int VSpace { get; set; }
        string Width { get; set; }
    }
}
