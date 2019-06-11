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
    /// Embedded image. See the IMG element definition in HTML 4.01.
    /// http://www.w3.org/TR/2003/REC-DOM-Level-2-HTML-20030109/html.html#ID-17701901
    /// </summary>
    public class HTMLImageElementBase : HTMLElementBase, HTMLImageElement
    {
        #region Constructors

        internal HTMLImageElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.IMG, ownerDocument)
        {

        }

        #endregion

        #region HTMLImageElement

        public string Name 
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
        }

        public string Align 
        { 
            get { return GetAttribute(HTMLAttributes.ALIGN); }
            set { SetAttribute(HTMLAttributes.ALIGN, value); }
        }
        
        public string Alt 
        { 
            get { return GetAttribute(HTMLAttributes.ALT); }
            set { SetAttribute(HTMLAttributes.ALT, value); }
        }

        public string Border 
        {
            get { return GetAttribute(HTMLAttributes.BORDER); }
            set { SetAttribute(HTMLAttributes.BORDER, value); }
        }
        
        public int Height 
        {
            get 
            {
                return this.GetAttributeAsInt(HTMLAttributes.HEIGHT, 0); 
            }
            set 
            {
                if (value != 0)
                {
                    this.SetAttributeAsInt(HTMLAttributes.HEIGHT, value);
                }
                else
                {
                    RemoveAttribute(HTMLAttributes.HEIGHT);
                }
            }
        }
        
        public int HSpace 
        {
            get 
            {
                return this.GetAttributeAsInt(HTMLAttributes.HSPACE, 0); 
            }
            set 
            {
                if (value != 0)
                {
                    this.SetAttributeAsInt(HTMLAttributes.HSPACE, value);
                }
                else
                {
                    RemoveAttribute(HTMLAttributes.HSPACE);
                }
            }
        }
        
        public bool IsMap 
        {
            get { return this.GetAttributeAsBool(HTMLAttributes.ISMAP, false); }
            set { this.SetAttributeAsBool(HTMLAttributes.ISMAP, value); }
        }
        
        public string LongDesc 
        {
            get { return GetAttribute(HTMLAttributes.LONGDESC); }
            set { SetAttribute(HTMLAttributes.LONGDESC, value); }
        }
        
        public string Src 
        {
            get { return GetAttribute(HTMLAttributes.SRC); }
            set { SetAttribute(HTMLAttributes.SRC, value); }
        }
        
        public string UseMap 
        {
            get { return GetAttribute(HTMLAttributes.USEMAP); }
            set { SetAttribute(HTMLAttributes.USEMAP, value); }
        }
        
        public int VSpace 
        {
            get 
            {
                return this.GetAttributeAsInt(HTMLAttributes.VSPACE, 0); 
            }
            set 
            {
                if (value != 0)
                {
                    this.SetAttributeAsInt(HTMLAttributes.VSPACE, value);
                }
                else
                {
                    RemoveAttribute(HTMLAttributes.VSPACE);
                }
            }
        }
        
        public int Width 
        {
            get 
            {
                return this.GetAttributeAsInt(HTMLAttributes.WIDTH, 0); 
            }
            set 
            {
                if (value != 0)
                {
                    this.SetAttributeAsInt(HTMLAttributes.WIDTH, value);
                }
                else
                {
                    RemoveAttribute(HTMLAttributes.WIDTH);
                }
            }
        }

        #endregion
    }
}
