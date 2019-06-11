/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.Views;
using SilverNote.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FileFormats.PlainText
{
    /// <summary>
    /// Convert an HTML document to a text document
    /// </summary>
    public static class TextDocument
    {
        public static string FromHTML(HTMLDocument htmlDocument)
        {
            HTMLFilters.FlattenLists(htmlDocument.Body);

            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                Convert(htmlDocument.Body, writer);
                return writer.ToString();
            }
        }

        private static void Convert(HTMLElement element, TextWriter writer)
        {
            var style = element.OwnerDocument.DefaultView().GetComputedStyle(element, "");

            if (element.TagName == HTMLElements.H1)
            {
                writer.WriteLine(element.TextContent);
                writer.WriteLine();
                return;
            }
            else if (element.TagName == HTMLElements.H2)
            {
                writer.WriteLine();
                string h2 = element.TextContent;
                writer.WriteLine(h2);
                writer.WriteLine(new string('=', h2.Length));
                return;
            }
            else if (element.TagName == HTMLElements.H3)
            {
                writer.WriteLine();
                string h3 = element.TextContent;
                writer.WriteLine(h3);
                writer.WriteLine(new string('-', h3.Length));
                return;
            }
            else if (element.TagName == HTMLElements.LI)
            {
                int listLevel = (int)CSSConverter.ToFloat(style.GetPropertyCSSValue("list-level"));
                writer.Write(new string(' ', listLevel * 4));
                writer.Write("* ");
            }
            else if (element.TagName == HTMLElements.BR)
            {
                writer.WriteLine();
                return;
            }
            else if (element.TagName == HTMLElements.A)
            {
                string text = element.TextContent;
                if (!String.IsNullOrEmpty(text))
                {
                    writer.Write("[{0}]({1})", text, ((HTMLAnchorElement)element).HRef);
                }
                return;
            }

            for (var child = element.FirstChild; child != null; child = child.NextSibling)
            {
                if (child is HTMLElement)
                {
                    Convert((HTMLElement)child, writer);
                }
                else if (child is Text)
                {
                    writer.Write(child.NodeValue);
                }
            }

            if (element.TagName == HTMLElements.P ||
                element.TagName == HTMLElements.LI ||
                element.TagName == HTMLElements.PRE)
            {
                writer.WriteLine();
                return;
            }
        }
    }
}
