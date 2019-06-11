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
    /// <summary>
    /// http://www.w3.org/TR/2002/WD-DOM-Level-3-Core-20020114/core.html#ID-637646024
    /// </summary>
    public interface Attr : Node
    {
        string Name { get; }
        bool Specified { get; }
        string Value { get; set; }
        Element OwnerElement { get; }
        TypeInfo SchemaTypeInfo { get; }
        bool IsId { get; }
    }
}
