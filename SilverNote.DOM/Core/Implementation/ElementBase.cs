using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DOM.LS;

namespace DOM.Internal
{
    public class ElementBase : NodeProxy, Element, ElementLS, ElementContext
    {
        #region Fields

        AttrMap _Attributes;
        List<string> _AttributeNames;

        #endregion

        #region Constructors

        internal ElementBase(Document ownerDocument, string nodeName)
            : base(ownerDocument, nodeName, NodeType.ELEMENT_NODE)
        {

        }

        internal ElementBase(Document ownerDocument, string namespaceURI, string qualifiedName)
            : base(ownerDocument, namespaceURI, qualifiedName, NodeType.ELEMENT_NODE)
        {

        }

        #endregion

        #region Node

        public override Node CloneNode(bool deep)
        {
            Element result;

            if (LocalName != null)
            {
                result = OwnerDocument.CreateElementNS(NamespaceURI, NodeName);
            }
            else
            {
                result = OwnerDocument.CreateElement(NodeName);
            }

            if (HasAttributes())
            {
                foreach (Attr attr in Attributes)
                {
                    var clone = (Attr)attr.CloneNode(true);

                    result.SetAttributeNode(clone);
                }
            }

            if (deep)
            {
                foreach (var childNode in ChildNodes)
                {
                    var clone = childNode.CloneNode(true);

                    result.AppendChild(clone);
                }
            }

            return result;
        }

        public override bool IsSupported(string feature, string version)
        {
            feature = feature.ToLower();

            switch (feature)
            {
                case DOMFeatures.LS:
                    return String.IsNullOrEmpty(version) || version == "3.0";
                default:
                    return base.IsSupported(feature, version);
            }
        }

        #endregion

        #region Element

        public string TagName 
        {
            get { return NodeName; }
        }

        public override NamedNodeMap Attributes
        {
            get 
            {
                if (Source != null)
                {
                    var result = new AttrMap(this);

                    var attributes = Source.GetNodeAttributes(this);
                    foreach (var attr in attributes)
                    {
                        string value = Source.GetNodeAttribute(this, attr);
                        if (value != null)
                        {
                            result.SetAttribute(attr, value);
                        }
                    }

                    return result;
                }
                else
                {
                    return InternalAttributes;
                }
            }
        }

        public override bool HasAttributes()
        {
            if (Source != null)
            {
                return Source.GetNodeAttributes(this).Count > 0;
            }
            else
            {
                return HasInternalAttributes();
            }
        }

        public virtual bool HasAttribute(string name)
        {
            if (Source != null)
            {
                return Source.GetNodeAttribute(this, name) != null;
            }
            else
            {
                return HasInternalAttribute(name);
            }
        }

        public virtual string GetAttribute(string name)
        {
            if (Source != null)
            {
                return Source.GetNodeAttribute(this, name);
            }
            else
            {
                return GetInternalAttribute(name);
            }
        }

        public virtual void SetAttribute(string name, string value)
        {
            if (Source != null)
            {
                Source.SetNodeAttribute(this, name, value);
            }
            else
            {
                SetInternalAttribute(name, value);
            }
        }

        public virtual void RemoveAttribute(string name)
        {
            if (Source != null)
            {
                Source.ResetNodeAttribute(this, name);
            }
            else
            {
                RemoveInternalAttribute(name);
            }
        }

        public Attr GetAttributeNode(string name)
        {
            string value = GetAttribute(name);
            if (value == null)
                return null;
            Attr attr = OwnerDocument.CreateAttribute(name);
            attr.Value = value;
            return attr;
        }

        public Attr SetAttributeNode(Attr newAttr)
        {
            var oldAttr = GetAttributeNode(newAttr.Name);
            SetAttribute(newAttr.Name, newAttr.Value);
            return oldAttr;
        }

        public Attr RemoveAttributeNode(Attr oldAttr)
        {
            RemoveAttribute(oldAttr.Name);
            return oldAttr;
        }

