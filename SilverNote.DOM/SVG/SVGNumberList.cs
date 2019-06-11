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
    public interface SVGNumberList
    {
        int NumberOfItems { get; }

        void Clear();
        SVGNumber Initialize(SVGNumber newItem);
        SVGNumber GetItem(int index);
        SVGNumber InsertItemBefore(SVGNumber newItem, int index);
        SVGNumber ReplaceItem(SVGNumber newItem, int index);
        SVGNumber RemoveItem(int index);
        SVGNumber AppendItem(SVGNumber newItem);
    }
}
