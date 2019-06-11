/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Editor
{
    public interface IListStyle
    {
        string Description { get; }
        bool IsOrdered { get; }
        string GetMarker(int index);
        IListStyle NextStyle { get; }
        IListStyle PreviousStyle { get; }
    }
}
