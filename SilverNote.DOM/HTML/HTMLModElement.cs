﻿/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.HTML
{
    public interface HTMLModElement : HTMLElement
    {
        string Cite { get; set; }
        string DateTime { get; set; }
    }
}
