/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.SVG
{
    public interface SVGStringList
    {
        int NumberOfItems { get; }

        void Clear();
        string Initialize(string newItem);
        string GetItem(int index);
        string InsertItemBefore(string newItem, int index);
        string ReplaceItem(string newItem, int index);
        string RemoveItem(int index);
        string AppendItem(string newItem);
    }
}
