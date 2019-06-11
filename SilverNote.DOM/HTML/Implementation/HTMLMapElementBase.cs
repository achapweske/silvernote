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
    public class HTMLMapElementBase : HTMLElementBase, HTMLMapElement
    {
        #region Constructors

        internal HTMLMapElementBase(HTMLDocumentImpl ownerDocument)
            : base(HTMLElements.MAP, ownerDocument)
        {

        }

        #endregion

        #region HTMLMapElement

        public HTMLCollection Areas 
        {
            get 
            {
                var areas = GetElementsByTagName(HTMLElements.AREA);
                return new HTMLCollectionBase(areas); 
            }
        }

        public string Name
        {
            get { return GetAttribute(HTMLAttributes.NAME); }
            set { SetAttribute(HTMLAttributes.NAME, value); }
        }

        #endregion
    }
}
