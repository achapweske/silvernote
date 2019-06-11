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
    public interface LSParser
    {
        DOMConfiguration Config { get; }
        LSParserFilter Filter { get; set; }
        bool Async { get; }
        bool Busy { get; }
        Document Parse(LSInput input);
        Document ParseURI(string uri);
        Node ParseWithContext(LSInput input, NodeContext context, ActionTypes action);
        void Abort();
    }
}
