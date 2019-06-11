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
    public class HTMLTableElementBase : HTMLElementBase, HTMLTableElement
    {
        #region Constructors

        internal HTMLTableElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.TABLE, ownerDocument)
        {

        }

        #endregion

        #region HTMLTableElement

        public HTMLTableCaptionElement Caption
        {
            get { return (HTMLTableCaptionElement)GetElementByTagName(HTMLElements.CAPTION, false); }
            set { throw new NotImplementedException(); }
        }

        public HTMLTableSectionElement THead
        {
            get { return (HTMLTableSectionElement)GetElementByTagName(HTMLElements.THEAD, false); }
            set { throw new NotImplementedException(); }
        }

        public HTMLTableSectionElement TFoot
        {
            get { return (HTMLTableSectionElement)GetElementByTagName(HTMLElements.TFOOT, false); }
            set { throw new NotImplementedException(); }
        }

        public HTMLCollection Rows
        {
            get { throw new NotImplementedException(); }
        }

        public HTMLCollection TBodies
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

        public string Border
        {
            get { return GetAttribute(HTMLAttributes.BORDER); }
            set { SetAttribute(HTMLAttributes.BORDER, value); }
        }

        public string CellPadding
        {
            get { return GetAttribute(HTMLAttributes.CELLPADDING); }
            set { SetAttribute(HTMLAttributes.CELLPADDING, value); }
        }

        public string CellSpacing
        {
            get { return GetAttribute(HTMLAttributes.CELLSPACING); }
            set { SetAttribute(HTMLAttributes.CELLSPACING, value); }
        }

        public string Frame
        {
            get { return GetAttribute(HTMLAttributes.FRAME); }
            set { SetAttribute(HTMLAttributes.FRAME, value); }
        }

        public string Rules
        {
            get { return GetAttribute(HTMLAttributes.RULES); }
            set { SetAttribute(HTMLAttributes.RULES, value); }
        }

        public string Summary
        {
            get { return GetAttribute(HTMLAttributes.SUMMARY); }
            set { SetAttribute(HTMLAttributes.SUMMARY, value); }
        }

        public string Width
        {
            get { return GetAttribute(HTMLAttributes.WIDTH); }
            set { SetAttribute(HTMLAttributes.WIDTH, value); }
        }

        public HTMLElement CreateTHead()
        {
            HTMLElement thead = (HTMLElement)GetElementByTagName(HTMLElements.THEAD, false);
            if (thead == null)
            {
                thead = (HTMLElement)OwnerDocument.CreateElement(HTMLElements.THEAD);
                AppendChild(thead);
            }
            return thead;
        }

        public void DeleteTHead()
        {
            HTMLElement thead = (HTMLElement)GetElementByTagName(HTMLElements.THEAD, false);
            if (thead != null)
            {
                RemoveChild(thead);
            }
        }

        public HTMLElement CreateTFoot()
        {
            HTMLElement tfoot = (HTMLElement)GetElementByTagName(HTMLElements.TFOOT, false);
            if (tfoot == null)
            {
                tfoot = (HTMLElement)OwnerDocument.CreateElement(HTMLElements.TFOOT);
                AppendChild(tfoot);
            }
            return tfoot;
        }

        public void DeleteTFoot()
        {
            HTMLElement tfoot = (HTMLElement)GetElementByTagName(HTMLElements.TFOOT, false);
            if (tfoot != null)
            {
                RemoveChild(tfoot);
            }
        }

        public HTMLElement CreateCaption()
        {
            HTMLElement caption = (HTMLElement)GetElementByTagName(HTMLElements.CAPTION, false);
            if (caption == null)
            {
                caption = (HTMLElement)OwnerDocument.CreateElement(HTMLElements.CAPTION);
                AppendChild(caption);
            }
            return caption;
        }

        public void DeleteCaption()
        {
            HTMLElement caption = (HTMLElement)GetElementByTagName(HTMLElements.CAPTION, false);
            if (caption != null)
            {
                RemoveChild(caption);
            }
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
