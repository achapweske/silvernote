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
using DOM.HTML;
using DOM.SVG;
using DOM.LS;

namespace DOM
{
    public static partial class DOMFactory
    {
        public static DocumentType CreateDocumentType(string qualifiedName, string publicId, string systemId)
        {
            return DOMImplementationBase.Default.CreateDocumentType(qualifiedName, publicId, systemId);
        }

        public static Document CreateDocument(string namespaceURI, string qualifiedName, DocumentType docType)
        {
            return DOMImplementationBase.Default.CreateDocument(namespaceURI, qualifiedName, docType);
        }

        public static HTMLDocument CreateHTMLDocument()
        {
            return (HTMLDocument)CreateDocument(null, HTMLElements.HTML, null);
        }

        public static SVGDocument CreateSVGDocument()
        {
            return (SVGDocument)CreateDocument(SVGElements.NAMESPACE, SVGElements.SVG, null);
        }

        public static CSSStyleDeclaration CreateCSSStyleDeclaration()
        {
            return new CSS.Internal.CSSStyleDeclarationBase();
        }

        public static LSParser CreateLSParser(DOMImplementationLSMode mode, string schemaType)
        {
            var dom = DOMImplementationBase.Default as DOMImplementationLS;
            if (dom != null)
            {
                return dom.CreateLSParser(mode, schemaType);
            }
            else
            {
                return null;
            }
        }

        public static LSSerializer CreateLSSerializer()
        {
            var dom = DOMImplementationBase.Default as DOMImplementationLS;
            if (dom != null)
            {
                return dom.CreateLSSerializer();
            }
            else
            {
                return null;
            }
        }

        public static LSInput CreateLSInput()
        {
            var dom = DOMImplementationBase.Default as DOMImplementationLS;
            if (dom != null)
            {
                return dom.CreateLSInput();
            }
            else
            {
                return null;
            }
        }

        public static LSOutput CreateLSOutput()
        {
            var dom = DOMImplementationBase.Default as DOMImplementationLS;
            if (dom != null)
            {
                return dom.CreateLSOutput();
            }
            else
            {
                return null;
            }
        }
    }
}
