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
    public partial interface Document : Node
    {
        DocumentType DocType { get; }
        DOMImplementation Implementation { get; }
        Element DocumentElement { get; }
        Element CreateElement(string tagName);
        DocumentFragment CreateDocumentFragment();
        Text CreateTextNode(string data);
        Comment CreateComment(string data);
        CDATASection CreateCDATASection(string data);
        ProcessingInstruction CreateProcessingInstruction(string target, string data);
        Attr CreateAttribute(string name);
        EntityReference CreateEntityReference(string name);
        NodeList GetElementsByTagName(string tagName);
        Node ImportNode(Node importedNode, bool deep);
        Element CreateElementNS(string namespaceURI, string qualifiedName);
        Attr CreateAttributeNS(string namespaceURI, string qualifiedName);
        NodeList GetElementsByTagNameNS(string namespaceURI, string localName);
        Element GetElementById(string elementId);
        string InputEncoding { get; }
        string XmlEncoding { get; }
        bool XmlStandalone { get; set; }
        string XmlVersion { get; set; }
        bool StrictErrorChecking { get; set; }
        string DocumentURI { get; set; }
        Node AdoptNode(Node source);
        DOMConfiguration DomConfig { get; }
        void NormalizeDocument();
        Node RenameNode(Node n, string namespaceURI, string qualifiedName);
    }
}
