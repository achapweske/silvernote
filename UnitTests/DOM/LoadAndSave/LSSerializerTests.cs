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
using DOM.Internal;
using DOM.LS;
using DOM.LS.Internal;

namespace UnitTests
{
    [TestClass]
    public class LSSerializerTests
    {
        [TestMethod]
        public void TestWrite()
        {
            var document = new DocumentBase("test");
            var serializer = new LSSerializerImpl();
            var output = serializer.WriteToString(document);

            string expected = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?><test />";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestWrite_WithComment()
        {
            var document = new DocumentBase("test");
            var comment = document.CreateComment("Created with SilverNote v1.0.0000.00000 (http://www.silver-note.com/)");
            document.InsertBefore(comment, document.DocumentElement);

            var serializer = new LSSerializerImpl();
            var output = serializer.WriteToString(document);

            string expected = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?><!--Created with SilverNote v1.0.0000.00000 (http://www.silver-note.com/)--><test />";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestWrite_NoXmlDeclaration()
        {
            var document = new DocumentBase("test");
            var serializer = new LSSerializerImpl();
            serializer.Config.SetParameter(LSSerializerParameters.XML_DECLARATION, false);
            var output = serializer.WriteToString(document);

            string expected = @"<test />";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestWrite_WithDocType()
        {
            var docType = new DocumentTypeBase("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd");
            var document = new DocumentBase(docType, "svg");

            var serializer = new LSSerializerImpl();
            serializer.Config.SetParameter(LSSerializerParameters.XML_DECLARATION, false);
            var output = serializer.WriteToString(document);

            string expected = @"<!DOCTYPE svg PUBLIC ""-//W3C//DTD SVG 1.1//EN"" ""http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd""><svg />";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestWrite_WithRootNamespace()
        {
            var document = new DocumentBase("http://www.w3.org/2000/svg", "svg");
            var serializer = new LSSerializerImpl();
            serializer.Config.SetParameter(LSSerializerParameters.XML_DECLARATION, false);
            var output = serializer.WriteToString(document);

            string expected = @"<svg xmlns=""http://www.w3.org/2000/svg"" />";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestWrite_WithRootAttribute()
        {
            var document = new DocumentBase("svg");
            document.DocumentElement.SetAttribute("version", "1.1");

            var serializer = new LSSerializerImpl();
            serializer.Config.SetParameter(LSSerializerParameters.XML_DECLARATION, false);
            var output = serializer.WriteToString(document);

            string expected = @"<svg version=""1.1"" />";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestWrite_WithChildElement()
        {
            var document = new DocumentBase("svg");
            var child = document.CreateElement("circle");
            document.DocumentElement.AppendChild(child);

            var serializer = new LSSerializerImpl();
            serializer.Config.SetParameter(LSSerializerParameters.XML_DECLARATION, false);
            var output = serializer.WriteToString(document);

            string expected = @"<svg><circle /></svg>";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestWrite_WithChildElementNS()
        {
            var document = new DocumentBase("http://www.w3.org/2000/svg", "svg");
            var child = document.CreateElementNS("http://www.w3.org/2000/svg", "circle");
            document.DocumentElement.AppendChild(child);

            var serializer = new LSSerializerImpl();
            serializer.Config.SetParameter(LSSerializerParameters.XML_DECLARATION, false);
            var output = serializer.WriteToString(document);

            string expected = @"<svg xmlns=""http://www.w3.org/2000/svg""><circle /></svg>";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestWrite_WithNamespacePrefix()
        {
            var document = new DocumentBase("svg");
            document.DocumentElement.SetAttributeNS("http://www.w3.org/2000/xmlns/", "xmlns:xlink", "http://www.w3.org/1999/xlink");

            var serializer = new LSSerializerImpl();
            serializer.Config.SetParameter(LSSerializerParameters.XML_DECLARATION, false);
            var output = serializer.WriteToString(document);

            string expected = @"<svg xmlns:xlink=""http://www.w3.org/1999/xlink"" />";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void TestWrite_WithPrefixedAttribute()
        {
            var document = new DocumentBase("svg");
            document.DocumentElement.SetAttributeNS("http://www.w3.org/2000/xmlns/", "xmlns:xlink", "http://www.w3.org/1999/xlink");
            var image = document.CreateElement("image");
            image.SetAttributeNS("http://www.w3.org/1999/xlink", "xlink:href", "foo.png");
            document.DocumentElement.AppendChild(image);

            var serializer = new LSSerializerImpl();
            serializer.Config.SetParameter(LSSerializerParameters.XML_DECLARATION, false);
            var output = serializer.WriteToString(document);

            string expected = @"<svg xmlns:xlink=""http://www.w3.org/1999/xlink""><image xlink:href=""foo.png"" /></svg>";

            Assert.AreEqual(expected, output);
        }

    }
}
