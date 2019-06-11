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
    public interface SVGMarkerElement : SVGElement, SVGLangSpace, SVGExternalResourcesRequired, SVGStylable, SVGFitToViewBox
    {
        SVGAnimatedLength RefX { get; }
        SVGAnimatedLength RefY { get; }
        SVGAnimatedEnumeration MarkerUnits { get; }
        SVGAnimatedLength MarkerWidth { get; }
        SVGAnimatedLength MarkerHeight { get; }
        SVGAnimatedEnumeration OrientType { get; }
        SVGAnimatedAngle OrientAngle { get; }

        void SetOrientToAuto();
        void SetOrientToAngle(SVGAngle angle);
    }
}
