/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.HTML
{
    public static class HTMLFormatter
    {
        public static void FormatNodes(TextWriter writer, IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                FormatNode(writer, node);
            }
        }

        public static void FormatNode(TextWriter writer, Node node)
        {
            switch (node.NodeType)
            {
                case NodeType.ELEMENT_NODE:
                    FormatElement(writer, (Element)node);
                    break;
                case NodeType.ATTRIBUTE_NODE:
                    FormatAttribute(writer, (Attr)node);
                    break;
                case NodeType.TEXT_NODE:
                    FormatText(writer, (Text)node);
                    break;
                case NodeType.COMMENT_NODE:
                    FormatComment(writer, (Comment)node);
                    break;
                default:
                    break;
            }
        }

        public static string FormatElement(Element element)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                FormatElement(writer, element);
                return writer.ToString();
            }
        }

        public static void FormatElement(TextWriter writer, Element element)
        {
            // http://dev.w3.org/html5/markup/syntax.html#syntax-elements

            // Start tag
            writer.Write('<');
            writer.Write(element.NodeName);
            if (element.HasAttributes())
            {
                writer.Write(' ');
                HTMLFormatter.FormatAttributes(writer, element.Attributes.OfType<Attr>());
            }
            if (IsVoidElement(element))
            {
                writer.Write('/');
                writer.Write('>');
                return;
            }
            writer.Write('>');

            // Contents
            FormatNodes(writer, element.ChildNodes);

            // End tag
            writer.Write('<');
            writer.Write('/');
            writer.Write(element.NodeName);
            writer.Write('>');
        }

        /// <summary>
        /// A void element is an element whose content model never allows it to have contents under any circumstances.
        /// 
        /// http://dev.w3.org/html5/markup/syntax.html#void-element
        /// </summary>
        public static bool IsVoidElement(Element element)
        {
            switch (element.NodeName)
            {
                case "area":
                case "base":
                case "br":
                case "col":
                case "command":
                case "embed":
                case "hr":
                case "img":
                case "input":
                case "keygen":
                case "link":
                case "meta":
                case "param":
                case "source":
                case "track":
                case "wbr":
                    return true;
                default:
                    return false;
            }
        }

        public static void FormatAttributes(TextWriter writer, IEnumerable<Attr> attributes)
        {
            var attr = attributes.GetEnumerator();

            if (attr.MoveNext())
            {
                FormatAttribute(writer, attr.Current);

                while (attr.MoveNext())
                {
                    writer.Write(' ');

                    FormatAttribute(writer, attr.Current);
                }
            }
        }

        public static void FormatAttribute(TextWriter writer, Attr attr)
        {
            writer.Write(attr.Name);
            writer.Write('=');
            writer.Write('\"');
            string value = EscapeAttributeValue(attr.Value);
            writer.Write(value);
            writer.Write('\"');
        }

        private static string EscapeAttributeValue(string value)
        {
            return value.Replace("\"", "&quot;");
        }

        public static void FormatText(TextWriter writer, Text text)
        {
            string data = EscapeText(text.Data);
            writer.Write(data);
        }

        public static string EscapeText(string text)
        {
            return text.Replace("<", "&lt;");
        }

        public static void FormatComment(TextWriter writer, Comment comment)
        {
            string data = EscapeComment(comment.Data);
            writer.Write(data);
        }

        public static string EscapeComment(string text)
        {
            return text;    // TODO
        }
    }
}
