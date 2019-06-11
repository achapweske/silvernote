/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Views
{
    public static class DocumentExtensions
    {
        public static AbstractView DefaultView(this Document document)
        {
            if (document is DocumentView)
            {
                return ((DocumentView)document).DefaultView;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