        public virtual bool HasAttributeNS(string namespaceURI, string localName)
        {
            if (Source != null)
            {
                return Source.GetNodeAttribute(this, localName) != null;
            }
            else if (_Attributes != null)
            {
                return _Attributes.HasAttributeNS(namespaceURI, localName);
            }
            else
            {
                return false;
            }
        }

        public virtual string GetAttributeNS(string namespaceURI, string localName)
        {
            if (Source != null)
            {
                return Source.GetNodeAttribute(this, localName);
            }
            else if (_Attributes != null)
            {
                return _Attributes.GetAttributeNS(namespaceURI, localName);
            }
            else
            {
                return String.Empty;
            }
        }

        public virtual void SetAttributeNS(string namespaceURI, string qualifiedName, string value)
        {
            if (Source != null)
            {
                Source.SetNodeAttribute(this, qualifiedName, value);
            }
            else
            {
                InternalAttributes.SetAttributeNS(namespaceURI, qualifiedName, value);
            }
        }

        public virtual void RemoveAttributeNS(string namespaceURI, string localName)
        {
            if (Source != null)
            {
                Source.ResetNodeAttribute(this, localName);
            }
            else if (_Attributes != null)
            {
                _Attributes.RemoveAttributeNS(namespaceURI, localName);
            }
        }

        public Attr GetAttributeNodeNS(string namespaceURI, string localName)
        {
            string value = GetAttributeNS(namespaceURI, localName);
            if (value == null)
                return null;
            Attr attr = OwnerDocument.CreateAttributeNS(namespaceURI, localName);
            attr.Value = value;
            return attr;
        }

        public Attr SetAttributeNodeNS(Attr newAttr)
        {
            var oldAttr = GetAttributeNodeNS(newAttr.NamespaceURI, newAttr.LocalName);
            string qualifiedName = newAttr.LocalName;
            if (!String.IsNullOrEmpty(newAttr.Prefix))
                qualifiedName = newAttr.Prefix + ":" + newAttr.LocalName;
            SetAttributeNS(newAttr.NamespaceURI, qualifiedName, newAttr.Value);
            return oldAttr;
        }

        public void SetIdAttribute(string name, bool isId)
        {
            throw new NotImplementedException();
        }

        public void SetIdAttributeNS(string namespaceURI, string localName, bool isId)
        {
            throw new NotImplementedException();
        }

        public void SetIdAttributeNode(Attr idAttr, bool isId)
        {
            throw new NotImplementedException();
        }

        public NodeList GetElementsByTagName(string tagName)
        {
            return new DynamicNodeList(this, tagName);
        }

        public NodeList GetElementsByTagNameNS(string namespaceURI, string localName)
        {
            return new DynamicNodeList(this, namespaceURI, localName);
        }

        public TypeInfo SchemaTypeInfo
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region ElementLS

