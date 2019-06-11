/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Diagnostics;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DOM;
using DOM.LS;

namespace UnitTests
{
    [TestClass]
    public class LSParserTests
    {
        Document ParseDocument(string str)
        {
            LSInput input = DOMFactory.CreateLSInput();
            input.StringData = str;
            LSParser parser = DOMFactory.CreateLSParser(DOMImplementationLSMode.MODE_SYNCHRONOUS, "");
            return parser.Parse(input);
        }

        [TestMethod]
        public void TestParse()
        {
            Document document = ParseDocument(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?><test />");

            Assert.IsNotNull(document);
            Assert.AreEqual("1.0", document.XmlVersion);
            Assert.AreEqual("UTF-8", document.XmlEncoding);
            Assert.AreEqual(false, document.XmlStandalone);
            Assert.IsNotNull(document.DocumentElement);
            Assert.AreEqual("test", document.DocumentElement.NodeName);
        }

        [TestMethod]
        public void TestParse_WithComment()
        {
            Document document = ParseDocument(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?><!--Created with SilverNote v1.0.0000.00000 (http://www.silver-note.com/)--><test />");

            Assert.IsNotNull(document);
            Assert.AreEqual("1.0", document.XmlVersion);
            Assert.AreEqual("UTF-8", document.XmlEncoding);
            Assert.AreEqual(false, document.XmlStandalone);
            Assert.IsNotNull(document.FirstChild);
            Assert.AreEqual(document.FirstChild.NodeType, NodeType.COMMENT_NODE);
            Assert.AreEqual(document.FirstChild.NodeValue, "Created with SilverNote v1.0.0000.00000 (http://www.silver-note.com/)");
            Assert.IsNotNull(document.DocumentElement);
            Assert.AreEqual("test", document.DocumentElement.NodeName);
        }

        [TestMethod]
        public void TestParse_NoXmlDeclaration()
        {
            Document document = ParseDocument(@"<test />");

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.DocumentElement);
            Assert.AreEqual("test", document.DocumentElement.NodeName);
        }

        [TestMethod]
        public void TestParse_WithDocType()
        {
            Document document = ParseDocument(@"<!DOCTYPE svg PUBLIC ""-//W3C//DTD SVG 1.1//EN"" ""http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd""><svg />");

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.DocType);
            Assert.AreEqual("svg", document.DocType.Name);
            Assert.AreEqual("-//W3C//DTD SVG 1.1//EN", document.DocType.PublicId);
            Assert.AreEqual("http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", document.DocType.SystemId);
            Assert.IsNotNull(document.DocumentElement);
            Assert.AreEqual("svg", document.DocumentElement.NodeName);
        }

        [TestMethod]
        public void TestParse_WithRootNamespace()
        {
            Document document = ParseDocument(@"<svg xmlns=""http://www.w3.org/2000/svg"" />");

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.DocumentElement);
            Assert.AreEqual("svg", document.DocumentElement.NodeName);
            Assert.AreEqual("svg", document.DocumentElement.LocalName);
            Assert.AreEqual("http://www.w3.org/2000/svg", document.DocumentElement.NamespaceURI);
        }

        [TestMethod]
        public void TestParse_WithRootAttribute()
        {
            Document document = ParseDocument(@"<svg version=""1.1"" />");

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.DocumentElement);
            Assert.AreEqual("svg", document.DocumentElement.NodeName);
            Assert.AreEqual("1.1", document.DocumentElement.GetAttribute("version"));
        }

        [TestMethod]
        public void TestParse_WithChildElement()
        {
            Document document = ParseDocument(@"<svg><circle /></svg>");

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.DocumentElement);
            Assert.AreEqual("svg", document.DocumentElement.NodeName);
            Assert.IsTrue(document.DocumentElement.HasChildNodes());
            Assert.IsNotNull(document.DocumentElement.FirstChild);
            Assert.AreEqual("circle", document.DocumentElement.FirstChild.NodeName);
        }

        [TestMethod]
        public void TestParse_WithChildElementNS()
        {
            Document document = ParseDocument(@"<svg xmlns=""http://www.w3.org/2000/svg""><circle /></svg>");

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.DocumentElement);
            Assert.AreEqual("svg", document.DocumentElement.NodeName);
            Assert.AreEqual("svg", document.DocumentElement.LocalName);
            Assert.IsTrue(document.DocumentElement.HasChildNodes());
            Assert.IsNotNull(document.DocumentElement.FirstChild);
            Assert.AreEqual("circle", document.DocumentElement.FirstChild.NodeName);
        }

        [TestMethod]
        public void TestParse_WithNamespacePrefix()
        {
            Document document = ParseDocument(@"<svg xmlns:xlink=""http://www.w3.org/1999/xlink"" />");

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.DocumentElement);
            Assert.AreEqual("svg", document.DocumentElement.NodeName);
        }

        [TestMethod]
        public void TestParse_WithPrefixedAttribute()
        {
            Document document = ParseDocument(@"<svg xmlns:xlink=""http://www.w3.org/1999/xlink""><image xlink:href=""foo.png"" /></svg>");

            Assert.IsNotNull(document);
            var svg = document.DocumentElement;
            Assert.IsNotNull(svg);
            Assert.AreEqual("svg", svg.NodeName);
            Assert.IsTrue(svg.HasChildNodes());
            var image = svg.FirstChild as Element;
            Assert.IsNotNull(image);
            Assert.AreEqual("image", image.NodeName);
            Assert.AreEqual("foo.png", image.GetAttributeNS("http://www.w3.org/1999/xlink", "href"));
            Assert.AreEqual("foo.png", image.GetAttribute("xlink:href"));
        }
    }
}
