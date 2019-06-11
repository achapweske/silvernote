/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace DOM.XPath.Internal
{
    internal class XPathDOMNavigator : XPathNavigator
    {
        #region Fields

        private Document _Document;
        private Node _CurrentNode;
        private int _CurrentIndex;
        private NameTable _NameTable;

        #endregion

        #region Constructors

        public XPathDOMNavigator(Document document)
            : this(document, document.DocumentElement)
        {

        }

        public XPathDOMNavigator(Document document, Node currentNode)
        {
            _Document = document;
            _CurrentNode = currentNode;
            _CurrentIndex = -1;
            _NameTable = new NameTable();
        }

        public XPathDOMNavigator(XPathDOMNavigator other)
        {
            _Document = other._Document;
            _CurrentNode = other._CurrentNode;
            _CurrentIndex = other._CurrentIndex;
            _NameTable = other._NameTable;
        }

        #endregion

        #region Properties

        public override string BaseURI
        {
            get { return String.Empty; }
        }

        public Node CurrentNode
        {
            get { return _CurrentNode; }
        }

        public override bool HasAttributes
        {
            get { return _CurrentNode.HasAttributes(); }
        }

        public override bool HasChildren
        {
            get { return _CurrentNode.HasChildNodes(); }
        }

        public override bool IsEmptyElement
        {
            get { return !HasChildren; }
        }

        public override string LocalName
        {
            get { return Atomize(_CurrentNode.NodeName); }
        }

        public override string Name
        {
            get { return Atomize(_CurrentNode.NodeName); }
        }

        public override string NamespaceURI
        {
            get { return Atomize(String.Empty); }
        }

        public override XmlNameTable NameTable
        {
            get { return _NameTable; }
        }

        public override XPathNodeType NodeType
        {
            get 
            {
                switch (_CurrentNode.NodeType)
                {
                    case DOM.NodeType.ELEMENT_NODE:
                        return XPathNodeType.Element;
                    case DOM.NodeType.ATTRIBUTE_NODE:
                        return XPathNodeType.Attribute;
                    case DOM.NodeType.TEXT_NODE:
                        return XPathNodeType.Text;
                    case DOM.NodeType.PROCESSING_INSTRUCTION_NODE:
                        return XPathNodeType.ProcessingInstruction;
                    case DOM.NodeType.COMMENT_NODE:
                        return XPathNodeType.Comment;
                    case DOM.NodeType.DOCUMENT_NODE:
                        return XPathNodeType.Root;
                    default:
                        // throw new NotImplementedException();
                        return XPathNodeType.Comment;
                }
            }
        }

        public override string Prefix
        {
            get 
            {
                return Atomize(String.Empty); 
            }
        }

        public override string Value
        {
            get
            {
                switch (_CurrentNode.NodeType)
                {
                    case DOM.NodeType.ELEMENT_NODE:
                        return _CurrentNode.InnerText;
                    default:
                        return _CurrentNode.NodeValue;
                }
            }
        }

        public override string XmlLang
        {
            get
            {
                return Atomize(String.Empty);
            }
        }

        #endregion

        #region Methods

        public override XPathNavigator Clone()
        {
            return new XPathDOMNavigator(this);
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            var element = _CurrentNode as Element;

            if (element == null)
            {
                return null;
            }

            if (!element.HasAttribute(localName))
            {
                return null;
            }

            return element.GetAttribute(localName);
        }

        public override string GetNamespace(string name)
        {
            return String.Empty;
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            var domNavigator = other as XPathDOMNavigator;
            
            if (domNavigator != null)
            {
                return domNavigator._CurrentNode == this._CurrentNode;
            }
            else
            {
                return false;
            }
        }

        public override bool MoveTo(XPathNavigator other)
        {
            var domNavigator = other as XPathDOMNavigator;

            if (domNavigator == null)
            {
                return false;
            }

            if (domNavigator._Document != this._Document)
            {
                return false;
            }

            _CurrentNode = domNavigator._CurrentNode;
            _CurrentIndex = domNavigator._CurrentIndex;

            return true;
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            if (!_CurrentNode.HasAttributes())
            {
                return false;
            }

            Node node = null;
            int index = 0;

            foreach (Node attr in _CurrentNode.Attributes)
            {
                if (attr.NodeName == localName)
                {
                    node = attr;
                    break;
                }
                index++;
            }

            if (node == null)
            {
                return false;
            }

            _CurrentNode = node;
            _CurrentIndex = index;

            return true;
        }

        public override bool MoveToFirst()
        {
            var parent = _CurrentNode.ParentNode;

            if (parent == null)
            {
                return false;
            }

            var first = parent.FirstChild;

            if (first == null)
            {
                return false;
            }

            _CurrentNode = first;
            _CurrentIndex = -1;

            return true;
        }

        public override bool MoveToFirstAttribute()
        {
            var firstAttribute = _CurrentNode.Attributes[0];

            if (firstAttribute == null)
            {
                return false;
            }

            _CurrentNode = firstAttribute;
            _CurrentIndex = 0;

            return true;
        }

        public override bool MoveToFirstChild()
        {
            var firstChild = _CurrentNode.FirstChild;

            if (firstChild == null)
            {
                return false;
            }

            _CurrentNode = firstChild;
            _CurrentIndex = -1;

            return true;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToId(string id)
        {
            var node = _Document.GetElementById(id);

            if (node != null)
            {
                _CurrentNode = node;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool MoveToNamespace(string name)
        {
            return false;
        }

        public override bool MoveToNext()
        {
            var next = _CurrentNode.NextSibling;

            if (next == null)
            {
                return false;
            }

            _CurrentNode = next;
            _CurrentIndex = -1;

            return true;
        }

        public override bool MoveToNextAttribute()
        {
            if (_CurrentNode.NodeType != DOM.NodeType.ATTRIBUTE_NODE)
            {
                return false;
            }

            Element ownerElement = ((Attr)_CurrentNode).OwnerElement;
            if (ownerElement == null)
            {
                return false;
            }

            var attributes = ownerElement.Attributes;
            if (attributes == null)
            {
                return false;
            }

            int nextIndex = _CurrentIndex + 1;
            Node nextAttribute = attributes[nextIndex];
            if (nextAttribute == null)
            {
                return false;
            }

            _CurrentNode = nextAttribute;
            _CurrentIndex = nextIndex;

            return true;
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToParent()
        {
            var parent = _CurrentNode.ParentNode;

            if (parent == null)
            {
                return false;
            }

            _CurrentNode = parent;
            _CurrentIndex = -1;

            return true;
        }

        public override bool MoveToPrevious()
        {
            var previous = _CurrentNode.PreviousSibling;

            if (previous == null)
            {
                return false;
            }

            _CurrentNode = previous;
            _CurrentIndex = -1;

            return true;
        }

        public override void MoveToRoot()
        {
            _CurrentNode = _Document;
            _CurrentIndex = -1;
        }

        #endregion

        #region Implementation

        private string Atomize(string name)
        {
            string result = _NameTable.Get(name);

            if (result == null)
            {
                result = _NameTable.Add(name);
            }

            return result;
        }

        #endregion
    }
}
