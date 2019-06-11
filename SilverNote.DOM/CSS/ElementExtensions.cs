/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS.Internal;

namespace DOM.CSS
{
    public static class ElementExtensions
    {
        public static void UpdateStyle(this Element element, bool deep)
        {
            if (element is CSSElement)
            {
                ((CSSElement)element).UpdateStyle(deep);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
