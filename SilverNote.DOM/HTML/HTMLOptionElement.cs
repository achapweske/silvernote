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
    public interface HTMLOptionElement : HTMLElement
    {
        HTMLFormElement Form { get; }
        bool DefaultSelected { get; set; }
        string Text { get; }
        int Index { get; }
        bool Disabled { get; set; }
        string Label { get; set; }
        bool Selected { get; set; }
        string Value { get; set; }
    }
}
