/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.HTML;
using DOM.XPath;
using SilverNote.Common;
using DOM.LS;
using DOM.SVG;
using DOM;
using System.IO;
using DOM.CSS;

namespace SilverNote.Editor
{
    public static class HTMLFilters
    {
        public static void ExportFilter(HTMLDocument document)
        {
            CleanFormatFilter(document);

            // IE requires a DOCTYPE
            var docType = DOMFactory.CreateDocumentType("html", null, null);
            document.InsertBefore(docType, document.FirstChild);

            // Add a margin around the page
            document.Body.Style.Margin = "6px";

            // Canvases are saved as PNG files for export
            // IE requires all images use the <img> tag
            var canvases = document.QuerySelectorAll(@"object[type='image/svg+xml']");

            foreach (HTMLObjectElement canvas in canvases)
            {
                var img = (HTMLImageElement)document.CreateElement(HTMLElements.IMG);

                img.SetAttribute(HTMLAttributes.WIDTH, canvas.Width);
                img.SetAttribute(HTMLAttributes.HEIGHT, canvas.Height);
                img.Style.CssText = canvas.Style.CssText;
                img.Src = Path.ChangeExtension(canvas.Data, ".png");

                canvas.ParentNode.ReplaceChild(img, canvas);
            }

            // Files should be rendered as a linked PNG
            var files = document.QuerySelectorAll(@"object[type='application/silvernote.file']");

            foreach (HTMLObjectElement file in files)
            {
                var icon = (HTMLImageElement)document.CreateElement(HTMLElements.IMG);
                icon.Src = Path.ChangeExtension(file.Data, ".icon.png");

                var link = (HTMLAnchorElement)document.CreateElement(HTMLElements.A);
                link.Style.CssText = file.Style.CssText;
                link.HRef = file.Data;
                link.AppendChild(icon);

                file.ParentNode.ReplaceChild(link, file);
            }
        }

        public static void CleanFormatFilter(HTMLDocument document)
        {
            HtmlFormatFilter(document);

            // Remove empty <span>s

            var spans = document.Select("//span[not(node())]");
            foreach (var span in spans)
            {
                span.ParentNode.RemoveChild(span);
            }

            // Remove trailing <br>s

            while (document.Body.HasChildNodes() &&
                document.Body.LastChild.NodeName == HTMLElements.BR)
            {
                document.Body.RemoveChild(document.Body.LastChild);
            }
        }

        public static void HtmlFormatFilter(HTMLDocument document)
        {
            // Add a "Created with ..." comment if not already there

            if (document.SelectSingleNode("//comment()[contains(., 'Created with')]") == null)
            {
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string createdWithString = String.Format("Created with SilverNote v{0} (http://www.silver-note.com/)", version);
                var createdWith = document.CreateComment(createdWithString);
                var head = document.QuerySelector("head");
                head.InsertBefore(createdWith, head.FirstChild);
            }

            var overlapped = document.QuerySelectorAll(@"[style*='position:overlapped']");

            foreach (HTMLElement node in overlapped)
            {
                var containerDiv = (HTMLElement)document.CreateElement(HTMLElements.DIV);
                var containerStyle = containerDiv.Style;
                var nodeStyle = node.Style;

                containerStyle.Position = "relative";
                nodeStyle.Position = "absolute";

                string right = nodeStyle.Right;
                if (!String.IsNullOrEmpty(right))
                {
                    containerStyle.Position = "absolute";
                    containerStyle.Right = right;
                    nodeStyle.Right = "";
                }

                containerStyle.MarginLeft = nodeStyle.MarginLeft;
                nodeStyle.MarginLeft = "";
                containerStyle.MarginRight = nodeStyle.MarginRight;
                nodeStyle.MarginRight = "";

                string width = nodeStyle.Width;
                if (String.IsNullOrEmpty(width))
                {
                    width = node.GetAttribute("width");
                    if (!String.IsNullOrEmpty(width) && !width.EndsWith("%"))
                    {
                        width += "px";
                    }
                }
                containerStyle.Width = width;

                node.ParentNode.ReplaceChild(containerDiv, node);
                containerDiv.AppendChild(node);
            }

            UnflattedLists(document.Body);
        }

        public static void HtmlParseFilter(HTMLDocument document)
        {
            // Remove all meaningless whitespace

            var whitespace = document.Select("/html/text()|/html/head/text()");
            if (whitespace != null)
            {
                foreach (Node node in whitespace)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }

            // Remove all <style> elements

            var styleElements = document.QuerySelectorAll("style");

            foreach (var styleElement in styleElements)
            {
                styleElement.ParentNode.RemoveChild(styleElement);
            }

            var overlapped = document.QuerySelectorAll(@"[style*='position:relative'] > [style*='position:absolute'], [style*='position:absolute'] > [style*='position:absolute']");

            foreach (HTMLElement node in overlapped)
            {
                var parent = (HTMLElement)node.ParentNode;

                node.Style.Position = "overlapped";
                node.Style.Right = parent.Style.Right;
                node.Style.MarginLeft = parent.Style.MarginLeft;
                node.Style.MarginRight = parent.Style.MarginRight;

                parent.ParentNode.ReplaceChild(node, parent);
            }

            var images = document.QuerySelectorAll("img");

            foreach (var image in images)
            {
                while (image.ParentNode != document.Body)
                {
                    var parent = image.ParentNode;
                    parent.RemoveChild(image);
                    parent.ParentNode.InsertBefore(image, parent);
                }
            }

            if (document.Body != null)
            {
                FlattenLists(document.Body);
            }
        }

