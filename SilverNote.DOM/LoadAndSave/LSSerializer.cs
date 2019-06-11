/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.LS
{
    public interface LSSerializer
    {
        DOMConfiguration Config { get; }
        string NewLine { get; set; }
        LSSerializerFilter Filter { get; set; }
        bool Write(Node node, LSOutput destination);
        bool WriteURI(Node node, string uri);
        string WriteToString(Node node);
    }
}
