using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DOM.Internal
{
    /// <summary>
    /// The Attr interface represents an attribute in an Element object. 
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-Core/core.html#ID-637646024
    /// </summary>
    public class AttrBase : NodeBase, Attr
    {
        #region Fields

        Element _OwnerElement;

        #endregion

        #region Constructors

        internal AttrBase(Document ownerDocument, string name)
            : base(ownerDocument, NodeType.ATTRIBUTE_NODE, name)
        {

        }

        internal AttrBase(Document ownerDocument, string namespaceURI, string qualifiedName)
            : base(ownerDocument, NodeType.ATTRIBUTE_NODE, namespaceURI, qualifiedName)
        {

        }

        #endregion

        #region Attr

        /// <summary>
        /// Get this attribute's name
        /// </summary>
        public string Name
        {
            get { return base.NodeName; }
        }

        /// <summary>
        /// Get/set this attribute's value
        /// </summary>
        public virtual string Value
        {
            get { return GetValue(); }
            set { SetValue(value); }
        }

        /// <summary>
        /// Get the type information associated with this attribute
        /// </summary>
        public TypeInfo SchemaTypeInfo
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Determine if this is an ID attribute
        /// </summary>
        public bool IsId
        {
            get { return Name.Equals("id", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Determine if this is a specified (vs. default) attribute
        /// </summary>
        public bool Specified
        {
            get { return true; }    // we don't currently support default attributes
        }

        /// <summary>
        /// Get the element this attribute belongs to (null if none)
        /// </summary>
        public Element OwnerElement
        {
            get { return _OwnerElement; }
        }

        #endregion

        #region Node

        public override string NodeValue
        {
            get { return Value; }
            set { Value = value; }
        }

        public override Node CloneNode(bool deep)
        {
            var result = OwnerDocument.CreateAttribute(Name);

            result.Value = this.Value;

            return result;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return String.Format("{0}=\"{1}\"", Name, Value);
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Set the element this attribute belongs to
        /// </summary>
        /// <param name="ownerElement"></param>
        internal virtual void SetOwnerElement(Element ownerElement)
        {
            _OwnerElement = ownerElement;
        }

        string GetValue()
        {
            if (!HasChildNodes())
            {
                // No children
                return String.Empty;
            }

            if (FirstChild.NodeType == NodeType.TEXT_NODE && FirstChild.NextSibling == null)
            {
                // A single text node (common case)
                return FirstChild.NodeValue;
            }

            // Concatenate text of all child nodes

            StringBuilder buffer = new StringBuilder();

            foreach (Node child in ChildNodes)
            {
                GetValue(child, buffer);
            }

            return buffer.ToString();
        }

        static void GetValue(Node node, StringBuilder buffer)
        {
            if (node.NodeType == NodeType.TEXT_NODE)
            {
                buffer.Append(node.NodeValue);
            }
            else if (node.NodeType == NodeType.ENTITY_REFERENCE_NODE)
            {
                foreach (Node child in node.ChildNodes)
                {
                    GetValue(child, buffer);
                }
            }
        }

        void SetValue(string newValue)
        {
            // Remove all children

            while (FirstChild != null)
            {
                RemoveChild(FirstChild);
            }

            // Add a text node containing the new value

            Text newText = OwnerDocument.CreateTextNode(newValue);
            AppendChild(newText);
        }

        #endregion
    }

}
