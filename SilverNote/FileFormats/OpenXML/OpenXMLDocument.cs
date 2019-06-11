/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.CSS;
using DOM.Helpers;
using DOM.HTML;
using DOM.Views;
using SilverNote;
using SilverNote.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml;

namespace FileFormats.OpenXML
{
    /// <summary>
    /// Convert an HTML document to an OpenXML document
    /// </summary>
    public class OpenXMLDocument
    {
        private static class Namespaces
        {
            public const string W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            public const string WP = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing";
            public const string A = "http://schemas.openxmlformats.org/drawingml/2006/main";
            public const string PIC = "http://schemas.openxmlformats.org/drawingml/2006/picture";
            public const string R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        }

        private class ConverterState
        {
            public int ImgID = 1;
            public bool MergeParagraph = false;
        }

        public static void Save(string fileName, HTMLDocument htmlDocument, Dictionary<string, byte[]> media)
        {
            var wordDocument = new XmlDocument();
            var document = wordDocument.CreateElement("w:document", Namespaces.W);
            document.SetAttribute("xmlns:r", Namespaces.R);
            document.SetAttribute("xmlns:wp", Namespaces.WP);
            wordDocument.AppendChild(document);

            SilverNote.Editor.HTMLFilters.FlattenLists(htmlDocument.Body);
            var state = new ConverterState();
            Convert(htmlDocument.Body, document, state);

            var styles = GenerateStyles(htmlDocument);
            var numbering = GenerateNumbering(htmlDocument);

            Save(fileName, wordDocument, styles, numbering, media);
        }

