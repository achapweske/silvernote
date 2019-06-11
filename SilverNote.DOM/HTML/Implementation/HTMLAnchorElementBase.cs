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

namespace DOM.HTML.Internal
{
    /// <summary>
    /// The anchor element.
    /// 
    /// http://www.w3.org/TR/2003/REC-DOM-Level-2-HTML-20030109/html.html#ID-48250443
    /// </summary>
    public class HTMLAnchorElementBase : HTMLElementBase, HTMLAnchorElement
    {
        #region Constructors

        internal HTMLAnchorElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.A, ownerDocument)
        {

        }

        #endregion

        #region HTMLAnchorElement

        public string AccessKey
        {
            get { return GetAttribute(HTMLAttributes.ACCESSKEY); }
            set { SetAttribute(HTMLAttributes.ACCESSKEY, value); }
        }

        public string Charset
        {
            get { return GetAttribute(HTMLAttributes.CHARSET); }
            set { SetAttribute(HTMLAttributes.CHARSET, value); }
        }

        public string Coords
        {
            get { return GetAttribute(HTMLAttributes.COORDS); }
            set { SetAttribute(HTMLAttributes.COORDS, value); }
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

        public string Name
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
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

        public string Shape
        {
            get { return GetAttribute(HTMLAttributes.SHAPE); }
            set { SetAttribute(HTMLAttributes.SHAPE, value); }
        }

        public int TabIndex
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.TABINDEX, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.TABINDEX, value); }
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

        public void Blur()
        {

        }

        public void Focus()
        {

        }

        #endregion
    }
}
