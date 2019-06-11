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
    public class HTMLTableRowElementBase : HTMLElementBase, HTMLTableRowElement
    {
        #region Constructors

        internal HTMLTableRowElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.TR, ownerDocument)
        {

        }

        #endregion

        #region HTMLTableRowElement

        public int RowIndex
        {
            get { throw new NotImplementedException(); }
        }

        public int SectionRowIndex
        {
            get { throw new NotImplementedException(); }
        }

        public HTMLCollection Cells
        {
            get { throw new NotImplementedException(); }
        }

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        public string BgColor
        {
            get { return GetAttribute(HTMLAttributes.BGCOLOR); }
            set { SetAttribute(HTMLAttributes.BGCOLOR, value); }
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

        public HTMLElement InsertCell(int index)
        {
            throw new NotImplementedException();
        }

        public void DeleteCell(int index)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