        /// <summary>
        /// Convert flat list items into hierarchical list items.
        /// </summary>
        /// <param name="element"></param>
        public static void UnflattedLists(HTMLElement element)
        {
            // Input:
            //
            // <li style="list-level: 0">List item 1</li>
            // <li style="list-level: 0">List item 2</li>
            // <li style="list-level: 1">List item 3</li>
            // <li style="list-level: 1">List item 4</li>
            // <li style="list-level: 0">List item 5</li>
            // <li style="list-level: 0">List item 6</li>
            //
            // Output:
            //
            // <ul>
            //     <li>List item 1</li>
            //     <li>List item 2
            //         <ul>
            //             <li>List item 3</li>
            //             <li>List item 4</li>
            //         </ul>
            //     </li>
            //     <li>List item 5</li>
            //     <li>List item 6</li>
            // </ul>

            // UL or OL elements
            var lists = new Stack<HTMLElement>();

            foreach (var childNode in element.ChildNodes.OfType<HTMLElement>())
            {
                UnflattedLists(childNode);

                // Reset if we hit a non-LI element
                if (childNode.TagName != HTMLElements.LI)
                {
                    lists.Clear();
                    continue;
                }

                // Get list-level for current element
                int listLevel = (int)CSSConverter.ToFloat(childNode.Style.GetPropertyCSSValue("list-level")) + 1;
                childNode.Style.RemoveProperty(CSSProperties.MarginLeft);
                childNode.Style.RemoveProperty("list-level");

                // Add/remove UL/OL elements as needed such that the top element
                // on the stack is the correct one to move the current child to.
                // In other words, if list-level=N there should be N nested elements
                // on the "lists" stack.

                while (lists.Count < listLevel)
                {
                    var newList = (HTMLElement)element.OwnerDocument.CreateElement(HTMLElements.UL);

                    // The first UL/OL is inserted at the root level
                    if (lists.Count == 0)
                    {
                        element.InsertBefore(newList, childNode);
                    }
                    // If the top UL/OL has children, append to the last child
                    else if (lists.Peek().HasChildNodes())
                    {
                        lists.Peek().LastChild.AppendChild(newList);
                    }
                    // Otherwise just append to the top UL/OL
                    else
                    {
                        lists.Peek().AppendChild(newList);
                    }

                    lists.Push(newList);
                }

                while (lists.Count > listLevel)
                {
                    lists.Pop();
                }

                childNode.ParentNode.RemoveChild(childNode);
                lists.Peek().AppendChild(childNode);
            }
        }

        public static void FlattenLists(HTMLElement element)
        {
            foreach (var childNode in element.ChildNodes.OfType<HTMLElement>())
            {
                if (childNode.TagName != HTMLElements.UL &&
                    childNode.TagName != HTMLElements.OL)
                {
                    FlattenLists(childNode);
                    continue;
                }

                var listItems = FlattenList(childNode);

                foreach (var listItem in listItems)
                {
                    childNode.ParentNode.InsertBefore(listItem, childNode);
                }

                childNode.ParentNode.RemoveChild(childNode);
            }
        }

        static IEnumerable<HTMLElement> FlattenList(HTMLElement list)
        {
            var listItems = new List<HTMLElement>();
            FlattenList(list, 0, listItems);
            return listItems;
        }

        static void FlattenList(HTMLElement list, int level, IList<HTMLElement> results)
        {
            foreach (var childNode in list.ChildNodes.OfType<HTMLElement>())
            {
                if (childNode.TagName == HTMLElements.LI)
                {
                    childNode.ParentNode.RemoveChild(childNode);
                    childNode.Style.SetProperty("list-level", CSSValues.Number(level).CssText, String.Empty);
                    childNode.Style.SetProperty(CSSProperties.MarginLeft, CSSValues.Pixels(level * 25).CssText, String.Empty);
                    results.Add(childNode);
                }

                if (childNode.TagName == HTMLElements.OL ||
                    childNode.TagName == HTMLElements.UL)
                {
                    childNode.ParentNode.RemoveChild(childNode);
                    FlattenList(childNode, level + 1, results);
                }
                else
                {
                    FlattenList(childNode, level, results);
                }
            }
        }
    }

    public delegate void HTMLFilterDelegate(HTMLDocument document);

}
