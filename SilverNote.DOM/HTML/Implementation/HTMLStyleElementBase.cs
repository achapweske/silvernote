/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DOM.Internal;
using DOM.CSS;
using DOM.CSS.Internal;
using DOM.Style;

namespace DOM.HTML.Internal
{
    /// <summary>
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-16428977
    /// </summary>
    public class HTMLStyleElementBase : HTMLElementBase, HTMLStyleElement, LinkStyle
    {
        #region Fields

        CSSStyleSheetImpl _Sheet;

        #endregion

        #region Constructors

        internal HTMLStyleElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.STYLE, ownerDocument)
        {
            _Sheet = new CSSStyleSheetImpl(this);
        }

        #endregion

        #region HTMLStyleElement

        public bool Disabled
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.DISABLED, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.DISABLED, value); }
        }

        public string Media
        {
            get { return GetAttribute(HTMLAttributes.MEDIA); }
            set { SetAttribute(HTMLAttributes.MEDIA, value); }
        }

        public string Type
        {
            get { return GetAttribute(HTMLAttributes.TYPE); }
            set { SetAttribute(HTMLAttributes.TYPE, value); }
        }

        #endregion

        #region LinkStyle

        public StyleSheet Sheet
        {
            get { return _Sheet; }
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
