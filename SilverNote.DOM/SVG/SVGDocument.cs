/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Events;

namespace DOM.SVG
{
    public interface SVGDocument : Document, DocumentEvent
    {
        string Title { get; }
        string Referrer { get; }
        string Domain { get; }
        string URL { get; }
        SVGSVGElement RootElement { get; }
    }
}
