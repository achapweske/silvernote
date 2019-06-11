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
using DOM.Style;

namespace DOM.SVG.Internal
{
    public class SVGStyleElementImpl : SVGElementBase, SVGStyleElement, LinkStyle
    {
        #region Fields

        CSSStyleSheetImpl _Sheet;

        #endregion

        #region Constructors

        internal SVGStyleElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.STYLE)
        {
            _Sheet = new CSSStyleSheetImpl(this);
        }

        #endregion

        #region SVGStyleElement

        public string Type
        {
            get { return GetAttribute(SVGAttributes.TYPE); }
            set { SetAttribute(SVGAttributes.TYPE, value); }
        }

        public string Media
        {
            get { return GetAttribute(SVGAttributes.MEDIA); }
            set { SetAttribute(SVGAttributes.MEDIA, value); }
        }

        public string Title
        {
            get { return GetAttribute(SVGAttributes.TITLE); }
            set { SetAttribute(SVGAttributes.TITLE, value); }
        }

        #endregion

        #region LinkStyle

        public StyleSheet Sheet
        {
            get { return _Sheet; }
        }

        #endregion

        #region SVGLangSpace

        public string XmlLang
        {
            get { return GetAttribute(SVGAttributes.LANG); }
            set { SetAttribute(SVGAttributes.LANG, value); }
        }

        public string XmlSpace
        {
            get { return GetAttribute(SVGAttributes.SPACE); }
            set { SetAttribute(SVGAttributes.SPACE, value); }
        }

        #endregion

        #region Implementation

        protected override void OnChildNodesChanged(IList<Node> oldNodes, IList<Node> newNodes)
        {
            _Sheet = CSSParser.ParseStylesheet(TextContent, _Sheet);
        }

        #endregion
    }
}
