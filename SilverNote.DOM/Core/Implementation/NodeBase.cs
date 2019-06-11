using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using DOM.Events;

namespace DOM.Internal
{
    public delegate void NodeChangedEventHandler(object sender, NodeChangedEventArgs e);

    public class NodeChangedEventArgs : EventArgs
    {
        public NodeChangedEventArgs(Node refNode, Node oldValue)
        {
            RefNode = refNode;
            OldValue = oldValue;
        }

        public readonly Node RefNode;
        public readonly Node OldValue;
    }

    /// <summary>
    /// http://www.w3.org/TR/DOM-Level-2-Core/core.html
    /// </summary>
    public abstract class NodeBase : Node, EventTarget, IXPathNavigable, ScriptTarget, IEnumerable<Node>
    {
        #region Fields

        Document _OwnerDocument;
        NodeType _NodeType;
        string _NodeName;
        string _NamespaceURI;
        string _Prefix;
        string _LocalName;
        Node _ParentNode;
        Node _FirstChild;
        Node _LastChild;
        Node _PreviousSibling;
        Node _NextSibling;

        #endregion

        #region Constructors

        public NodeBase(Document ownerDocument, NodeType nodeType, string nodeName)
        {
            _OwnerDocument = ownerDocument;
            _NodeType = nodeType;
            _NodeName = nodeName;

            if (ownerDocument is DOM.HTML.HTMLDocument)
            {
                _NodeName = _NodeName.ToLower();
            }
        }

        public NodeBase(Document ownerDocument, NodeType nodeType, string namespaceURI, string qualifiedName)
        {
            _OwnerDocument = ownerDocument;
            _NodeType = nodeType;
            _NamespaceURI = namespaceURI;
            _NodeName = qualifiedName;
            ParseQualifiedName(qualifiedName, out _Prefix, out _LocalName);
        }

        #endregion

        #region Node

        public virtual Document OwnerDocument
        {
            get { return _OwnerDocument; }
        }

        public virtual NodeType NodeType
        {
            get { return _NodeType; }
        }

        public virtual string NodeName
        {
            get { return _NodeName; }
        }

        public virtual string NamespaceURI
        {
            get { return _NamespaceURI; }
        }

