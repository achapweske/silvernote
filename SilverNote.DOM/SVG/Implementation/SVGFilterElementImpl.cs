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
    public class SVGFilterElementImpl : SVGElementBase, SVGFilterElement
    {
        #region Constructors

        public SVGFilterElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.FILTER)
        {

        }

        #endregion

        #region SVGFilterElement

        SVGAnimatedEnumeration _FilterUnits;

        public SVGAnimatedEnumeration FilterUnits
        {
            get
            {
                if (_FilterUnits == null)
                {
                    _FilterUnits = new SVGAnimatedEnumerationImpl(this, SVGAttributes.FILTER_UNITS, SVGUnitTypesParser.Instance);
                }
                return _FilterUnits;
            }
        }

        SVGAnimatedEnumeration _PrimitiveUnits;

        public SVGAnimatedEnumeration PrimitiveUnits
        {
            get
            {
                if (_PrimitiveUnits == null)
                {
                    _PrimitiveUnits = new SVGAnimatedEnumerationImpl(this, SVGAttributes.PRIMITIVE_UNITS, SVGUnitTypesParser.Instance);
                }
                return _PrimitiveUnits;
            }
        }

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

        public SVGAnimatedInteger FilterResX
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public SVGAnimatedInteger FilterResY
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void SetFilterRes(int filterResX, int filterResY)
        {
            throw new NotImplementedException();
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

        #region SVGExternalResourcesRequiredImpl

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
    }
}
