using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Internal
{
    public class AttrProxy : AttrBase
    {
        #region Fields

        IAttributeSource _Source;

        #endregion

        #region Constructors

        internal AttrProxy(Document ownerDocument, string name)
            : base(ownerDocument, name)
        {
            _Source = null;
        }

        #endregion

        #region Attr

        public override string Value
        {
            get { return GetValue(); }
            set { SetValue(value); }
        }

        #endregion

        #region Implementation

        internal override void SetOwnerElement(Element ownerElement)
        {
            SetOwnerElement(ownerElement, true);
        }

        internal void SetOwnerElement(Element ownerElement, bool updateSource)
        {
            base.SetOwnerElement(ownerElement);

            var newSource = ownerElement as IAttributeSource;

            if (newSource == _Source)
            {
                return;
            }

            if (_Source != null)
            {
                base.Value = _Source.OnGetAttribute(Name);
                _Source.OnResetAttribute(Name);
            }

            _Source = newSource;

            if (_Source != null && updateSource)
            {
                _Source.OnSetAttribute(Name, base.Value);
            }
        }

        void SetValue(string newValue)
        {
            if (_Source == null)
            {
                base.Value = newValue;
            }
            else if (!String.IsNullOrEmpty(NamespaceURI))
            {
                _Source.OnSetAttributeNS(NamespaceURI, Name, newValue);
            }
            else
            {
                _Source.OnSetAttribute(Name, newValue);
            }
        }

        string GetValue()
        {
            if (_Source == null)
            {
                return base.Value;
            }
            else if (!String.IsNullOrEmpty(NamespaceURI))
            {
                return _Source.OnGetAttributeNS(NamespaceURI, LocalName) ?? String.Empty;
            }
            else
            {
                return _Source.OnGetAttribute(Name) ?? String.Empty;
            }
        }

        #endregion
    }

    public interface IAttributeSource
    {
        IList<string> OnGetAttributes();
        string OnGetAttribute(string name);
        void OnSetAttribute(string name, string value);
        void OnResetAttribute(string name);
        string OnGetAttributeNS(string namespaceURI, string localName);
        void OnSetAttributeNS(string namespaceURI, string qualifiedName, string value);
        void OnResetAttributeNS(string namespaceURI, string localName);
    }
}
