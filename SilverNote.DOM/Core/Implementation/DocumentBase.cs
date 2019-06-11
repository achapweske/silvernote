using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using DOM.Views;
using DOM.Events;
using DOM.Events.Internal;
using DOM.LS;

namespace DOM.Internal
{
    public class DocumentBase : NodeProxy, Document, DocumentEvent, DocumentView, DocumentLS, IXPathNavigable
    {
        #region Fields

        DOMImplementation _Implementation;
        DocumentType _DocType;
        Element _DocumentElement;
        string _DocumentURI;
        AbstractView _DefaultView;
        string _InputEncoding = null;
        string _XmlEncoding = null;
        string _XmlVersion = "1.0";
        bool _XmlStandalone = false;
        DocumentConfig _DomConfig = new DocumentConfig();

        #endregion

        #region Constructors

        public DocumentBase()
            : base(null, "#document", NodeType.DOCUMENT_NODE)
        {

        }

        public DocumentBase(DOMImplementation implementation)
            : this()
        {
            _Implementation = implementation;
        }

        public DocumentBase(DOMImplementation implementation, DocumentType docType)
            : this(implementation)
        {
            if (docType != null)
            {
                AppendChild(docType);
            }
        }

        public DocumentBase(DocumentType docType)
            : this()
        {
            if (docType != null)
            {
                AppendChild(docType);
            }
        }

        public DocumentBase(DOMImplementation implementation, DocumentType docType, string localName)
            : this(implementation, docType)
        {
            if (!String.IsNullOrWhiteSpace(localName))
            {
                var documentElement = CreateElement(localName);

                AppendChild(documentElement);
            }
        }

        public DocumentBase(DocumentType docType, string localName)
            : this(docType)
        {
            if (!String.IsNullOrWhiteSpace(localName))
            {
                var documentElement = CreateElement(localName);

                AppendChild(documentElement);
            }
        }

        public DocumentBase(DOMImplementation implementation, string localName)
            : this(implementation)
        {
            if (!String.IsNullOrWhiteSpace(localName))
            {
                var documentElement = CreateElement(localName);

                AppendChild(documentElement);
            }
        }

        public DocumentBase(string localName)
            : this()
        {
            if (!String.IsNullOrWhiteSpace(localName))
            {
                var documentElement = CreateElement(localName);

                AppendChild(documentElement);
            }
        }

        public DocumentBase(DOMImplementation implementation, DocumentType docType, string namespaceURI, string qualifiedName)
            : this(implementation, docType)
        {
            if (!String.IsNullOrWhiteSpace(qualifiedName))
            {
                var documentElement = CreateElementNS(namespaceURI, qualifiedName);

                AppendChild(documentElement);
            }
        }

        public DocumentBase(DOMImplementation implementation, string namespaceURI, string qualifiedName)
            : this(implementation)
        {
            if (!String.IsNullOrWhiteSpace(qualifiedName))
            {
                var documentElement = CreateElementNS(namespaceURI, qualifiedName);

                AppendChild(documentElement);
            }
        }

        public DocumentBase(string namespaceURI, string qualifiedName)
            : this()
        {
            if (!String.IsNullOrWhiteSpace(qualifiedName))
            {
                var documentElement = CreateElementNS(namespaceURI, qualifiedName);

                AppendChild(documentElement);
            }
        }

        public DocumentBase(DOMImplementation implementation, string version, string encoding, bool standalone)
            : this(implementation)
        {
            _InputEncoding = encoding;
            _XmlVersion = version;
            _XmlEncoding = encoding;
            _XmlStandalone = standalone;
        }

        #endregion

        #region Node

        public override Node InsertBefore(Node newChild, Node refChild)
        {
            if (newChild.NodeType == NodeType.ELEMENT_NODE && _DocumentElement != null ||
                newChild.NodeType == NodeType.DOCUMENT_TYPE_NODE && _DocType != null)
            {
                throw new DOMException(DOMException.HIERARCHY_REQUEST_ERR);
            }

            base.InsertBefore(newChild, refChild);

            if (newChild.NodeType == NodeType.ELEMENT_NODE)
            {
                _DocumentElement = (Element)newChild;
            }
            if (newChild.NodeType == NodeType.DOCUMENT_TYPE_NODE)
            {
                _DocType = (DocumentType)newChild;
            }

            return newChild;
        }

