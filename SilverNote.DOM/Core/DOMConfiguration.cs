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
    public interface DOMConfiguration
    {
        void SetParameter(string name, object value);
        object GetParameter(string name);
        bool CanSetParameter(string name, object value);
        DOMStringList ParameterNames { get; }
    }
}
