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
    public interface SVGViewSpec : SVGZoomAndPan, SVGFitToViewBox
    {
        SVGTransformList Transform { get; }
        SVGElement ViewTarget { get; }
        string ViewBoxString { get; }
        string PreserveAspectRatioString { get; }
        string TransformString { get; }
        string ViewTargetString { get; }
    }
}
