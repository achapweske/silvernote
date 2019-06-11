using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.Internal
{
    /// <summary>
    /// A collection of nodes that can be accessed by name
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Core-20001113/core.html#ID-1780488922
    /// </summary>
    public class NamedNodeMapBase : NamedNodeMap, IEnumerable<Node>
    {
        #region Fields

        Element _OwnerElement;
        List<Node> _Items;

        #endregion

        #region Constructors

        public NamedNodeMapBase(Element ownerElement)
        {
            _Items = new List<Node>();
            _OwnerElement = ownerElement;
        }

        #endregion

        #region NamedNodeMap

        /// <summary>
        /// The number of nodes in this map
        /// </summary>
        public virtual int Length
        {
            get { return _Items.Count; }
        }

        /// <summary>
        /// Get the node at the given index.
        /// </summary>
        /// <param name="index">An index in the range 0 to Length - 1</param>
        /// <returns>The node at the given index, or null if index is out of range</returns>
        public virtual Node this[int index]
        {
            get
            {
                if (index >= 0 && index < _Items.Count)
                {
                    return _Items[index];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the node with the specified name
        /// </summary>
        /// <param name="name">Name of the node to retrieve.</param>
        /// <returns>The retrieved node, or null if not found</returns>
        public virtual Node GetNamedItem(string nodeName)
        {
            int index = IndexOf(nodeName);

            if (index != -1)
            {
                return _Items[index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Add a node to this map.
        /// 
        /// If a node with the same name is already in the map, that node is replaced.
        /// </summary>
        /// <param name="newNode">The node to be added.</param>
        /// <returns>The node that was replaced by the new node, or null if no node was replaced.</returns>
        /// <exception cref="DOMException">
        /// WRONG_DOCUMENT_ERR: Raised if newNode was created from a different document than the one that created this map.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this map is readonly.
        /// INUSE_ATTRIBUTE_ERR: Raised if newNode is an Attr that is already an attribute of another Element object.
        /// </exception>
        public virtual Node SetNamedItem(Node newNode)
        {
            if (IsReadOnly)
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }

            // Make sure newNode belongs to the correct document

            if (newNode.OwnerDocument != OwnerElement.OwnerDocument)
            {
                throw new DOMException(DOMException.WRONG_DOCUMENT_ERR);
            }

            // Make sure newNode does not already belong to another NamedNodeMap

            var oldOwnerElement = GetOwnerElement(newNode);

            if (oldOwnerElement != null && oldOwnerElement != this.OwnerElement)
            {
                throw new DOMException(DOMException.INUSE_ATTRIBUTE_ERR);
            }

            // Get the index of the node to be replaced (-1 if none)

            int index = IndexOf(newNode.NodeName);

            // Now actually add the node to this collection

            return ReplaceItem(newNode, index);
        }

        /// <summary>
        /// Remove the node with the given name
        /// </summary>
        /// <param name="nodeName">Name of the node to be removed</param>
        /// <returns>The removed node</returns>
        /// <exception cref="DOMException">
        /// NOT_FOUND_ERR: Raised if there is no node with the specified namespaceURI and localName in this map.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this map is readonly.
        /// </exception>
        public virtual Node RemoveNamedItem(string nodeName)
        {
            if (IsReadOnly)
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }

            int index = IndexOf(nodeName);

            if (index == -1)
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }

            return RemoveAt(index);
        }

        /// <summary>
        /// Get the node with the specified namespace URI and local name
        /// </summary>
        /// <param name="namespaceURI">Namespace URI of the node to retrieve</param>
        /// <param name="localName">Local name of the node to retrieve</param>
        /// <returns>The retrieved node, or null if not found</returns>
        public virtual Node GetNamedItemNS(string namespaceURI, string localName)
        {
            int index = IndexOfNS(namespaceURI, localName);

            if (index != -1)
            {
                return _Items[index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Add a node to this map.
        /// 
        /// If a node with the same name is already in the map, that node is replaced.
        /// </summary>
        /// <param name="newNode">The node to be added.</param>
        /// <returns>The node that was replaced by the new node, or null if no node was replaced.</returns>
        /// <exception cref="DOMException">
        /// WRONG_DOCUMENT_ERR: Raised if newNode was created from a different document than the one that created this map.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this map is readonly.
        /// INUSE_ATTRIBUTE_ERR: Raised if newNode is an Attr that is already an attribute of another Element object.
        /// </exception>
        public virtual Node SetNamedItemNS(Node newNode)
        {
            if (IsReadOnly)
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }

            // Make sure newNode belongs to the correct document

            if (newNode.OwnerDocument != OwnerElement.OwnerDocument)
            {
                throw new DOMException(DOMException.WRONG_DOCUMENT_ERR);
            }

            // Make sure newNode does not already belong to another NamedNodeMap

            var oldOwnerElement = GetOwnerElement(newNode);

            if (oldOwnerElement != null && oldOwnerElement != this.OwnerElement)
            {
                throw new DOMException(DOMException.INUSE_ATTRIBUTE_ERR);
            }

            // Get the index of the node to be replaced (-1 if none)

            int index = IndexOfNS(newNode.NamespaceURI, newNode.LocalName);

            // Now actually add the node to this collection

            return ReplaceItem(newNode, index);
        }

        /// <summary>
        /// Remove the node with the given namespace URI and local name
        /// </summary>
        /// <param name="namespaceURI">Namespace URI of the node to be removed</param>
        /// <param name="localName">Local name of the node to be removed</param>
        /// <returns>The removed node</returns>
        /// <exception cref="DOMException">
        /// NOT_FOUND_ERR: Raised if there is no node with the specified namespaceURI and localName in this map.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this map is readonly.
        /// </exception>
        public virtual Node RemoveNamedItemNS(string namespaceURI, string localName)
        {
            if (IsReadOnly)
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }

            int index = IndexOfNS(namespaceURI, localName);

            if (index == -1)
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }

            return RemoveAt(index);
        }

        #endregion

        #region IEnumerable

        public virtual IEnumerator<Node> GetEnumerator()
        {
            int length = this.Length;

            for (int i = 0; i < length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation

        protected bool IsReadOnly { get; set; }

        /// <summary>
        /// Get the element associated with this map.
        /// </summary>
        protected Element OwnerElement
        {
            get { return _OwnerElement; }
        }

        /// <summary>
        /// Get the index of the node with the given name
        /// </summary>
        /// <param name="nodeName">Name of the node to lookup</param>
        /// <returns>Index of the retrieved node, or -1 if not found</returns>
        protected int IndexOf(string nodeName)
        {
            for (int i = 0; i < _Items.Count; i++)
            {
                Node node = _Items[i];

                if (String.Equals(node.NodeName, nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the index of the node with the given namespace URI and local name
        /// </summary>
        /// <param name="namespaceURI">Namespace URI of the node to lookup</param>
        /// <param name="localName">Local name of the node to lookup</param>
        /// <returns>Index of the retrieved node, or -1 if not found</returns>
        protected int IndexOfNS(string namespaceURI, string localName)
        {
            for (int i = 0; i < _Items.Count; i++)
            {
                Node node = _Items[i];

                if (String.Equals(node.NamespaceURI, namespaceURI, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(node.LocalName, localName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Remove the node at the given index
        /// </summary>
        /// <param name="index">Index of the node to be removed</param>
        /// <returns>The removed node</returns>
        protected Node RemoveAt(int index)
        {
            Node oldNode = _Items[index];

            _Items.RemoveAt(index);

            SetOwnerElement(oldNode, null);

            return oldNode;
        }

        /// <summary>
        /// Replace a node with the given node.
        /// </summary>
        /// <param name="newNode">The node to be added</param>
        /// <param name="index">Index of the node to be replaced, or -1 for none.</param>
        /// <returns>The newly-added node.</returns>
        protected Node ReplaceItem(Node newNode, int index)
        {
            Node replacedNode = null;

            // Remove the element at the given index

            if (index != -1)
            {
                replacedNode = _Items[index];
                _Items.RemoveAt(index);
                SetOwnerElement(replacedNode, null);
            }

            // Append newNode and update its OwnerElement

            _Items.Add(newNode);

            SetOwnerElement(newNode, OwnerElement);

            return replacedNode;
        }


        /// <summary>
        /// Remove the given node from this collection
        /// </summary>
        /// <param name="oldNode">The node to be removed</param>
        /// <returns>The removed node</returns>
        /// <exception cref="DOMException">
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this node is readonly.
        /// NOT_FOUND_ERR: Raised if oldNode is not in this collection.
        /// </exception>
        protected virtual Node RemoveNamedItem(Node oldNode)
        {
            if (IsReadOnly)
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }

            if (!_Items.Remove(oldNode))
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }

            SetOwnerElement(oldNode, null);

            return oldNode;
        }

        private static Element GetOwnerElement(Node node)
        {
            if (node.NodeType == NodeType.ATTRIBUTE_NODE)
            {
                return ((Attr)node).OwnerElement;
            }
            else
            {
                return null;
            }
        }

        private static void SetOwnerElement(Node node, Element ownerElement)
        {
            if (node is AttrBase)
            {
                ((AttrBase)node).SetOwnerElement(ownerElement);
            }
        }

        #endregion

    }
}
