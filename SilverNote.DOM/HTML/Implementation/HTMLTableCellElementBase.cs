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
    /// Represents the TH and TD elements. See the TD element definition in HTML 4.01.
    /// 
    /// DOM: http://www.w3.org/TR/2003/REC-DOM-Level-2-HTML-20030109/html.html#ID-82915075
    /// HTML 4.01: http://www.w3.org/TR/1999/REC-html401-19991224/struct/tables.html#edef-TD
    /// HTML 5: http://www.whatwg.org/specs/web-apps/current-work/multipage/tabular-data.html#the-td-element
    /// </summary>
    public class HTMLTableCellElementBase : HTMLElementBase, HTMLTableCellElement
    {
        #region Constructors

        internal HTMLTableCellElementBase(string nodeName, HTMLDocumentImpl ownerDocument)
            : base(nodeName, ownerDocument)
        {

        }

        #endregion

        #region HTMLTableCellElement

        public string Abbr
        {
            get { return GetAttribute(HTMLAttributes.ABBR); }
            set { SetAttribute(HTMLAttributes.ABBR, value); }
        }

        public string Align
        {
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }

        public string Axis
        {
            get { return GetAttribute(HTMLAttributes.AXIS); }
            set { SetAttribute(HTMLAttributes.AXIS, value); }
        }

        public string BgColor
        {
            get { return GetAttribute(HTMLAttributes.BGCOLOR); }
            set { SetAttribute(HTMLAttributes.BGCOLOR, value); }
        }

        public int CellIndex
        {
            get { return int.Parse(GetAttribute(HTMLAttributes.CELLINDEX)); }
            set { SetAttribute(HTMLAttributes.CELLINDEX, value.ToString()); }
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

        public int ColSpan
        {
            get
            {
                return this.GetAttributeAsInt(HTMLAttributes.COLSPAN, 0);
            }
            set
            {
                if (value != 0)
                {
                    this.SetAttributeAsInt(HTMLAttributes.COLSPAN, value);
                }
                else
                {
                    RemoveAttribute(HTMLAttributes.COLSPAN);
                }
            }
        }

        public string Headers
        {
            get { return GetAttribute(HTMLAttributes.HEADERS); }
            set { SetAttribute(HTMLAttributes.HEADERS, value); }
        }

        public string Height
        {
            get { return GetAttribute(HTMLAttributes.HEIGHT); }
            set { SetAttribute(HTMLAttributes.HEIGHT, value); }
        }

        public bool NoWrap
        {
            get { return bool.Parse(GetAttribute(HTMLAttributes.NOWRAP)); }
            set { SetAttribute(HTMLAttributes.NOWRAP, value.ToString()); }
        }

        public int RowSpan
        {
            get 
            {
                return this.GetAttributeAsInt(HTMLAttributes.ROWSPAN, 0); 
            }
            set
            {
                if (value != 0)
                {
                    this.SetAttributeAsInt(HTMLAttributes.ROWSPAN, value);
                }
                else
                {
                    RemoveAttribute(HTMLAttributes.ROWSPAN);
                }
            }
        }

        public string Scope
        {
            get { return GetAttribute(HTMLAttributes.SCOPE); }
            set { SetAttribute(HTMLAttributes.SCOPE, value); }
        }

        public string VAlign
        {
            get { return GetAttribute(HTMLAttributes.VALIGN); }
            set { SetAttribute(HTMLAttributes.VALIGN, value); }
        }

        public string Width
        {
            get { return GetAttribute(HTMLAttributes.WIDTH); }
            set { SetAttribute(HTMLAttributes.WIDTH, value); }
        }

        #endregion

        #region HTMLElement

        /// <summary>
        /// Overriden from base class
        /// </summary>
        public override string InnerText
        {
            get
            {
                return base.InnerText + "\n";
            }
            set
            {
                base.InnerText = value;
            }
        }

        #endregion

    }
}
