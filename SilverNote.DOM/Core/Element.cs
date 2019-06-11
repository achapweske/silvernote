/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM
{
    public partial interface Element : Node
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-104682815:
        /// "The name of the element. If Node.localName is different from null, 
        /// this attribute is a qualified name... Note that this is case-preserving 
        /// in XML, as are all of the operations of the DOM. The HTML DOM returns 
        /// the tagName of an HTML element in the canonical uppercase form, 
        /// regardless of the case in the source HTML document."
        /// </remarks>
        string TagName { get; }

        string GetAttribute(string name);
        void SetAttribute(string name, string value);
        void RemoveAttribute(string name);
        Attr GetAttributeNode(string name);
        Attr SetAttributeNode(Attr newAttr);
        Attr RemoveAttributeNode(Attr oldAttr);
        NodeList GetElementsByTagName(string name);
        string GetAttributeNS(string namespaceURI, string localName);
        void SetAttributeNS(string namespaceURI, string qualifiedName, string value);
        void RemoveAttributeNS(string namespaceURI, string localName);
        Attr GetAttributeNodeNS(string namespaceURI, string localName);
        Attr SetAttributeNodeNS(Attr newAttr);
        NodeList GetElementsByTagNameNS(string namespaceURI, string localName);
        bool HasAttribute(string name);
        bool HasAttributeNS(string namespaceURI, string localName);
        TypeInfo SchemaTypeInfo { get; }
        void SetIdAttribute(string name, bool isId);
        void SetIdAttributeNS(string namespaceURI, string localName, bool isId);
        void SetIdAttributeNode(Attr idAttr, bool isId);
    }
}
