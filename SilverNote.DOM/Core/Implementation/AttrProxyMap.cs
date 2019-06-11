using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    /// <summary>
    /// Manages attributes for elements that implement IAttributeSource
    /// </summary>
    public class AttrProxyMap : AttrMap
    {
        #region Fields

        IAttributeSource _Source;

        #endregion

        #region Constructors

        public AttrProxyMap(Element ownerElement)
            : base(ownerElement)
        {
            _Source = ownerElement as IAttributeSource;

            if (_Source == null)
            {
                throw new ArgumentException("ownerElement");
            }
        }

        #endregion

        #region AttrMap

        public override bool HasAttribute(string name)
        {
            return FindAttribute(name) != null;
        }

        public override string GetAttribute(string name)
        {
            return _Source.OnGetAttribute(name);
        }

        public override void SetAttribute(string name, string value)
        {
            _Source.OnSetAttribute(name, value);
        }

        public override void RemoveAttribute(string name)
        {
            _Source.OnResetAttribute(name);
        }

        public override bool HasAttributeNS(string namespaceURI, string localName)
        {
            return FindAttributeNS(namespaceURI, localName) != null;
        }

        public override string GetAttributeNS(string namespaceURI, string localName)
        {
            return _Source.OnGetAttributeNS(namespaceURI, localName);
        }

        public override void SetAttributeNS(string namespaceURI, string qualifiedName, string value)
        {
            _Source.OnSetAttributeNS(namespaceURI, qualifiedName, value);
        }

        public override void RemoveAttributeNS(string namespaceURI, string localName)
        {
            _Source.OnResetAttributeNS(namespaceURI, localName);
        }

        #endregion

        #region NamedNodeMap

        public override int Length
        {
            get
            {
                return _Source.OnGetAttributes().Count;
            }
        }

        public override Node this[int index]
        {
            get
            {
                var attributes = _Source.OnGetAttributes();

                if (index >= 0 && index < attributes.Count)
                {
                    return GetOrCreateAttribute(attributes[index]);
                }
                else
                {
                    return null;
                }
            }
        }

        public override Node GetNamedItem(string nodeName)
        {
            string attr = FindAttribute(nodeName);

            if (attr != null)
            {
                return GetOrCreateAttribute(attr);
            }
            else
            {
                return null;
            }
        }

        public override Node GetNamedItemNS(string namespaceURI, string localName)
        {
            string attr = FindAttributeNS(namespaceURI, localName);

            if (attr != null)
            {
                return GetOrCreateAttribute(attr);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Implementation

        private string FindAttribute(string name)
        {
            var attributes = _Source.OnGetAttributes();

            for (int i = 0; i < attributes.Count; i++)
            {
                string attr = attributes[i];

                if (attr.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                    attr.EndsWith(" " + name, StringComparison.OrdinalIgnoreCase))
                {
                    return attr;
                }
            }

            return null;
        }

        private string FindAttributeNS(string namespaceURI, string localName)
        {
            var attributes = _Source.OnGetAttributes();

            for (int i = 0; i < attributes.Count; i++)
            {
                string attr = attributes[i];

                if ((String.IsNullOrEmpty(namespaceURI)
                    || attr.StartsWith(namespaceURI + " ", StringComparison.OrdinalIgnoreCase))
                    && (attr.EndsWith(" " + localName, StringComparison.OrdinalIgnoreCase)
                    || attr.EndsWith(":" + localName, StringComparison.OrdinalIgnoreCase)))
                {
                    return attr;
                }
            }

            return null;
        }

        Node GetOrCreateAttribute(string attr)
        {
            int i = attr.IndexOf(' ');
            if (i != -1)
            {
                string namespaceURI = attr.Substring(0, i);
                string qualifiedName = attr.Substring(i + 1);
                string prefix = "";
                string localName = qualifiedName;

                int j = qualifiedName.IndexOf(':');
                if (j != -1)
                {
                    prefix = qualifiedName.Substring(0, j);
                    localName = qualifiedName.Substring(j + 1);
                }

                Node node = base.GetNamedItemNS(namespaceURI, localName);

                if (node == null)
                {
                    node = OwnerElement.OwnerDocument.CreateAttributeNS(namespaceURI, qualifiedName);
                    ((AttrProxy)node).SetOwnerElement(OwnerElement, false);
                    base.SetNamedItem(node);
                }

                return node;
            }
            else
            {
                Node node = base.GetNamedItem(attr);

                if (node == null)
                {
                    node = OwnerElement.OwnerDocument.CreateAttribute(attr);
                    ((AttrProxy)node).SetOwnerElement(OwnerElement, false);
                    base.SetNamedItem(node);
                }

                return node;
            }
        }

        #endregion
    }
}
