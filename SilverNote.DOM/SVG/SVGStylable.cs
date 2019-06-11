/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS;

namespace DOM.SVG
{
    public interface SVGStylable
    {
        SVGAnimatedString ClassName { get; }
        CSS3StyleDeclaration Style { get; }
        CSSValue GetPresentationAttribute(string name);
    }
}
