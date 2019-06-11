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
    public interface SVGTransformList
    {
        int NumberOfItems { get; }
        void Clear();
        SVGTransform Initialize(SVGTransform newItem);
        SVGTransform GetItem(int index);
        SVGTransform InsertItemBefore(SVGTransform newItem, int index);
        SVGTransform ReplaceItem(SVGTransform newItem, int index);
        SVGTransform RemoveItem(int index);
        SVGTransform AppendItem(SVGTransform newItem);
        SVGTransform CreateSVGTransformFromMatrix(SVGMatrix matrix);
        SVGTransform Consolidate();
    }
}
