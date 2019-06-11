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
    public interface DocumentLS
    {
        bool Async { get; set; }
        void Abort();
        bool Load(string uri);
        bool LoadXML(string source);
        string SaveXML(Node node);
    }
}
