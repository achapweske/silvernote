using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public abstract class NodeProxy : NodeBase, NodeContext
    {
        #region Constructors

        internal NodeProxy(Document ownerDocument, string nodeName, NodeType nodeType)
            : base(ownerDocument, nodeType, nodeName)
        {

        }

        internal NodeProxy(Document ownerDocument, string namespaceURI, string qualifiedName, NodeType nodeType)
            : base(ownerDocument, nodeType, namespaceURI, qualifiedName)
        {

        }

        #endregion

        #region Node

        public override NodeType NodeType
        {
            get
            {
                if (Source != null)
                {
                    return Source.GetNodeType(this);
                }
                else
                {
                    return base.NodeType;
                }
            }
        }

        public override string NamespaceURI
        {
            get
            {
                if (Source != null)
                {
                    return ParseSourceNamespaceURI(Source.GetNodeName(this));
                }
                else
                {
                    return base.NamespaceURI;
                }
            }
        }

        public override string LocalName
        {
            get
            {
                if (Source != null)
                {
                    return ParseSourceLocalName(Source.GetNodeName(this));
                }
                else 
                {
                    return base.LocalName;
                }
            }
        }

        public override string NodeName
        {
            get
            {
                if (Source != null)
                {
                    return ParseSourceNodeName(Source.GetNodeName(this));
                }
                else
                {
                    return base.NodeName;
                }
            }
        }

        public override NodeList ChildNodes
        {
            get
            {
                if (Source != null)
                {
                    return new NodeListBase(SourceChildNodes);
                }
                else
                {
                    return base.ChildNodes;
                }
            }
        }

        public override Node ParentNode
        {
            get
            {
                if (Source != null)
                {
                    return SourceParentNode;
                }
                else
                {
                    return base.ParentNode;
                }
            }
        }

        public override Node FirstChild
        {
            get
            {
                if (Source != null)
                {
                    return FirstSourceChild;
                }
                else
                {
                    return base.FirstChild;
                }
            }
        }

        public override Node LastChild
        {
            get
            {
                if (Source != null)
                {
                    return LastSourceChild;
                }
                else
                {
                    return base.LastChild;
                }
            }
        }

        public override Node PreviousSibling
        {
            get
            {
                if (Source != null)
                {
                    return PreviousSourceSibling;
                }
                else
                {
                    return base.PreviousSibling;
                }
            }
        }

        public override Node NextSibling
        {
            get
            {
                if (Source != null)
                {
                    return NextSourceSibling;
                }
                else
                {
                    return base.NextSibling;
                }
            }
        }

        public override Node InsertBefore(Node newChild, Node refChild)
        {
            if (newChild.ParentNode != null)
            {
                newChild.ParentNode.RemoveChild(newChild);
            }

            if (Source != null && newChild is NodeProxy && (refChild is NodeProxy || refChild == null))
            {
                if (refChild != null)
                {
                    SourceInsertBefore((NodeProxy)newChild, (NodeProxy)refChild);
                }
                else
                {
                    SourceAppendChild((NodeProxy)newChild);
                }
            }
            else
            {
                base.InsertBefore(newChild, refChild);
            }

            return newChild;
        }

        public override Node RemoveChild(Node oldChild)
        {
            if (Source != null && oldChild is NodeProxy)
            {
                SourceRemoveChild((NodeProxy)oldChild);
            }
            else
            {
                base.RemoveChild(oldChild);
            }

            return oldChild;
        }

        #endregion

        #region Source

        private INodeSource _Source;

        /// <summary>
        /// An object to which DOM operations are delegated (optional)
        /// </summary>
        public INodeSource Source
        {
            get 
            { 
                return _Source; 
            }
        }

        /// <summary>
        /// Set an object to which DOM operations will be delegated
        /// </summary>
        /// <param name="newSource"></param>
        public void Bind(INodeSource newSource)
        {
            var oldSource = _Source;

            if (newSource != oldSource)
            {
                RaiseSourceChanging(newSource);
                if (oldSource != null)
                    oldSource.NodeEvent -= Source_NodeEvent;
                _Source = newSource;
                if (newSource != null)
                    newSource.NodeEvent += Source_NodeEvent;
                RaiseSourceChanged(oldSource);
            }
        }

        /// <summary>
        /// Invoked when Source is about to change
        /// </summary>
        public event EventHandler<SourceChangingEventArgs> SourceChanging;

        /// <summary>
        /// This method calls OnSourceChanging() and raises the SourceChanging event
        /// </summary>
        protected void RaiseSourceChanging(INodeSource newSource)
        {
            OnSourceChanging(newSource);

            if (SourceChanging != null)
            {
                SourceChanging(this, new SourceChangingEventArgs(newSource));
            }
        }

        /// <summary>
        /// Called when Source is about to change
        /// </summary>
        protected virtual void OnSourceChanging(INodeSource newSource)
        {

        }

        /// <summary>
        /// Invoked after Source has changed
        /// </summary>
        public event EventHandler<SourceChangedEventArgs> SourceChanged;

        /// <summary>
        /// This method calls OnSourceChanged() and raises the SourceChanged event
        /// </summary>
        protected void RaiseSourceChanged(INodeSource oldSource)
        {
            OnSourceChanged(oldSource);

            if (SourceChanged != null)
            {
                SourceChanged(this, new SourceChangedEventArgs(oldSource));
            }
        }

        /// <summary>
        /// Called after Source has changed
        /// </summary>
        /// <param name="oldSource"></param>
        protected virtual void OnSourceChanged(INodeSource oldSource)
        {

        }

        private void Source_NodeEvent(IEventSource evt)
        {
            evt.Dispatch(this);
        }

        /// <summary>
        /// Set the delegate object
        /// </summary>
        /// <param name="newSource">New delegate</param>
        public void Render(INodeSource newSource)
        {
            if (newSource == null)
            {
                Bind(newSource);
                return;
            }

            // Filter each node through newSource.CreateNode().

            var oldChildren = ChildNodes.ToArray();
            var newChildren = new List<object>();

            foreach (var childNode in oldChildren)
            {
                object newNode = childNode;

                if (childNode is NodeProxy)
                {
                    newNode = newSource.CreateNode((NodeProxy)childNode);

                    if (newNode is INodeSource)
                    {
                        ((NodeProxy)childNode).Render((INodeSource)newNode);
                    }
                }

                if (newNode != null)
                {
                    newChildren.Add(newNode);
                }
            }

            // Then, in a bottom-up fashion, initialize each INodeSource.

            if (HasAttributes())
            {
                foreach (Attr attr in Attributes)
                {
                    try
                    {
                        newSource.SetNodeAttribute((ElementContext)this, attr.Name, attr.Value);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error rendering attribute \"{0}\" for element \"{1}\":\n\t{2}", attr.Name, this.NodeName, e.Message);
                    }
                }
            }

            OnRender(newSource);

            var value = NodeValue;

            Bind(newSource);

            NodeValue = value;

            // Add children to their parent AFTER the parent is initialized

            foreach (var childNode in newChildren)
            {
                newSource.AppendNode(this, childNode);
            }
        }

        protected virtual void OnRender(INodeSource newSource)
        {

        }

        /// <summary>
        /// Determine if the currently-set source is valid
        /// </summary>
        public bool IsSourceValid
        {
            get
            {
                // The delegate associated with this node must report
                // the same node type and name used to create this node.

                if (Source != null &&
                    base.NodeType == Source.GetNodeType(this) &&
                    String.Equals(base.NamespaceURI, ParseSourceNamespaceURI(Source.GetNodeName(this)), StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(base.LocalName, ParseSourceLocalName(Source.GetNodeName(this)), StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(base.NodeName, ParseSourceNodeName(Source.GetNodeName(this)), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get all child nodes associated with our source
        /// </summary>
        private IEnumerable<Node> SourceChildNodes
        {
            get
            {
                var childNodes = Source.GetChildNodes(this);

                if (childNodes != null)
                {
                    foreach (var child in childNodes)
                    {
                        if (child is Node)
                        {
                            yield return (Node)child;
                        }
                        else if (child is INodeSource)
                        {
                            yield return OwnerDocument.GetNode((INodeSource)child);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the parent node associated with our source
        /// </summary>
        private Node SourceParentNode
        {
            get
            {
                var parent = Source.GetParentNode(this);

                if (parent is Node)
                {
                    return (Node)parent;
                }
                else if (parent is INodeSource)
                {
                    return OwnerDocument.GetNode((INodeSource)parent);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the first child node associated with our source
        /// </summary>
        private Node FirstSourceChild
        {
            get
            {
                var childNodes = Source.GetChildNodes(this);

                if (childNodes != null)
                {
                    var firstChild = childNodes.FirstOrDefault();

                    if (firstChild is Node)
                    {
                        return (Node)firstChild;
                    }
                    else if (firstChild is INodeSource)
                    {
                        return OwnerDocument.GetNode((INodeSource)firstChild);
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Get the last child node associated with our source
        /// </summary>
        private Node LastSourceChild
        {
            get
            {
                var childNodes = Source.GetChildNodes(this);

                if (childNodes != null)
                {
                    var lastChild = childNodes.LastOrDefault();

                    if (lastChild is Node)
                    {
                        return (Node)lastChild;
                    }
                    else if (lastChild is INodeSource)
                    {
                        return OwnerDocument.GetNode((INodeSource)lastChild);
                    }
                }

                return null;
            }
        }

        private Node NextSourceSibling
        {
            get
            {
                return null;
            }
        }

        private Node PreviousSourceSibling
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Append a node to our source's list of children
        /// </summary>
        private void SourceAppendChild(Node newChild)
        {
            var oldParentNode = newChild.ParentNode;

            // Filter the new child through Source.CreateNode()

            object newObject = newChild;

            if (newChild is NodeProxy)
            {
                newObject = Source.CreateNode((NodeProxy)newChild);

                if (newObject != newChild)
                {
                    if (newObject is INodeSource)
                    {
                        ((NodeProxy)newChild).Render((INodeSource)newObject);
                    }
                    else
                    {
                        throw new DOMException(DOMException.HIERARCHY_REQUEST_ERR);
                    }
                }
            }

            try
            {
                Source.AppendNode(this, newObject);
            }
            catch (InvalidCastException)
            {
                throw new DOMException(DOMException.HIERARCHY_REQUEST_ERR);
            }

            if (newChild.ParentNode != oldParentNode && newChild is NodeProxy)
            {
                ((NodeProxy)newChild).RaiseParentNodeChanged(oldParentNode);
            }
        }

        /// <summary>
        /// Insert a node into our source's list of children
        /// </summary>
        private void SourceInsertBefore(Node newChild, Node refChild)
        {
            var oldParentNode = newChild.ParentNode;

            // Filter the new child through Source.CreateNode()

            object newObject = newChild;

            if (newChild is NodeProxy)
            {
                newObject = Source.CreateNode((NodeProxy)newChild);

                if (newObject != newChild)
                {
                    if (newObject is INodeSource)
                    {
                        ((NodeProxy)newChild).Render((INodeSource)newObject);
                    }
                    else
                    {
                        throw new DOMException(DOMException.HIERARCHY_REQUEST_ERR);
                    }
                }
            }

            try
            {
                Source.InsertNode(this, newObject, refChild.SourceOrSelf());
            }
            catch (InvalidCastException)
            {
                throw new DOMException(DOMException.HIERARCHY_REQUEST_ERR);
            }

            if (newChild.ParentNode != oldParentNode && newChild is NodeProxy)
            {
                ((NodeProxy)newChild).RaiseParentNodeChanged(oldParentNode);
            }
        }

        /// <summary>
        /// Remove a node from our source's list of children
        /// </summary>
        /// <param name="oldChild"></param>
        private void SourceRemoveChild(Node oldChild)
        {
            Source.RemoveNode(this, oldChild.SourceOrSelf());

            if (oldChild is NodeProxy)
            {
                ((NodeProxy)oldChild).RaiseParentNodeChanged(this);
            }
        }

        internal static string ParseSourceNamespaceURI(string name)
        {
            int index = name.IndexOf(' ');
            if (index != -1)
                return name.Substring(0, index);
            else
                return null;
        }

        internal static string ParseSourceLocalName(string name)
        {
            int i = name.IndexOf(' ');
            if (i == -1)
                return null;
            int j = name.IndexOf(':', i);
            if (j != -1)
                return name.Substring(j + 1);
            else
                return name.Substring(i + 1);
        }

        internal static string ParseSourceNodeName(string name)
        {
            int index = name.IndexOf(' ');
            if (index != -1)
                return name.Substring(index + 1).ToLower();
            else
                return name.ToLower();
        }

        #endregion
    }

    public class SourceChangingEventArgs : EventArgs
    {
        public SourceChangingEventArgs(INodeSource newSource)
        {
            _NewSource = newSource;
        }

        private INodeSource _NewSource;

        public INodeSource NewSource
        {
            get { return _NewSource; }
        }
    }

    public class SourceChangedEventArgs : EventArgs
    {
        public SourceChangedEventArgs(INodeSource oldSource)
        {
            _OldSource = oldSource;
        }

        private INodeSource _OldSource;

        public INodeSource OldSource
        {
            get { return _OldSource; }
        }
    }
}
