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
    public interface SVGTextContentElement : SVGElement, SVGTests, SVGLangSpace, SVGExternalResourcesRequired, SVGStylable
    {
        SVGAnimatedLength TextLength { get; }
        SVGAnimatedEnumeration LengthAdjust { get; }

        int GetNumberOfChars();
        double GetComputedTextLength();
        double GetSubStringLength(int charnum, int nchars);
        SVGPoint GetStartPositionOfChar(int charnum);
        SVGPoint GetEndPositionOfChar(int charnum);
        SVGRect GetExtentOfChar(int charnum);
        double GetRotationOfChar(int charnum);
        int GetCharNumAtPosition(SVGPoint point);
        void SelectSubString(int charnum, int nchars);
    }
}
