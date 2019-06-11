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
    public interface NameList
    {
        int Length { get; }
        string GetName(int index);
        string GetNamespaceURI(int index);
        bool Contains(string str);
        bool ContainsNS(string namespaceURI, string name);
    }
}
