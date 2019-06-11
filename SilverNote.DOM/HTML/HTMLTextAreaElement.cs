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
    public interface HTMLTextAreaElement : HTMLElement
    {
        string DefaultValue { get; set; }
        HTMLFormElement Form { get; }
        string AccessKey { get; set; }
        int Cols { get; set; }
        bool Disabled { get; set; }
        string Name { get; set; }
        bool ReadOnly { get; set; }
        int Rows { get; set; }
        int TabIndex { get; set; }
        string Type { get; }
        string Value { get; set; }
        void Blur();
        void Focus();
        void Select();
    }
}
