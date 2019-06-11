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
    public class SVGDefsElementImpl : SVGElementBase, SVGDefsElement
    {
        #region Constructors

        internal SVGDefsElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.DEFS)
        {

        }

        #endregion

        #region SVGTests

        public SVGStringList RequiredFeatures
        {
            get { throw new NotImplementedException(); }
        }

        public SVGStringList RequiredExtensions
        {
            get { throw new NotImplementedException(); }
        }

        public SVGStringList SystemLanguage
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasExtension(string extension)
        {
            throw new NotImplementedException();
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

        #region SVGExternalResourcesRequired

        SVGAnimatedBoolean _ExternalResourcesRequired;

        public SVGAnimatedBoolean ExternalResourcesRequired
        {
            get
            {
                if (_ExternalResourcesRequired == null)
                {
                    _ExternalResourcesRequired = new SVGAnimatedBooleanImpl(this, SVGAttributes.EXTERNAL_RESOURCES_REQUIRED);
                }
                return _ExternalResourcesRequired;
            }
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

        #region SVGTransformable

        public SVGAnimatedTransformList Transform
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region SVGLocatable

        public SVGElement NearestViewportElement
        {
            get { throw new NotImplementedException(); }
        }

        public SVGElement FarthestViewportElement
        {
            get { throw new NotImplementedException(); }
        }

        public SVGRect GetBBox()
        {
            throw new NotImplementedException();
        }

        public SVGMatrix GetCTM()
        {
            throw new NotImplementedException();
        }

        public SVGMatrix GetScreenCTM()
        {
            throw new NotImplementedException();
        }

        public SVGMatrix GetTransformToElement(SVGElement element)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
