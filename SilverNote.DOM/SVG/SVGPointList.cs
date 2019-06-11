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
    public interface SVGPointList
    {
        int NumberOfItems { get; }

        void Clear();
        SVGPoint Initialize(SVGPoint newItem);
        SVGPoint GetItem(int index);
        SVGPoint InsertItemBefore(SVGPoint newItem, int index);
        SVGPoint ReplaceItem(SVGPoint newItem, int index);
        SVGPoint RemoveItem(int index);
        SVGPoint AppendItem(SVGPoint newItem);
    }
}
