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
    /// <summary>
    /// Generic embedded object.
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-9893177
    /// </summary>
    public interface HTMLObjectElement : HTMLElement
    {
        HTMLFormElement Form { get; }
        string Code { get; set; }
        string Align { get; set; }
        string Archive { get; set; }
        string Border { get; set; }
        string CodeBase { get; set; }
        string CodeType { get; set; }
        string Data { get; set; }
        bool Declare { get; set; }
        string Height { get; set; }
        int HSpace { get; set; }
        string Name { get; set; }
        string Standby { get; set; }
        int TabIndex { get; set; }
        string Type { get; set; }
        string UseMap { get; set; }
        int VSpace { get; set; }
        string Width { get; set; }
        Document ContentDocument { get; }
    }
}
