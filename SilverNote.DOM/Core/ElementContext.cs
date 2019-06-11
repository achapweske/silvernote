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
    public interface ElementContext : Element, NodeContext
    {
        bool HasInternalAttributes();
        bool HasInternalAttribute(string name);
        IList<string> GetInternalAttributes();
        string GetInternalAttribute(string name);
        void SetInternalAttribute(string name, string value);
        void RemoveInternalAttribute(string name);
    }
}