        public string MarkupContent
        {
            get
            {
                if (!HasChildNodes())
                {
                    return String.Empty;
                }

                var dom = OwnerDocument.Implementation as DOMImplementationLS;
                if (dom == null)
                {
                    return String.Empty;
                }

                var buffer = new StringBuilder();
                var serializer = dom.CreateLSSerializer();

                foreach (var node in ChildNodes)
                {
                    string markup = serializer.WriteToString(node);
                    buffer.Append(markup);
                }

                return buffer.ToString();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ElementContext

        static readonly string[] _EmptyAttributes = new string[0];

        public bool HasInternalAttributes()
        {
            if (_Attributes != null)
            {
                return _Attributes.Length > 0;
            }
            else
            {
                return false;
            }
        }

        public bool HasInternalAttribute(string name)
        {
            if (_Attributes != null)
            {
                return _Attributes.HasAttribute(name);
            }
            else
            {
                return false;
            }
        }

        public IList<string> GetInternalAttributes()
        {
            if (_AttributeNames != null)
            {
                return _AttributeNames;
            }
            else
            {
                return _EmptyAttributes;
            }
        }

        public string GetInternalAttribute(string name)
        {
            if (_Attributes != null)
            {
                return _Attributes.GetAttribute(name);
            }
            else
            {
                return String.Empty;
            }
        }

        public void SetInternalAttribute(string name, string value)
        {
            InternalAttributes.SetAttribute(name, value);

            int i = InternalAttributeNames.IndexOf(name);
            if (i == -1)
            {
                InternalAttributeNames.Add(name);
            }
        }

        public void RemoveInternalAttribute(string name)
        {
            if (_Attributes != null)
            {
                _Attributes.RemoveAttribute(name);
            }

            if (_AttributeNames != null)
            {
                _AttributeNames.Remove(name);
            }
        }

        #endregion

        #region Selectors

        public Element QuerySelector(string selectors)
        {
            return DOMSelectors.QuerySelector(this, selectors);
        }

        public NodeList QuerySelectorAll(string selectors)
        {
            return DOMSelectors.QuerySelectorAll(this, selectors);
        }

        #endregion

        #region Extensions

        /// <summary>
        /// Get the first descendant element with the given name
        /// </summary>
        /// <param name="tagName">Name of the element to look for</param>
        /// <param name="deep">true to search all descendants, false to search immediate children only</param>
        /// <returns>An element, or null if not found</returns>
        public Element GetElementByTagName(string tagName, bool deep)
        {
            return GetElementByTagName(this, tagName, deep);
        }

        /// <summary>
        /// Get the first descendant element with the given name
        /// </summary>
        /// <param name="root">Root element to be searched</param>
        /// <param name="tagName">Name of the element to look for</param>
        /// <param name="deep">true to search all descendants, false to search immediate children only</param>
        /// <returns>An element, or null if not found</returns>
        static Element GetElementByTagName(Element root, string tagName, bool deep)
        {
            for (Node child = root.FirstChild; child != null; child = child.NextSibling)
            {
                if (child.NodeType == NodeType.ELEMENT_NODE)
                {
                    Element element = (Element)child;

                    if (String.Equals(tagName, element.TagName, StringComparison.OrdinalIgnoreCase))
                    {
                        return element;
                    }

                    if (deep)
                    {
                        element = GetElementByTagName(element, tagName, deep);
                        if (element != null)
                        {
                            return element;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the ancestor element with the given name
        /// </summary>
        /// <param name="tagName">Element name to look for</param>
        /// <returns>An element, or null if not found</returns>
        public Element GetAncestorByTagName(string tagName)
        {
            return GetAncestorByTagName(tagName, int.MaxValue);
        }

        /// <summary>
        /// Get the ancestor element with the given name
        /// </summary>
        /// <param name="tagName">Element name to look for</param>
        /// <param name="maxLevels">Maximum number of ancestors to search</param>
        /// <returns>An element, or null if not found</returns>
        public Element GetAncestorByTagName(string tagName, int maxLevels)
        {
            return GetAncestorByTagName(this, tagName, maxLevels);
        }

        /// <summary>
        /// Get the ancestor element with the given name
        /// </summary>
        /// <param name="node">Node whose ancestors are to be searched</param>
        /// <param name="tagName">Element name to look for</param>
        /// <param name="maxLevels">Maximum number of ancestors to search</param>
        /// <returns>An element, or null if not found</returns>
        static Element GetAncestorByTagName(Node node, string tagName, int maxLevels)
        {
            if (maxLevels == 0)
            {
                return null;
            }

            Node parent = node.ParentNode;

            if (parent == null)
            {
                return null;
            }

            if (parent.NodeType == NodeType.ELEMENT_NODE)
            {
                Element element = (Element)node;

                if (String.Equals(element.TagName, tagName, StringComparison.OrdinalIgnoreCase))
                {
                    return element;
                }
            }

            return GetAncestorByTagName(parent, tagName, maxLevels - 1);
        }

        #endregion

        #region Implementation

        AttrMap InternalAttributes
        {
            get
            {
                if (_Attributes == null)
                {
                    _Attributes = new AttrMap(this);
                }

                return _Attributes;
            }
        }

        List<string> InternalAttributeNames
        {
            get
            {
                if (_AttributeNames == null)
                {
                    _AttributeNames = new List<string>();
                }

                return _AttributeNames;
            }
        }

        #endregion

    }
}
