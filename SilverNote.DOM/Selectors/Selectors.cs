/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM
{
    /// <summary>
    /// http://www.w3.org/TR/selectors-api/
    /// </summary>
    public partial interface Element
    {
        Element QuerySelector(string selectors);
        NodeList QuerySelectorAll(string selectors);
    }

    public partial interface Document
    {
        Element QuerySelector(string selectors);
        NodeList QuerySelectorAll(string selectors);
    }

    public partial interface DocumentFragment
    {
        Element QuerySelector(string selectors);
        NodeList QuerySelectorAll(string selectors);
    }
}
