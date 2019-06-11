/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.CSS
{
    public interface CSS3Properties : CSS2Properties
    {
        string BorderRadius { get; set; }
        string BorderBottomLeftRadius { get; set; }
        string BorderBottomRightRadius { get; set; }
        string BorderTopLeftRadius { get; set; }
        string BorderTopRightRadius { get; set; }
    }
}
