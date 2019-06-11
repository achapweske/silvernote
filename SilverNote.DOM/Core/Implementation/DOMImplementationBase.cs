using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS;
using DOM.LS;

namespace DOM.Internal
{
    public class DOMImplementationBase : DOMImplementation, DOMImplementationLS
    {
        #region DOMImplementation

        /// <summary>
        /// Test if the DOM implementation implements a specific feature and version, as specified in DOM Features.
        /// </summary>
        /// <param name="feature">The name of the feature to test. </param>
        /// <param name="version">This is the version number of the feature to test. </param>
        /// <returns>true if the feature is implemented in the specified version, false otherwise.</returns>
        public virtual bool HasFeature(string feature, string version)
        {
            return false;
        }

        /// <summary>
        /// This method returns a specialized object which implements the specialized APIs of the specified feature and version, as specified in DOM Features.
        /// </summary>
        /// <param name="feature">The name of the feature requested. Note that any plus sign "+" prepended to the name of the feature will be ignored since it is not significant in the context of this method.</param>
        /// <param name="version">This is the version number of the feature to test. </param>
        /// <returns>Returns an object which implements the specialized APIs of the specified feature and version, if any, or null if there is no object which implements interfaces associated with that feature.</returns>
        public virtual object GetFeature(string feature, string version)
        {
            return null;
        }

        /// <summary>
        /// Creates an empty DocumentType node. Entity declarations and notations are not made available. Entity reference expansions and default attribute additions do not occur..
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the document type to be created.</param>
        /// <param name="publicId">The external subset public identifier.</param>
        /// <param name="systemId">The external subset system identifier.</param>
        /// <returns>A new DocumentType node with Node.ownerDocument set to null.</returns>
        public virtual DocumentType CreateDocumentType(string qualifiedName, string publicId, string systemId)
        {
            return new DocumentTypeBase(qualifiedName, publicId, systemId);
        }

        /// <summary>
        /// Creates a DOM Document object of the specified type with its document element.
        /// </summary>
        /// <param name="namespaceURI">The namespace URI of the document element to create or null.</param>
        /// <param name="qualifiedName">The qualified name of the document element to be created or null.</param>
        /// <param name="docType">The type of document to be created or null.</param>
        /// <returns>A new Document object with its document element. If the NamespaceURI, qualifiedName, and doctype are null, the returned Document is empty with no document element.</returns>
        public virtual Document CreateDocument(string namespaceURI, string qualifiedName, DocumentType docType)
        {
            if (qualifiedName.Equals("html", StringComparison.OrdinalIgnoreCase))
            {
                return new DOM.HTML.Internal.HTMLDocumentImpl(this, docType);
            }
            else if (qualifiedName.Equals("svg", StringComparison.OrdinalIgnoreCase))
            {
                return new DOM.SVG.Internal.SVGDocumentImpl(this, docType);
            }
            else
            {
                return new DocumentBase(this, docType, namespaceURI, qualifiedName);
            }
        }

        #endregion

        #region DOMImplementationCSS

        public CSSStyleSheet CreateCSSStyleSheet(string title, string media)
        {
            return new DOM.CSS.Internal.CSSStyleSheetImpl(title, media);
        }

        #endregion

        #region DOMImplementationLS

        public LSParser CreateLSParser(DOMImplementationLSMode mode, string schemaType)
        {
            return new DOM.LS.Internal.LSParserImpl();
        }

        public LSSerializer CreateLSSerializer()
        {
            return new DOM.LS.Internal.LSSerializerImpl();
        }

        public LSInput CreateLSInput()
        {
            return new DOM.LS.Internal.LSInputImpl();
        }

        public LSOutput CreateLSOutput()
        {
            return new DOM.LS.Internal.LSOutputImpl();
        }

        #endregion

        #region Extensions

        private static DOMImplementation _Default;

        public static DOMImplementation Default
        {
            get
            {
                if (_Default == null)
                {
                    _Default = new DOMImplementationBase();
                }

                return _Default;
            }
        }

        #endregion
    }
}
