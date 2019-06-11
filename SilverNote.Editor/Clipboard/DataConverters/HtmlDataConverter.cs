/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace SilverNote.Editor
{
    public class HtmlDataConverter : IDataConverter
    {
        public string Format
        {
            get { return DataFormats.Html; }
        }

        public void SetData(IDataObject obj, IList<object> items)
        {
            var panel = new DocumentPanel();
            foreach (var item in items.OfType<UIElement>())
            {
                panel.Append(item);
            }

            try
            {
                panel.OwnerDocument = DOMFactory.CreateHTMLDocument();
                panel.OwnerDocument.SetUserStyleSheet(CSSParser.ParseStylesheet(NoteEditor.DEFAULT_STYLESHEET));
                panel.SetNodeName(HTMLElements.BODY);
                panel.OwnerDocument.Body.Bind(panel);
                panel.OwnerDocument.Body.UpdateStyle(deep: true);
                var clone = (HTMLDocument)panel.OwnerDocument.CloneNode(true);
                HTMLFilters.HtmlFormatFilter(clone);

                string html = clone.ToString();

                foreach (string fileName in panel.ResourceNames)
                {
                    var fileData = panel.GetResourceData(fileName);

                    string tempFilePath = Path.GetTempFileName();
                    tempFilePath += Path.GetExtension(fileName);

                    File.WriteAllBytes(tempFilePath, fileData);
                    html = html.Replace(fileName, tempFilePath);
                }

                string tempHtmlPath = Path.GetTempFileName();
                tempHtmlPath += ".html";
                File.WriteAllText(tempHtmlPath, html, Encoding.UTF8);

                var data = new NHtmlData();
                data.SourceURL = "file:///" + tempHtmlPath.Replace("\\", "/");
                data.Html = html;

                obj.SetData(DataFormats.Html, data.ToString());
            }
            finally
            {
                panel.RemoveAll();
            }
        }

        public IList<object> GetData(IDataObject obj)
        {
            var html = new NHtmlData(obj);

            var panel = new DocumentPanel();
            panel.SetNodeName(HTMLElements.BODY);
            panel.OwnerDocument = DOMFactory.CreateHTMLDocument();
            panel.OwnerDocument.Body.Bind(panel);
            panel.OwnerDocument.Open();
            panel.OwnerDocument.Write(html.Html);
            panel.OwnerDocument.Close();
            panel.OwnerDocument = HtmlParseFilter(panel.OwnerDocument);
            panel.OwnerDocument.DocumentURI = html.SourceURL;
            panel.RemoveAll();
            panel.OwnerDocument.Body.Bind(panel, render: true);

            var results = panel.Children.OfType<object>().ToArray();

            panel.RemoveAll();

            return results;
        }

        #region HtmlParseFilter

        public static HTMLDocument HtmlParseFilter(HTMLDocument document)
        {
            var result = DOMFactory.CreateHTMLDocument();

            if (document.Body != null)
            {
                HtmlParseNode(document.Body, result, result.Body);
            }
            else
            {
                HtmlParseNode(document.DocumentElement, result, result.Body);
            }

            return result;
        }

        /// <summary>
        /// Copy a node to the given document
        /// </summary>
        /// <param name="node">The node to be processed</param>
        /// <param name="document">Document to accept the results</param>
        /// <param name="context">Element to accept the results</param>
        /// <returns></returns>
        private static Element HtmlParseNode(Node node, Document document, Element context)
        {
            switch (node.NodeType)
            {
                case NodeType.ELEMENT_NODE:
                    return HtmlParseElement((Element)node, document, context);
                case NodeType.TEXT_NODE:
                    return HtmlParseText((Text)node, document, context);
                default:
                    return context;
            }
        }

        /// <summary>
        /// Copy an element to the given document
        /// </summary>
        /// <returns></returns>
        private static Element HtmlParseElement(Element element, Document document, Element context)
        {
            if (element.NodeName == HTMLElements.NOSCRIPT ||
                element.NodeName == HTMLElements.CAPTION)
            {
                return context;
            }

            var style = element.OwnerDocument.DefaultView().GetComputedStyle(element, "");
            if (style.GetPropertyCSSValue(CSSProperties.Display) == CSSValues.None)
            {
                return context;
            }

            var visibility = style.GetPropertyCSSValue(CSSProperties.Visibility);
            if (visibility == CSSValues.Hidden || visibility == CSSValues.Collapse)
            {
                return context;
            }

            // Block elements: 

            var floatValue = style.GetPropertyCSSValue(CSSProperties.Float);
            if (CSSProperties.IsBlockElement(element) && floatValue == CSSValues.None)
            {
                // Prepend line break if top margin or padding is sufficiently large

                double marginTop = CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.MarginTop));
                double paddingTop = CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.PaddingTop));

                if (marginTop >= 12 || paddingTop >= 12)
                {
                    context = InsertLineBreak(context, true);
                }

                // Update context

                if (context.NodeName == HTMLElements.P)
                {
                    context = (Element)context.ParentNode;
                }
            }

            var display = style.GetPropertyCSSValue(CSSProperties.Display);
            if (display == CSSValues.ListItem && floatValue == CSSValues.None)
            {
                context = HtmlParseListItem(element, document, context);
            }
            else if (display == CSSValues.Table || element.NodeName == HTMLElements.TABLE)
            {
                context = HtmlParseTable(element, document, context);
            }
            else if (display == CSSValues.TableHeaderGroup || display == CSSValues.TableRowGroup || display == CSSValues.TableFooterGroup)
            {
                context = HtmlParseTableRowGroup(element, document, context);
            }
            else if (display == CSSValues.TableRow)
            {
                context = HtmlParseTableRow(element, document, context);
            }
            else if (display == CSSValues.TableCell)
            {
                context = HtmlParseTableCell((HTMLTableCellElement)element, document, context);
            }
            else if (display == CSSValues.TableCaption)
            {
                context = HtmlParseCaption(element, document, context);
            }
            else
            {
                switch (element.NodeName)
                {
                    case HTMLElements.BR:
                        context = HtmlParseBr(element, document, context);
                        break;
                    case HTMLElements.IMG:
                        context = HtmlParseImage((HTMLImageElement)element, document, context);
                        break;
                    case HTMLElements.SPAN:
                        context = HtmlParseSpan(element, document, context);
                        break;
                    default:
                        break;
                }
            }

            // Process children

            for (Node child = element.FirstChild; child != null; child = child.NextSibling)
            {
                context = HtmlParseNode(child, document, context);
            }

            if (CSSProperties.IsBlockElement(element) && floatValue == CSSValues.None)
            {
                // Update context

                if ((context.NodeName == HTMLElements.P || context.NodeName == HTMLElements.LI) && floatValue == CSSValues.None)
                {
                    context = (Element)context.ParentNode;
                }

                if (display == CSSValues.Table || element.NodeName == HTMLElements.TABLE)
                {
                    context = GetTableContainer(context);
                }
                else if (display == CSSValues.TableRow)
                {
                    context = GetTable(context);
                }
                else if (display == CSSValues.TableCell)
                {
                    context = GetTableRow(context);
                }
                else if (display == CSSValues.TableCaption)
                {
                    context = GetTable(context);
                }

                // Insert line break if bottom margin is sufficiently large

                // (note this won't always work since margin and padding can be % values)
                double marginBottom = CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.MarginBottom));
                double paddingBottom = CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.PaddingBottom));

                if (marginBottom >= 12 || paddingBottom >= 12)
                {
                    context = InsertLineBreak(context, true);
                }
            }

            return context;
        }

        /// <summary>
        /// Add a line break to result.
        /// </summary>
        private static Element HtmlParseBr(Element br, Document document, Element context)
        {
            if (context.NodeName == HTMLElements.P)
            {
                context = (Element)context.ParentNode;
            }
            else
            {
                context = InsertLineBreak(context);
            }

            return context;
        }

        /// <summary>
        /// Append a line break to the given element.
        /// 
        /// If element cannot accept line breaks, then it is added to the 
        /// first ancestor that can.
        /// </summary>
        /// <param name="element">The element to which the line break is to be added.</param>
        /// <param name="simplify">If true, do not add line breaks to elements 
        /// that already end with a line break.</param>
        /// <returns>The element to which the line break was actually added</returns>
        private static Element InsertLineBreak(Element element, bool simplify = false)
        {
            element = GetParagraphContainer(element);

            if (simplify &&
                element.LastChild != null &&
                element.LastChild.NodeType == NodeType.ELEMENT_NODE &&
                element.LastChild.NodeName == HTMLElements.BR)
            {
                return element;
            }

            Element br = element.OwnerDocument.CreateElement(HTMLElements.BR);
            element.AppendChild(br);

            return element;
        }

        /// <summary>
        /// Add the given caption to result
        /// </summary>
        private static Element HtmlParseCaption(Element caption, Document document, Element context)
        {
            Element table = GetTable(context);
            if (table == null)
            {
                return context;
            }

            var newCaption = document.CreateElement(HTMLElements.CAPTION);
            newCaption.InnerText = caption.InnerText;
            table.AppendChild(newCaption);

            return newCaption;
        }

        /// <summary>
        /// Copy an image to the given document
        /// </summary>
        /// <returns>The newly-inserted image</returns>
        private static Element HtmlParseImage(HTMLImageElement img, Document document, Element context)
        {
            Element container = GetElementContainer(context);

            var newImg = (HTMLImageElement)document.CreateElement(HTMLElements.IMG);
            newImg.Src = img.OwnerDocument.ResolveURL(img.Src);
            newImg.Width = img.Width;
            newImg.Height = img.Height;
            container.AppendChild(newImg);

            return newImg;
        }

        /// <summary>
        /// Add the given list item to result
        /// </summary>
        /// <returns>The newly-inserted list item</returns>
        private static Element HtmlParseListItem(Element element, Document document, Element context)
        {
            var style = (CSS3StyleDeclaration)element.OwnerDocument.DefaultView().GetComputedStyle(element, "");
            if (style.Display != "list-item")
            {
                return context;
            }

            Element container = GetParagraphContainer(context);

            var li = (HTMLElement)context.OwnerDocument.CreateElement(HTMLElements.LI);
            int listLevel = GetListLevel(element);
            li.Style.SetProperty("list-level", CSSValues.Number(listLevel).CssText, String.Empty);
            li.Style.ListStyleType = style.ListStyleType;
            double indent = Math.Max(listLevel * 30 - 30, 0);
            li.Style.SetProperty(CSSProperties.MarginLeft, CSSValues.Pixels(indent).CssText, String.Empty);
            container.AppendChild(li);

            return li;
        }

        private static int GetListLevel(Element element)
        {
            int result = 0;

            if (element.TagName == HTMLElements.OL ||
                element.TagName == HTMLElements.UL)
            {
                result = 1;
            }

            Element parent = element.ParentNode as Element;

            if (parent != null)
            {
                return result + GetListLevel(parent);
            }
            else
            {
                return result;
            }
        }

        private static Element HtmlParseSpan(Element span, Document document, Element context)
        {
            // Microsoft applications indicate tab count for a paragraph by 
            // prepending a span to the paragraph, where the span contains
            // a "mso-tab-count" property indicating the indentation level
            // and the contents of that span is an equivalent number of
            // &nbsp's. 

            var computedStyle = (CSS3StyleDeclaration)span.OwnerDocument.DefaultView().GetComputedStyle(span, "");
            var tabCountProperty = computedStyle.GetPropertyCSSValue("mso-tab-count") as CSSPrimitiveValue;
            if (tabCountProperty != null && tabCountProperty.PrimitiveType == CSSPrimitiveType.CSS_NUMBER)
            {
                double tabCount = tabCountProperty.GetFloatValue(CSSPrimitiveType.CSS_NUMBER);
                context = GetSpanContainer(span, document, context);
                ((HTMLElement)context).Style.MarginLeft = CSSValues.Pixels(tabCount * 25).CssText;
            }

            return context;
        }

        /// <summary>
        /// Add the given table to result
        /// </summary>
        /// <returns>The newly-inserted table</returns>
        private static Element HtmlParseTable(Element table, Document document, Element context)
        {
            Element container = GetTableContainer(context);

            var newTable = (HTMLTableElement)document.CreateElement(HTMLElements.TABLE);

            var computedStyle = (CSS3StyleDeclaration)table.OwnerDocument.DefaultView().GetComputedStyle(table, "");
            newTable.Style.MarginLeft = "20px";
            newTable.Style.BackgroundColor = computedStyle.BackgroundColor;
            newTable.Style.BorderTopStyle = computedStyle.BorderTopStyle;
            newTable.Style.BorderTopColor = computedStyle.BorderTopColor;
            newTable.Style.BorderTopWidth = computedStyle.BorderTopWidth;
            newTable.Style.BorderRightStyle = computedStyle.BorderRightStyle;
            newTable.Style.BorderRightColor = computedStyle.BorderRightColor;
            newTable.Style.BorderRightWidth = computedStyle.BorderRightWidth;
            newTable.Style.BorderBottomStyle = computedStyle.BorderBottomStyle;
            newTable.Style.BorderBottomColor = computedStyle.BorderBottomColor;
            newTable.Style.BorderBottomWidth = computedStyle.BorderBottomWidth;
            newTable.Style.BorderLeftStyle = computedStyle.BorderLeftStyle;
            newTable.Style.BorderLeftColor = computedStyle.BorderLeftColor;
            newTable.Style.BorderLeftWidth = computedStyle.BorderLeftWidth;
            newTable.Style.BorderTopLeftRadius = computedStyle.BorderTopLeftRadius;
            newTable.Style.BorderTopRightRadius = computedStyle.BorderTopRightRadius;
            newTable.Style.BorderBottomRightRadius = computedStyle.BorderBottomRightRadius;
            newTable.Style.BorderBottomLeftRadius = computedStyle.BorderBottomLeftRadius;
            newTable.Style.Width = computedStyle.Width;

            container.AppendChild(newTable);

            return newTable;
        }

        /// <summary>
        /// Add the given table row group to result
        /// </summary>
        /// <returns>The newly-inserted row</returns>
        private static Element HtmlParseTableRowGroup(Element group, Document document, Element context)
        {
            Element table = GetTable(context);
            if (table == null)
            {
                return context;
            }

            var newGroup = document.CreateElement(group.NodeName);
            table.AppendChild(newGroup);

            return newGroup;
        }

        /// <summary>
        /// Add the given table row to result
        /// </summary>
        /// <returns>The newly-inserted row</returns>
        private static Element HtmlParseTableRow(Element row, Document document, Element context)
        {
            Element container;
            switch (context.NodeName)
            {
                case HTMLElements.THEAD:
                case HTMLElements.TFOOT:
                case HTMLElements.TBODY:
                    container = context;
                    break;
                default:
                    container = GetTable(context);
                    break;
            }

            if (container == null)
            {
                return context;
            }

            var newRow = (HTMLTableRowElement)document.CreateElement(HTMLElements.TR);
            var computedStyle = (CSS3StyleDeclaration)row.OwnerDocument.DefaultView().GetComputedStyle(row, "");
            newRow.Style.BackgroundColor = computedStyle.BackgroundColor;
            container.AppendChild(newRow);

            return newRow;
        }

        /// <summary>
        /// Add the given table cell to result
        /// </summary>
        /// <returns>The newly-inserted cell</returns>
        private static Element HtmlParseTableCell(HTMLTableCellElement cell, Document document, Element context)
        {
            Element tr = GetTableRow(context);
            if (tr == null)
            {
                return context;
            }

            var newCell = (HTMLTableCellElement)document.CreateElement(cell.TagName);
            newCell.RowSpan = cell.RowSpan;
            newCell.ColSpan = cell.ColSpan;
            var computedStyle = (CSS3StyleDeclaration)cell.OwnerDocument.DefaultView().GetComputedStyle(cell, "");
            newCell.Style.BackgroundColor = GetTableCellBackgroundColor(cell);
            newCell.Style.BorderTopStyle = computedStyle.BorderTopStyle;
            newCell.Style.BorderTopColor = computedStyle.BorderTopColor;
            newCell.Style.BorderTopWidth = computedStyle.BorderTopWidth;
            newCell.Style.BorderRightStyle = computedStyle.BorderRightStyle;
            newCell.Style.BorderRightColor = computedStyle.BorderRightColor;
            newCell.Style.BorderRightWidth = computedStyle.BorderRightWidth;
            newCell.Style.BorderBottomStyle = computedStyle.BorderBottomStyle;
            newCell.Style.BorderBottomColor = computedStyle.BorderBottomColor;
            newCell.Style.BorderBottomWidth = computedStyle.BorderBottomWidth;
            newCell.Style.BorderLeftStyle = computedStyle.BorderLeftStyle;
            newCell.Style.BorderLeftColor = computedStyle.BorderLeftColor;
            newCell.Style.BorderLeftWidth = computedStyle.BorderLeftWidth;
            newCell.Style.BorderTopLeftRadius = computedStyle.BorderTopLeftRadius;
            newCell.Style.BorderTopRightRadius = computedStyle.BorderTopRightRadius;
            newCell.Style.BorderBottomRightRadius = computedStyle.BorderBottomRightRadius;
            newCell.Style.BorderBottomLeftRadius = computedStyle.BorderBottomLeftRadius;
            newCell.Style.PaddingTop = computedStyle.PaddingTop;
            newCell.Style.PaddingRight = computedStyle.PaddingRight;
            newCell.Style.PaddingBottom = computedStyle.PaddingBottom;
            newCell.Style.PaddingLeft = computedStyle.PaddingLeft;

            tr.AppendChild(newCell);

            return newCell;
        }

        /// <summary>
        /// Get a table cell's background color.
        /// 
        /// If no color was specified by the author, return the background
        /// color assigned to its row, section, or table.
        /// </summary>
        private static string GetTableCellBackgroundColor(Element cell)
        {
            var style = (CSS3StyleDeclaration)cell.OwnerDocument.DefaultView().GetComputedStyle(cell, "");

            if (style.BackgroundColor != "transparent")
            {
                return style.BackgroundColor;
            }

            return GetTableCellAncestorBackgroundColor(cell);
        }

        private static string GetTableCellAncestorBackgroundColor(Element element)
        {
            switch (element.ParentNode.NodeName)
            {
                case HTMLElements.TABLE:
                case HTMLElements.THEAD:
                case HTMLElements.TBODY:
                case HTMLElements.TFOOT:
                case HTMLElements.TR:
                    break;
                default:
                    return "transparent";
            }

            Element parent = (Element)element.ParentNode;
            var style = (CSS3StyleDeclaration)parent.OwnerDocument.DefaultView().GetComputedStyle(parent, "");

            if (style.BackgroundColor != "transparent")
            {
                return style.BackgroundColor;
            }

            return GetTableCellAncestorBackgroundColor(parent);
        }

        /// <summary>
        /// Add text to result.
        /// 
        /// If element cannot accept text, then text is added to a new
        /// paragraph and that paragraph is added to element. If element
        /// cannot accept a paragraph, then we add it to the first ancestor
        /// that can.
        /// </summary>
        /// <param name="text">The text to be added</param>
        /// <param name="context">The element to which text will be added</param>
        /// <returns>The element to which text was actually added</returns>
        private static Element HtmlParseText(Text text, Document document, Element context)
        {
            switch (context.TagName)
            {
                case HTMLElements.TABLE:
                case HTMLElements.THEAD:
                case HTMLElements.TBODY:
                case HTMLElements.TFOOT:
                case HTMLElements.TR:
                    return context;
            }

            string textContent = text.Data;
            var style = (CSS3StyleDeclaration)DOMFactory.CreateCSSStyleDeclaration();
            string href = null;

            // Compute text's style and hyperlink URL

            var parentElement = text.ParentNode as Element;

            if (parentElement != null)
            {
                var computedStyle = (CSS3StyleDeclaration)parentElement.OwnerDocument.DefaultView().GetComputedStyle(parentElement, "");

                if (computedStyle.GetPropertyCSSValue("mso-tab-count") != null)
                {
                    // Microsoft applications indicate tab count for a paragraph by 
                    // prepending a span to the paragraph, where the span contains
                    // a "mso-tab-count" property indicating the indentation level
                    // and the contents of that span is an equivalent number of
                    // &nbsp's. Since we process the mso-tab-count property, we
                    // should ignore the containing nbsp's.
                    return context;
                }

                if (computedStyle.GetPropertyCSSValue(CSSProperties.WhiteSpace) != CSSValues.Pre)
                {
                    textContent = CollapseWhitespace(textContent);
                    if (String.IsNullOrWhiteSpace(textContent))
                    {
                        return context;
                    }
                }

                if (text == parentElement.FirstChild)
                {
                    double marginLeft = CSSConverter.ToLength(computedStyle.GetPropertyCSSValue(CSSProperties.MarginLeft));
                    int spacingLeft = (int)Math.Max(marginLeft / 4, 0);
                    if (spacingLeft > 0)
                    {
                        textContent = new string(' ', spacingLeft) + textContent;
                    }
                }

                if (text == parentElement.LastChild)
                {
                    double marginRight = CSSConverter.ToLength(computedStyle.GetPropertyCSSValue(CSSProperties.MarginRight));
                    int spacingRight = (int)Math.Max(marginRight / 4, 0);
                    if (spacingRight > 0)
                    {
                        textContent = textContent + new string(' ', spacingRight);
                    }
                }

                style.FontFamily = computedStyle.FontFamily;
                style.FontVariant = computedStyle.FontVariant;
                style.FontSize = computedStyle.FontSize;
                style.FontSizeAdjust = computedStyle.FontSizeAdjust;
                style.FontWeight = computedStyle.FontWeight;
                style.FontStyle = computedStyle.FontStyle;
                style.FontStretch = computedStyle.FontStretch;
                style.TextDecoration = computedStyle.TextDecoration;
                style.Color = computedStyle.Color;

                href = GetHRef(parentElement);
            }

            context = GetSpanContainer(parentElement, document, context);

            // Translate leading tabs to indentation

            /*
            while (textContent.StartsWith("\t"))
            {
                double marginLeft = ((HTMLElement)context).Style.GetMarginLeft();
                ((HTMLElement)context).Style.SetMarginLeft(marginLeft + 25);
                textContent = textContent.Substring(1);
            }
            */

            // Package text into a <span> element

            Element span;
            if (String.IsNullOrEmpty(href))
            {
                span = context.OwnerDocument.CreateElement(HTMLElements.SPAN);
            }
            else
            {
                span = context.OwnerDocument.CreateElement(HTMLElements.A);
                span.SetAttribute(HTMLAttributes.HREF, href);
            }

            span.SetAttribute(HTMLAttributes.STYLE, style.CssText);
            span.TextContent = textContent;
            Debug.Assert(!textContent.Contains("Any Unicode"));
            // Add <span> to result

            // must compensate for not handling whitespace properly in the DOM
            if (!String.IsNullOrWhiteSpace(span.TextContent) || (context.HasChildNodes() && !context.TextContent.EndsWith(" ")))
            {
                context.AppendChild(span);
            }

            return context;
        }

        private static string CollapseWhitespace(string str)
        {
            var result = new StringBuilder();

            int i = 0;
            while (i < str.Length)
            {
                char c = str[i];

                if (!Char.IsWhiteSpace(c))
                {
                    result.Append(c);
                    i++;
                    continue;
                }

                result.Append(' ');

                while (++i < str.Length)
                {
                    if (!Char.IsWhiteSpace(str[i]))
                    {
                        break;
                    }
                }
            }

            return result.ToString();
        }

        private static string GetHRef(Element element)
        {
            string href = element.GetAttribute(HTMLAttributes.HREF);
            if (!String.IsNullOrEmpty(href))
            {
                return href;
            }

            Element parent = element.ParentNode as Element;
            if (parent != null)
            {
                return GetHRef(parent);
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Get an element that can contain other elements.
        /// </summary>
        private static Element GetElementContainer(Element refElement)
        {
            switch (refElement.TagName)
            {
                case HTMLElements.BODY:
                case HTMLElements.TD:
                case HTMLElements.TH:
                    return refElement;
                default:
                    break;
            }

            if (refElement.ParentNode.NodeType == NodeType.DOCUMENT_NODE)
            {
                Element body = refElement.OwnerDocument.CreateElement(HTMLElements.BODY);
                refElement.AppendChild(body);
                return body;
            }

            return GetElementContainer((Element)refElement.ParentNode);
        }

        /// <summary>
        /// Get an element that can contain paragraph elements
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static Element GetParagraphContainer(Element refElement)
        {
            return GetElementContainer(refElement);
        }

        /// <summary>
        /// Get an element that can contain table elements
        /// </summary>
        /// <param name="refElement"></param>
        /// <returns></returns>
        private static Element GetTableContainer(Element refElement)
        {
            if (refElement.TagName == HTMLElements.BODY)
            {
                return refElement;
            }

            if (refElement.ParentNode.NodeType == NodeType.DOCUMENT_NODE)
            {
                Element body = refElement.OwnerDocument.CreateElement(HTMLElements.BODY);
                refElement.AppendChild(body);
                return body;
            }

            return GetTableContainer((Element)refElement.ParentNode);
        }

        /// <summary>
        /// Get the table that contains the given refElement.
        /// 
        /// If refElement is a table, then refElement itself is returned.
        /// </summary>
        private static Element GetTable(Element refElement)
        {
            if (refElement == refElement.OwnerDocument.DocumentElement)
            {
                return null;
            }
            else if (refElement.TagName == HTMLElements.TABLE)
            {
                return refElement;
            }
            else
            {
                return GetTable((Element)refElement.ParentNode);
            }
        }

        /// <summary>
        /// Get the table row that contains the given refElement.
        /// 
        /// If refElement is a table row, then refElement itself is returned.
        /// </summary>
        private static Element GetTableRow(Element refElement)
        {
            if (refElement == refElement.OwnerDocument.DocumentElement)
            {
                return null;
            }
            else if (refElement.TagName == HTMLElements.TR)
            {
                return refElement;
            }
            else
            {
                return GetTableRow((Element)refElement.ParentNode);
            }
        }

        /// <summary>
        /// Get an element that can contain text nodes.
        /// 
        /// If the specified element can contain text nodes, then that element
        /// is returned. If it cannot, then a new paragraph is added to element 
        /// and that paragraph is returned instead. If element cannot accept 
        /// paragraphs, then the paragraph is added to the first ancestor that 
        /// can.
        /// </summary>
        private static Element GetSpanContainer(Element element, Document document, Element context)
        {
            switch (context.TagName)
            {
                // NoteEditor allows text to be added to the following element:
                case HTMLElements.CAPTION:
                case HTMLElements.H1:
                case HTMLElements.H2:
                case HTMLElements.H3:
                case HTMLElements.H4:
                case HTMLElements.H5:
                case HTMLElements.H6:
                case HTMLElements.LI:
                case HTMLElements.P:
                    return context;
                default:
                    break;
            }

            Element container = GetParagraphContainer(context);

            var p = (HTMLParagraphElement)document.CreateElement(HTMLElements.P);
            var computedStyle = (CSS3StyleDeclaration)element.OwnerDocument.DefaultView().GetComputedStyle(element, "");
            p.Style.TextAlign = computedStyle.TextAlign;
            p.Style.LineHeight = computedStyle.LineHeight;
            p.Style.Margin = "0";

            container.AppendChild(p);

            return p;
        }

        #endregion

    }
}
