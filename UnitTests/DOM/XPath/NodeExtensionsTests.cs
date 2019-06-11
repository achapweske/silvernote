/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM;
using DOM.LS;
using DOM.XPath;
using System.IO;

namespace UnitTests.DOM.XPath
{
    [TestClass]
    public class NodeExtensionsTests
    {
        const string TestDocumentString =
@"<?xml version=""1.0"" encoding=""ISO-8859-1""?>
<bookstore specialty=""Programming"">
    <book category=""COOKING"">
      <title lang=""en"">Everyday Italian</title>
      <author>Giada De Laurentiis</author>
      <year>2005</year>
      <price>30.00</price>
      <description />
    </book>
    <book category=""CHILDREN"">
      <title lang=""en"">Harry Potter</title>
      <author>J K. Rowling</author>
      <year>2005</year>
      <price>29.99</price>
    </book>
    <book category=""WEB"">
      <title lang=""en"">XQuery Kick Start</title>
      <author>James McGovern</author>
      <author>Per Bothner</author>
      <author>Kurt Cagle</author>
      <author>James Linn</author>
      <author>Vaidyanathan Nagarajan</author>
      <year>2003</year>
      <price>49.99</price>
    </book>
    <book category=""WEB"" style=""Programming"">
      <title lang=""en"">Learning XML</title>
      <author>Erik T. Ray</author>
      <year>2003</year>
      <price>39.95</price>
    </book>
</bookstore>";

        static Document _TestDocument;

        static Document TestDocument
        {
            get
            {
                if (_TestDocument == null)
                {
                    _TestDocument = DocumentFromString(TestDocumentString);
                }

                return _TestDocument;
            }
        }

        static Document DocumentFromString(string str)
        {
            using (var reader = new StringReader(str))
            {
                LSInput input = DOMFactory.CreateLSInput();
                input.CharacterStream = reader;
                LSParser parser = DOMFactory.CreateLSParser(DOMImplementationLSMode.MODE_SYNCHRONOUS, "");
                return parser.Parse(input);
            }
        }

        [TestMethod]
        public void TestSelect_DocumentElement()
        {
            var result = TestDocument.Select("/bookstore");

            Assert.AreEqual(1, result.Length);
        }

        [TestMethod]
        public void TestSelect_DescendantsByName()
        {
            var result = TestDocument.Select("//author");

            Assert.AreEqual(8, result.Length);
            Assert.AreEqual("Giada De Laurentiis", result[0].TextContent);
            Assert.AreEqual("J K. Rowling", result[1].TextContent);
            Assert.AreEqual("James McGovern", result[2].TextContent);
            Assert.AreEqual("Per Bothner", result[3].TextContent);
            Assert.AreEqual("Kurt Cagle", result[4].TextContent);
            Assert.AreEqual("James Linn", result[5].TextContent);
            Assert.AreEqual("Vaidyanathan Nagarajan", result[6].TextContent);
            Assert.AreEqual("Erik T. Ray", result[7].TextContent);
        }

        [TestMethod]
        public void TestSelect_DescendantsByMatchingAttribute()
        {
            var result = TestDocument.Select("book[/bookstore/@specialty=@style]");

            Assert.AreEqual(1, result.Length);
        }

        [TestMethod]
        public void TestSelect_ChildrenByName()
        {
            var result = TestDocument.Select("./book");

            Assert.AreEqual(4, result.Length);
        }

        [TestMethod]
        public void TestSelect_GrandchildrenByName()
        {
            var result = TestDocument.Select("/bookstore/book/title");

            Assert.AreEqual(4, result.Length);
        }

        [TestMethod]
        public void TestSelect_GrandchildrenByIndexAndName()
        {
            var result = TestDocument.Select("/bookstore/book[1]/title");

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("Everyday Italian", result[0].TextContent);
        }

        [TestMethod]
        public void TestSelect_GrandchildrenTextByName()
        {
            var result = TestDocument.Select("/bookstore/book/price/text()");

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("30.00", result[0].NodeValue);
            Assert.AreEqual("29.99", result[1].NodeValue);
            Assert.AreEqual("49.99", result[2].NodeValue);
            Assert.AreEqual("39.95", result[3].NodeValue);
        }

        [TestMethod]
        public void TestSelect_GrandchildrenTextByNameAndComparison()
        {
            var result = TestDocument.Select("/bookstore/book[price>35]/title");

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("XQuery Kick Start", result[0].TextContent);
            Assert.AreEqual("Learning XML", result[1].TextContent);
        }

        [TestMethod]
        public void TestSelect_EmptyNodesByName()
        {
            var result = TestDocument.Select("//description[not(node())]");

            Assert.AreEqual(1, result.Length);
        }

    }
}
