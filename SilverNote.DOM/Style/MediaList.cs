/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Style
{
    public interface MediaList
    {
        string MediaText { get; set; }
        int Length { get; }
        string this[int index] { get; }
        void DeleteMedium(string oldMedium);
        void AppendMedium(string newMedium);
    }
}
