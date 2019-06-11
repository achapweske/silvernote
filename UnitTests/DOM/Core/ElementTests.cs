/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DOM;

namespace UnitTests.DOM.Core
{
    [TestClass]
    public class ElementTests
    {
        [TestMethod]
        public void TestOwnerDocument()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var expected = document;
            var actual = element.OwnerDocument;

            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void TestNodeType()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var expected = NodeType.ELEMENT_NODE;
            var actual = element.NodeType;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNamespaceURI()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "div");

            var expected = (string)"http://www.w3.org/1999/xhtml";
            var actual = element.NamespaceURI;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNamespaceURI_Null()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var expected = (string)null;
            var actual = element.NamespaceURI;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetPrefix()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "html:div");

            var expected = (string)"html";
            var actual = element.Prefix;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetPrefix_Null()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var expected = (string)null;
            var actual = element.Prefix;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSetPrefix()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "html:div");

            element.Prefix = "foo";

            var expected = "foo";
            var actual = element.Prefix;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual("div", element.LocalName);
            Assert.AreEqual("foo:div", element.NodeName);
            Assert.AreEqual("foo:div", element.TagName);
            Assert.AreEqual("http://www.w3.org/1999/xhtml", element.NamespaceURI);
        }

        [TestMethod]
        public void TestSetPrefix_Null()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "html:div");

            element.Prefix = null;

            var expected = (string)null;
            var actual = element.Prefix;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual("div", element.LocalName);
            Assert.AreEqual("div", element.NodeName);
            Assert.AreEqual("div", element.TagName);
            Assert.AreEqual("http://www.w3.org/1999/xhtml", element.NamespaceURI);
        }

        [TestMethod]
        public void TestNodeName()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var expected = "div";
            var actual = element.NodeName;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNodeName_Qualified()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "html:div");

            var expected = "html:div";
            var actual = element.NodeName;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLocalName()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "html:div");

            var expected = "div";
            var actual = element.LocalName;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLocalName_Null()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var expected = (string)null;
            var actual = element.LocalName;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLookupNamespaceURI()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            document.DocumentElement.SetAttributeNS("http://www.w3.org/XML/1998/namespace", "xmlns:xlink", "http://www.w3.org/1999/xlink");
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "div");
            document.DocumentElement.AppendChild(element);

            var expected = "http://www.w3.org/1999/xlink";
            var actual = element.LookupNamespaceURI("xlink");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLookupNamespaceURI_Default()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "div");
            document.DocumentElement.AppendChild(element);

            var expected = "http://www.w3.org/1999/xhtml";
            var actual = element.LookupNamespaceURI(null);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLookupPrefix()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            document.DocumentElement.SetAttributeNS("http://www.w3.org/XML/1998/namespace", "xmlns:xlink", "http://www.w3.org/1999/xlink");
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "div");
            document.DocumentElement.AppendChild(element);

            var expected = "xlink";
            var actual = element.LookupPrefix("http://www.w3.org/1999/xlink");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLookupPrefix_NotFound()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            document.DocumentElement.SetAttributeNS("http://www.w3.org/XML/1998/namespace", "xmlns:xlink", "http://www.w3.org/1999/xlink");
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "div");
            document.DocumentElement.AppendChild(element);

            var expected = (string)null;
            var actual = element.LookupPrefix("http://www.w3.org/1999/foo");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIsDefaultNamespace()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            document.DocumentElement.SetAttributeNS("http://www.w3.org/XML/1998/namespace", "xmlns:xlink", "http://www.w3.org/1999/xlink");
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "div");
            document.DocumentElement.AppendChild(element);

            var expected = true;
            var actual = element.IsDefaultNamespace("http://www.w3.org/1999/xhtml");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIsDefaultNamespace_False()
        {
            Document document = DOMFactory.CreateDocument("http://www.w3.org/1999/xhtml", "xhtml", null);
            document.DocumentElement.SetAttributeNS("http://www.w3.org/XML/1998/namespace", "xmlns:xlink", "http://www.w3.org/1999/xlink");
            Element element = document.CreateElementNS("http://www.w3.org/1999/xhtml", "div");
            document.DocumentElement.AppendChild(element);

            var expected = false;
            var actual = element.IsDefaultNamespace("http://www.w3.org/1999/xlink");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetNodeValue()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var expected = (string)null;
            var actual = element.NodeValue;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSetNodeValue()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            element.NodeValue = "foo";

            var expected = (string)null;
            var actual = element.NodeValue;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestHasAttributes()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("input");
            element.SetAttribute("id", "foo");
            element.SetAttribute("class", "bar");
            element.SetAttribute("type", "checkbox");

            var expected = true;
            var actual = element.HasAttributes();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestHasAttributes_False()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var expected = false;
            var actual = element.HasAttributes();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAttributes()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("input");
            element.SetAttribute("id", "foo");
            element.SetAttribute("class", "bar");
            element.SetAttribute("type", "checkbox");

            var attributes = element.Attributes;
            Assert.IsNotNull(attributes);
            Assert.AreEqual(3, attributes.Length);
            for (int i = 0; i < 3; i++)
            {
                var attr = attributes[i];
                Assert.IsNotNull(attr);
                Assert.IsTrue(attr.NodeName == "id" && attr.NodeValue == "foo" ||
                    attr.NodeName == "class" && attr.NodeValue == "bar" ||
                    attr.NodeName == "type" && attr.NodeValue == "checkbox");
            }
            Assert.IsFalse(attributes[0].NodeName == attributes[1].NodeName);
            Assert.IsFalse(attributes[1].NodeName == attributes[2].NodeName);
            Assert.IsFalse(attributes[0].NodeName == attributes[2].NodeName);
        }

        [TestMethod]
        public void TestAttributes_Empty()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            Assert.IsTrue(element.Attributes == null || element.Attributes.Length == 0);
        }

        [TestMethod]
        public void TestHasChildNodes()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            element.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            element.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            element.AppendChild(child2);

            var expected = true;
            var actual = element.HasChildNodes();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestHasChildNodes_False()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var expected = false;
            var actual = element.HasChildNodes();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestChildNodes()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            element.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            element.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            element.AppendChild(child2);

            var childNodes = element.ChildNodes;
            Assert.IsNotNull(childNodes);
            Assert.AreEqual(3, childNodes.Length);
            Assert.AreSame(child0, childNodes[0]);
            Assert.AreSame(child1, childNodes[1]);
            Assert.AreSame(child2, childNodes[2]);
        }

        [TestMethod]
        public void TestChildNodes_Empty()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var childNodes = element.ChildNodes;
            Assert.IsNotNull(childNodes);
            Assert.AreEqual(0, childNodes.Length);
        }

        [TestMethod]
        public void TestParentNode()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            document.DocumentElement.AppendChild(element);

            var expected = document.DocumentElement;
            var actual = element.ParentNode;

            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void TestParentNode_NotSet()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            var expected = (Node)null;
            var actual = element.ParentNode;

            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void TestParentNode_Removed()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            document.DocumentElement.AppendChild(element);
            document.DocumentElement.RemoveChild(element);

            var expected = (Node)null;
            var actual = element.ParentNode;

            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void TestFirstChild()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            parent.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            parent.AppendChild(child2);

            var expected = child0;
            var actual = parent.FirstChild;

            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void TestFirstChild_None()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");

            var expected = (Node)null;
            var actual = parent.FirstChild;

            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void TestLastChild()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            parent.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            parent.AppendChild(child2);

            var expected = child2;
            var actual = parent.LastChild;

            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void TestLastChild_None()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");

            var expected = (Node)null;
            var actual = parent.LastChild;

            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void TestPreviousSibling()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            parent.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            parent.AppendChild(child2);

            Assert.AreSame(null, child0.PreviousSibling);
            Assert.AreSame(child0, child1.PreviousSibling);
            Assert.AreSame(child1, child2.PreviousSibling);
        }

        [TestMethod]
        public void TestNextSibling()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            parent.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            parent.AppendChild(child2);

            Assert.AreSame(child1, child0.NextSibling);
            Assert.AreSame(child2, child1.NextSibling);
            Assert.AreSame(null, child2.NextSibling);
        }

        [TestMethod]
        public void TestAppendChild()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);

            Assert.AreEqual(1, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);

            Element child1 = document.CreateElement("span");
            parent.AppendChild(child1);

            Assert.AreEqual(2, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);
            Assert.AreSame(child1, parent.ChildNodes[1]);

            Element child2 = document.CreateElement("span");
            parent.AppendChild(child2);

            Assert.AreEqual(3, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);
            Assert.AreSame(child1, parent.ChildNodes[1]);
            Assert.AreSame(child2, parent.ChildNodes[2]);
        }

        [TestMethod]
        public void TestAppendChild_AlreadyInTree()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent0 = document.CreateElement("div");
            document.DocumentElement.AppendChild(parent0);
            Element parent1 = document.CreateElement("div");
            document.DocumentElement.AppendChild(parent1);
            Element child = document.CreateElement("span");
            parent0.AppendChild(child);
            parent1.AppendChild(child);

            Assert.IsFalse(parent0.HasChildNodes());
            Assert.AreEqual(1, parent1.ChildNodes.Length);
            Assert.AreSame(child, parent1.ChildNodes[0]);
        }

        [TestMethod]
        public void TestAppendChild_DocumentFragment()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            DocumentFragment fragment = document.CreateDocumentFragment();
            Element child0 = document.CreateElement("span");
            fragment.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            fragment.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            fragment.AppendChild(child2);

            Element parent = document.CreateElement("div");
            parent.AppendChild(fragment);

            Assert.AreEqual(3, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);
            Assert.AreSame(child1, parent.ChildNodes[1]);
            Assert.AreSame(child2, parent.ChildNodes[2]);
        }

        [TestMethod]
        public void TestInsertBefore()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.InsertBefore(child0, null);

            Assert.AreEqual(1, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);

            Element child1 = document.CreateElement("span");
            parent.InsertBefore(child1, child0);

            Assert.AreEqual(2, parent.ChildNodes.Length);
            Assert.AreSame(child1, parent.ChildNodes[0]);
            Assert.AreSame(child0, parent.ChildNodes[1]);

            Element child2 = document.CreateElement("span");
            parent.InsertBefore(child2, child1);

            Assert.AreEqual(3, parent.ChildNodes.Length);
            Assert.AreSame(child2, parent.ChildNodes[0]);
            Assert.AreSame(child1, parent.ChildNodes[1]);
            Assert.AreSame(child0, parent.ChildNodes[2]);

            Element child3 = document.CreateElement("span");
            parent.InsertBefore(child3, child1);

            Assert.AreEqual(4, parent.ChildNodes.Length);
            Assert.AreSame(child2, parent.ChildNodes[0]);
            Assert.AreSame(child3, parent.ChildNodes[1]);
            Assert.AreSame(child1, parent.ChildNodes[2]);
            Assert.AreSame(child0, parent.ChildNodes[3]);

            Element child4 = document.CreateElement("span");
            parent.InsertBefore(child4, null);

            Assert.AreEqual(5, parent.ChildNodes.Length);
            Assert.AreSame(child2, parent.ChildNodes[0]);
            Assert.AreSame(child3, parent.ChildNodes[1]);
            Assert.AreSame(child1, parent.ChildNodes[2]);
            Assert.AreSame(child0, parent.ChildNodes[3]);
            Assert.AreSame(child4, parent.ChildNodes[4]);
        }

        [TestMethod]
        public void TestInsertBefore_AlreadyInTree()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent0 = document.CreateElement("div");
            document.DocumentElement.AppendChild(parent0);
            Element parent1 = document.CreateElement("div");
            document.DocumentElement.AppendChild(parent1);
            Element child0 = document.CreateElement("span");
            parent0.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            parent1.AppendChild(child1);
            parent1.InsertBefore(child0, child1);

            Assert.IsFalse(parent0.HasChildNodes());
            Assert.AreEqual(2, parent1.ChildNodes.Length);
            Assert.AreSame(child0, parent1.ChildNodes[0]);
            Assert.AreSame(child1, parent1.ChildNodes[1]);
        }

        [TestMethod]
        public void TestInsertBefore_DocumentFragment()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            DocumentFragment fragment = document.CreateDocumentFragment();
            Element child0 = document.CreateElement("span");
            fragment.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            fragment.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            fragment.AppendChild(child2);

            Element parent = document.CreateElement("div");
            Element child3 = document.CreateElement("span");
            parent.AppendChild(child3);
            parent.InsertBefore(fragment, child3);

            Assert.AreEqual(4, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);
            Assert.AreSame(child1, parent.ChildNodes[1]);
            Assert.AreSame(child2, parent.ChildNodes[2]);
            Assert.AreSame(child3, parent.ChildNodes[3]);
        }

        [TestMethod]
        public void TestRemoveChild()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            parent.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            parent.AppendChild(child2);
            Element child3 = document.CreateElement("span");
            parent.AppendChild(child3);

            Node actual = parent.RemoveChild(child3);
            Assert.AreSame(child3, actual);
            Assert.AreEqual(3, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);
            Assert.AreSame(child1, parent.ChildNodes[1]);
            Assert.AreSame(child2, parent.ChildNodes[2]);

            actual = parent.RemoveChild(child1);
            Assert.AreEqual(child1, actual);
            Assert.AreEqual(2, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);
            Assert.AreSame(child2, parent.ChildNodes[1]);

            actual = parent.RemoveChild(child0);
            Assert.AreEqual(child0, actual);
            Assert.AreEqual(1, parent.ChildNodes.Length);
            Assert.AreSame(child2, parent.ChildNodes[0]);
        }

        [TestMethod]
        public void TestRemoveChild_NotFoundErr()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            parent.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            parent.AppendChild(child2);

            Element child3 = document.CreateElement("span");

            ushort errorCode = 0;
            try
            {
                parent.RemoveChild(child3);
            }
            catch (DOMException e)
            {
                errorCode = e.Code;
            }

            Assert.AreEqual(DOMException.NOT_FOUND_ERR, errorCode);
        }

        [TestMethod]
        public void TestReplaceChild()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);
            Element child1 = document.CreateElement("span");

            Node actual = parent.ReplaceChild(child1, child0);

            Assert.AreSame(child0, actual);
            Assert.AreEqual(1, parent.ChildNodes.Length);
            Assert.AreSame(child1, parent.ChildNodes[0]);
        }

        [TestMethod]
        public void TestReplaceChild_FirstChild()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            parent.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            parent.AppendChild(child2);
            Element child3 = document.CreateElement("span");

            Node actual = parent.ReplaceChild(child3, child0);

            Assert.AreSame(child0, actual);
            Assert.AreEqual(3, parent.ChildNodes.Length);
            Assert.AreSame(child3, parent.ChildNodes[0]);
            Assert.AreSame(child1, parent.ChildNodes[1]);
            Assert.AreSame(child2, parent.ChildNodes[2]);
        }

        [TestMethod]
        public void TestReplaceChild_MiddleChild()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            parent.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            parent.AppendChild(child2);
            Element child3 = document.CreateElement("span");

            Node actual = parent.ReplaceChild(child3, child1);

            Assert.AreSame(child1, actual);
            Assert.AreEqual(3, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);
            Assert.AreSame(child3, parent.ChildNodes[1]);
            Assert.AreSame(child2, parent.ChildNodes[2]);
        }

        [TestMethod]
        public void TestReplaceChild_LastChild()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element child0 = document.CreateElement("span");
            parent.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            parent.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            parent.AppendChild(child2);
            Element child3 = document.CreateElement("span");

            Node actual = parent.ReplaceChild(child3, child2);

            Assert.AreSame(child2, actual);
            Assert.AreEqual(3, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);
            Assert.AreSame(child1, parent.ChildNodes[1]);
            Assert.AreSame(child3, parent.ChildNodes[2]);
        }

        [TestMethod]
        public void TestReplaceChild_DocumentFragment()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            DocumentFragment fragment = document.CreateDocumentFragment();
            Element child0 = document.CreateElement("span");
            fragment.AppendChild(child0);
            Element child1 = document.CreateElement("span");
            fragment.AppendChild(child1);
            Element child2 = document.CreateElement("span");
            fragment.AppendChild(child2);

            Element parent = document.CreateElement("div");
            Element child3 = document.CreateElement("span");
            parent.AppendChild(child3);
            
            var actual = parent.ReplaceChild(fragment, child3);

            Assert.AreSame(child3, actual);
            Assert.AreEqual(3, parent.ChildNodes.Length);
            Assert.AreSame(child0, parent.ChildNodes[0]);
            Assert.AreSame(child1, parent.ChildNodes[1]);
            Assert.AreSame(child2, parent.ChildNodes[2]);
        }

        [TestMethod]
        public void TestIsSameNode()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");

            Assert.IsTrue(node0.IsSameNode(node0));

            Element node1 = document.CreateElement("div");

            Assert.IsFalse(node0.IsSameNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");

            Assert.IsTrue(node0.IsEqualNode(node0));

            Element node1 = document.CreateElement("div");

            Assert.IsTrue(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentTypes()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");
            Attr node1 = document.CreateAttribute("div");

            Assert.IsFalse(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentTypes2()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");
            Attr node1 = document.CreateAttribute("div");

            Assert.IsFalse(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentNames()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");
            Element node1 = document.CreateElement("span");

            Assert.IsFalse(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentNamespaceURI()
        {
            Document document = DOMFactory.CreateSVGDocument();
            Element element0 = document.CreateElementNS("http://www.w3.org/1999/xhtml", "div");
            Element element1 = document.CreateElementNS("http://www.w3.org/1999/foo", "div");

            Assert.IsFalse(element0.IsEqualNode(element1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentPrefix()
        {
            Document document = DOMFactory.CreateSVGDocument();
            Element element0 = document.CreateElementNS("http://www.w3.org/1999/xhtml", "html:div");
            Element element1 = document.CreateElementNS("http://www.w3.org/1999/xhtml", "xhtml:div");

            Assert.IsFalse(element0.IsEqualNode(element1));
        }

        [TestMethod]
        public void TestIsEqualNode_SameAttributes()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("input");
            node0.SetAttribute("id", "main");
            node0.SetAttribute("class", "container");
            node0.SetAttribute("type", "button");
            Element node1 = document.CreateElement("input");
            node1.SetAttribute("class", "container");
            node1.SetAttribute("id", "main");
            node1.SetAttribute("type", "button");

            Assert.IsTrue(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentAttributes()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("input");
            node0.SetAttribute("id", "foo");
            node0.SetAttribute("class", "container");
            node0.SetAttribute("type", "button");
            Element node1 = document.CreateElement("input");
            node1.SetAttribute("class", "container");
            node1.SetAttribute("id", "main");
            node1.SetAttribute("type", "button");

            Assert.IsFalse(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentAttributes2()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("input");
            node0.SetAttribute("class", "container");
            node0.SetAttribute("type", "button");
            Element node1 = document.CreateElement("input");
            node1.SetAttribute("class", "container");
            node1.SetAttribute("id", "main");
            node1.SetAttribute("type", "button");

            Assert.IsFalse(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentAttributes3()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("input");
            node0.SetAttribute("id", "main");
            node0.SetAttribute("class", "container");
            node0.SetAttribute("type", "button");
            Element node1 = document.CreateElement("input");
            node1.SetAttribute("id", "main");
            node1.SetAttribute("type", "button");

            Assert.IsFalse(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_SameChildNodes()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("input");
            node0.AppendChild(document.CreateElement("span"));
            node0.AppendChild(document.CreateElement("input"));
            node0.AppendChild(document.CreateElement("img"));
            Element node1 = document.CreateElement("input");
            node1.AppendChild(document.CreateElement("span"));
            node1.AppendChild(document.CreateElement("input"));
            node1.AppendChild(document.CreateElement("img"));

            Assert.IsTrue(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentChildNodes()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("input");
            node0.AppendChild(document.CreateElement("foo"));
            node0.AppendChild(document.CreateElement("input"));
            node0.AppendChild(document.CreateElement("img"));
            Element node1 = document.CreateElement("input");
            node1.AppendChild(document.CreateElement("span"));
            node1.AppendChild(document.CreateElement("input"));
            node1.AppendChild(document.CreateElement("img"));

            Assert.IsFalse(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentChildNodes2()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("input");
            node0.AppendChild(document.CreateElement("img"));
            node0.AppendChild(document.CreateElement("span"));
            node0.AppendChild(document.CreateElement("input"));
            Element node1 = document.CreateElement("input");
            node1.AppendChild(document.CreateElement("span"));
            node1.AppendChild(document.CreateElement("input"));
            node1.AppendChild(document.CreateElement("img"));

            Assert.IsFalse(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentChildNodes3()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("input");
            Element node1 = document.CreateElement("input");
            node1.AppendChild(document.CreateElement("span"));
            node1.AppendChild(document.CreateElement("input"));
            node1.AppendChild(document.CreateElement("img"));

            Assert.IsFalse(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestIsEqualNode_DifferentChildNodes4()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("input");
            Element node1 = document.CreateElement("input");
            node1.AppendChild(document.CreateElement("span"));
            node1.AppendChild(document.CreateElement("input"));
            node1.AppendChild(document.CreateElement("img"));

            Assert.IsFalse(node0.IsEqualNode(node1));
        }

        [TestMethod]
        public void TestCompareDocumentPosition()
        {
            //               DocumentElement
            //         +-----------+-----------+
            //       node0                   node1  

            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node0);
            Element node1 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node1);

            var expected = DOMDocumentPosition.DOCUMENT_POSITION_FOLLOWING;
            var actual = node0.CompareDocumentPosition(node1);

            Assert.AreEqual(expected, actual);

            expected = DOMDocumentPosition.DOCUMENT_POSITION_PRECEDING;
            actual = node1.CompareDocumentPosition(node0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCompareDocumentPosition_Siblings()
        {
            //               DocumentElement
            //         +-----------+-----------+
            //         |           |           |
            //       node0       node1       node2  

            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node0);
            Element node1 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node1);
            Element node2 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node2);

            var expected = DOMDocumentPosition.DOCUMENT_POSITION_FOLLOWING;
            var actual = node0.CompareDocumentPosition(node2);

            Assert.AreEqual(expected, actual);

            expected = DOMDocumentPosition.DOCUMENT_POSITION_PRECEDING;
            actual = node2.CompareDocumentPosition(node0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCompareDocumentPosition_Parent()
        {
            //               DocumentElement
            //                      |
            //                    node0
            //                      |
            //                    node1

            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node0);
            Element node1 = document.CreateElement("div");
            node0.AppendChild(node1);

            var expected = DOMDocumentPosition.DOCUMENT_POSITION_CONTAINED_BY;
            var actual = node0.CompareDocumentPosition(node1);

            Assert.AreEqual(expected, actual);

            expected = DOMDocumentPosition.DOCUMENT_POSITION_CONTAINS;
            actual = node1.CompareDocumentPosition(node0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCompareDocumentPosition_Grandparent()
        {
            //               DocumentElement
            //                      |
            //                    node0
            //                      |
            //                    node1
            //                      |
            //                    node2

            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node0);
            Element node1 = document.CreateElement("div");
            node0.AppendChild(node1);
            Element node2 = document.CreateElement("div");
            node1.AppendChild(node2);

            var expected = DOMDocumentPosition.DOCUMENT_POSITION_CONTAINED_BY;
            var actual = node0.CompareDocumentPosition(node2);

            Assert.AreEqual(expected, actual);

            expected = DOMDocumentPosition.DOCUMENT_POSITION_CONTAINS;
            actual = node2.CompareDocumentPosition(node0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCompareDocumentPosition_Cousins()
        {
            //               DocumentElement
            //         +-----------+-----------+
            //       node0                   node1  
            //         |                       |
            //       node2                   node3
            //

            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node0);
            Element node1 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node1);
            Element node2 = document.CreateElement("div");
            node0.AppendChild(node2);
            Element node3 = document.CreateElement("div");
            node1.AppendChild(node3);

            var expected = DOMDocumentPosition.DOCUMENT_POSITION_FOLLOWING;
            var actual = node2.CompareDocumentPosition(node3);

            Assert.AreEqual(expected, actual);

            expected = DOMDocumentPosition.DOCUMENT_POSITION_PRECEDING;
            actual = node3.CompareDocumentPosition(node2);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCompareDocumentPosition_Uncle()
        {
            //              DocumentElement
            //         +-----------+-----------+
            //       node0                   node1  
            //                                 |
            //                               node2
            //

            Document document = DOMFactory.CreateHTMLDocument();
            Element node0 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node0);
            Element node1 = document.CreateElement("div");
            document.DocumentElement.AppendChild(node1);
            Element node2 = document.CreateElement("div");
            node1.AppendChild(node2);

            var expected = DOMDocumentPosition.DOCUMENT_POSITION_PRECEDING;
            var actual = node2.CompareDocumentPosition(node0);

            Assert.AreEqual(expected, actual);

            expected = DOMDocumentPosition.DOCUMENT_POSITION_FOLLOWING;
            actual = node0.CompareDocumentPosition(node2);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCloneNode()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            document.DocumentElement.AppendChild(element);

            Element clone = (Element)element.CloneNode(false);

            Assert.AreEqual("div", clone.NodeName);
            Assert.IsNull(clone.ParentNode);
            Assert.IsTrue(element.IsEqualNode(clone));
            Assert.AreNotSame(element, clone);
        }

        [TestMethod]
        public void TestCloneNode_WithAttributes()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            document.DocumentElement.AppendChild(element);
            element.SetAttribute("id", "main");
            element.SetAttribute("class", "container");

            Element clone = (Element)element.CloneNode(false);

            Assert.AreEqual("div", clone.NodeName);
            Assert.AreEqual("main", clone.GetAttribute("id"));
            Assert.AreEqual("container", clone.GetAttribute("class"));
        }

        [TestMethod]
        public void TestCloneNode_Shallow()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            document.DocumentElement.AppendChild(parent);
            Element child = document.CreateElement("span");
            parent.AppendChild(child);

            Element clone = (Element)parent.CloneNode(false);

            Assert.AreEqual("div", clone.NodeName);
            Assert.IsFalse(clone.HasChildNodes());
        }

        [TestMethod]
        public void TestCloneNode_Deep()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            document.DocumentElement.AppendChild(parent);
            Element child = document.CreateElement("span");
            parent.AppendChild(child);

            Element clone = (Element)parent.CloneNode(true);

            Assert.AreEqual("div", clone.NodeName);
            Assert.AreEqual(1, clone.ChildNodes.Length);
            Assert.AreEqual("span", clone.ChildNodes[0].NodeName);
            Assert.IsTrue(child.IsEqualNode(clone.ChildNodes[0]));
            Assert.AreNotSame(child, clone.ChildNodes[0]);
        }

        [TestMethod]
        public void TestNormalize()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            Text text0 = document.CreateTextNode("foo");
            element.AppendChild(text0);
            Text text1 = document.CreateTextNode("bar");
            element.AppendChild(text1);

            element.Normalize();

            Assert.AreEqual(1, element.ChildNodes.Length);
            Assert.AreEqual(NodeType.TEXT_NODE, element.ChildNodes[0].NodeType);
            Assert.AreEqual("foobar", element.ChildNodes[0].NodeValue);
        }

        [TestMethod]
        public void TestNormalize2()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            Text text0 = document.CreateTextNode("foo");
            element.AppendChild(text0);
            Element middle = document.CreateElement("span");
            element.AppendChild(middle);
            Text text1 = document.CreateTextNode("bar");
            element.AppendChild(text1);

            element.Normalize();

            Assert.AreEqual(3, element.ChildNodes.Length);
            Assert.AreEqual("foo", element.ChildNodes[0].NodeValue);
            Assert.AreEqual("span", element.ChildNodes[1].NodeName);
            Assert.AreEqual("bar", element.ChildNodes[2].NodeValue);
        }

        [TestMethod]
        public void TestNormalize_Empty()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            Text text0 = document.CreateTextNode("");
            element.AppendChild(text0);

            element.Normalize();

            Assert.IsFalse(element.HasChildNodes());
        }

        [TestMethod]
        public void TestNormalize_Empty2()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            Text text0 = document.CreateTextNode("foo");
            element.AppendChild(text0);
            Text text1 = document.CreateTextNode("");
            element.AppendChild(text1);

            element.Normalize();

            Assert.AreEqual(1, element.ChildNodes.Length);
            Assert.AreEqual(NodeType.TEXT_NODE, element.ChildNodes[0].NodeType);
            Assert.AreEqual("foo", element.ChildNodes[0].NodeValue);
        }

        [TestMethod]
        public void TestNormalize_Empty3()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");
            Text text0 = document.CreateTextNode("");
            element.AppendChild(text0);
            Text text1 = document.CreateTextNode("bar");
            element.AppendChild(text1);

            element.Normalize();

            Assert.AreEqual(1, element.ChildNodes.Length);
            Assert.AreEqual(NodeType.TEXT_NODE, element.ChildNodes[0].NodeType);
            Assert.AreEqual("bar", element.ChildNodes[0].NodeValue);
        }

        [TestMethod]
        public void TestNormalize_Deep()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element parent = document.CreateElement("div");
            Element element = document.CreateElement("div");
            parent.AppendChild(element);
            Text text0 = document.CreateTextNode("foo");
            element.AppendChild(text0);
            Text text1 = document.CreateTextNode("bar");
            element.AppendChild(text1);

            parent.Normalize();

            Assert.AreEqual(1, element.ChildNodes.Length);
            Assert.AreEqual(NodeType.TEXT_NODE, element.ChildNodes[0].NodeType);
            Assert.AreEqual("foobar", element.ChildNodes[0].NodeValue);
        }

        [TestMethod]
        public void TestIsSupported()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            bool expected = true;
            bool actual = element.IsSupported(DOMFeatures.Core, "3.0");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestIsSupported_False()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element element = document.CreateElement("div");

            bool expected = false;
            bool actual = element.IsSupported("foo", "3.0");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetTextContent()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element div = document.CreateElement("div");
            div.AppendChild(document.CreateTextNode("This is "));
            Element span = document.CreateElement("span");
            span.AppendChild(document.CreateTextNode("some"));
            div.AppendChild(span);
            div.AppendChild(document.CreateTextNode(" text"));

            var expected = "This is some text";
            var actual = div.TextContent;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSetTextContent()
        {
            Document document = DOMFactory.CreateHTMLDocument();
            Element div = document.CreateElement("div");
            div.AppendChild(document.CreateTextNode("This is "));
            Element span = document.CreateElement("span");
            span.AppendChild(document.CreateTextNode("some"));
            div.AppendChild(span);
            div.AppendChild(document.CreateTextNode(" text"));

            div.TextContent = "The quick brown fox jumped over the lazy dog";

            Assert.AreEqual(1, div.ChildNodes.Length);
            Assert.AreEqual(NodeType.TEXT_NODE, div.ChildNodes[0].NodeType);
            Assert.AreEqual("The quick brown fox jumped over the lazy dog", div.ChildNodes[0].NodeValue);
        }

    }
}