        private static void Convert(HTMLElement element, XmlElement context, ConverterState state)
        {
            if (element.TagName == HTMLElements.BODY)
            {
                var body = context.OwnerDocument.CreateElement("w:body", Namespaces.W);
                context.AppendChild(body);
                context = body;
            }
            else if (element.TagName == HTMLElements.H1 || element.TagName == HTMLElements.H2 || 
                     element.TagName == HTMLElements.H3 || element.TagName == HTMLElements.H4 ||
                     element.TagName == HTMLElements.H5 || element.TagName == HTMLElements.H6)
            {
                var p = context.OwnerDocument.CreateElement("w:p", Namespaces.W);
                context.AppendChild(p);
                var pPr = context.OwnerDocument.CreateElement("w:pPr", Namespaces.W);
                p.AppendChild(pPr);
                var pStyle = context.OwnerDocument.CreateElement("w:pStyle", Namespaces.W);
                pPr.AppendChild(pStyle);
                pStyle.SetAttribute("val", Namespaces.W, "Heading" + element.TagName.Substring(1));
                context = p;
            }
            else if (element.TagName == HTMLElements.P || element.TagName == HTMLElements.BR || element.TagName == HTMLElements.PRE)
            {
                var p = context.LastChild as XmlElement;
                if (p == null || p.Name != "w:p" || !state.MergeParagraph)
                {
                    p = context.OwnerDocument.CreateElement("w:p", Namespaces.W);
                    context.AppendChild(p);
                }
                state.MergeParagraph = false;
                if (element.TagName == HTMLElements.PRE)
                {
                    var pPr = context.OwnerDocument.CreateElement("w:pPr", Namespaces.W);
                    p.AppendChild(pPr);
                    var pStyle = context.OwnerDocument.CreateElement("w:pStyle", Namespaces.W);
                    pPr.AppendChild(pStyle);
                    pStyle.SetAttribute("val", Namespaces.W, "SourceCode");
                }
                context = p;
            }
            else if (element.TagName == HTMLElements.LI)
            {
                var p = context.OwnerDocument.CreateElement("w:p", Namespaces.W);
                context.AppendChild(p);
                var pPr = context.OwnerDocument.CreateElement("w:pPr", Namespaces.W);
                p.AppendChild(pPr);
                var pStyle = context.OwnerDocument.CreateElement("w:pStyle", Namespaces.W);
                pPr.AppendChild(pStyle);
                pStyle.SetAttribute("val", Namespaces.W, "ListParagraph");
                var numPr = context.OwnerDocument.CreateElement("w:numPr", Namespaces.W);
                pPr.AppendChild(numPr);
                var ilvl = context.OwnerDocument.CreateElement("w:ilvl", Namespaces.W);
                numPr.AppendChild(ilvl);
                string listLevel = element.Style.GetPropertyValue("list-level");
                ilvl.SetAttribute("val", Namespaces.W, listLevel);
                var numId = context.OwnerDocument.CreateElement("w:numId", Namespaces.W);
                numPr.AppendChild(numId);
                var listStyle = element.Style.GetPropertyCSSValue(CSSProperties.ListStyleType);
                if (CSSValues.NumericListStyleTypes.Contains(listStyle))
                {
                    numId.SetAttribute("val", Namespaces.W, "2");
                }
                else
                {
                    numId.SetAttribute("val", Namespaces.W, "1");
                }
                context = p;
            }
            else if (element.TagName == HTMLElements.SPAN || 
                    element.TagName == HTMLElements.A && !element.ChildNodes.Any(child => child.NodeName == HTMLElements.IMG) ||
                    element.TagName == HTMLElements.B || 
                    element.TagName == HTMLElements.I || 
                    element.TagName == HTMLElements.U || 
                    element.TagName == HTMLElements.SUP ||
                    element.TagName == HTMLElements.SUB)
            {
                var style = element.OwnerDocument.DefaultView().GetComputedStyle(element, "");
                var run = context.OwnerDocument.CreateElement("w:r", Namespaces.W);
                context.AppendChild(run);
                var rPr = context.OwnerDocument.CreateElement("w:rPr", Namespaces.W);
                if (DOMHelper.HasClass(element.ClassName, "serif"))
                {
                    var rStyle = context.OwnerDocument.CreateElement("w:rStyle", Namespaces.W);
                    rPr.AppendChild(rStyle);
                    rStyle.SetAttribute("val", Namespaces.W, "Serif");
                }
                else if (DOMHelper.HasClass(element.ClassName, "cursive"))
                {
                    var rStyle = context.OwnerDocument.CreateElement("w:rStyle", Namespaces.W);
                    rPr.AppendChild(rStyle);
                    rStyle.SetAttribute("val", Namespaces.W, "Cursive");
                }
                else if (DOMHelper.HasClass(element.ClassName, "monospace"))
                {
                    var rStyle = context.OwnerDocument.CreateElement("w:rStyle", Namespaces.W);
                    rPr.AppendChild(rStyle);
                    rStyle.SetAttribute("val", Namespaces.W, "Monospace");
                }
                
                double fontSize = CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.FontSize));
                if (fontSize > 0 && fontSize != 16.0)
                {
                    // <w:sz w:val="..."/>
                    var sz = context.OwnerDocument.CreateElement("w:sz", Namespaces.W);
                    rPr.AppendChild(sz);
                    fontSize = 2.0 * fontSize * 72.0 / 96.0;  // Convert to half-points
                    sz.SetAttribute("val", Namespaces.W, fontSize.ToString(CultureInfo.InvariantCulture));
                }
                if (style.GetPropertyCSSValue(CSSProperties.FontWeight) == CSSValues.Bold)
                {
                    var b = context.OwnerDocument.CreateElement("w:b", Namespaces.W);
                    rPr.AppendChild(b);
                }
                if (style.GetPropertyCSSValue(CSSProperties.FontStyle) == CSSValues.Italic)
                {
                    var i = context.OwnerDocument.CreateElement("w:i", Namespaces.W);
                    rPr.AppendChild(i);
                }
                var textDecorations = (CSSValueList)style.GetPropertyCSSValue(CSSProperties.TextDecoration);
                if (textDecorations.Contains(CSSValues.Underline))
                {
                    var u = context.OwnerDocument.CreateElement("w:u", Namespaces.W);
                    rPr.AppendChild(u);
                    u.SetAttribute("val", Namespaces.W, "single");
                }
                if (textDecorations.Contains(CSSValues.LineThrough))
                {
                    var strike = context.OwnerDocument.CreateElement("w:strike", Namespaces.W);
                    rPr.AppendChild(strike);
                    strike.SetAttribute("val", Namespaces.W, "true");
                }
                var verticalAlign = style.GetPropertyCSSValue(CSSProperties.VerticalAlign);
                if (verticalAlign == CSSValues.Super)
                {
                    var vertAlign = context.OwnerDocument.CreateElement("w:vertAlign", Namespaces.W);
                    rPr.AppendChild(vertAlign);
                    vertAlign.SetAttribute("val", Namespaces.W, "superscript");
                }
                else if (verticalAlign == CSSValues.Sub)
                {
                    var vertAlign = context.OwnerDocument.CreateElement("w:vertAlign", Namespaces.W);
                    rPr.AppendChild(vertAlign);
                    vertAlign.SetAttribute("val", Namespaces.W, "subscript");
                }
                var cssColor = (CSSPrimitiveValue)element.Style.GetPropertyCSSValue(CSSProperties.Color);
                if (cssColor != null && cssColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                {
                    var color = context.OwnerDocument.CreateElement("w:color", Namespaces.W);
                    rPr.AppendChild(color);
                    var rgbColor = cssColor.GetRGBColorValue();
                    byte red = (byte)CSSConverter.ToColorComponent(rgbColor.Red, false);
                    byte green = (byte)CSSConverter.ToColorComponent(rgbColor.Green, false);
                    byte blue = (byte)CSSConverter.ToColorComponent(rgbColor.Blue, false);
                    string rgb = red.ToString("X2") + green.ToString("X2") + blue.ToString("X2");
                    color.SetAttribute("val", Namespaces.W, rgb);
                }
                var cssBackground = (CSSPrimitiveValue)element.Style.GetPropertyCSSValue(CSSProperties.BackgroundColor);
                if (cssBackground != null && cssBackground.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                {
                    var highlight = context.OwnerDocument.CreateElement("w:highlight", Namespaces.W);
                    rPr.AppendChild(highlight);
                    highlight.SetAttribute("val", Namespaces.W, "yellow");
                }
                if (rPr.HasChildNodes)
                {
                    run.AppendChild(rPr);
                }
                context = run;
            }
            else if (element.TagName == HTMLElements.TABLE)
            {
                var style = element.OwnerDocument.DefaultView().GetComputedStyle(element, "");
                var containerStyle = element.OwnerDocument.DefaultView().GetComputedStyle((Element)element.ParentNode, "");
                // <w:tbl>
                var tbl = context.OwnerDocument.CreateElement("w:tbl", Namespaces.W);
                context.AppendChild(tbl);
                // <w:tblPr>
                var tblPr = context.OwnerDocument.CreateElement("w:tblPr", Namespaces.W);
                tbl.AppendChild(tblPr);
                // <w:tblStyle w:val="TableGrid">
                var tblStyle = context.OwnerDocument.CreateElement("w:tblStyle", Namespaces.W);
                tblPr.AppendChild(tblStyle);
                tblStyle.SetAttribute("val", Namespaces.W, "TableGrid");
                // <w:tblW w:type="dxa" w:w="..."/>
                var tblW = context.OwnerDocument.CreateElement("w:tblW", Namespaces.W);
                tblPr.AppendChild(tblW);
                tblW.SetAttribute("type", Namespaces.W, "dxa");
                double width = CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.Width));
                width = Math.Floor(20.0 * width * 72.0 / 96.0);    // convert to twips
                tblW.SetAttribute("w", Namespaces.W, width.ToString(CultureInfo.InvariantCulture));
                // <w:tblpPr w:horzAnchor="margin" w:tblpXSpec="...">
                var tblpPr = context.OwnerDocument.CreateElement("w:tblpPr", Namespaces.W);
                tblPr.AppendChild(tblpPr);
                tblpPr.SetAttribute("horzAnchor", Namespaces.W, "text");
                if (containerStyle.GetPropertyCSSValue(CSSProperties.Right) != CSSValues.Auto)
                {
                    tblpPr.SetAttribute("tblpXSpec", Namespaces.W, "right");
                }
                else if (containerStyle.GetPropertyCSSValue(CSSProperties.MarginLeft) == CSSValues.Auto && 
                        containerStyle.GetPropertyCSSValue(CSSProperties.MarginRight) == CSSValues.Auto)
                {
                    tblpPr.SetAttribute("tblpXSpec", Namespaces.W, "center");
                }
                else
                {
                    double left = CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.Left));
                    left = Math.Floor(20.0 * left * 72.0 / 96.0);    // convert to twips
                    tblpPr.SetAttribute("tblpX", Namespaces.W, left.ToString(CultureInfo.InvariantCulture));
                }
                tblpPr.SetAttribute("vertAnchor", Namespaces.W, "text");
                double top = 20 + CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.Top));
                top = Math.Floor(20.0 * top * 72.0 / 96.0);    // convert to twips
                tblpPr.SetAttribute("tblpY", Namespaces.W, top.ToString(CultureInfo.InvariantCulture));
                // <w:tblCaption w:val="This is the caption text"/>
                var captionElement = ((HTMLTableElement)element).Caption;
                if (captionElement != null)
                {
                    var tblCaption = context.OwnerDocument.CreateElement("w:tblCaption", Namespaces.W);
                    tblPr.AppendChild(tblCaption);
                    tblCaption.SetAttribute("val", Namespaces.W, captionElement.InnerText.TrimEnd());
                }
                context = tbl;
            }
            else if (element.TagName == HTMLElements.CAPTION)
            {
                return;
            }
            else if (element.TagName == HTMLElements.TR)
            {
                // <w:tr>
                var tr = context.OwnerDocument.CreateElement("w:tr", Namespaces.W);
                context.AppendChild(tr);
                context = tr;
            }
            else if (element.TagName == HTMLElements.TD)
            {
                var tc = context.OwnerDocument.CreateElement("w:tc", Namespaces.W);
                context.AppendChild(tc);
                context = tc;
            }
            else if (element.TagName == HTMLElements.IMG)
            {
                var style = element.OwnerDocument.DefaultView().GetComputedStyle(element, "");
                var containerStyle = element.OwnerDocument.DefaultView().GetComputedStyle((Element)element.ParentNode, "");
                XmlElement p = context.OwnerDocument.CreateElement("w:p", Namespaces.W);
                context.AppendChild(p);
                state.MergeParagraph = true;
                var r = context.OwnerDocument.CreateElement("w:r", Namespaces.W);
                p.AppendChild(r);
                var drawing = context.OwnerDocument.CreateElement("w:drawing", Namespaces.W);
                r.AppendChild(drawing);
                // <wp:anchor distT="0" distB="0" distL="114300" distR="114300" simplePos="0" relativeHeight="251658240" behindDoc="0" locked="0" layoutInCell="1" allowOverlap="1" wp14:anchorId="7B854429" wp14:editId="4294E336">
                var anchor = context.OwnerDocument.CreateElement("wp:anchor", Namespaces.WP);
                drawing.AppendChild(anchor);
                anchor.SetAttribute("distT", "0");
                anchor.SetAttribute("distB", "0");
                anchor.SetAttribute("distL", "114300");
                anchor.SetAttribute("distR", "114300");
                anchor.SetAttribute("simplePos", "0");
                anchor.SetAttribute("relativeHeight", "251658240");
                anchor.SetAttribute("behindDoc", "0");
                anchor.SetAttribute("locked", "0");
                anchor.SetAttribute("layoutInCell", "1");
                anchor.SetAttribute("allowOverlap", "1");
                // <wp:simplePos x="0" y="0"/>
                var simplePos = context.OwnerDocument.CreateElement("wp:simplePos", Namespaces.WP);
                anchor.AppendChild(simplePos);
                simplePos.SetAttribute("x", "0");
                simplePos.SetAttribute("y", "0");
                // <wp:positionH relativeFrom="column">
                var positionH = context.OwnerDocument.CreateElement("wp:positionH", Namespaces.WP);
                anchor.AppendChild(positionH);
                positionH.SetAttribute("relativeFrom", "margin");
                if (containerStyle.GetPropertyCSSValue(CSSProperties.Right) != CSSValues.Auto)
                {
                    // <wp:align>right</wp:align>
                    var align = context.OwnerDocument.CreateElement("wp:align", Namespaces.WP);
                    positionH.AppendChild(align);
                    var alignValue = context.OwnerDocument.CreateTextNode("right");
                    align.AppendChild(alignValue);
                }
                else if (containerStyle.GetPropertyCSSValue(CSSProperties.MarginLeft) == CSSValues.Auto &&
                        containerStyle.GetPropertyCSSValue(CSSProperties.MarginRight) == CSSValues.Auto)
                {
                    // <wp:align>center</wp:align>
                    var align = context.OwnerDocument.CreateElement("wp:align", Namespaces.WP);
                    positionH.AppendChild(align);
                    var alignValue = context.OwnerDocument.CreateTextNode("center");
                    align.AppendChild(alignValue);
                }
                else
                {
                    // <wp:posOffset>...</wp:posOffset>
                    var posOffsetH = context.OwnerDocument.CreateElement("wp:posOffset", Namespaces.WP);
                    positionH.AppendChild(posOffsetH);
                    double left = CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.Left));
                    left = Math.Floor(914400.0 * left / 96.0);    // convert to EMUs
                    var posOffsetHValue = context.OwnerDocument.CreateTextNode(left.ToString(CultureInfo.InvariantCulture));
                    posOffsetH.AppendChild(posOffsetHValue);
                }
                // <wp:positionV relativeFrom="paragraph">
                var positionV = context.OwnerDocument.CreateElement("wp:positionV", Namespaces.WP);
                anchor.AppendChild(positionV);
                positionV.SetAttribute("relativeFrom", "paragraph");
                // <wp:posOffset>...</wp:posOffset>
                var posOffsetV = context.OwnerDocument.CreateElement("wp:posOffset", Namespaces.WP);
                positionV.AppendChild(posOffsetV);
                double top = CSSConverter.ToLength(style.GetPropertyCSSValue(CSSProperties.Top));
                top = Math.Floor(914400.0 * top / 96.0);    // convert to EMUs
                var posOffsetVValue = context.OwnerDocument.CreateTextNode(top.ToString(CultureInfo.InvariantCulture));
                posOffsetV.AppendChild(posOffsetVValue);
                // <wp:extent cx="..." cy="..."/>
                var extent = context.OwnerDocument.CreateElement("wp:extent", Namespaces.WP);
                anchor.AppendChild(extent);
                double width = SafeConvert.ToDouble(element.GetAttribute("width"));
                width = Math.Floor(914400.0 * width / 96.0);    // convert to EMUs
                extent.SetAttribute("cx", width.ToString(CultureInfo.InvariantCulture));
                double height = SafeConvert.ToDouble(element.GetAttribute("height"));
                height = Math.Floor(914400.0 * height / 96.0);  // convert to EMUs
                extent.SetAttribute("cy", height.ToString(CultureInfo.InvariantCulture));
                // <wp:wrapNone/>
                var wrapNone = context.OwnerDocument.CreateElement("wp:wrapNone", Namespaces.WP);
                anchor.AppendChild(wrapNone);
                // <wp:docPr id="1" name="Picture 1"/>
                var docPr = context.OwnerDocument.CreateElement("wp:docPr", Namespaces.WP);
                anchor.AppendChild(docPr);
                docPr.SetAttribute("id", "1");
                docPr.SetAttribute("name", "Image");
                /*
                // <wp:inline distT="0" distB="0" distL="0" distR="0">
                var inline = context.OwnerDocument.CreateElement("wp:inline", Namespaces.WP);
                drawing.AppendChild(inline);
                inline.SetAttribute("distT", "0");
                inline.SetAttribute("distB", "0");
                inline.SetAttribute("distL", "0");
                inline.SetAttribute("distR", "0");
                // <wp:extent cx="..." cy="..."/>
                var extent = context.OwnerDocument.CreateElement("wp:extent", Namespaces.WP);
                inline.AppendChild(extent);
                double width = SafeConvert.ToDouble(element.GetAttribute("width"));
                width = Math.Floor(914400.0 * width / 96.0);    // convert to EMUs
                extent.SetAttribute("cx", width.ToString(CultureInfo.InvariantCulture));
                double height = SafeConvert.ToDouble(element.GetAttribute("height"));
                height = Math.Floor(914400.0 * height / 96.0);  // convert to EMUs
                extent.SetAttribute("cy", height.ToString(CultureInfo.InvariantCulture));
                // <wp:docPr id="1" name="Picture 1"/>
                var docPr = context.OwnerDocument.CreateElement("wp:docPr", Namespaces.WP);
                inline.AppendChild(docPr);
                docPr.SetAttribute("id", "1");
                docPr.SetAttribute("name", "Image");
                */
                // <a:graphic xmlns:a="...">
                var graphic = context.OwnerDocument.CreateElement("a:graphic", Namespaces.A);
                anchor.AppendChild(graphic);
                var graphicData = context.OwnerDocument.CreateElement("a:graphicData", Namespaces.A);
                graphic.AppendChild(graphicData);
                graphicData.SetAttribute("uri", Namespaces.PIC);
                var pic = context.OwnerDocument.CreateElement("pic:pic", Namespaces.PIC);
                graphicData.AppendChild(pic);
                var nvPicPr = context.OwnerDocument.CreateElement("pic:nvPicPr", Namespaces.PIC);
                pic.AppendChild(nvPicPr);
                var cNvPr = context.OwnerDocument.CreateElement("pic:cNvPr", Namespaces.PIC);
                nvPicPr.AppendChild(cNvPr);
                cNvPr.SetAttribute("id", "1");
                cNvPr.SetAttribute("name", element.GetAttribute("src"));
                var cNvPicPr = context.OwnerDocument.CreateElement("pic:cNvPicPr", Namespaces.PIC);
                nvPicPr.AppendChild(cNvPicPr);
                var blipFill = context.OwnerDocument.CreateElement("pic:blipFill", Namespaces.PIC);
                pic.AppendChild(blipFill);
                var blip = context.OwnerDocument.CreateElement("a:blip", Namespaces.A);
                blipFill.AppendChild(blip);
                string imgId = "imgId" + (state.ImgID++).ToString(CultureInfo.InvariantCulture);
                blip.SetAttribute("embed", Namespaces.R, imgId);
                blip.SetAttribute("cstate", "print");
                var stretch = context.OwnerDocument.CreateElement("a:stretch", Namespaces.A);
                blipFill.AppendChild(stretch);
                var fillRect = context.OwnerDocument.CreateElement("a:fillRect", Namespaces.A);
                stretch.AppendChild(fillRect);
                var spPr = context.OwnerDocument.CreateElement("pic:spPr", Namespaces.PIC);
                pic.AppendChild(spPr);
                var xfrm = context.OwnerDocument.CreateElement("a:xfrm", Namespaces.A);
                spPr.AppendChild(xfrm);
                var off = context.OwnerDocument.CreateElement("a:off", Namespaces.A);
                xfrm.AppendChild(off);
                off.SetAttribute("x", "0");
                off.SetAttribute("y", "0");
                var ext = context.OwnerDocument.CreateElement("a:ext", Namespaces.A);
                xfrm.AppendChild(ext);
                ext.SetAttribute("cx", width.ToString(CultureInfo.InvariantCulture));
                ext.SetAttribute("cy", height.ToString(CultureInfo.InvariantCulture));
                var prstGeom = context.OwnerDocument.CreateElement("a:prstGeom", Namespaces.A);
                spPr.AppendChild(prstGeom);
                prstGeom.SetAttribute("prst", "rect");
                var avLst = context.OwnerDocument.CreateElement("a:avLst", Namespaces.A);
                prstGeom.AppendChild(avLst);
                context = drawing;
            }

            if (element.TagName == HTMLElements.H1 || element.TagName == HTMLElements.H2 ||
                element.TagName == HTMLElements.H3 || element.TagName == HTMLElements.H4 ||
                element.TagName == HTMLElements.H5 || element.TagName == HTMLElements.H6 ||
                element.TagName == HTMLElements.P || element.TagName == HTMLElements.LI)
            {
                var style = element.OwnerDocument.DefaultView().GetComputedStyle(element, "");
                var textAlign = style.GetPropertyCSSValue(CSSProperties.TextAlign);
                if (textAlign == CSSValues.Center)
                {
                    var pPr = context.OwnerDocument.CreateElement("w:pPr", Namespaces.W);
                    context.AppendChild(pPr);
                    var jc = context.OwnerDocument.CreateElement("w:jc", Namespaces.W);
                    pPr.AppendChild(jc);
                    jc.SetAttribute("val", Namespaces.W, "center");
                }
                else if (textAlign == CSSValues.Right)
                {
                    var pPr = context.OwnerDocument.CreateElement("w:pPr", Namespaces.W);
                    context.AppendChild(pPr);
                    var jc = context.OwnerDocument.CreateElement("w:jc", Namespaces.W);
                    pPr.AppendChild(jc);
                    jc.SetAttribute("val", Namespaces.W, "right");
                }
            }

            for (var child = element.FirstChild; child != null; child = child.NextSibling)
            {
                if (child is HTMLElement)
                {
                    Convert((HTMLElement)child, context, state);
                }
                else if (child is Text)
                {
                    Convert((Text)child, context);
                }
            }
        }

        private static void Convert(Text text, XmlElement context)
        {
            var textElement = context.OwnerDocument.CreateElement("w:t", Namespaces.W);
            context.AppendChild(textElement);

            XmlNode textNode = context.OwnerDocument.CreateNode(XmlNodeType.Text, "w:t", Namespaces.W);
            textNode.Value = text.Data;
            textElement.AppendChild(textNode);
        }

        private static XmlDocument GenerateStyles(HTMLDocument htmlDocument)
        {
            string xml = Resource.ReadText("SilverNote.FileFormats.OpenXML.styles.xml");
            XmlDocument result = new XmlDocument();
            result.LoadXml(xml);
            return result;
        }

        private static XmlDocument GenerateNumbering(HTMLDocument htmlDocument)
        {
            string xml = Resource.ReadText("SilverNote.FileFormats.OpenXML.numbering.xml");
            XmlDocument result = new XmlDocument();
            result.LoadXml(xml);
            return result;
        }

        public static void Save(string fileName, XmlDocument document, XmlDocument styles, XmlDocument numbering, Dictionary<string, byte[]> media)
        {
            int id = 1;

            using (var package = Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                // Document
                Uri documentUri = new Uri("/word/document.xml", UriKind.Relative);
                string documentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml";
                var documentPart = package.CreatePart(documentUri, documentType);
                using (var stream = documentPart.GetStream(FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        document.Save(writer);
                        package.Flush();
                    }
                }
                package.CreateRelationship(documentUri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "rId" + id++);
                package.Flush();

                // Numbering
                Uri numberingUri = new Uri("/word/numbering.xml", UriKind.Relative);
                string numberingType = "application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml";
                var numberingPart = package.CreatePart(numberingUri, numberingType);
                using (var stream = numberingPart.GetStream(FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        numbering.Save(writer);
                        package.Flush();
                    }
                }
                documentPart.CreateRelationship(numberingUri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering", "rId" + id++);
                package.Flush();

                // Styles
                Uri stylesUri = new Uri("/word/styles.xml", UriKind.Relative);
                string stylesType = "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml";
                var stylesPart = package.CreatePart(stylesUri, stylesType);
                using (var stream = stylesPart.GetStream(FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        styles.Save(writer);
                        package.Flush();
                    }
                }
                documentPart.CreateRelationship(stylesUri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles", "rId" + id++);
                package.Flush();

                // Images
                int imgID = 1;
                foreach (var medium in media.Where(m => IsImageFileName(m.Key)))
                {
                    Uri imageUri = new Uri("/word/media/" + Uri.EscapeUriString(medium.Key), UriKind.Relative);
                    string imageType = MimeTypeFromFileName(medium.Key);
                    var imagePart = package.CreatePart(imageUri, imageType);
                    using (var stream = imagePart.GetStream(FileMode.Create, FileAccess.Write))
                    {
                        stream.Write(medium.Value, 0, medium.Value.Length);
                        package.Flush();
                    }
                    documentPart.CreateRelationship(imageUri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image", "imgId" + imgID++);
                    package.Flush();
                }
            }
        }

        private static string MimeTypeFromFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            switch (extension.ToLower())
            {
                case ".bmp":
                    return "image/bmp";
                case ".gif":
                    return "image/gif";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".svg":
                    return "image/svg+xml";
                case ".tiff":
                    return "image/tiff";
                default:
                    return "";
            }
        }

        private static bool IsImageFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            switch (extension.ToLower())
            {
                case ".bmp":
                case ".gif":
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".svg":
                case ".tiff":
                    return true;
                default:
                    return false;
            }
        }
    }
}
