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
    public interface StyleSheet
    {
        Node OwnerNode { get; }
        StyleSheet ParentStyleSheet { get; }
        string Title { get; }
        string Type { get; }
        MediaList Media { get; }
        string HRef { get; }
        bool Disabled { get; set; }
    }
}
