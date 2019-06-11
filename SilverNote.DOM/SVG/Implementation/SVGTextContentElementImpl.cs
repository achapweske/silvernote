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
    public class SVGTextContentElementImpl : SVGElementBase, SVGTextContentElement
    {
        #region Constructors

        public SVGTextContentElementImpl(SVGDocument ownerDocument, string namespaceURI, string qualifiedName)
            : base(ownerDocument, namespaceURI, qualifiedName)
        {

        }

        #endregion

        #region SVGTextContentElement

        public SVGAnimatedLength TextLength
        {
            get { throw new NotImplementedException(); }
        }

        public SVGAnimatedEnumeration LengthAdjust
        {
            get { throw new NotImplementedException(); }
        }

        public int GetNumberOfChars()
        {
            throw new NotImplementedException();
        }

        public double GetComputedTextLength()
        {
            throw new NotImplementedException();
        }

        public double GetSubStringLength(int charnum, int nchars)
        {
            throw new NotImplementedException();
        }

        public SVGPoint GetStartPositionOfChar(int charnum)
        {
            throw new NotImplementedException();
        }

        public SVGPoint GetEndPositionOfChar(int charnum)
        {
            throw new NotImplementedException();
        }

        public SVGRect GetExtentOfChar(int charnum)
        {
            throw new NotImplementedException();
        }

        public double GetRotationOfChar(int charnum)
        {
            throw new NotImplementedException();
        }

        public int GetCharNumAtPosition(SVGPoint point)
        {
            throw new NotImplementedException();
        }

        public void SelectSubString(int charnum, int nchars)
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

    }
}
