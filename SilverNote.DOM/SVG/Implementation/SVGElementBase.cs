/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS;
using DOM.CSS.Internal;
using DOM.LS;

namespace DOM.SVG.Internal
{
    public class SVGElementBase : CSSElement, SVGElement, ElementCSSInlineStyle
    {
        #region Constructors

        public SVGElementBase(SVGDocument ownerDocument, string namespaceURI, string qualifiedName)
            : base(ownerDocument, namespaceURI, qualifiedName)
        {

        }

        #endregion

        #region SVGElement

        public string ID
        {
            get { return GetAttribute(SVGAttributes.ID); }
            set { SetAttribute(SVGAttributes.ID, value); }
        }

        public string XmlBase
        {
            get { return GetAttribute("xml:base"); }
            set { SetAttribute("xml:base", value); }
        }

        public SVGSVGElement OwnerSVGElement 
        {
            get
            {
                for (var ancestor = ParentNode; ancestor != null; ancestor = ancestor.ParentNode)
                {
                    if (ancestor.NodeName == SVGElements.SVG)
                    {
                        return (SVGSVGElement)ancestor;
                    }
                }
                return null;
            }
        }

        public SVGElement ViewportElement 
        {
            get { return OwnerSVGElement; }
        }

        #endregion

        #region ElementCSSInlineStyle

        /// <summary>
        /// This element's inline style
        /// </summary>
        public CSS3StyleDeclaration Style
        {
            get { return InlineStyle; }
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