        public override Node RemoveChild(Node oldChild)
        {
            base.RemoveChild(oldChild);

            if (oldChild.NodeType == NodeType.ELEMENT_NODE)
            {
                _DocumentElement = null;
            }
            if (oldChild.NodeType == NodeType.DOCUMENT_TYPE_NODE)
            {
                _DocType = null;
            }

            return oldChild;
        }

        public override Node CloneNode(bool deep)
        {
            var result = Implementation.CreateDocument ( 
                DocumentElement.NamespaceURI, 
                DocumentElement.NodeName, 
                this.DocType
            );

            if (deep)
            {
                result.RemoveChild(result.DocumentElement);

                foreach (var childNode in ChildNodes)
                {
                    var clone = childNode.CloneNode(true);

                    result.AdoptNode(clone);

                    result.AppendChild(clone);
                }
            }

            return result;
        }

        #endregion

        #region Document

        /// <summary>
        /// Get the DOMImplementation to which this document belongs
        /// </summary>
        public DOMImplementation Implementation
        {
            get { return _Implementation; }
        }

        /// <summary>
        /// Get the DocumentType node (null if none)
        /// </summary>
        public DocumentType DocType
        {
            get { return _DocType; }
        }

        /// <summary>
        /// Get the root element node (null if none)
        /// </summary>
        public Element DocumentElement 
        {
            get { return _DocumentElement; }
        }

        /// <summary>
        /// Create a new Element node
        /// </summary>
        /// <param name="tagName">The new element's tag name</param>
        /// <returns>The newly-created Element node</returns>
        public Element CreateElement(string tagName)
        {
            var result = OnCreateElement(tagName);

            RegisterNode(result);

            return result;
        }

        /// <summary>
        /// Create a new DocumentFragment node
        /// </summary>
        /// <returns>The newly-created DocumentFragment node</returns>
        public DocumentFragment CreateDocumentFragment()
        {
            var result = OnCreateDocumentFragment();

            RegisterNode(result);

            return result;
        }

        /// <summary>
        /// Create a new Text node
        /// </summary>
        /// <param name="data">The new node's initial text</param>
        /// <returns>The newly-created Text node</returns>
        public Text CreateTextNode(string data)
        {
            var result = OnCreateTextNode(data);

            RegisterNode(result);

            return result;
        }

        /// <summary>
        /// Create a new Comment node
        /// </summary>
        /// <param name="data">The new node's initial text</param>
        /// <returns>The newly-created Comment node</returns>
        public Comment CreateComment(string data)
        {
            var result = OnCreateComment(data);

            RegisterNode(result);

            return result;
        }

        /// <summary>
        /// Create a new CDATASection node
        /// </summary>
        /// <param name="data">The new node's initial text</param>
        /// <returns>The newly-created CDATASection node</returns>
        public CDATASection CreateCDATASection(string data)
        {
            var result = OnCreateCDATASection(data);

            RegisterNode(result);

            return result;
        }

        /// <summary>
        /// Create a new ProcessingInstruction node
        /// </summary>
        /// <param name="target">The processing instruction target</param>
        /// <param name="data">The processing instruction data</param>
        /// <returns>The newly-created ProcessingInstruction node</returns>
        public ProcessingInstruction CreateProcessingInstruction(string target, string data)
        {
            var result = OnCreateProcessingInstruction(target, data);

            RegisterNode(result);

            return result;
        }

        /// <summary>
        /// Create a new Attr node
        /// </summary>
        /// <param name="name">Name of the new Attr node</param>
        /// <returns>The newly-created Attr node</returns>
        public Attr CreateAttribute(string name)
        {
            var result = OnCreateAttribute(name);

            RegisterNode(result);

            return result;
        }

        /// <summary>
        /// Create a new EntityReference node
        /// </summary>
        /// <param name="name">Name of the new EntityReference node</param>
        /// <returns>The newly-created EntityReference node</returns>
        public EntityReference CreateEntityReference(string name)
        {
            var result = OnCreateEntityReference(name);

            RegisterNode(result);

            return result;
        }

