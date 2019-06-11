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
    public interface SVGElementInstance : EventTarget
    {
        SVGElement CorrespondingElement { get; }
        SVGUseElement CorrespondingUseElement { get; }
        SVGElementInstance ParentNode { get; }
        SVGElementInstanceList ChildNodes { get; }
        SVGElementInstance FirstChild { get; }
        SVGElementInstance LastChild { get; }
        SVGElementInstance PreviousSibling { get; }
        SVGElementInstance NextSibling { get; }
    }
}
