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
    public interface DOMImplementationLS
    {
        LSParser CreateLSParser(DOMImplementationLSMode mode, string schemaType);
        LSSerializer CreateLSSerializer();
        LSInput CreateLSInput();
        LSOutput CreateLSOutput();
    }
}