        /// <summary>
        /// Create a new Element node
        /// </summary>
        /// <param name="namespaceURI">The new element's namespace URI</param>
        /// <param name="qualifiedName">The new element's qualified name</param>
        /// <returns>The newly-created Element node</returns>
        public Element CreateElementNS(string namespaceURI, string qualifiedName)
        {
            var result = OnCreateElementNS(namespaceURI, qualifiedName);

            RegisterNode(result);

            return result;
        }

        /// <summary>
        /// Create a new Attr node
        /// </summary>
        /// <param name="namespaceURI">The new attribute's namespace URI</param>
        /// <param name="qualifiedName">The new attribute's qualified name</param>
        /// <returns>The newly-created Attr node</returns>
        public Attr CreateAttributeNS(string namespaceURI, string qualifiedName)
        {
            var result = OnCreateAttributeNS(namespaceURI, qualifiedName);

            RegisterNode(result);

            return result;
        }

        /// <summary>
        /// Get all elements in this document with the given name
        /// </summary>
        /// <param name="tagName">Element name to look for ("*" for all elements)</param>
        /// <returns></returns>
        public NodeList GetElementsByTagName(string tagName)
        {
            if (DocumentElement != null)
            {
                return new DynamicNodeList(DocumentElement, tagName);
            }
            else
            {
                return NodeListBase.Empty;
            }
        }

        /// <summary>
        /// Get all elements in this document with the given namespace URI and local name
        /// </summary>
        /// <param name="namespaceURI"></param>
        /// <param name="localName"></param>
        /// <returns></returns>
        public NodeList GetElementsByTagNameNS(string namespaceURI, string localName)
        {
            if (DocumentElement != null)
            {
                return new DynamicNodeList(DocumentElement, namespaceURI, localName);
            }
            else
            {
                return NodeListBase.Empty;
            }
        }

        /// <summary>
        /// Get the element with the given ID
        /// </summary>
        /// <param name="elementId">Target element ID</param>
        /// <returns>The retrieved element, or null if not found</returns>
        public Element GetElementById(string elementId)
        {
            return GetElementById(DocumentElement, elementId);
        }

