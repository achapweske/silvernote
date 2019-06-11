/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.Style.Internal
{
    /// <summary>
    /// The StyleSheet interface is the abstract base interface for any type of style sheet.
    /// 
    /// http://www.w3.org/TR/DOM-Level-2-Style/stylesheets.html#StyleSheets-StyleSheet
    /// </summary>
    public class StyleSheetBase : StyleSheet
    {
        #region Fields

        Node _OwnerNode;
        StyleSheet _ParentStylesheet;
        string _Title;
        MediaListBase _Media;

        #endregion

        #region Constructors

        public StyleSheetBase(string title, string media)
        {
            _Title = title;
            _Media = new MediaListBase();
            _Media.MediaText = media;
        }

        public StyleSheetBase(Node ownerNode)
        {
            _OwnerNode = ownerNode;
        }

        public StyleSheetBase(Node ownerNode, StyleSheet parentStylesheet)
            : this(ownerNode)
        {
            _ParentStylesheet = parentStylesheet;
        }

        #endregion

        #region StyleSheet

        /// <summary>
        /// The node that associates this style sheet with the document.
        /// 
        /// For HTML, this may be the corresponding LINK or STYLE element.
        /// </summary>
        public virtual Node OwnerNode
        {
            get { return _OwnerNode; }
        }

        /// <summary>
        /// Parent style sheet, or null if this is a top-level style sheet
        /// </summary>
        public virtual StyleSheet ParentStyleSheet
        {
            get { return _ParentStylesheet; }
        }

        /// <summary>
        /// The advisory title.
        /// </summary>
        public virtual string Title
        {
            get 
            {
                if (OwnerNode != null)
                {
                    return GetOwnerAttribute(Attributes.TITLE);
                }
                else
                {
                    return _Title;
                }
            }
        }

        /// <summary>
        /// Style sheet language for this style sheet (e.g. "text/css")
        /// </summary>
        public virtual string Type
        {
            get { return GetOwnerAttribute(Attributes.TYPE); }
        }

        /// <summary>
        /// The intended destination media for style information.
        /// </summary>
        public virtual MediaList Media
        {
            get
            {
                if (_Media == null)
                {
                    _Media = new MediaListBase();
                }

                if (OwnerNode != null)
                {
                    _Media.MediaText = GetOwnerAttribute(Attributes.MEDIA);
                }

                return _Media;
            }
        }

        /// <summary>
        /// The style sheet's location (linked styles only)
        /// </summary>
        public virtual string HRef
        {
            get { return GetOwnerAttribute(Attributes.HREF); }
        }

        /// <summary>
        /// True to disable this stylesheet; otherwise false.
        /// </summary>
        public virtual bool Disabled { get; set; }

        #endregion

        #region Implementation

        private string GetOwnerAttribute(string attributeName)
        {
            var owner = OwnerNode as Element;
            if (owner != null)
            {
                return owner.GetAttribute(attributeName) ?? "";
            }
            else
            {
                return String.Empty;
            }
        }

        #endregion
    }
}
