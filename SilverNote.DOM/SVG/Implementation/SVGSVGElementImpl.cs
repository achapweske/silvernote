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
using DOM.CSS.Internal;
using DOM.Views;
using DOM.Events;
using DOM.Style;

namespace DOM.SVG.Internal
{
    public class SVGSVGElementImpl : SVGElementBase, SVGSVGElement
    {
        #region Constructors

        internal SVGSVGElementImpl(SVGDocument ownerDocument)
            : base(ownerDocument, SVGElements.NAMESPACE, SVGElements.SVG)
        {

        }

        #endregion

        #region SVGSVGElement

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

        public string ContentScriptType
        {
            get 
            {
                // http://www.w3.org/TR/SVG/script.html#ContentScriptTypeAttribute:
                //
                // "...  The default value is 'application/ecmascript' [RFC4329]."

                string type = GetAttribute(SVGAttributes.CONTENT_SCRIPT_TYPE);
                if (!String.IsNullOrEmpty(type))
                {
                    return type;
                }
                else
                {
                    return "application/ecmascript";
                }
            }

            set { SetAttribute(SVGAttributes.CONTENT_SCRIPT_TYPE, value); }
        }

        public string ContentStyleType
        {
            get { return GetAttribute(SVGAttributes.CONTENT_STYLE_TYPE); }
            set { SetAttribute(SVGAttributes.CONTENT_STYLE_TYPE, value); }
        }

        public SVGRect ViewPort
        {
            get { throw new NotImplementedException(); }
        }

        public double PixelUnitToMillimeterX
        {
            get { return 26; }
        }

        public double PixelUnitToMillimeterY
        {
            get { return 26; }
        }

        public double ScreenPixelToMillimeterX
        {
            get { return 26; }
        }

        public double ScreenPixelToMillimeterY
        {
            get { return 26; }
        }

        public bool UseCurrentView
        {
            get { throw new NotImplementedException(); }
        }

        public SVGViewSpec CurrentView
        {
            get { throw new NotImplementedException(); }
        }

        public double CurrentScale
        {
            get { return 1; }
            set { throw new NotImplementedException(); }
        }

        public SVGPoint CurrentTranslate
        {
            get { throw new NotImplementedException(); }
        }

        public int SuspendRedraw(int maxWaitMilliseconds)
        {
            throw new NotImplementedException();
        }

        public void UnsuspendRedraw(int suspendHandleID)
        {
            throw new NotImplementedException();
        }

        public void UnsuspendRedrawAll()
        {
            throw new NotImplementedException();
        }

        public void ForceRedraw()
        {
            throw new NotImplementedException();
        }

        public void PauseAnimations()
        {
            throw new NotImplementedException();
        }

        public void UnpauseAnimations()
        {
            throw new NotImplementedException();
        }

        public bool AnimationsPaused()
        {
            return false;
        }

        public double GetCurrentTime()
        {
            return 0;
        }

        public void SetCurrentTime(double seconds)
        {
            throw new NotImplementedException();
        }

        public NodeList GetIntersectionList(SVGRect rect, SVGElement referenceElement)
        {
            throw new NotImplementedException();
        }

        public NodeList GetEnclosureList(SVGRect rect, SVGElement referenceElement)
        {
            throw new NotImplementedException();
        }

        public bool CheckIntersection(SVGElement element, SVGRect rect)
        {
            throw new NotImplementedException();
        }

        public bool CheckEnclosure(SVGElement element, SVGRect rect)
        {
            throw new NotImplementedException();
        }

        public void DeselectAll()
        {
            throw new NotImplementedException();
        }

        public SVGNumber CreateSVGNumber()
        {
            return new MutableSVGNumber();
        }

        public SVGLength CreateSVGLength()
        {
            return new MutableSVGLength();
        }

        public SVGAngle CreateSVGAngle()
        {
            return new MutableSVGAngle();
        }

        public SVGPoint CreateSVGPoint()
        {
            return new MutableSVGPoint();
        }

        public SVGMatrix CreateSVGMatrix()
        {
            return new MutableSVGMatrix();
        }

        public SVGRect CreateSVGRect()
        {
            return new MutableSVGRect();
        }

        public SVGTransform CreateSVGTransform()
        {
            return new MutableSVGTransform();
        }

        public SVGTransform CreateSVGTransformFromMatrix(SVGMatrix matrix)
        {
            throw new NotImplementedException();
        }

        public Element GetElementById(string elementId)
        {
            throw new NotImplementedException();
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

        #region SVGFitToViewBox

        public SVGAnimatedRect ViewBox
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public SVGAnimatedPreserveAspectRatio PreserveAspectRatio
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region SVGZoomAndPan

        public SVGZoomAndPanType ZoomAndPan
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region DocumentEvent

        public Event CreateEvent(string eventType)
        {
            var document = OwnerDocument as DocumentEvent;
            if (document != null)
            {
                return document.CreateEvent(eventType);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region ViewCSS

        public DocumentView Document
        {
            get { return ((AbstractView)OwnerDocument).Document; }
        }

        public CSSStyleDeclaration GetComputedStyle(Element elt, string pseudoElt)
        {
            if (elt is CSSElement)
            {
                return ((CSSElement)elt).ComputedStyle;
            }
            else
            {
                return null;
            } 
        }

        #endregion

        #region DocumentCSS

        public StyleSheetList StyleSheets
        {
            get { return ((DocumentCSS)OwnerDocument).StyleSheets; }
        }

        public CSSStyleDeclaration GetOverrideStyle(Element elt, string pseudoElt)
        {
            return ((DocumentCSS)OwnerDocument).GetOverrideStyle(elt, pseudoElt);
        }

        public CSSStyleSheet UserStyleSheet
        {
            get { return ((DocumentCSS)OwnerDocument).UserStyleSheet; }
            set { ((DocumentCSS)OwnerDocument).UserStyleSheet = value; }
        }

        #endregion
    }
}
