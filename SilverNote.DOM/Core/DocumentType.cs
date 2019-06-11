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
    public interface DocumentType : Node
    {
        string Name { get; }
        NamedNodeMap Entities { get; }
        NamedNodeMap Notations { get; }
        string PublicId { get; }
        string SystemId { get; }
        string InternalSubset { get; }
    }
}
