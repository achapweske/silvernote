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
    public interface Text : CharacterData
    {
        Text SplitText(int offset);
        bool IsElementContentWhitespace { get; }
        string WholeText { get; }
        Text ReplaceWholeText(string content);
    }
}
