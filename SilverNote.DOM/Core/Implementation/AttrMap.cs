using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    /// <summary>
    /// A NamedNodeMap specialized for storing attribute nodes
    /// </summary>
    public class AttrMap : NamedNodeMapBase
    {
        #region Constructors

        public AttrMap(Element ownerElement)
            : base(ownerElement)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the value of the attribute with the given name.
        /// </summary>
        /// <param name="name">Name of the attribute to retrieve.</param>
        /// <returns>The retrieved attribute value, or String.Empty if not set</returns>
        public virtual string GetAttribute(string name)
        {
            Attr attr = GetAttributeNode(name);

            if (attr != null)
            {
                return attr.Value;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Get the attribute with the given name.
        /// </summary>
        /// <param name="name">Name of the attribute to retrieve.</param>
        /// <returns>The retrieved attribute, or null if not found</returns>
        public virtual Attr GetAttributeNode(string name)
        {
            return (Attr)GetNamedItem(name);
        }

        /// <summary>
        /// Determine if this collection contains an attribute with the given name.
        /// </summary>
        public virtual bool HasAttribute(string name)
        {
            return GetNamedItem(name) != null;
        }

        /// <summary>
        /// Remove the attribute with the given name
        /// </summary>
        /// <exception cref="DOMException">
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this node is readonly.
        /// </exception>
        public virtual void RemoveAttribute(string name)
        {
            if (HasAttribute(name))
            {
                RemoveNamedItem(name);
            }
        }

        /// <summary>
        /// Remove the given attribute from this collection
        /// </summary>
        /// <param name="oldAttr">The attribute to be removed</param>
        /// <returns>The removed attribute</returns>
        /// <exception cref="DOMException">
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this node is readonly.
        /// NOT_FOUND_ERR: Raised if oldAttr is not an attribute of the element.
        /// </exception>
        public virtual Attr RemoveAttrNode(Attr oldAttr)
        {
            return (Attr)RemoveNamedItem(oldAttr);
        }

        /// <summary>
        /// Set the value of the attribute with the given name.
        /// 
        /// If this collection does not contain an attribute with the given
        /// name, one is added.
        /// </summary>
        /// <exception cref="DOMException">
        /// INVALID_CHARACTER_ERR: Raised if the specified name contains an illegal character.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this node is readonly.
        /// </exception>
        public virtual void SetAttribute(string name, string value)
        {
            if (IsReadOnly)
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }

            var attr = GetAttributeNode(name);
            if (attr != null)
            {
                attr.Value = value;
                return;
            }

            attr = OwnerElement.OwnerDocument.CreateAttribute(name);
            attr.Value = value;

            SetAttributeNode(attr);
        }

        /// <summary>
        /// Add a node to this map.
        /// 
        /// If a node with the same name is already in the map, that node is
        /// replaced (and the replaced node is returned).
        /// </summary>
        /// <param name="newNode">The node to be added.</param>
        /// <returns>The node that was replaced by the new node, or null if
        /// no node was replaced.</returns>
        /// <exception cref="DOMException">
        public virtual Attr SetAttributeNode(Attr newAttr)
        {
            return (Attr)SetNamedItem(newAttr);
        }

        /// <summary>
        /// Get the value of the attribute with the given name.
        /// </summary>
        /// <returns>The retrieved attribute value, or String.Empty if not set</returns>
        public virtual string GetAttributeNS(string namespaceURI, string localName)
        {
            Attr attr = GetAttributeNodeNS(namespaceURI, localName);

            if (attr != null)
            {
                return attr.Value;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Get the attribute with the given name.
        /// </summary>
        /// <returns>The retrieved attribute, or null if not found</returns>
        public virtual Attr GetAttributeNodeNS(string namespaceURI, string localName)
        {
            return (Attr)GetNamedItemNS(namespaceURI, localName);
        }

        /// <summary>
        /// Determine if this collection contains an attribute with the given name.
        /// </summary>
        public virtual bool HasAttributeNS(string namespaceURI, string localName)
        {
            return GetNamedItemNS(namespaceURI, localName) != null;
        }

        /// <summary>
        /// Remove the attribute with the given name
        /// </summary>
        /// <exception cref="DOMException">
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this node is readonly.
        /// </exception>
        public virtual void RemoveAttributeNS(string namespaceURI, string localName)
        {
            if (HasAttributeNS(namespaceURI, localName))
            {
                RemoveNamedItemNS(namespaceURI, localName);
            }
        }

        /// <summary>
        /// Set the value of the attribute with the given name.
        /// 
        /// If this collection does not contain an attribute with the given
        /// name, one is added.
        /// </summary>
        /// <exception cref="DOMException">
        /// INVALID_CHARACTER_ERR: Raised if the specified name contains an illegal character.
        /// NO_MODIFICATION_ALLOWED_ERR: Raised if this node is readonly.
        /// </exception>
        public virtual void SetAttributeNS(string namespaceURI, string qualifiedName, string value)
        {
            if (IsReadOnly)
            {
                throw new DOMException(DOMException.NO_MODIFICATION_ALLOWED_ERR);
            }

            string prefix = "";
            string localName = qualifiedName;

            int i = qualifiedName.IndexOf(':');
            if (i != -1)
            {
                prefix = qualifiedName.Substring(0, i);
                localName = qualifiedName.Substring(i + 1);
            }

            var attr = GetAttributeNodeNS(namespaceURI, localName);
            if (attr != null)
            {
                attr.Prefix = prefix;
                attr.Value = value;
                return;
            }

            attr = OwnerElement.OwnerDocument.CreateAttributeNS(namespaceURI, qualifiedName);
            attr.Value = value;

            SetAttributeNodeNS(attr);
        }

        /// <summary>
        /// Add a node to this map.
        /// 
        /// If a node with the same name is already in the map, that node is
        /// replaced (and the replaced node is returned).
        /// </summary>
        /// <param name="newNode">The node to be added.</param>
        /// <returns>The node that was replaced by the new node, or null if
        /// no node was replaced.</returns>
        /// <exception cref="DOMException">
        public virtual Attr SetAttributeNodeNS(Attr newAttr)
        {
            return (Attr)SetNamedItemNS(newAttr);
        }

        #endregion
    }
}