        public virtual string BaseURI
        {
            get 
            {
                if (OwnerDocument.IsSupported("HTML", ""))
                {
                    // If the Document supports the feature "HTML", the base
                    // URI is specified by the BASE element. If this element 
                    // does not exist, default to the document's DocumentURI.

                    throw new NotImplementedException();
                }
                else
                {
                    // See XML Base: http://www.w3.org/TR/xmlbase/

                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-NodeNSPrefix
        /// </summary>
        public virtual string Prefix
        {
            get 
            { 
                return _Prefix; 
            }
            set 
            {
                // "Setting the prefix to null makes it unspecified"

                if (value == null)
                {
                    _Prefix = null;
                    _NodeName = _LocalName;
                    return;
                }

                // "INVALID_CHARACTER_ERR: Raised if the specified prefix 
                // contains an illegal character according to the XML version 
                // in use specified in the Document.xmlVersion attribute."
                //
                // See the following link for valid prefix characters:
                // http://www.w3.org/TR/2006/REC-xml-names11-20060816/#ns-qualnames
                //
                // We are tolerant here. Except for the most blatent invalid characters,
                // let the serializer/parser decide what's acceptable.

                if (value.IndexOfAny(": \t\r\n".ToCharArray()) != -1)
                {
                    throw new DOMException(DOMException.INVALID_CHARACTER_ERR);
                }

                // "NAMESPACE_ERR: Raised if the specified prefix is malformed 
                // per the Namespaces in XML specification, if the namespaceURI 
                // of this node is null, if the specified prefix is "xml" and 
                // the namespaceURI of this node is different from 
                // "http://www.w3.org/XML/1998/namespace", if this node is an 
                // attribute and the specified prefix is "xmlns" and the 
                // namespaceURI of this node is different from 
                // "http://www.w3.org/2000/xmlns/", or if this node is an 
                // attribute and the qualifiedName of this node is "xmlns" 
                // [XML Namespaces].

                if (NamespaceURI == null ||
                    value == "xml" && NamespaceURI != "http://www.w3.org/XML/1998/namespace")
                {
                    throw new DOMException(DOMException.NAMESPACE_ERR);
                }

                if (NodeType == NodeType.ATTRIBUTE_NODE)
                {
                    if (value == "xmlns" && NamespaceURI != "http://www.w3.org/2000/xmlns/" ||
                        NodeName == "xmlns")
                    {
                        throw new DOMException(DOMException.NAMESPACE_ERR);
                    }
                }

                _Prefix = value;
                _NodeName = FormatQualifiedName(value, _LocalName);
            }
        }

        public virtual string LocalName
        {
            get { return _LocalName; }
        }

        public virtual string LookupNamespaceURI(string prefix)
        {
            return LookupNamespaceURI(this, prefix);
        }

        public virtual string LookupPrefix(string namespaceURI)
        {
            return LookupPrefix(this, namespaceURI);
        }

        public virtual bool IsDefaultNamespace(string namespaceURI)
        {
            return IsDefaultNamespace(this, namespaceURI);
        }

        public virtual string NodeValue 
        {
            get { return null; }
            set {  }
        }

        public virtual NamedNodeMap Attributes 
        {
            get { return null; }
        }

        public virtual bool HasAttributes()
        {
            return false;
        }

        public virtual NodeList ChildNodes
        {
            get 
            {
                var results = new List<Node>();

                for (var node = FirstChild; node != null; node = node.NextSibling)
                {
                    results.Add(node);
                }

                return new NodeListBase(results);
            }
        }

        public virtual Node ParentNode
        {
            get { return _ParentNode; }
        }

        public virtual Node FirstChild
        {
            get { return _FirstChild; }
        }

        public virtual Node LastChild
        {
            get { return _LastChild; }
        }

        public virtual Node PreviousSibling
        {
            get { return _PreviousSibling; }
        }

        public virtual Node NextSibling
        {
            get { return _NextSibling; }
        }

        public virtual Node AppendChild(Node newChild)
        {
            return InsertBefore(newChild, null);
        }

        public virtual Node InsertBefore(Node newChild, Node refChild)
        {
            // http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-952280727:
            
            var newChildNode = newChild as NodeBase;

            if (newChildNode == null)
            {
                throw new ArgumentException();
            }

            // If newChild is a DocumentFragment object, all of its children are inserted, in the same order, before refChild. 

            if (newChild.NodeType == DOM.NodeType.DOCUMENT_FRAGMENT_NODE)
            {
                foreach (var fragmentChild in newChild.ChildNodes.ToArray())
                {
                    InsertBefore(fragmentChild, refChild);
                }
                return newChild;
            }

            // WRONG_DOCUMENT_ERR: Raised if newChild was created from a different document than the one that created this node.

            if (newChild.OwnerDocument != null && 
                newChild.OwnerDocument != this.OwnerDocument &&
                newChild.OwnerDocument != this /* this shouldn't happen, but let's be tolerant of errors */)
            {
                throw new DOMException(DOMException.WRONG_DOCUMENT_ERR);
            }

            // NOT_FOUND_ERR: Raised if refChild is not a child of this node.

            if (refChild != null && !IsChild(refChild))
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }

            // HIERARCHY_REQUEST_ERR: Raised if:
            //   * this node is of a type that does not allow children of the type of the newChild node
            //   * the node to insert is one of this node's ancestors or this node itself
            //   * this node is of type Document and the DOM application attempts to insert a second DocumentType or Element node

            if (newChild == this || IsAncestor(newChild))
            {
                throw new DOMException(DOMException.HIERARCHY_REQUEST_ERR);
            }

            // If the newChild is already in the tree, it is first removed.

            if (newChildNode.ParentNode != null)
            {
                newChildNode.ParentNode.RemoveChild(newChildNode);
            }

            // Inserts the node newChild before the existing child node refChild.
            // 
            // If refChild is null, insert newChild at the end of the list of children.

            NodeBase insertBefore;
            NodeBase insertAfter;

            if (refChild != null)
            {
                insertBefore = (NodeBase)refChild;
                insertAfter = (NodeBase)insertBefore._PreviousSibling;
            }
            else
            {
                insertBefore = null;
                insertAfter = (NodeBase)_LastChild;
            }

            InsertNode(newChildNode, this, insertAfter, insertBefore);

            // Raise notifications

            RaiseChildNodesChanged(null, newChildNode);
            newChildNode.RaiseParentNodeChanged(null);

            return newChild;
        }

        public virtual Node RemoveChild(Node oldChild)
        {
            var oldChildNode = oldChild as NodeBase;

            if (oldChildNode == null)
            {
                throw new ArgumentException();
            }

            // NOT_FOUND_ERR: Raised if oldChild is not a child of this node.

            if (!IsChild(oldChild))
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }

            var oldParentNode = (NodeBase)oldChild.ParentNode;

            RemoveNode(oldChildNode);

            // Raise notifications

            RaiseChildNodesChanged(oldChildNode, null);
            oldChildNode.RaiseParentNodeChanged(oldParentNode);

            return oldChild;
        }

        public virtual Node ReplaceChild(Node newChild, Node oldChild)
        {
            Node nextChild = oldChild.NextSibling;

            RemoveChild(oldChild);

            if (nextChild != null)
            {
                InsertBefore(newChild, nextChild);
            }
            else
            {
                // It should be okay to all InsertBefore() with null as the 
                // second parameter, but since a faulty derived implementation
                // is more likely to implement AppendChild() corrretly than to 
                // properly support a null parameter to InsertBefore, we'll call 
                // the former.
                AppendChild(newChild);
            }

            return oldChild;
        }

        private static void InsertNode(NodeBase node, NodeBase parent, NodeBase previous, NodeBase next)
        {
            // Set parent

            node._ParentNode = parent;

            // Set previous sibling

            node._PreviousSibling = previous;

            if (previous != null)
            {
                previous._NextSibling = node;
            }
            else
            {
                parent._FirstChild = node;
            }

            // Set next sibling

            node._NextSibling = next;

            if (next != null)
            {
                next._PreviousSibling = node;
            }
            else
            {
                parent._LastChild = node;
            }
        }

        private static void RemoveNode(NodeBase node)
        {
            NodeBase parent = (NodeBase)node.ParentNode;
            NodeBase previousSibling = (NodeBase)node.PreviousSibling;
            NodeBase nextSibling = (NodeBase)node.NextSibling;

            // Set parent

            node._ParentNode = null;

            // Set previous sibling

            node._PreviousSibling = null;
            
            if (previousSibling != null)
            {
                previousSibling._NextSibling = nextSibling;
            }
            else
            {
                parent._FirstChild = nextSibling;
            }

            // Set next sibling

            node._NextSibling = null;

            if (nextSibling != null)
            {
                nextSibling._PreviousSibling = previousSibling;
            }
            else
            {
                parent._LastChild = previousSibling;
            }
        }

        public virtual bool HasChildNodes()
        {
            return FirstChild != null;
        }

        public virtual bool IsSameNode(Node other)
        {
            return Object.ReferenceEquals(this, other);
        }

        public virtual bool IsEqualNode(Node other)
        {
            // Two nodes are equal if and only if:
            //
            //  -The two nodes are of the same type.
            //  -The following string attributes are equal: nodeName, localName, namespaceURI, prefix, nodeValue.
            //  -The attributes are equal (order is not significant)
            //  -The childNodes are equal (order is significant)

            if (NodeType != other.NodeType)
            {
                return false;
            }

            if (NodeName != other.NodeName ||
                LocalName != other.LocalName ||
                NamespaceURI != other.NamespaceURI ||
                Prefix != other.Prefix ||
                NodeValue != other.NodeValue)
            {
                return false;
            }

            // Attributes (order not significant)

            var attributes = Attributes;
            var otherAttributes = other.Attributes;

            if (attributes != null || otherAttributes != null)
            {
                if (attributes == null || otherAttributes == null)
                {
                    return false;
                }

                var attributesList = attributes.ToList();
                var otherAttributesList = otherAttributes.ToList();

                if (attributesList.Count != otherAttributesList.Count)
                {
                    return false;
                }

                foreach (var attr in attributesList)
                {
                    var otherAttr = otherAttributesList.FirstOrDefault(a => a.IsEqualNode(attr));
                    if (otherAttr == null)
                    {
                        return false;
                    }
                    otherAttributesList.Remove(otherAttr);
                }
            }

            // Children (order IS significant)

            var children = ChildNodes.ToList();
            var otherChildren = other.ChildNodes.ToList();

            if (children.Count != otherChildren.Count)
            {
                return false;
            }

            for (int i = 0; i < children.Count; i++)
            {
                if (!children[i].IsEqualNode(otherChildren[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual DOMDocumentPosition CompareDocumentPosition(Node other)
        {
            // http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#Node3-compareDocumentPosition:
            //
            // DOCUMENT_POSITION_CONTAINED_BY
            //  The node is contained by the reference node. A node which is contained is always following, too.
            // DOCUMENT_POSITION_CONTAINS
            //  The node contains the reference node. A node which contains is always preceding, too.
            // DOCUMENT_POSITION_DISCONNECTED
            //  The two nodes are disconnected. Order between disconnected nodes is always implementation-specific.
            // DOCUMENT_POSITION_FOLLOWING
            //  The node follows the reference node.
            // DOCUMENT_POSITION_IMPLEMENTATION_SPECIFIC
            //  The determination of preceding versus following is implementation-specific.
            // DOCUMENT_POSITION_PRECEDING
            //  The second node precedes the reference node.
            //
            // (this = the reference node)

            // 1) Find all ancestors of the reference node

            var refAncestors = new List<Node>();

            for (Node ancestor = this; ancestor != null; ancestor = ancestor.ParentNode)
            {
                refAncestors.Add(ancestor);
            }

            // 2) Find all ancestors of the other node

            var otherAncestors = new List<Node>();

            for (Node ancestor = other; ancestor != null; ancestor = ancestor.ParentNode)
            {
                otherAncestors.Add(ancestor);
            }

            // 3) Find the common ancestor

            var commonAncestor = refAncestors.Intersect(otherAncestors).FirstOrDefault();

            if (commonAncestor == null)
            {
                return DOMDocumentPosition.DOCUMENT_POSITION_DISCONNECTED;
            }

            if (commonAncestor == this)
            {
                return DOMDocumentPosition.DOCUMENT_POSITION_CONTAINED_BY;
            }

            if (commonAncestor == other)
            {
                return DOMDocumentPosition.DOCUMENT_POSITION_CONTAINS;
            }

            // 4) Compare the common ancestor's children

            int i = refAncestors.IndexOf(commonAncestor);
            int j = otherAncestors.IndexOf(commonAncestor);

            var refUncle = refAncestors[i - 1];
            var otherUncle = otherAncestors[j - 1];

            foreach (var node in commonAncestor.ChildNodes)
            {
                if (node == refUncle)
                {
                    return DOMDocumentPosition.DOCUMENT_POSITION_FOLLOWING;
                }
                if (node == otherUncle)
                {
                    return DOMDocumentPosition.DOCUMENT_POSITION_PRECEDING;
                }
            }

            // If we've reached this point, the other node is not 
            // implemented correctly or the tree changed during
            // the execution of this method.

            return DOMDocumentPosition.DOCUMENT_POSITION_IMPLEMENTATION_SPECIFIC;
        }

        public abstract Node CloneNode(bool deep);

        public virtual void Normalize()
        {
            // Combine adjacent Text nodes and remove empty Text nodes

            Node[] childNodes = ChildNodes.ToArray();

            for (int i = 0; i < childNodes.Length; i++)
            {
                Node childNode = childNodes[i];

                childNode.Normalize();

                if (childNode.NodeType != NodeType.TEXT_NODE)
                {
                    continue;
                }

                if (String.IsNullOrEmpty(childNode.NodeValue))
                {
                    RemoveChild(childNode);
                    continue;
                }

                if (i == childNodes.Length - 1 || childNodes[i + 1].NodeType != NodeType.TEXT_NODE)
                {
                    continue;
                }

                Text nextNode = (Text)childNodes[i + 1];
                nextNode.InsertData(0, childNode.NodeValue);
                RemoveChild(childNode);
            }
        }

        public virtual bool IsSupported(string feature, string version)
        {
            // http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#DOMFeatures:

            // Features are case-insensitive

            feature = feature.ToLower();

            // Notes:
            // * feature name preceeded by "+" means the interface associated 
            //   may not be directly castable from an instance of a Node object,
            //   but can be obtained via GetFeature()
            // * version may be null or an empty string to test for a feature
            //   regardless of its version

            feature = feature.TrimStart('+');

            if (feature == DOMFeatures.Core.ToLower())
            {
                return String.IsNullOrEmpty(version) || version == "3.0";
            }

            if (feature == DOMFeatures.XML.ToLower())
            {
                return String.IsNullOrEmpty(version) || version == "3.0" || version == "2.0" || version == "1.0";
            }

            if (feature == DOMFeatures.Events.ToLower())
            {
                return String.IsNullOrEmpty(version) || version == "3.0";
            }

            return false;
        }

        public virtual object GetFeature(string feature, string version)
        {
            feature = feature.ToLower().TrimStart('+');

            if (feature == DOMFeatures.Core.ToLower())
            {
                if (String.IsNullOrEmpty(version) || version == "3.0")
                {
                    return this;
                }
            }

            if (feature == DOMFeatures.XML.ToLower())
            {
                if (String.IsNullOrEmpty(version) || version == "3.0" || version == "2.0" || version == "1.0")
                {
                    return this;
                }
            }

            if (feature == DOMFeatures.Events.ToLower())
            {
                if (String.IsNullOrEmpty(version) || version == "3.0")
                {
                    return this;
                }
            }

            return null;
        }

        public virtual string TextContent
        {
            get
            {
                switch (NodeType)
                {
                    // nodeValue
                    case DOM.NodeType.ATTRIBUTE_NODE:
                    case DOM.NodeType.TEXT_NODE:
                    case DOM.NodeType.CDATA_SECTION_NODE:
                    case DOM.NodeType.COMMENT_NODE:
                    case DOM.NodeType.PROCESSING_INSTRUCTION_NODE:
                        return NodeValue;
                    // empty string
                    case DOM.NodeType.DOCUMENT_TYPE_NODE:
                    case DOM.NodeType.NOTATION_NODE:
                        return String.Empty;
                    default:
                        break;
                }

                // concatenation of the textContent attribute value of every child node, 
                // excluding COMMENT_NODE and PROCESSING_INSTRUCTION_NODE nodes

                var buffer = new StringBuilder();

                foreach (var child in ChildNodes)
                {
                    if (child.NodeType != NodeType.COMMENT_NODE && child.NodeType != NodeType.PROCESSING_INSTRUCTION_NODE)
                    {
                        // TODO: get rid of this decode
                        string text = HttpUtility.HtmlDecode(child.TextContent);
                        buffer.Append(text);
                    }
                }
                return buffer.ToString();
            }
            set
            {
                while (FirstChild != null)
                {
                    RemoveChild(FirstChild);
                }

                var text = OwnerDocument.CreateTextNode(value);
                AppendChild(text);
            }
        }

        public virtual string InnerText
        {
            get
            {
                switch (NodeType)
                {
                    // nodeValue
                    case DOM.NodeType.ATTRIBUTE_NODE:
                    case DOM.NodeType.TEXT_NODE:
                    case DOM.NodeType.CDATA_SECTION_NODE:
                    case DOM.NodeType.COMMENT_NODE:
                    case DOM.NodeType.PROCESSING_INSTRUCTION_NODE:
                        return NodeValue;
                    // empty string
                    case DOM.NodeType.DOCUMENT_TYPE_NODE:
                    case DOM.NodeType.NOTATION_NODE:
                        return String.Empty;
                    default:
                        break;
                }

                // concatenation of the textContent attribute value of every child node, 
                // excluding COMMENT_NODE and PROCESSING_INSTRUCTION_NODE nodes

                var buffer = new StringBuilder();
                foreach (var child in ChildNodes)
                {
                    if (child.NodeType != NodeType.COMMENT_NODE && child.NodeType != NodeType.PROCESSING_INSTRUCTION_NODE)
                    {
                        string text = HttpUtility.HtmlDecode(child.InnerText);
                        buffer.Append(text);
                    }
                }
                return buffer.ToString();
            }
            set
            {
                while (FirstChild != null)
                {
                    RemoveChild(FirstChild);
                }

                var text = OwnerDocument.CreateTextNode(value);
                AppendChild(text);
            }
        }

        struct DOMUserData
        {
            public object Data;
            public DOMUserDataHandler Handler;
        }

        Dictionary<string, DOMUserData> _UserData = new Dictionary<string, DOMUserData>();

        Dictionary<string, DOMUserData> UserData
        {
            get
            {
                if (_UserData == null)
                {
                    _UserData = new Dictionary<string, DOMUserData>();
                }
                return _UserData;
            }
        }

        public object SetUserData(string key, object data, DOMUserDataHandler handler)
        {
            DOMUserData userData;
            if (!UserData.TryGetValue(key, out userData))
            {
                userData = new DOMUserData();
            }

            object oldValue = userData.Data;

            userData.Data = data;
            userData.Handler = handler;

            return oldValue;
        }

        public object GetUserData(string key)
        {
            DOMUserData userData;
            if (UserData.TryGetValue(key, out userData))
            {
                return userData.Data;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region EventTarget

        class EventListenerCollection : Dictionary<string, IList<EventListener>>
        {
            public EventListenerCollection()
                : base(StringComparer.OrdinalIgnoreCase)
            { }
        }

        EventListenerCollection _CapturingEvents;
        EventListenerCollection _NonCapturingEvents;

        /// <summary>
        /// Register an event listener.
        /// 
        /// Multiple calls with the same parameters result in a single 
        /// registration.
        /// </summary>
        /// <param name="type">Event type (e.g. "MouseEvents")</param>
        /// <param name="listener">Listener to be registered</param>
        /// <param name="useCapture">Register as a capturing listener</param>
        public void AddEventListener(string type, EventListener listener, bool useCapture)
        {
            EventListenerCollection events;

            if (useCapture)
            {
                if (_CapturingEvents == null)
                {
                    _CapturingEvents = new EventListenerCollection();
                }

                events = _CapturingEvents;
            }
            else
            {
                if (_NonCapturingEvents == null)
                {
                    _NonCapturingEvents = new EventListenerCollection();
                }

                events = _NonCapturingEvents;
            }

            IList<EventListener> listeners;

            if (!events.TryGetValue(type, out listeners))
            {
                listeners = new List<EventListener>();
                events.Add(type, listeners);
            }

            var ownerDocument = (OwnerDocument ?? (Document)this) as DocumentBase;

            if (ownerDocument != null)
            {
                if (listeners.Remove(listener))
                {
                    ownerDocument.OnEventListenerRemoved(type, listener, useCapture);
                }

                listeners.Add(listener);

                ownerDocument.OnEventListenerAdded(type, listener, useCapture);
            }
        }

        /// <summary>
        /// Unregister an event listener
        /// </summary>
        /// <param name="type">Event type (e.g. "MouseEvents")</param>
        /// <param name="listener">Listener to be unregistered</param>
        /// <param name="useCapture">Registered as a capturing listener</param>
        public void RemoveEventListener(string type, EventListener listener, bool useCapture)
        {
            var ownerDocument = (OwnerDocument ?? (Document)this) as DocumentBase;

            if (useCapture)
            {
                if (_CapturingEvents != null)
                {
                    IList<EventListener> listeners;

                    if (_CapturingEvents.TryGetValue(type, out listeners))
                    {
                        if (listeners.Remove(listener) && ownerDocument != null)
                        {
                            ownerDocument.OnEventListenerRemoved(type, listener, useCapture);
                        }
                    }
                }
            }
            else
            {
                if (_NonCapturingEvents != null)
                {
                    IList<EventListener> listeners;

                    if (_NonCapturingEvents.TryGetValue(type, out listeners))
                    {
                        if (listeners.Remove(listener) && ownerDocument != null)
                        {
                            ownerDocument.OnEventListenerRemoved(type, listener, useCapture);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dispatch an event using this node as the EventTarget
        /// </summary>
        /// <param name="evt">Event to be dispatched</param>
        /// <returns>true if the event was NOT cancelled (i.e. PreventDefault() was not called)</returns>
        public bool DispatchEvent(Event evt)
        {
            var ownerDocument = (OwnerDocument ?? (Document)this) as DocumentBase;

            if (ownerDocument != null &&
                !ownerDocument.HasEventListeners(evt.Type, true) &&
                !ownerDocument.HasEventListeners(evt.Type, false))
            {
                return true;
            }

            var targets = new List<Node>();

            for (Node node = this; node != null; node = node.ParentNode)
            {
                targets.Add(node);
            }

            var dispatcher = (Events.Internal.EventBase)evt;

            return dispatcher.Dispatch(targets.OfType<EventTarget>().ToArray(), (e) => 
            {
                ((NodeBase)e.CurrentTarget).OnEvent(e);
            });
        }

        /// <summary>
        /// Handle a dispatched event
        /// </summary>
        /// <param name="evt">Event to be handled</param>
        protected virtual void OnEvent(Event evt)
        {
            switch (evt.EventPhase)
            {
                case EventPhaseType.CAPTURING_PHASE:

                    // Handle capturing events

                    if (_CapturingEvents != null)
                    {
                        IList<EventListener> listeners;

                        if (_CapturingEvents.TryGetValue(evt.Type, out listeners))
                        {
                            foreach (var listener in listeners.ToArray())
                            {
                                listener.HandleEvent(evt);
                            }
                        }
                    }
                    break;

                case EventPhaseType.AT_TARGET:
                case EventPhaseType.BUBBLING_PHASE:

                    // Handle non-capturing events

                    if (_NonCapturingEvents != null)
                    {
                        IList<EventListener> listeners;

                        if (_NonCapturingEvents.TryGetValue(evt.Type, out listeners))
                        {
                            foreach (var listener in listeners.ToArray())
                            {
                                listener.HandleEvent(evt);
                            }
                        }
                    }

                    break;
            }
        }

        #endregion

        #region MutationEvents

        /// <summary>
        /// Dispatch a mutation event
        /// </summary>
        /// <exception cref="DOMException">
        /// NOT_SUPPORTED_ERR: Raised if the implementation does not support MutationEvent
        /// </exception>
        protected void DispatchMutationEvent(string type, bool canBubble, bool cancelable, Node relatedNode = null, string prevValue = "", string newValue = "", string attrName = "", AttrChangeType attrChange = 0)
        {
            var document = OwnerDocument as DocumentEvent;

            if (document != null)
            {
                var evt = (MutationEvent)document.CreateEvent(EventTypes.MutationEvents);

                evt.InitMutationEvent(type, canBubble, cancelable, relatedNode, prevValue, newValue, attrName, attrChange);

                DispatchEvent(evt);
            }
        }

        /// <summary>
        /// This is a general event for notification of all changes to the document.
        /// </summary>
        protected void DispatchSubtreeModifiedEvent()
        {
            DispatchMutationEvent(MutationEventTypes.DOMSubtreeModified, true, false);
        }

        /// <summary>
        /// Fired when a node has been added as a child of another node.
        /// </summary>
        /// <param name="parentNode"></param>
        protected void DispatchNodeInsertedEvent(Node parentNode)
        {
            DispatchMutationEvent(MutationEventTypes.DOMNodeInserted, true, false, parentNode);
        }

        /// <summary>
        /// Fired when a node is being removed from its parent node.
        /// </summary>
        /// <param name="parentNode"></param>
        protected void DispatchNodeRemovedEvent(Node parentNode)
        {
            DispatchMutationEvent(MutationEventTypes.DOMNodeRemoved, true, false, parentNode);
        }

        /// <summary>
        /// Fired when a node is being removed from a document, either through 
        /// direct removal of the Node or removal of a subtree in which it is 
        /// contained.
        /// </summary>
        protected void DispatchNodeRemovedFromDocumentEvent()
        {
            DispatchMutationEvent(MutationEventTypes.DOMNodeRemovedFromDocument, false, false);
        }

        /// <summary>
        /// Fired when a node is being inserted into a document, either through 
        /// direct insertion of the Node or insertion of a subtree in which it 
        /// is contained. 
        /// </summary>
        protected void DispatchNodeInsertedIntoDocumentEvent()
        {
            DispatchMutationEvent(MutationEventTypes.DOMNodeInsertedIntoDocument, false, false);
        }

        /// <summary>
        /// Fired after an Attr has been modified on a node. 
        /// </summary>
        protected void DispatchAttrModifiedEvent(Attr attr, string attrName, AttrChangeType attrChange, string prevValue, string newValue)
        {
            DispatchMutationEvent(MutationEventTypes.DOMAttrModified, true, false, attr, prevValue, newValue, attrName, attrChange);
        }

        /// <summary>
        /// Fired after CharacterData within a node has been modified but the 
        /// node itself has not been inserted or deleted.
        /// </summary>
        /// <param name="prevValue"></param>
        /// <param name="newValue"></param>
        protected void DispatchCharacterDataModifiedEvent(string prevValue, string newValue)
        {
            DispatchMutationEvent(MutationEventTypes.DOMCharacterDataModified, true, false, null, prevValue, newValue);
        }

        #endregion

        #region IXPathNavigable

        public virtual XPathNavigator CreateNavigator()
        {
            return new DOM.XPath.Internal.XPathDOMNavigator(OwnerDocument, this);
        }

        #endregion

        #region ScriptTarget

        public object ScriptContext { get; set; }

        #endregion

        #region IEnumerable

        public IEnumerator<Node> GetEnumerator()
        {
            return ChildNodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the value of ParentNode changes
        /// </summary>
        public NodeChangedEventHandler ParentNodeChanged;

        /// <summary>
        /// Calls OnParentNodeChanged() and raises the PrentNodeChanged event
        /// </summary>
        /// <param name="oldValue"></param>
        protected void RaiseParentNodeChanged(Node oldValue)
        {
            OnParentNodeChanged(oldValue);

            if (ParentNodeChanged != null)
            {
                ParentNodeChanged(this, new NodeChangedEventArgs(this, oldValue));
            }
        }

        /// <summary>
        /// Called when the value of ParentNode changes
        /// </summary>
        protected virtual void OnParentNodeChanged(Node oldValue)
        {

        }

        /// <summary>
        /// Invoked when a child node is added or removed
        /// </summary>
        public DOMCollectionChangedEventHandler<Node> ChildNodesChanged;

        /// <summary>
        /// Overloaded
        /// </summary>
        protected void RaiseChildNodesChanged(Node oldNode, Node newNode)
        {
            IList<Node> oldNodes = null;
            IList<Node> newNodes = null;

            if (oldNode != null)
            {
                oldNodes = new Node[] { oldNode };
            }

            if (newNode != null)
            {
                newNodes = new Node[] { newNode };
            }

            RaiseChildNodesChanged(oldNodes, newNodes);
        }

        /// <summary>
        /// Calls OnChildNodesChanged() and raises the ChildNodesChanged event
        /// </summary>
        /// <param name="oldNodes"></param>
        /// <param name="newNodes"></param>
        protected void RaiseChildNodesChanged(IList<Node> oldNodes, IList<Node> newNodes)
        {
            OnChildNodesChanged(oldNodes, newNodes);

            if (ChildNodesChanged != null)
            {
                ChildNodesChanged(this, new DOMCollectionChangedEventArgs<Node>(oldNodes, newNodes));
            }
        }

        /// <summary>
        /// Called when a child node is added or removed
        /// </summary>
        protected virtual void OnChildNodesChanged(IList<Node> oldNodes, IList<Node> newNodes)
        {

        }

        #endregion

        #region Implementation

        static void ParseQualifiedName(string qualifiedName, out string prefix, out string localName)
        {
            int index = qualifiedName.IndexOf(':');
            if (index != -1)
            {
                prefix = qualifiedName.Remove(index);
                localName = qualifiedName.Remove(0, index + 1);
            }
            else
            {
                prefix = null;
                localName = qualifiedName;
            }
        }

        static string FormatQualifiedName(string prefix, string localName)
        {
            if (!String.IsNullOrEmpty(prefix))
            {
                return prefix + ":" + localName;
            }
            else
            {
                return localName;
            }
        }

        /// <summary>
        /// Look up the namespace URI associated to the given prefix.
        /// 
        /// The lookup is performed according to the algorithm described here:
        /// 
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/namespaces-algorithms.html#lookupNamespaceURIAlgo
        /// </summary>
        /// <param name="node">The target node</param>
        /// <param name="prefix">The prefix to lookup (null to get the default namespace URI)</param>
        /// <returns>The retrieved namespace URI, or null if not found.</returns>
        static string LookupNamespaceURI(Node node, string prefix)
        {
            switch (node.NodeType)
            {
                case DOM.NodeType.ELEMENT_NODE:
                    {
                        if (prefix == node.Prefix && node.NamespaceURI != null)
                        {
                            // Note: prefix could be "null" in this case we are looking for default namespace 
                            return node.NamespaceURI;
                        }
                        if (node.HasAttributes())
                        {
                            foreach (Attr attr in node.Attributes)
                            {
                                if (attr.Prefix == "xmlns" && attr.LocalName == prefix)
                                {
                                    if (!String.IsNullOrEmpty(attr.NodeValue))
                                    {
                                        return attr.NodeValue;
                                    }
                                    return null;
                                }
                                else if (attr.LocalName == "xmlns" && prefix == null)
                                {
                                    // default namespace
                                    if (!String.IsNullOrEmpty(attr.NodeValue))
                                    {
                                        return attr.NodeValue;
                                    }
                                    return null;
                                }
                            }
                        }
                        var ancestorElement = GetAncestorElement(node);
                        if (ancestorElement != null)
                        {
                            // EntityReferences may have to be skipped to get to it 
                            return ancestorElement.LookupNamespaceURI(prefix);
                        }
                        return null;
                    }
                case DOM.NodeType.DOCUMENT_NODE:
                    var documentElement = ((Document)node).DocumentElement;
                    return documentElement.LookupNamespaceURI(prefix);
                case DOM.NodeType.ENTITY_NODE:
                case DOM.NodeType.NOTATION_NODE:
                case DOM.NodeType.DOCUMENT_TYPE_NODE:
                case DOM.NodeType.DOCUMENT_FRAGMENT_NODE:
                    return null;
                case DOM.NodeType.ATTRIBUTE_NODE:
                    var ownerElement = ((Attr)node).OwnerElement;
                    if (ownerElement != null)
                    {
                        return ownerElement.LookupNamespaceURI(prefix);
                    }
                    else
                    {
                        return null;
                    }
                default:
                    {
                        var ancestorElement = GetAncestorElement(node);
                        if (ancestorElement != null)
                        {
                            return ancestorElement.LookupNamespaceURI(prefix);
                        }
                        else
                        {
                            return null;
                        }
                    }
            }
        }

        /// <summary>
        /// Look up the prefix associated with the given namespace URI.
        /// 
        /// This lookup is performed according to the algorithm described here:
        /// 
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/namespaces-algorithms.html#lookupNamespacePrefixAlgo
        /// </summary>
        /// <param name="node">The target node</param>
        /// <param name="namespaceURI">The namespace URI to lookup</param>
        /// <returns>The retrieved namespace prefix, or null if not found.</returns>
        static string LookupPrefix(Node node, string namespaceURI)
        {
            if (String.IsNullOrEmpty(namespaceURI)) 
            {
                return null;
            }

            NodeType type = node.NodeType; 
            switch (type) 
            { 
                case NodeType.ELEMENT_NODE: 
                { 
                    return LookupNamespacePrefix(node, namespaceURI, (Element)node); 
                } 
                case NodeType.DOCUMENT_NODE:
                { 
                    var documentElement = ((Document)node).DocumentElement;
                    return LookupPrefix(documentElement, namespaceURI); 
                } 
                case NodeType.ENTITY_NODE : 
                case NodeType.NOTATION_NODE: 
                case NodeType.DOCUMENT_FRAGMENT_NODE: 
                case NodeType.DOCUMENT_TYPE_NODE: 
                    return null;  // type is unknown  
                case NodeType.ATTRIBUTE_NODE:
                {
                    var ownerElement = ((Attr)node).OwnerElement;
                    if (ownerElement != null) 
                    { 
                        return LookupPrefix(ownerElement, namespaceURI); 
                    } 
                    return null; 
                } 
                default:
                { 
                    var ancestor = GetAncestorElement(node);
                    if (ancestor != null)
                    { 
                        return LookupPrefix(ancestor, namespaceURI); 
                    } 
                    return null; 
                } 
            } 
        }

        /// <summary>
        /// Helper for the LookupPrefix() method
        /// </summary>
        static string LookupNamespacePrefix(Node refNode, string namespaceURI, Element originalElement)
        { 
             if (!String.IsNullOrEmpty(refNode.NamespaceURI) &&
                 refNode.NamespaceURI == namespaceURI &&
                 !String.IsNullOrEmpty(refNode.Prefix) &&
                 originalElement.LookupNamespaceURI(refNode.Prefix) == namespaceURI)
            { 
                return refNode.Prefix; 
            } 
            if (refNode.HasAttributes())
            {
                foreach (Attr attr in refNode.Attributes)
                {
                    if (attr.Prefix == "xmlns" &&
                        attr.Value == namespaceURI &&
                        originalElement.LookupNamespaceURI(attr.LocalName) == namespaceURI)
                    {
                        return attr.LocalName;
                    }
                }
            }

            var ancestor = GetAncestorElement(refNode);
            if (ancestor != null)
            {
                return LookupNamespacePrefix(ancestor, namespaceURI, originalElement);
            }
            return null;
        }

        /// <summary>
        /// Determine if the given namespace is the default namespace.
        /// 
        /// This is performed according to the algorithm described here:
        /// 
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/namespaces-algorithms.html#isDefaultNamespaceAlgo
        /// </summary>
        /// <param name="node">The target node</param>
        /// <param name="namespaceURI">The namespace URI to test</param>
        /// <returns>true if namespaceURI is the default namespace</returns>
        static bool IsDefaultNamespace(Node node, string namespaceURI)
        {
            switch (node.NodeType) 
            {
                case NodeType.ELEMENT_NODE: 
                {
                    if (String.IsNullOrEmpty(node.Prefix))
                    {
                        return namespaceURI == node.NamespaceURI;
                    }
                    if (node.HasAttributes())
                    {
                        foreach (Attr attr in node.Attributes)
                        {
                            if (attr.LocalName == "xmlns")
                            {
                                return namespaceURI == attr.Value;
                            }
                        }
                    }

                    var ancestorElement = GetAncestorElement(node);
                    if (ancestorElement != null)
                    {
                        return ancestorElement.IsDefaultNamespace(namespaceURI);
                    }
                    else 
                    {
                        return false;
                    }    
                }
                case NodeType.DOCUMENT_NODE:
                    var documentElement = ((Document)node).DocumentElement;
                    return documentElement.IsDefaultNamespace(namespaceURI);
                case NodeType.ENTITY_NODE:
                case NodeType.NOTATION_NODE:
                case NodeType.DOCUMENT_TYPE_NODE:
                case NodeType.DOCUMENT_FRAGMENT_NODE:
                    return false;
                case NodeType.ATTRIBUTE_NODE:
                    var ownerElement = ((Attr)node).OwnerElement;
                    if (ownerElement != null)
                    {          
                        return ownerElement.IsDefaultNamespace(namespaceURI);
                    }
                    else 
                    {
                        return false;
                    }    
                default:
                {
                    var ancestorElement = GetAncestorElement(node);
                    if (ancestorElement != null)
                    {          
                        return ancestorElement.IsDefaultNamespace(namespaceURI);
                    }
                    else 
                    {
                        return false;
                    }   
                }
            }
        }

        /// <summary>
        /// Helper for the LookupNamespaceURI() and LookupPrefix() methods
        /// </summary>
        /// <param name="refNode"></param>
        /// <returns></returns>
        static Node GetAncestorElement(Node refNode)
        {
            Node parentNode = refNode.ParentNode;

            if (parentNode == null)
            {
                return null;
            }
            else if (parentNode.NodeType == DOM.NodeType.ELEMENT_NODE)
            {
                return parentNode;
            }
            else
            {
                return GetAncestorElement(parentNode);
            }
        }

        /// <summary>
        /// Determine if the given node is a child of this node
        /// </summary>
        bool IsChild(Node node)
        {
            try
            {
                return node.ParentNode == this;
            }
            catch (NotImplementedException)
            {
                // Occurs if a derived class doesn't implement ParentNode
                return !ChildNodes.Contains(node);
            }
        }

        /// <summary>
        /// Determine if the given node is an ancestor of this node
        /// </summary>
        bool IsAncestor(Node node)
        {
            try
            {
                Node ancestor = ParentNode;

                while (ancestor != null)
                {
                    if (node == ancestor)
                    {
                        return true;
                    }
                    ancestor = ancestor.ParentNode;
                }

                return false;
            }
            catch (NotImplementedException)
            {
                // Occurs if a derived class doesn't implement ParentNode
                return false;
            }
        }

        internal void SetOwnerDocument(Document document)
        {
            _OwnerDocument = document;

            if (HasAttributes())
            {
                foreach (AttrBase attr in Attributes)
                {
                    attr.SetOwnerDocument(document);
                }
            }

            if (HasChildNodes())
            {
                foreach (NodeBase child in ChildNodes)
                {
                    child.SetOwnerDocument(document);
                }
            }
        }


        #endregion

    }

}
