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
    public class SVGLineElementImpl : SVGElementBase, SVGLineElement
    {
        #region Constructors

        internal SVGLineElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.LINE)
        {

        }

        #endregion

        #region SVGLineElement

        SVGAnimatedLength _X1;

        public SVGAnimatedLength X1
        {
            get
            {
                if (_X1 == null)
                {
                    _X1 = new SVGAnimatedLengthImpl(this, SVGAttributes.X1);
                }
                return _X1;
            }
        }

        SVGAnimatedLength _Y1;

        public SVGAnimatedLength Y1
        {
            get
            {
                if (_Y1 == null)
                {
                    _Y1 = new SVGAnimatedLengthImpl(this, SVGAttributes.Y1);
                }
                return _Y1;
            }
        }

        SVGAnimatedLength _X2;

        public SVGAnimatedLength X2
        {
            get
            {
                if (_X2 == null)
                {
                    _X2 = new SVGAnimatedLengthImpl(this, SVGAttributes.X2);
                }
                return _X2;
            }
        }

        SVGAnimatedLength _Y2;

        public SVGAnimatedLength Y2
        {
            get
            {
                if (_Y2 == null)
                {
                    _Y2 = new SVGAnimatedLengthImpl(this, SVGAttributes.Y2);
                }
                return _Y2;
            }
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
