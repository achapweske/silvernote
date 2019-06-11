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
    /// http://www.w3.org/TR/2003/REC-DOM-Level-2-HTML-20030109/html.html#ID-26809268
    /// </summary>
    public interface HTMLDocument : Document
    {
        string Title { get; set; }
        string Referrer { get; }
        string Domain { get; }
        string URL { get; }
        HTMLElement Body { get; set; }
        HTMLCollection Images { get; }
        HTMLCollection Applets { get; }
        HTMLCollection Links { get; }
        HTMLCollection Forms { get; }
        HTMLCollection Anchors { get; }
        string Cookie { get; set; }

        void Open();
        void Close();
        void Write(string text);
        void WriteLn(string text);
        NodeList GetElementsByName(string elementName);
    }
}
