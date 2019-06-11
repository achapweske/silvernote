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
    public interface SVGLengthList
    {
        int NumberOfItems { get; }

        void Clear();
        SVGLength Initialize(SVGLength newItem);
        SVGLength GetItem(int index);
        SVGLength InsertItemBefore(SVGLength newItem, int index);
        SVGLength ReplaceItem(SVGLength newItem, int index);
        SVGLength RemoveItem(int index);
        SVGLength AppendItem(SVGLength newItem);
    }
}
