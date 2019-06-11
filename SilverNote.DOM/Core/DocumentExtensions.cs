/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Internal;

namespace DOM
{
    public static class DocumentExtensions
    {
        public static string ResolveURL(this Document document, string url)
        {
            if (document is DOM.HTML.Internal.HTMLDocumentImpl)
            {
                return ((DOM.HTML.Internal.HTMLDocumentImpl)document).ResolveURL(url);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static Node GetNode(this Document owner, INodeSource source)
        {
            if (owner is DocumentBase)
            {
                return ((DocumentBase)owner).GetNode(source);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
