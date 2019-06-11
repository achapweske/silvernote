/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.Internal;
using DOM.CSS;
using DOM.LS;
using DOM.Style;
using DOM.Style.Internal;

namespace DOM.SVG.Internal
{
    /// <summary>
    /// An HTMLDocument is the root of the HTML hierarchy and holds the entire content. Besides providing access to the hierarchy, it also provides some convenience methods for accessing certain sets of information from the document.
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-26809268
    /// </summary>
    public class SVGDocumentImpl : DocumentBase, SVGDocument, DocumentCSS
    {
        #region Fields

        string _Referrer;
        string _Domain;
        SVGTitleElement _Title;
        MutableStyleSheetList _StyleSheets;

        #endregion

        #region Constructors

        public SVGDocumentImpl(DOMImplementationBase implementation, DocumentType docType)
            : base(implementation, docType)
        {
            var documentElement = CreateElementNS(SVGElements.NAMESPACE, SVGElements.SVG);
            documentElement.SetAttributeNS(null, SVGAttributes.VERSION, "1.1");
            AppendChild(documentElement);
        }

        #endregion

        #region SVGDocument

        public string Title
        {
            get
            {
                if (_Title != null)
                {
                    return _Title.TextContent;
                }
                else
                {
                    return String.Empty;
                }
            }
            set
            {
                if (value != null)
                {
                    GetOrCreateTitle().TextContent = value;
                }
                else if (_Title != null)
                {
                    _Title.ParentNode.RemoveChild(_Title);
                }
            }
        }

        public string Referrer
        {
            get { return _Referrer; }
        }

        public string Domain
        {
            get { return _Domain; }
        }

        public string URL
        {
            get { return DocumentURI; }
        }

        public SVGSVGElement RootElement
        {
            get { return (SVGSVGElement)DocumentElement; }
        }

        #endregion

        #region DocumentCSS

        public StyleSheetList StyleSheets
        {
            get
            {
                if (_StyleSheets != null)
                {
                    return _StyleSheets;
                }
                else
                {
                    return StyleSheetListBase.Empty;
                }
            }
        }

        public CSSStyleDeclaration GetOverrideStyle(Element elt, string pseudoElt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// User style sheet (optional)
        /// </summary>
        public CSSStyleSheet UserStyleSheet { get; set; }

        #endregion

        #region Extensions

        public void SetReferrer(string newReferrer)
        {
            _Referrer = newReferrer;
        }

        public void SetDomain(string newDomain)
        {
            _Domain = newDomain;
        }

        #endregion

        #region Implementation

        SVGTitleElement GetOrCreateTitle()
        {
            if (_Title != null)
            {
                return _Title;
            }

            var title = (SVGTitleElement)CreateElement(SVGElements.TITLE);

            DocumentElement.InsertBefore(title, null);

            return title;
        }

        MutableStyleSheetList InternalStyleSheets
        {
            get
            {
                if (_StyleSheets == null)
                {
                    _StyleSheets = new MutableStyleSheetList();
                }

                return _StyleSheets;
            }
        }

        protected override Element OnCreateElement(string tagName)
        {
            throw new InvalidOperationException();
        }

        protected override Element OnCreateElementNS(string namespaceURI, string qualifiedName)
        {
            if (SVGElements.NAMESPACE.Equals(namespaceURI, StringComparison.OrdinalIgnoreCase))
            {
                switch (qualifiedName)
                {
                    case SVGElements.DEFS:
                        return new SVGDefsElementImpl(this);
                    case SVGElements.FILTER:
                        return new SVGFilterElementImpl(this);
                    case SVGElements.LINE:
                        return new SVGLineElementImpl(this);
                    case SVGElements.LINEAR_GRADIENT:
                        return new SVGLinearGradientElementImpl(this);
                    case SVGElements.SCRIPT:
                        return new SVGScriptElementImpl(this);
                    case SVGElements.STOP:
                        return new SVGStopElementImpl(this);
                    case SVGElements.STYLE:
                        return new SVGStyleElementImpl(this);
                    case SVGElements.SVG:
                        return new SVGSVGElementImpl(this);
                    case SVGElements.TITLE:
                        return new SVGTitleElementImpl(this);
                    case SVGElements.TEXT:
                        return new SVGTextElementImpl(this);
                    case SVGElements.TSPAN:
                        return new SVGTSpanElementImpl(this);
                    case SVGElements.USE:
                        return new SVGUseElementImpl(this);
                    default:
                        break;
                }
            }

            return new SVGElementBase(this, namespaceURI, qualifiedName);
        }

        protected override void OnNodeAdded(Node node)
        {
            if (node.NodeType == NodeType.ELEMENT_NODE)
            {
                switch (node.NodeName)
                {
                    case SVGElements.TITLE:
                        _Title = (SVGTitleElement)node;
                        break;
                    case SVGElements.STYLE:
                        var style = (SVGStyleElementImpl)node;
                        InternalStyleSheets.AppendSheet(style.Sheet);
                        break;
                }
            }
        }

        protected override void OnNodeRemoved(Node node)
        {
            if (node.NodeType == NodeType.ELEMENT_NODE)
            {
                switch (node.NodeName)
                {
                    case SVGElements.TITLE:
                        _Title = null;
                        break;
                    case SVGElements.STYLE:
                        var style = (SVGStyleElementImpl)node;
                        InternalStyleSheets.RemoveSheet(style.Sheet);
                        break;
                }
            }
        }

        #endregion

        #region Object

        public override string ToString()
        {
            LSSerializer serializer = DOMFactory.CreateLSSerializer();
            serializer.Config.SetParameter(LSSerializerParameters.XML_DECLARATION, false);
            return serializer.WriteToString(this);
        }

        #endregion

    }
}
