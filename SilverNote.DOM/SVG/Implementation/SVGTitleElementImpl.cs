/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DOM.CSS;

namespace DOM.SVG.Internal
{
    public class SVGTitleElementImpl : SVGElementBase, SVGTitleElement
    {
        #region Constructors

        internal SVGTitleElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.TITLE)
        {

        }

        #endregion

        #region SVGStyleable

        SVGAnimatedString _ClassName;

        public SVGAnimatedString ClassName
        {
            get
            {
                if (_ClassName == null)
                {
                    _ClassName = new SVGAnimatedStringImpl(this, SVGAttributes.CLASS);
                }
                return _ClassName;
            }
        }

        public CSSValue GetPresentationAttribute(string name)
        {
            return Style.GetPropertyCSSValue(name);
        }

        #endregion

        #region SVGLangSpace

        public string XmlLang
        {
            get { return GetAttribute(SVGAttributes.LANG); }
            set { SetAttribute(SVGAttributes.LANG, value); }
        }

        public string XmlSpace
        {
            get { return GetAttribute(SVGAttributes.SPACE); }
            set { SetAttribute(SVGAttributes.SPACE, value); }
        }

        #endregion
    }
}
