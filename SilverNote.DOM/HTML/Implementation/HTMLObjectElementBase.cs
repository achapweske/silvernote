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
    /// Generic embedded object.
    /// http://www.w3.org/TR/DOM-Level-2-HTML/html.html#ID-9893177
    /// </summary>
    public class HTMLObjectElementBase : HTMLElementBase, HTMLObjectElement
    {
        #region Constructors

        internal HTMLObjectElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.OBJECT, ownerDocument)
        {

        }

        #endregion

        #region HTMLObjectElement

        public HTMLFormElement Form
        {
            get { return (HTMLFormElement)GetAncestorByTagName(HTMLElements.FORM); }
        }

        public string Code 
        {
            get { return GetAttribute(HTMLAttributes.CODE); }
            set { SetAttribute(HTMLAttributes.CODE, value); }
        }

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        public string Archive
        {
            get { return GetAttribute(HTMLAttributes.ARCHIVE); }
            set { SetAttribute(HTMLAttributes.ARCHIVE, value); }
        }

        public string Border
        {
            get { return GetAttribute(HTMLAttributes.BORDER); }
            set { SetAttribute(HTMLAttributes.BORDER, value); }
        }

        public string CodeBase
        {
            get { return GetAttribute(HTMLAttributes.CODEBASE); }
            set { SetAttribute(HTMLAttributes.CODEBASE, value); }
        }

        public string CodeType
        {
            get { return GetAttribute(HTMLAttributes.CODETYPE); }
            set { SetAttribute(HTMLAttributes.CODETYPE, value); }
        }

        public string Data
        {
            get { return GetAttribute(HTMLAttributes.DATA); }
            set { SetAttribute(HTMLAttributes.DATA, value); }
        }

        public bool Declare
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.DECLARE, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.DECLARE, value); }
        }

        public string Height
        {
            get { return GetAttribute(HTMLAttributes.HEIGHT); }
            set { SetAttribute(HTMLAttributes.HEIGHT, value); }
        }

        public int HSpace
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.HSPACE, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.HSPACE, value); }
        }

        public string Name
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
        }

        public string Standby
        {
            get { return GetAttribute(HTMLAttributes.STANDBY); }
            set { SetAttribute(HTMLAttributes.STANDBY, value); }
        }

        public int TabIndex
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.TABINDEX, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.TABINDEX, value); }
        }

        public string Type
        {
            get { return GetAttribute(HTMLAttributes.TYPE); }
            set { SetAttribute(HTMLAttributes.TYPE, value); }
        }

        public string UseMap
        {
            get { return GetAttribute(HTMLAttributes.USEMAP); }
            set { SetAttribute(HTMLAttributes.USEMAP, value); }
        }

        public int VSpace
        {
            get { return this.GetAttributeAsInt(HTMLAttributes.VSPACE, 0); }
            set { this.SetAttributeAsInt(HTMLAttributes.VSPACE, value); }
        }

        public string Width
        {
            get { return GetAttribute(HTMLAttributes.WIDTH); }
            set { SetAttribute(HTMLAttributes.WIDTH, value); }
        }

        public Document ContentDocument 
        {
            get { return null; } 
        }

        #endregion
    }
}
