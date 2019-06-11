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
    public enum ActionTypes : ushort
    {
        ACTION_APPEND_AS_CHILDREN      = 1,
        ACTION_REPLACE_CHILDREN        = 2,
        ACTION_INSERT_BEFORE           = 3,
        ACTION_INSERT_AFTER            = 4,
        ACTION_REPLACE                 = 5
    }
}
