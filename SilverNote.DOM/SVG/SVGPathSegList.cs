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
    public interface ISVGPathSegList
    {
        int NumberOfItems { get; }

        void Clear();
        SVGPathSeg Initialize(SVGPathSeg newItem);
        SVGPathSeg GetItem(int index);
        SVGPathSeg InsertItemBefore(SVGPathSeg newItem, int index);
        SVGPathSeg ReplaceItem(SVGPathSeg newItem, int index);
        SVGPathSeg RemoveItem(int index);
        SVGPathSeg AppendItem(SVGPathSeg newItem);
    }
}
