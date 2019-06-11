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
    public interface HTMLParamElement : HTMLElement
    {
        string Name { get; set; }
        string Type { get; set; }
        string Value { get; set; }
        string ValueType { get; set; }
    }
}
