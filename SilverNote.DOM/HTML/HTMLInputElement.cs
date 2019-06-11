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
    public interface HTMLInputElement : HTMLElement
    {
        string DefaultValue { get; set; }
        bool DefaultChecked { get; set; }
        HTMLFormElement Form { get; }
        string Accept { get; set; }
        string AccessKey { get; set; }
        string Align { get; set; }
        string Alt { get; set; }
        bool Checked { get; set; }
        bool Disabled { get; set; }
        int MaxLength { get; set; }
        string Name { get; set; }
        bool ReadOnly { get; set; }
        int Size { get; set; }
        string Src { get; set; }
        int TabIndex { get; set; }
        string Type { get; set; }
        string UseMap { get; set; }
        string Value { get; set; }
        void Blur();
        void Focus();
        void Select();
        void Click();
    }
}
