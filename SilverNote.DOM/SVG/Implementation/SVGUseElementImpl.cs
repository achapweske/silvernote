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
    public class SVGUseElementImpl : SVGElementBase, SVGUseElement
    {
        #region Constructors

        internal SVGUseElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.USE)
        {

        }

        #endregion

        #region SVGUseElement

        SVGAnimatedLength _X;

        public SVGAnimatedLength X
        {
            get
            {
                if (_X == null)
                {
                    _X = new SVGAnimatedLengthImpl(this, SVGAttributes.X);
                }
                return _X;
            }
        }

        SVGAnimatedLength _Y;

        public SVGAnimatedLength Y
        {
            get
            {
                if (_Y == null)
                {
                    _Y = new SVGAnimatedLengthImpl(this, SVGAttributes.Y);
                }
                return _Y;
            }
        }

        SVGAnimatedLength _Width;

        public SVGAnimatedLength Width
        {
            get
            {
                if (_Width == null)
                {
                    _Width = new SVGAnimatedLengthImpl(this, SVGAttributes.WIDTH);
                }
                return _Width;
            }
        }

        SVGAnimatedLength _Height;

        public SVGAnimatedLength Height
        {
            get
            {
                if (_Height == null)
                {
                    _Height = new SVGAnimatedLengthImpl(this, SVGAttributes.HEIGHT);
                }
                return _Height;
            }
        }

        SVGElementInstance _InstanceRoot;

        public SVGElementInstance InstanceRoot
        {
            get
            {
                if (_InstanceRoot == null)
                {
                    _InstanceRoot = new SVGElementInstanceImpl(this, false);
                }
                return _InstanceRoot;
            }
        }

        SVGElementInstance _AnimatedInstanceRoot;

        public SVGElementInstance AnimatedInstanceRoot
        {
            get
            {
                if (_AnimatedInstanceRoot == null)
                {
                    _AnimatedInstanceRoot = new SVGElementInstanceImpl(this, true);
                }
                return _AnimatedInstanceRoot;
            }
        }

        #endregion

        #region SVGURIReference

        SVGAnimatedString _HRef;

        public SVGAnimatedString HRef
        {
            get
            {
                if (_HRef == null)
                {
                    _HRef = new SVGAnimatedStringImpl(this, SVGAttributes.XLINK_HREF);
                }
                return _HRef;
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

        #region SVGTransformable

        public SVGAnimatedTransformList Transform
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
