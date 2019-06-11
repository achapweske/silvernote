/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Windows;

namespace DOM.SVG
{
    public static class WindowExtensions
    {
        public static SVGDocument SVGDocument(this DOM.Windows.Window window)
        {
            return window.Document as SVGDocument;
        }
    }
}
