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
    public interface HTMLFormElement : HTMLElement
    {
        HTMLCollection Elements { get; }
        int Length { get; }
        string Name { get; set; }
        string AcceptCharset { get; set; }
        string Action { get; set; }
        string EncType { get; set; }
        string Method { get; set; }
        string Target { get; set; }
        void Submit();
        void Reset();
    }
}
