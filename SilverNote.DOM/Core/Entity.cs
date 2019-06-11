/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM
{
    public interface Entity : Node
    {
        string PublicId { get; }
        string SystemId { get; }
        string NotationName { get; }
        string InputEncoding { get; }
        string XmlEncoding { get; }
        string XmLVersion { get; }
    }
}
