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
using DOM.Views;

namespace DOM.Helpers
{
    public static class WindowExtensions
    {
        public static Query Query(this Window window, string selector)
        {
            return Query(window, selector, window.Document);
        }

        public static Query Query(this Window window, Document document)
        {
            return new Query(document);
        }

        public static Query Query(this Window window, string selector, Document document)
        {
            return new Query(selector, document);
        }

        public static Query Query(this Window window, Element element)
        {
            return new Query(element);
        }

        public static Query Query(this Window window, string selector, ElementContext context)
        {
            return new Query(selector, context);
        }

        public static Query Query(this Window window, DocumentView documentView)
        {
            var document = documentView as Document;
            if (document != null)
            {
                return Query(window, document);
            }
            else
            {
                return Helpers.Query.Empty;
            }
        }

        public static Query Query(this Window window, string selector, DocumentView documentView)
        {
            var document = documentView as Document;
            if (document != null)
            {
                return Query(window, selector, document);
            }
            else
            {
                return Helpers.Query.Empty;
            }
        }
    }
}
