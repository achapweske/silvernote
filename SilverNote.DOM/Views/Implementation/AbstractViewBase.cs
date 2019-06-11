/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Views.Internal
{
    /// <summary>
    /// A base interface that all views shall derive from.
    /// 
    /// http://www.w3.org/TR/2000/REC-DOM-Level-2-Views-20001113/views.html
    /// </summary>
    public class AbstractViewBase : AbstractView
    {
        #region Fields

        DocumentView _Document;

        #endregion

        #region Constructors

        public AbstractViewBase(DocumentView document)
        {
            _Document = document;
        }

        #endregion

        #region AbstractView

        /// <summary>
        /// The source DocumentView of which this is an AbstractView.
        /// </summary>
        public DocumentView Document
        {
            get { return _Document; }
        }

        #endregion
    }
}
