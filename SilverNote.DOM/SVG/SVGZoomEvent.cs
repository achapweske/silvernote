/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Events;

namespace DOM.SVG
{
    public interface SVGZoomEvent : UIEvent
    {
        SVGRect ZoomRectScreen { get; }
        double PreviousScale { get; }
        SVGPoint PreviousTranslate { get; }
        double NewScale { get; }
        SVGPoint NewTranslate { get; }
    }
}
