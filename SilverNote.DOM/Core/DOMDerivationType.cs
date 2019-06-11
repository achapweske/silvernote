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
    public enum DOMDerivationType : int
    {
        DERIVATION_RESTRICTION = 0x00000001,
        DERIVATION_EXTENSION = 0x00000002,
        DERIVATION_UNION = 0x00000004,
        DERIVATION_LIST = 0x00000008
    }
}
