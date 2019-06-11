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
    public interface HTMLSelectElement : HTMLElement
    {
        string Type { get; }
        int SelectedIndex { get; set; }
        string Value { get; set; }
        int Length { get; set; }
        HTMLFormElement Form { get; }
        HTMLOptionsCollection Options { get; }
        bool Disabled { get; set; }
        bool Multiple { get; set; }
        string Name { get; set; }
        int Size { get; set; }
        int TabIndex { get; set; }
        void Add(HTMLElement element, HTMLElement before);
        void Remove(int index);
        void Blur();
        void Focus();
    }
}
