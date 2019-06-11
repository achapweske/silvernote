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
    public interface TypeInfo
    {
        string TypeName { get; }
        string TypeNamespace { get; }
        bool IsDerivedFrom(string typeNamespaceArg, string typeNameArg, DOMDerivationType derivationMethod);
    }
}
