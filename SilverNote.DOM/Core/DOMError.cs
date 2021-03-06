﻿/*
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
    public interface DOMError
    {
        DOMErrorSeverityType Severity { get; }
        string Message { get; }
        string Type { get; }
        object RelatedException { get; }
        object RelatedData { get; }
        DOMLocator Location { get; }
    }
}
