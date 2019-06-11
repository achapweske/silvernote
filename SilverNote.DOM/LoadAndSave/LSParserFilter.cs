/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Traversal;

namespace DOM.LS
{
    public interface LSParserFilter
    {
        FilterResultType StartElement(Element element);
        FilterResultType AcceptNode(Node node);
        FilterType WhatToShow { get; }
    }
}
