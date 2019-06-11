/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.HTML.Internal
{
    public class HTMLTableSectionElementBase : HTMLElementBase, HTMLTableSectionElement
    {
        #region Constructors

        internal HTMLTableSectionElementBase(string nodeName, HTMLDocumentImpl ownerDocument)
            : base(nodeName, ownerDocument)
        {

        }

        #endregion

        #region HTMLTableSectionElement

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        public string Ch
        {
            get { return GetAttribute(HTMLAttributes.CH); }
            set { SetAttribute(HTMLAttributes.CH, value); }
        }

        public string ChOff
        {
            get { return GetAttribute(HTMLAttributes.CHOFF); }
            set { SetAttribute(HTMLAttributes.CHOFF, value); }
        }

        public string VAlign
        {
            get { return GetAttribute(HTMLAttributes.VALIGN); }
            set { SetAttribute(HTMLAttributes.VALIGN, value); }
        }

        public HTMLCollection Rows
        {
            get { throw new NotImplementedException(); }
        }

        public HTMLElement InsertRow(int index)
        {
            throw new NotImplementedException();
        }

        public void DeleteRow(int index)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