        private static Element GetElementById(Element root, string elementId)
        {
            if (root.GetAttribute(DOM.Attributes.ID) == elementId)
            {
                return root;
            }

            foreach (var child in root.ChildNodes.OfType<ElementBase>())
            {
                var result = GetElementById(child, elementId);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public string InputEncoding
        {
            get { return _InputEncoding; }
        }

        public string XmlEncoding
        {
            get { return _XmlEncoding; }
        }

        public bool XmlStandalone
        {
            get { return _XmlStandalone; }
            set { _XmlStandalone = value; }
        }

        public string XmlVersion
        {
            get { return _XmlVersion; }
            set { _XmlVersion = value; }
        }

        public bool StrictErrorChecking
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string DocumentURI
        {
            get { return _DocumentURI; }
            set { _DocumentURI = value; }
        }

        public Node ImportNode(Node importedNode, bool deep)
        {
            throw new NotImplementedException();
        }

        public virtual Node AdoptNode(Node source)
        {
            var sourceNode = source as NodeBase;
            var oldOwnerDocument = source.OwnerDocument as DocumentBase;
            var newOwnerDocument = this;

            if (sourceNode == null || oldOwnerDocument == null)
            {
                return null;
            }

            oldOwnerDocument.UnregisterNode(source);

            sourceNode.SetOwnerDocument(newOwnerDocument);

            newOwnerDocument.RegisterNode(source);

            return source;
        }

        public DOMConfiguration DomConfig
        {
            get { return _DomConfig; }
        }

        public void NormalizeDocument()
        {
            if (!_DomConfig.Comments)
            {
                NormalizeDocument_RemoveComments(this);
            }

            if (!_DomConfig.ElementContentWhitespace)
            {
                NormalizeDocument_RemoveWhitespace(this);
            }

            Normalize();
        }

        static void NormalizeDocument_RemoveComments(Node node)
        {
            foreach (var childNode in node.ChildNodes.ToArray())
            {
                if (childNode.NodeType == NodeType.COMMENT_NODE)
                {
                    node.RemoveChild(childNode);
                }
                else
                {
                    NormalizeDocument_RemoveComments(node);
                }
            }
        }

        static void NormalizeDocument_RemoveWhitespace(Node node)
        {
            foreach (var childNode in node.ChildNodes.ToArray())
            {
                if (childNode.NodeType == NodeType.TEXT_NODE && 
                    ((Text)childNode).IsElementContentWhitespace)
                {
                    node.RemoveChild(childNode);
                }
                else
                {
                    NormalizeDocument_RemoveWhitespace(node);
                }
            }
        }

        public Node RenameNode(Node n, string namespaceURI, string qualifiedName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region DocumentEvent

        /// <summary>
        /// Create an Event
        /// </summary>
        /// <param name="eventType">The eventType parameter specifies the type of Event interface to be created</param>
        /// <returns>The newly created Event</returns>
        /// <exception cref="DOMException">
        /// NOT_SUPPORTED_ERR: Raised if the implementation does not support the type of Event interface requested
        /// </exception>
        public Event CreateEvent(string eventType)
        {
            switch (eventType.ToLower())
            {
                case "htmlevents":
                    return new EventBase();
                case "mouseevents":
                    return new MouseEventImpl();
                case "mutationevents":
                    return new MutationEventImpl();
                case "uievents":
                    return new UIEventImpl();
                default:
                    throw new DOMException(DOMException.NOT_SUPPORTED_ERR);
            }
        }

        #endregion

        #region DocumentView

        public virtual AbstractView DefaultView
        {
            get
            {
                if (_DefaultView == null)
                {
                    _DefaultView = new DOM.Views.Internal.AbstractViewBase(this);
                }

                return _DefaultView;
            }
        }

        #endregion

        #region DocumentLS

        public bool Async { get; set; }

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public bool Load(string uri)
        {
            throw new NotImplementedException();
        }

        public bool LoadXML(string source)
        {
            throw new NotImplementedException();
        }

        public string SaveXML(Node node)
        {
            var dom = Implementation as DOMImplementationLS;
            if (dom != null)
            {
                var serializer = dom.CreateLSSerializer();
                return serializer.WriteToString(node);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Selectors

        public Element QuerySelector(string selectors)
        {
            if (DocumentElement != null)
            {
                return DOMSelectors.QuerySelector(DocumentElement, selectors);
            }
            else
            {
                return null;
            }
        }

        public NodeList QuerySelectorAll(string selectors)
        {
            if (DocumentElement != null)
            {
                return DOMSelectors.QuerySelectorAll(DocumentElement, selectors);
            }
            else
            {
                return NodeListBase.Empty;
            }
        }

        #endregion

        #region IXPathNavigable

        public override XPathNavigator CreateNavigator()
        {
            return new DOM.XPath.Internal.XPathDOMNavigator(this);
        }

        #endregion

        #region Implementation

        internal void SetEncoding(string encoding)
        {
            _InputEncoding = encoding;
            _XmlEncoding = encoding;
        }

        private Node CreateNode(NodeType nodeType, string nodeName)
        {
            switch (nodeType)
            {
                case DOM.NodeType.ATTRIBUTE_NODE:
                    return CreateAttribute(nodeName);
                case DOM.NodeType.ELEMENT_NODE:
                    return CreateElement(nodeName);
                case DOM.NodeType.TEXT_NODE:
                    return CreateTextNode(String.Empty);
                default:
                    return null;
            }
        }

        private Node CreateNodeNS(NodeType nodeType, string namespaceURI, string qualifiedName)
        {
            switch (nodeType)
            {
                case DOM.NodeType.ATTRIBUTE_NODE:
                    return CreateAttributeNS(namespaceURI, qualifiedName);
                case DOM.NodeType.ELEMENT_NODE:
                    return CreateElementNS(namespaceURI, qualifiedName);
                case DOM.NodeType.TEXT_NODE:
                    return CreateTextNode(String.Empty);
                default:
                    return null;
            }
        }

        protected virtual CDATASection OnCreateCDATASection(string data)
        {
            return new CDATASectionBase(this, data);
        }

        protected virtual DocumentFragment OnCreateDocumentFragment()
        {
            return new DocumentFragmentBase(this);
        }

        protected virtual Attr OnCreateAttribute(string name)
        {
            return new AttrBase(this, name);
        }

        protected virtual EntityReference OnCreateEntityReference(string name)
        {
            throw new NotImplementedException();
        }

        protected virtual Element OnCreateElement(string tagName)
        {
            return new ElementBase(this, tagName);
        }

        protected virtual Text OnCreateTextNode(string data)
        {
            return new TextBase(this, data);
        }

        protected virtual Comment OnCreateComment(string data)
        {
            return new CommentBase(this, data);
        }

        protected virtual ProcessingInstruction OnCreateProcessingInstruction(string target, string data)
        {
            return new ProcessingInstructionBase(this, target, data);
        }

        protected virtual Element OnCreateElementNS(string namespaceURI, string qualifiedName)
        {
            return new ElementBase(this, namespaceURI, qualifiedName);
        }

        protected virtual Attr OnCreateAttributeNS(string namespaceURI, string qualifiedName)
        {
            return new AttrBase(this, namespaceURI, qualifiedName);
        }

        private Dictionary<INodeSource, NodeProxy> _Nodes = new Dictionary<INodeSource, NodeProxy>();

        /// <summary>
        /// Get the node associated with the given source
        /// </summary>
        /// <param name="source">The source whose node is to be retrieved</param>
        /// <returns></returns>
        public NodeBase GetNode(INodeSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            NodeProxy node;

            if (!_Nodes.TryGetValue(source, out node) || !node.IsSourceValid)
            {
                node = CreateNode(source);
            }

            return node;
        }

        private NodeProxy CreateNode(INodeSource source)
        {
            NodeProxy node;

            string sourceName = source.GetNodeName(this);
            string localName = NodeProxy.ParseSourceLocalName(sourceName);
            if (!String.IsNullOrEmpty(localName))
            {
                string namespaceURI = NodeProxy.ParseSourceNamespaceURI(sourceName);
                node = (NodeProxy)CreateNodeNS(source.GetNodeType(this), namespaceURI, localName);
            }
            else
            {
                string nodeName = NodeProxy.ParseSourceNodeName(sourceName);
                node = (NodeProxy)CreateNode(source.GetNodeType(this), nodeName);
            }

            node.Bind(source);

            return node;
        }

        /// <summary>
        /// Register a node with this document.
        /// 
        /// This is called for every node that was created by this document,
        /// regardless of whether or not it was actually added to the document 
        /// tree.
        /// </summary>
        /// <param name="newNode">The node to be registered.</param>
        private void RegisterNode(Node newNode)
        {
            NodeBase node = newNode as NodeBase;

            if (node != null)
            {
                node.ParentNodeChanged += Node_ParentNodeChanged;
            }

            // Map INodeSource objects to their associated Nodes

            NodeProxy proxy = newNode as NodeProxy;

            if (proxy != null)
            {
                if (proxy.Source != null)
                {
                    _Nodes[proxy.Source] = proxy;
                }

                proxy.SourceChanging += Node_SourceChanging;
            }
        }

        private void UnregisterNode(Node oldNode)
        {
            NodeBase node = oldNode as NodeBase;

            if (node != null)
            {
                node.ParentNodeChanged -= Node_ParentNodeChanged;
            }

            NodeProxy proxy = oldNode as NodeProxy;

            if (proxy != null)
            {
                if (proxy.Source != null)
                {
                    _Nodes.Remove(proxy.Source);
                }

                proxy.SourceChanging -= Node_SourceChanging;
            }
        }

        private void Node_SourceChanging(object sender, SourceChangingEventArgs e)
        {
            NodeProxy node = (NodeProxy)sender;

            if (node.Source != null)
            {
                _Nodes.Remove(node.Source);
            }

            if (e.NewSource != null)
            {
                _Nodes[e.NewSource] = node;
            }
        }

        /// <summary>
        /// Called when any node's Parent property changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Node_ParentNodeChanged(object sender, NodeChangedEventArgs e)
        {
            NodeBase node = (NodeBase)sender;

            // Node is in the document tree

            if (e.OldValue != null)
            {
                if (IsSelfOrDescendant(this, e.OldValue))
                {
                    OnSubtreeRemovedInternal(node);
                }
            }

            if (node.ParentNode != null)
            {
                if (IsSelfOrDescendant(this, node.ParentNode))
                {
                    OnSubtreeAddedInternal(node);
                }
            }
        }

        private static bool IsSelfOrDescendant(Node refNode, Node node)
        {
            while (node != null)
            {
                if (node == refNode)
                {
                    return true;
                }

                node = node.ParentNode;
            }

            return false;
        }

        /// <summary>
        /// Called when a sub-tree is added to this document tree
        /// </summary>
        /// <param name="node">Root of the sub-tree that was added</param>
        private void OnSubtreeAddedInternal(Node node)
        {
            OnSubtreeAdded(node);
            OnNodeAddedInternal(node);
        }

        /// <summary>
        /// Called when a sub-tree is removed from this document tree
        /// </summary>
        /// <param name="node">Root of the sub-tree that was removed</param>
        private void OnSubtreeRemovedInternal(Node node)
        {
            OnNodeRemovedInternal(node);
            OnSubtreeRemoved(node);
        }

        /// <summary>
        /// Called when a node is added to this document tree
        /// </summary>
        /// <param name="node">The node that was added.</param>
        private void OnNodeAddedInternal(Node node)
        {
            OnNodeAdded(node);

            foreach (var childNode in node.ChildNodes)
            {
                OnNodeAddedInternal(childNode);
            }
        }

        /// <summary>
        /// Called when a node is removed from this document tree.
        /// </summary>
        /// <param name="node">The node that was removed.</param>
        private void OnNodeRemovedInternal(Node node)
        {
            foreach (var childNode in node.ChildNodes)
            {
                OnNodeRemovedInternal(childNode);
            }

            OnNodeRemoved(node);
        }

        /// <summary>
        /// Called when a sub-tree is added to this document tree
        /// </summary>
        /// <param name="node">Root of the sub-tree that was added</param>
        protected virtual void OnSubtreeAdded(Node node)
        {

        }

        /// <summary>
        /// Called when a sub-tree is removed from this document tree
        /// </summary>
        /// <param name="node">Root of the sub-tree that was removed</param>
        protected virtual void OnSubtreeRemoved(Node node)
        {

        }

        /// <summary>
        /// Called when a node is added to this document tree
        /// </summary>
        /// <param name="node">The node that was added</param>
        protected virtual void OnNodeAdded(Node node)
        {

        }

        /// <summary>
        /// Called when a node is removed from this document tree
        /// </summary>
        /// <param name="node">The node that was removed</param>
        protected virtual void OnNodeRemoved(Node node)
        {

        }

        // Keep track of the number of event listeners for optimizations

        Dictionary<string, int> _CapturingEventListenerCounts;
        Dictionary<string, int> _NonCapturingEventListenerCounts;

        Dictionary<string, int> GetEventListenerCounts(bool useCapture)
        {
            if (useCapture)
            {
                if (_CapturingEventListenerCounts == null)
                {
                    _CapturingEventListenerCounts = new Dictionary<string, int>();
                }
                return _CapturingEventListenerCounts;
            }
            else
            {
                if (_NonCapturingEventListenerCounts == null)
                {
                    _NonCapturingEventListenerCounts = new Dictionary<string, int>();
                }
                return _NonCapturingEventListenerCounts;
            }
        }

        internal bool HasEventListeners(string eventType, bool useCapture)
        {
            int count;
            var collection = GetEventListenerCounts(useCapture);
            collection.TryGetValue(eventType, out count);
            return count > 0;
        }

        internal void OnEventListenerAdded(string eventType, EventListener listener, bool useCapture)
        {
            int count;
            var collection = GetEventListenerCounts(useCapture);
            collection.TryGetValue(eventType, out count);
            collection[eventType] = count + 1;
        }

        internal void OnEventListenerRemoved(string eventType, EventListener listener, bool useCapture)
        {
            var collection = GetEventListenerCounts(useCapture);
            collection[eventType]--;
        }

        #endregion
    }

}
