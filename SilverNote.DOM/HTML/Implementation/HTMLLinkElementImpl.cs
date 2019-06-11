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
using DOM.Style;

namespace DOM.HTML.Internal
{
    public class HTMLLinkElementImpl : HTMLElementBase, HTMLLinkElement, LinkStyle
    {
        #region Fields

        StyleSheet _Sheet;

        #endregion

        #region Constructors

        internal HTMLLinkElementImpl(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.LINK, ownerDocument)
        {

        }

        #endregion

        #region HTMLLinkElement

        public bool Disabled 
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.DISABLED, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.DISABLED, value); } 
        }

        public string Charset 
        {
            get { return GetAttribute(HTMLAttributes.CHARSET); }
            set { SetAttribute(HTMLAttributes.CHARSET, value); } 
        }

        public string HRef
        {
            get { return GetAttribute(HTMLAttributes.HREF); }
            set { SetAttribute(HTMLAttributes.HREF, value); }
        }

        public string HRefLang
        {
            get { return GetAttribute(HTMLAttributes.HREFLANG); }
            set { SetAttribute(HTMLAttributes.HREFLANG, value); }
        }

        public string Media
        {
            get { return GetAttribute(HTMLAttributes.MEDIA); }
            set { SetAttribute(HTMLAttributes.MEDIA, value); }
        }

        public string Rel
        {
            get { return GetAttribute(HTMLAttributes.REL); }
            set { SetAttribute(HTMLAttributes.REL, value); }
        }

        public string Rev
        {
            get { return GetAttribute(HTMLAttributes.REV); }
            set { SetAttribute(HTMLAttributes.REV, value); }
        }

        public string Target
        {
            get { return GetAttribute(HTMLAttributes.TARGET); }
            set { SetAttribute(HTMLAttributes.TARGET, value); }
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

        internal void SetStyleSheet(StyleSheet sheet)
        {
            _Sheet = sheet;
        }

        #endregion
    }
}
