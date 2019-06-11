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
    public class SVGGradientElementImpl : SVGElementBase, SVGGradientElement
    {
        #region Constructors

        internal SVGGradientElementImpl(SVGDocument ownerDocument, string namespaceURI, string qualifiedName)
            : base(ownerDocument, namespaceURI, qualifiedName)
        {

        }

        #endregion

        #region SVGGradientElement

        SVGAnimatedEnumeration _GradientUnits;

        public SVGAnimatedEnumeration GradientUnits
        {
            get
            {
                if (_GradientUnits == null)
                {
                    _GradientUnits = new SVGAnimatedEnumerationImpl(this, SVGAttributes.GRADIENT_UNITS, SVGUnitTypesParser.Instance);
                }
                return _GradientUnits;
            }
        }

        public SVGAnimatedTransformList GradientTransform
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        SVGAnimatedEnumeration _SpreadMethod;

        public SVGAnimatedEnumeration SpreadMethod
        {
            get
            {
                if (_SpreadMethod == null)
                {
                    _SpreadMethod = new SVGAnimatedEnumerationImpl(this, SVGAttributes.SPREAD_METHOD, SVGSpreadMethodTypesParser.Instance);
                }
                return _SpreadMethod;
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
