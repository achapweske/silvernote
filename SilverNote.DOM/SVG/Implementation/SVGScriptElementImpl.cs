using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DOM.SVG.Internal
{
    public class SVGScriptElementImpl : SVGElementBase, SVGScriptElement
    {
        #region Constructors

        public SVGScriptElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.SCRIPT)
        {

        }

        #endregion

        #region SVGScriptElement

        public string Type
        {
            get 
            {
                // http://www.w3.org/TR/SVG/script.html#ScriptElement:
                //
                // type = "content-type"
                //   Identifies the scripting language for the given ‘script’ 
                //   element. The value content-type specifies a media type, 
                //   per Multipurpose Internet Mail Extensions (MIME) Part Two
                //   [RFC2046]. If a ‘type’ is not provided, the value of 
                //   ‘contentScriptType’ on the ‘svg’ element shall be used, 
                //   which in turn defaults to "application/ecmascript" 
                //   [RFC4329]. If a ‘script’ element falls outside of the 
                //   outermost svg element and the ‘type’ is not provided, the 
                //   ‘type’ must default to "application/ecmascript" [RFC4329].

                string type = GetAttribute(SVGAttributes.TYPE);

                if (!String.IsNullOrWhiteSpace(type))
                {
                    return type;
                }

                SVGSVGElement svgElement = OwnerSVGElement;
                if (svgElement != null)
                {
                    return svgElement.ContentScriptType;
                }
                else
                {
                    return "application/ecmascript";
                }
            }
            set 
            { 
                SetAttribute(SVGAttributes.TYPE, value); 
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
    }
}
